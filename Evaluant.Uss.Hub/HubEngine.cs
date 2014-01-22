using System.Collections;
using Evaluant.Uss.Commands;
using System.Globalization;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Domain;
using System.Collections.Generic;
using Evaluant.NLinq;
using System;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.Hub
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// The ids that are returned are those from the default persistence engine. Thus a TraceEngine can't be used 
    /// as a Default one.
    /// </remarks>
    public class HubEngine : IPersistenceEngine, IPersistenceEngineAsync
    {
        private IList<IPersistenceEngine> _Delegators;
        public IList<IPersistenceEngine> Delegators
        {
            get { return _Delegators; }
            set { _Delegators = value; }
        }

        private int _DefaultEngineIndex = 0;
        /// <summary>
        /// Gets or set the index of the engine used for reading
        /// </summary>
        public int DefaultEngineIndex
        {
            get { return _DefaultEngineIndex; }
            set { _DefaultEngineIndex = value; }
        }

        public HubEngine(IList<IPersistenceEngine> delegators)
        {
            _Delegators = delegators;
        }

        public void InitializeRepository()
        {
            foreach (IPersistenceEngine engine in _Delegators)
                engine.InitializeRepository();
        }

        public void Initialize()
        {
            foreach (IPersistenceEngine engine in _Delegators)
                engine.Initialize();
        }

        public void BeforeProcessCommands(Transaction tx)
        {
            foreach (IPersistenceEngine pe in _Delegators)
                pe.BeforeProcessCommands(tx);
        }

        public void ProcessCommands(Transaction tx)
        {
            foreach (IPersistenceEngine pe in _Delegators)
                pe.ProcessCommands(tx);
        }

        public void AfterProcessCommands(Transaction tx)
        {
            foreach (IPersistenceEngine pe in _Delegators)
                pe.AfterProcessCommands(tx);
        }

        #region IPersistenceEngine Members

        public IList<Entity> Load(Evaluant.NLinq.NLinqQuery query)
        {
            return _Delegators[_DefaultEngineIndex].Load(query);
        }

        public IList<Entity> Load(string query)
        {
            return _Delegators[_DefaultEngineIndex].Load(query);
        }

        public IList<Entity> Load(string query, int first, int max)
        {
            return _Delegators[_DefaultEngineIndex].Load(query, first, max);
        }

        public IList<Entity> Load(Evaluant.NLinq.NLinqQuery query, int first, int max)
        {
            return _Delegators[_DefaultEngineIndex].Load(query, first, max);
        }

        public Entity LoadWithId(string type, string id)
        {
            return _Delegators[_DefaultEngineIndex].LoadWithId(type, id);
        }

        public void LoadReference(string reference, params Entity[] entities)
        {
            _Delegators[_DefaultEngineIndex].LoadReference(reference, entities);
        }

        public double LoadScalar(Evaluant.NLinq.NLinqQuery query)
        {
            return LoadScalar<double>(query);
        }

        public T LoadScalar<T>(Evaluant.NLinq.NLinqQuery query)
        {
            return _Delegators[_DefaultEngineIndex].LoadScalar<T>(query);
        }

        public T LoadScalar<T, U>(Evaluant.NLinq.NLinqQuery query) where T : class, IEnumerable<U>
        {
            return _Delegators[_DefaultEngineIndex].LoadScalar<T, U>(query);
        }

        public IPersistenceProvider Factory { get; set; }

        #endregion

        #region IPersistenceEngine Members


        public IList<Entity> LoadWithId(string type, string[] id)
        {
            return _Delegators[_DefaultEngineIndex].LoadWithId(type, id);
        }

        public void LoadReference(string reference, IEnumerable<Entity> entities, NLinqQuery query)
        {
            _Delegators[_DefaultEngineIndex].LoadReference(reference, entities, query);
        }

        public double LoadScalar(string query)
        {
            return _Delegators[_DefaultEngineIndex].LoadScalar<double>(query);
        }

        public T LoadScalar<T>(string query)
        {
            return _Delegators[_DefaultEngineIndex].LoadScalar<T>(new NLinqQuery(query));
        }

        public T LoadScalar<T, U>(string query, int first, int max) where T : class, IEnumerable<U>
        {
            return _Delegators[_DefaultEngineIndex].LoadScalar<T, U>(new NLinqQuery(query), first, max);

        }

        public T LoadScalar<T, U>(NLinqQuery query, int first, int max) where T : class, IEnumerable<U>
        {
            return _Delegators[_DefaultEngineIndex].LoadScalar<T, U>(query, first, max);

        }

        #endregion

        #region IPersistenceEngine Members


        public void CreateId(Entity e)
        {
            _Delegators[_DefaultEngineIndex].CreateId(e);
        }

        #endregion

        private IPersistenceEngine GetDefaultPersistenceEngine()
        {
            return _Delegators[_DefaultEngineIndex];
        }

        #region IPersistenceEngineAsync Members

        public void BeginLoad(System.AsyncCallback callback, NLinqQuery query, object asyncState)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                peAsync.BeginLoad(callback, query, asyncState);
            else
                new LoadAction<IList<Entity>, Entity>(pe.Load).BeginInvoke(query, callback, asyncState);
        }

        public void BeginLoad(System.AsyncCallback callback, string query, object asyncState)
        {
            BeginLoad(callback, new NLinqQuery(query), asyncState);
        }

        public void BeginLoad(System.AsyncCallback callback, string query, int first, int max, object asyncState)
        {
            BeginLoad(callback, new NLinqQuery(query), first, max, asyncState);
        }

        public void BeginLoad(System.AsyncCallback callback, NLinqQuery query, int first, int max, object asyncState)
        {
            BeginLoad(callback, query.Page(first, max), asyncState);
        }

        public IList<Entity> EndLoad(System.IAsyncResult result)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                return peAsync.EndLoad(result);
            else
                return new LoadAction<IList<Entity>, Entity>(pe.Load).EndInvoke(result);
        }

        public void BeginLoadWithId(System.AsyncCallback callback, string type, string id, object asyncState)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                peAsync.BeginLoadWithId(callback, type, id, asyncState);
            else
                BeginLoadWithIds(callback, type, new string[] { id }, asyncState);

        }

        public void BeginLoadWithIds(System.AsyncCallback callback, string type, string[] id, object asyncState)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                peAsync.BeginLoadWithIds(callback, type, id, asyncState);
            else
                new LoadWithIdAction(pe.LoadWithId).BeginInvoke(type, id, callback, asyncState);
        }

        public Entity EndLoadWithId(System.IAsyncResult result)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                return peAsync.EndLoadWithId(result);
            else
            {
                IList<Entity> entities = new LoadWithIdAction(pe.LoadWithId).EndInvoke(result);
                if (entities != null && entities.Count > 0)
                    return entities[0];
                return null;
            }

        }

        public IList<Entity> EndLoadWithIds(System.IAsyncResult result)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                return peAsync.EndLoadWithIds(result);
            else
                return new LoadWithIdAction(pe.LoadWithId).EndInvoke(result);

        }

        public void BeginLoadReference(System.AsyncCallback callback, string reference, object asyncState, params Entity[] entity)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                peAsync.BeginLoadReference(callback, reference, asyncState, entity);
            else
                BeginLoadReference(callback, reference, entity, PersistenceEngineAsyncImplementation.GetQuery(this.FactoryAsync, entity), asyncState);
        }

        public void BeginLoadReference(System.AsyncCallback callback, string reference, IEnumerable<Entity> entities, NLinqQuery query, object asyncState)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                peAsync.BeginLoadReference(callback, reference, entities, query, asyncState);
            else
            {
                new LoadReferencesAction(pe.LoadReference).BeginInvoke(reference, entities, query, callback, asyncState);
            }
        }

        public void BeginLoadScalar<T>(System.AsyncCallback callback, NLinqQuery query, object asyncState)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                peAsync.BeginLoadScalar<T>(callback, query, asyncState);
            else
            {
                new LoadSingleAction<T>(pe.LoadScalar<T>).BeginInvoke(query, callback, asyncState);
            }
        }

        public void BeginLoadScalar<T, U>(System.AsyncCallback callback, NLinqQuery query, object asyncState) where T : class, IEnumerable<U>
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                peAsync.BeginLoadScalar<T, U>(callback, query, asyncState);
            else
            {
                new LoadAction<T, U>(pe.LoadScalar<T, U>).BeginInvoke(query, callback, asyncState);
            }
        }

        public void BeginLoadScalar<T, U>(System.AsyncCallback callback, NLinqQuery query, int first, int max, object asyncState) where T : class, IEnumerable<U>
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                peAsync.BeginLoadScalar<T, U>(callback, query, first, max, asyncState);
            else
            {
                BeginLoadScalar<T, U>(callback, query.Page(first, max), asyncState);
            }
        }

        public void BeginLoadScalar<T>(System.AsyncCallback callback, string query, object asyncState)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                peAsync.BeginLoadScalar<T>(callback, query, asyncState);
            else
            {
                BeginLoadScalar<T>(callback, query, asyncState);
            }
        }

        public void BeginLoadScalar<T, U>(System.AsyncCallback callback, string query, int first, int max, object asyncState) where T : class, IEnumerable<U>
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                peAsync.BeginLoadScalar<T, U>(callback, query, first, max, asyncState);
            else
            {
                BeginLoadScalar<T, U>(callback, new NLinqQuery(query).Page(first, max), asyncState);
            }
        }

        public T EndLoadScalar<T>(System.IAsyncResult result)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                return peAsync.EndLoadScalar<T>(result);
            else
            {
                return new LoadSingleAction<T>(pe.LoadScalar<T>).EndInvoke(result);
            }
        }

        public void BeginInitializeRepository(System.AsyncCallback callback, object asyncState)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                peAsync.BeginInitializeRepository(callback, asyncState);
            else
            {
                new System.Action(pe.InitializeRepository).BeginInvoke(callback, asyncState);
            }
        }

        public void EndInitializeRepository(System.IAsyncResult result)
        {
            IPersistenceEngine pe = GetDefaultPersistenceEngine();
            IPersistenceEngineAsync peAsync = pe as IPersistenceEngineAsync;
            if (peAsync != null)
                peAsync.EndInitializeRepository(result);
            else
            {
                new System.Action(pe.InitializeRepository).EndInvoke(result);
            }
        }

        #endregion

        #region IPersistenceEngineAsync Members


        public IPersistenceProviderAsync FactoryAsync
        {
            get;
            private set;
        }

        public CultureInfo Culture
        {
            get;
            set;
        }

        #endregion

        #region IPersistenceEngineAsync Members


        public void BeginProcessCommands(Transaction tx, System.AsyncCallback callback, object asyncState)
        {
            foreach (IPersistenceEngineAsync peAsync in _Delegators)
            {
                if (peAsync != null)
                    peAsync.BeginProcessCommands(tx, callback, asyncState);
            }
        }

        public void EndProcessCommands(System.IAsyncResult result)
        {
            foreach (IPersistenceEngineAsync peAsync in _Delegators)
            {
                if (peAsync != null)
                    peAsync.EndProcessCommands(result);
            }
        }

        #endregion
    }
}
