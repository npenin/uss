using System.Collections;
//using Evaluant.Uss.Commands;
//using Evaluant.Uss.Common;
using System.Globalization;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Domain;
using Evaluant.NLinq;
using System.Collections.Generic;
using Evaluant.Uss.CommonVisitors;

namespace Evaluant.Uss.Remoting
{
    /// <summary>
    /// Simple Delegate Engine implementation to show the interception capabilities of the framework
    /// </summary>
    public class RemotingPersistenceEngine : IPersistenceEngine
    {
        private RemoteController _RemoteEngine;
        public RemoteController RemoteEngine
        {
            get
            {
                return _RemoteEngine;
            }
        }

        public CultureInfo Culture
        {
            get { return null; }
            set { }
        }

        public IPersistenceProvider Factory { get; set; }

        public RemotingPersistenceEngine(RemoteController remoteEngine)
        {
            _RemoteEngine = remoteEngine;
        }

        public IList<Entity> Load(NLinqQuery query)
        {
            return Load(query, 1, 0);
        }

        public IList<Entity> Load(string query)
        {
            return Load(query, 1, 0);
        }

        public Entity LoadWithId(string type, string id)
        {
            IList<Entity> result = LoadWithId(type, new string[] { id });
            return result.Count > 0 ? result[0] : null;
        }

        public void LoadReference(Entity entity)
        {
            LoadReference(new Entity[] { entity });
        }

        public void LoadReference(Entity entity, string[] references)
        {
            LoadReference(new Entity[] { entity }, references);
        }

        public void LoadReference(IEnumerable<Entity> entities)
        {
            LoadReference(entities, new string[0]);
        }

        public void LoadReference(IEnumerable<Entity> entities, params string[] references)
        {
            IEnumerable TmpEntities = _RemoteEngine.LoadReference(entities, references);

            ArrayList al1 = new ArrayList();
            foreach (Entity e in entities)
                al1.Add(e);
            ArrayList al2 = new ArrayList();
            foreach (Entity e in TmpEntities)
                al2.Add(e);
            for (int i = 0; i < al1.Count; i++)
            {
                ((Entity)al1[i]).EntityEntries = ((Entity)al2[i]).EntityEntries;
            }
        }


        public void InitializeRepository()
        {
            _RemoteEngine.InitializeRepository();
        }

        public void Initialize()
        {
            _RemoteEngine.Initialize();
        }

        public IList<Entity> Load(NLinqQuery query, int first, int max)
        {
            ExpressionWriter<ExpressionUpdater>.WriteToString(query.Expression);
            return _RemoteEngine.Load(ExpressionWriter<ExpressionUpdater>.WriteToString(query.Expression), first, max);
        }

        public IList<Entity> Load(string query, int first, int max)
        {
            return _RemoteEngine.Load(query, first, max);
        }

        public IList<Entity> LoadWithId(string type, string[] id)
        {
            return _RemoteEngine.LoadWithId(type, id);
        }

        public object LoadScalar(string opath)
        {
            return _RemoteEngine.LoadScalar(opath);
        }

        public object LoadScalar(NLinqQuery opath)
        {
            return _RemoteEngine.LoadScalar(opath);
        }

        public void BeforeProcessCommands(Transaction tx)
        {
            Transaction remoteTx = _RemoteEngine.BeforeProcessCommands(tx);
            tx.NewIds = remoteTx.NewIds;
            tx.PendingCommands = remoteTx.PendingCommands;
        }

        public void ProcessCommands(Transaction tx)
        {
            Transaction remoteTx = _RemoteEngine.ProcessCommands(tx);
            tx.NewIds = remoteTx.NewIds;
            tx.PendingCommands = remoteTx.PendingCommands;
        }

        public void AfterProcessCommands(Transaction tx)
        {
            Transaction remoteTx = _RemoteEngine.AfterProcessCommands(tx);
            tx.NewIds = remoteTx.NewIds;
            tx.PendingCommands = remoteTx.PendingCommands;
        }

        public Evaluant.Uss.Models.Model Model
        {
            get { return _RemoteEngine.Model; }
        }

        #region IPersistenceEngine Members


        double IPersistenceEngine.LoadScalar(NLinqQuery query)
        {
            return 
        }

        public T LoadScalar<T>(NLinqQuery query)
        {
            throw new System.NotImplementedException();
        }

        public T LoadScalar<T, U>(NLinqQuery query) where T : IEnumerable<U>
        {
            throw new System.NotImplementedException();
        }

        public T LoadScalar<T, U>(NLinqQuery query, int first, int max) where T : IEnumerable<U>
        {
            throw new System.NotImplementedException();
        }

        double IPersistenceEngine.LoadScalar(string query)
        {
            throw new System.NotImplementedException();
        }

        public T LoadScalar<T>(string query)
        {
            throw new System.NotImplementedException();
        }

        public T LoadScalar<T, U>(string query, int first, int max) where T : IEnumerable<U>
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}
