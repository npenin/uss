using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public class ProxyPersistenceEngineAsync : IPersistenceEngineAsync
    {
        protected IPersistenceEngineAsync innerEngine;
        private IPersistenceProviderAsync factory;
        public ProxyPersistenceEngineAsync(IPersistenceEngineAsync innerEngine, IPersistenceProviderAsync factory)
        {
            this.innerEngine = innerEngine;
            this.factory = factory;
        }
        #region IPersistenceEngineAsync Members

        public virtual void BeginLoad(AsyncCallback callback, NLinq.NLinqQuery query, object asyncState)
        {
            innerEngine.BeginLoad(callback, query, asyncState);
        }

        public virtual void BeginLoad(AsyncCallback callback, string query, object asyncState)
        {
            innerEngine.BeginLoad(callback, query, asyncState);
        }

        public virtual void BeginLoad(AsyncCallback callback, string query, int first, int max, object asyncState)
        {
            innerEngine.BeginLoad(callback, query, first, max, asyncState);
        }

        public virtual void BeginLoad(AsyncCallback callback, NLinq.NLinqQuery query, int first, int max, object asyncState)
        {
            innerEngine.BeginLoad(callback, query, first, max, asyncState);
        }

        public virtual IList<Domain.Entity> EndLoad(IAsyncResult result)
        {
            return innerEngine.EndLoad(result);
        }

        public virtual void BeginLoadWithId(AsyncCallback callback, string type, string id, object asyncState)
        {
            innerEngine.BeginLoadWithId(callback, type, id, asyncState);
        }

        public virtual void BeginLoadWithIds(AsyncCallback callback, string type, string[] id, object asyncState)
        {
            innerEngine.BeginLoadWithIds(callback, type, id, asyncState);
        }

        public virtual Domain.Entity EndLoadWithId(IAsyncResult result)
        {
            return innerEngine.EndLoadWithId(result);
        }

        public virtual IList<Domain.Entity> EndLoadWithIds(IAsyncResult result)
        {
            return innerEngine.EndLoadWithIds(result);
        }

        public virtual void BeginLoadReference(AsyncCallback callback, string reference, object asyncState, params Domain.Entity[] entities)
        {
            innerEngine.BeginLoadReference(callback, reference, asyncState, entities);
        }

        public virtual void BeginLoadReference(AsyncCallback callback, string reference, IEnumerable<Domain.Entity> entities, NLinq.NLinqQuery query, object asyncState)
        {
            innerEngine.BeginLoadReference(callback, reference, entities, query, asyncState);
        }

        public virtual void BeginLoadScalar<T>(AsyncCallback callback, NLinq.NLinqQuery query, object asyncState)
        {
            innerEngine.BeginLoadScalar<T>(callback, query, asyncState);
        }

        public virtual void BeginLoadScalar<T, U>(AsyncCallback callback, NLinq.NLinqQuery query, object asyncState) where T : class, IEnumerable<U>
        {
            innerEngine.BeginLoadScalar<T, U>(callback, query, asyncState);
        }

        public virtual void BeginLoadScalar<T, U>(AsyncCallback callback, NLinq.NLinqQuery query, int first, int max, object asyncState) where T : class, IEnumerable<U>
        {
            innerEngine.BeginLoadScalar<T, U>(callback, query, first, max, asyncState);
        }

        public virtual void BeginLoadScalar<T>(AsyncCallback callback, string query, object asyncState)
        {
            innerEngine.BeginLoadScalar<T>(callback, query, asyncState);
        }

        public virtual void BeginLoadScalar<T, U>(AsyncCallback callback, string query, int first, int max, object asyncState) where T : class, IEnumerable<U>
        {
            innerEngine.BeginLoadScalar<T, U>(callback, query, first, max, asyncState);
        }

        public virtual T EndLoadScalar<T>(IAsyncResult result)
        {
            return innerEngine.EndLoadScalar<T>(result);
        }

        public virtual void BeginInitializeRepository(AsyncCallback callback, object asyncState)
        {
            innerEngine.BeginInitializeRepository(callback, asyncState);
        }

        public virtual void EndInitializeRepository(IAsyncResult result)
        {
            innerEngine.EndInitializeRepository(result);
        }

        public virtual void Initialize()
        {
            innerEngine.Initialize();
        }

        public virtual void BeforeProcessCommands(Transaction tx)
        {
            innerEngine.BeforeProcessCommands(tx);
        }

        public virtual void BeginProcessCommands(Transaction tx, AsyncCallback callback, object asyncState)
        {
            innerEngine.BeginProcessCommands(tx, callback, asyncState);
        }

        public virtual void EndProcessCommands(IAsyncResult result)
        {
            innerEngine.EndProcessCommands(result);
        }

        public virtual void AfterProcessCommands(Transaction tx)
        {
            innerEngine.AfterProcessCommands(tx);
        }

        public virtual IPersistenceProviderAsync FactoryAsync
        {
            get { return factory; }
        }

        public virtual System.Globalization.CultureInfo Culture
        {
            get
            {
                return innerEngine.Culture;
            }
            set
            {
                innerEngine.Culture = value;
            }
        }

        public virtual void CreateId(Domain.Entity e)
        {
            innerEngine.CreateId(e);
        }

        #endregion
    }
}
