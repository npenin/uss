using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public class ProxyPersistenceEngine : ProxyPersistenceEngineAsync, IPersistenceEngine
    {
        protected new IPersistenceEngine innerEngine;
        private IPersistenceProvider factory;
        public ProxyPersistenceEngine(IPersistenceEngine innerEngine, IPersistenceProvider factory)
            : base(innerEngine, factory)
        {
            this.innerEngine = innerEngine;
            this.factory = factory;
        }

        #region IPersistenceEngine Members

        public virtual IList<Domain.Entity> Load(NLinq.NLinqQuery query)
        {
            return innerEngine.Load(query);
        }

        public virtual IList<Domain.Entity> Load(string query)
        {
            return innerEngine.Load(query);
        }

        public virtual IList<Domain.Entity> Load(string query, int first, int max)
        {
            return innerEngine.Load(query, first, max);
        }

        public virtual IList<Domain.Entity> Load(NLinq.NLinqQuery query, int first, int max)
        {
            return innerEngine.Load(query, first, max);
        }

        public virtual Domain.Entity LoadWithId(string type, string id)
        {
            return innerEngine.LoadWithId(type, id);
        }

        public virtual IList<Domain.Entity> LoadWithId(string type, string[] id)
        {
            return innerEngine.LoadWithId(type, id);
        }

        public virtual void LoadReference(string reference, params Domain.Entity[] entity)
        {
            innerEngine.LoadReference(reference, entity);
        }

        public virtual void LoadReference(string reference, IEnumerable<Domain.Entity> entities, NLinq.NLinqQuery queryUsedToLoadEntities)
        {
            innerEngine.LoadReference(reference, entities, queryUsedToLoadEntities);
        }

        public virtual double LoadScalar(NLinq.NLinqQuery query)
        {
            return innerEngine.LoadScalar(query);
        }

        public virtual T LoadScalar<T>(NLinq.NLinqQuery query)
        {
            return innerEngine.LoadScalar<T>(query);
        }

        public virtual T LoadScalar<T, U>(NLinq.NLinqQuery query) where T : class, IEnumerable<U>
        {
            return innerEngine.LoadScalar<T, U>(query);
        }

        public virtual T LoadScalar<T, U>(NLinq.NLinqQuery query, int first, int max) where T : class, IEnumerable<U>
        {
            return innerEngine.LoadScalar<T, U>(query, first, max);
        }

        public virtual double LoadScalar(string query)
        {
            return innerEngine.LoadScalar(query);
        }

        public virtual T LoadScalar<T>(string query)
        {
            return innerEngine.LoadScalar<T>(query);
        }

        public virtual T LoadScalar<T, U>(string query, int first, int max) where T : class, IEnumerable<U>
        {
            return innerEngine.LoadScalar<T, U>(query, first, max);
        }

        public virtual void InitializeRepository()
        {
            innerEngine.InitializeRepository();
        }

        public virtual void Initialize()
        {
            innerEngine.Initialize();
        }

        public virtual void BeforeProcessCommands(Transaction tx)
        {
            innerEngine.BeforeProcessCommands(tx);
        }

        public virtual void ProcessCommands(Transaction tx)
        {
            innerEngine.ProcessCommands(tx);
        }

        public virtual void AfterProcessCommands(Transaction tx)
        {
            innerEngine.AfterProcessCommands(tx);
        }

        public virtual IPersistenceProvider Factory
        {
            get { return factory; }
        }

        #endregion
    }
}
