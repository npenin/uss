using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.MongoDB.Protocol;
using Evaluant.Uss.MongoDB.Connections;
using Evaluant.Uss.Domain;
using Evaluant.Uss.MongoDB.Visitors;

namespace Evaluant.Uss.MongoDB
{
    public class MongoPersistenceEngine : PersistenceEngineImplementation
    {
        Connections.Connection connection;

        public override void InitializeRepository()
        {
            string database = new MongoConnectionStringBuilder(connection.ConnectionString).Database;

            connection.Open();

            //InsertMessage im = new InsertMessage();
            //im.FullCollectionName = database;
            //Entity e = new DynamicEntity();
            //e.AddValue("create", im.FullCollectionName);
            //im.Documents.Add(e);
            //connection.SendMessage(im);

            foreach (Model.Entity entityModel in Factory.Model)
            {
                QueryMessage qm = new QueryMessage();
                qm.NumberToReturn = 1;
                qm.NumberToSkip = 0;
                Entity e = new Entity(entityModel.Type);
                qm.FullCollectionName = database + ".$cmd";
                e.Add("create", e.Type);
                qm.Query = e;
                connection.SendTwoWayMessage(qm);
            }
            connection.Close();
        }

        public MongoPersistenceEngine(IPersistenceProvider provider, Connections.Connection connection)
            : base(provider)
        {
            this.connection = connection;
        }

        public override IList<Evaluant.Uss.Domain.Entity> LoadWithId(string type, string[] id)
        {
            throw new NotImplementedException();
        }

        public override void LoadReference(string reference, IEnumerable<Entity> entities, NLinq.NLinqQuery query)
        {
            throw new NotImplementedException();
        }

        public override void ProcessCommands(Transaction tx)
        {
            MongoCommandProcessor processor = new MongoCommandProcessor(new MongoConnectionStringBuilder(connection.ConnectionString).Database);
            processor.Visit(tx);
            connection.Open();
            foreach (IRequestMessage message in processor.Commands)
            {
                connection.SendMessage(message);
            }
            connection.Close();
        }

        protected override T LoadSingle<T>(Evaluant.NLinq.NLinqQuery query)
        {
            NLinqVisitor visitor = new NLinqVisitor(new MongoConnectionStringBuilder(connection.ConnectionString).Database, Factory.Model);
            visitor.Visit(query.Expression);
            EntitySet result = GetEntities(visitor.Query);
            if (result.Count == 0)
                return default(T);
            if (typeof(T) == typeof(Entity))
            {
                result[0].Id = result[0]["_id"].Value.ToString();
                return (T)(object)result[0];
            }
            return (T)Convert.ChangeType(result[0].EntityEntries[0].Value, typeof(T));
        }

        private EntitySet GetEntities(QueryMessage query)
        {
            connection.Open();
            ReplyMessage reply = connection.SendTwoWayMessage(query);
            connection.Close();
            return new EntitySet(reply.Documents);
        }

        protected override TEnumerableOfU LoadMany<TEnumerableOfU, T>(Evaluant.NLinq.NLinqQuery query)
        {
            NLinqVisitor visitor = new NLinqVisitor(new MongoConnectionStringBuilder(connection.ConnectionString).Database, Factory.Model);
            visitor.Visit(query.Expression);
            EntitySet result = GetEntities(visitor.Query);
            if (result.Count == 0)
                return default(TEnumerableOfU);
            if (typeof(T) == typeof(Entity))
            {
                foreach (Entity e in result)
                {
                    e.Id = e["_id"].Value.ToString();
                }
                return (TEnumerableOfU)(object)result;
            }
            return (TEnumerableOfU)Convert.ChangeType(result[0].EntityEntries["n"].Value, typeof(T));
        }

        public override void CreateId(Evaluant.Uss.Domain.Entity e)
        {
            Oid id = Oid.NewOid();
            e.Id = id.ToString();
            e.Add("_id", id);
        }
    }
}
