using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Domain;
using Evaluant.Uss.CommonVisitors;

namespace Evaluant.Uss.WcfClient
{
    public class WcfPersistenceEngine : PersistenceEngineAsyncImplementation
    {
        public WcfPersistenceEngine(IPersistenceProviderAsync factory)
            : base(factory)
        {

        }

        ServiceReference.IRemotePersistenceEngine client = new ServiceReference.RemotePersistenceEngineClient();

        public IList<Entity> LoadWithId(string type, string[] id)
        {
            return client.LoadWithIds(type, id);
        }

        public void LoadReference(IEnumerable<Entity> entities, NLinq.NLinqQuery[] references)
        {
            client.LoadReferences(entities, ToString(references));
        }

        private string[] ToString(params NLinq.NLinqQuery[] queries)
        {
            string[] stringQueries = new string[queries.Length];
            for (int i = 0; i < stringQueries.Length; i++)
            {
                stringQueries[i] = ToString(queries[i]);
            }
            return stringQueries;
        }

        private string ToString(NLinq.NLinqQuery query)
        {
            return ExpressionWriter<ExpressionUpdater>.WriteToString(query.Expression);
        }

        protected override void BeginLoadMany<T, U>(AsyncCallback callback, NLinq.NLinqQuery query, object asyncState)
        {
            client.BeginLoad(ToString(query), callback, asyncState);
        }

        public override void BeginLoadReference(AsyncCallback callback, string reference, IEnumerable<Entity> entities, NLinqQuery query, object asyncState)
        {
            client.BeginLoadReferencesOnMany(callback, reference, entities, ToString(query), asyncState);
        }

        protected override void BeginLoadSingle<T>(AsyncCallback callback, NLinq.NLinqQuery query, object asyncState)
        {
            throw new NotImplementedException();
        }

        public override void BeginLoadWithIds(AsyncCallback callback, string type, string[] id, object asyncState)
        {
            throw new NotImplementedException();
        }

        public override void BeginProcessCommands(Transaction tx, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }

        protected override Commands.CommandCollection ComputeCascadeDeleteCommands(string id, string type, IDictionary<string, Commands.Command> processed)
        {
            throw new NotImplementedException();
        }

        public override void CreateId(Entity e)
        {
            throw new NotImplementedException();
        }

        protected override T EndLoadMany<T, U>(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public override void EndLoadReference(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        protected override T EndLoadSingle<T>(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public override IList<Entity> EndLoadWithIds(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public override void EndProcessCommands(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
    }
}
