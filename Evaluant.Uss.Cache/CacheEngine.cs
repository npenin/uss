using System;
using Evaluant.Uss.Commands;
//using Evaluant.Uss.Common;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Domain;
using Evaluant.NLinq;
using System.Collections.Generic;

namespace Evaluant.Uss.Cache
{
    /// <summary>
    /// IPersistenceEngine which acts has a cache for another IPersistenceEngine. A Delegator must be specified.
    /// All the object are "weakly" kept into memory until the garbage collector is to collect them. Objects
    /// queried by Id are directly taken from memory if they can be found. On the contrary the OPath request is 
    /// processed in the Delegator and the object are collected into the cache.
    /// 
    /// In the main list, entities are kept with all the attributes and all the loaded references. The loaded references 
    /// contain only the entries, without the attributes nor the references. Its just a cache of pointed items. Those items
    /// can then be loaded from the main list no to have to load them from the delegator.
    /// 
    /// Caches indexes can't be contained statically by the Cache class as several ObjectContext from different 
    /// ObjectService (ie different repositories) would then use the same references. The solution is to let those references
    /// in the ObjectService instance which is unique by repository.
    /// </summary>
    public class CacheEngine : PersistenceEngineImplementation
    {
        // private static PerformanceCounter _Counter;

        private IdentityMap _Entities;
        private Dictionary<string, object> _Scalars;
        private Hashtable _Loads;
        private ReaderWriterLock _RWL;

        static CacheEngine()
        {
            // _Counter = new PerformanceCounter(CacheProvider.CATEGORY, CacheProvider.NAME, "Value", false);
        }

        public CacheEngine(IPersistenceEngine delegator, CacheProvider provider)
            : base(provider)
        {
            _CommandProcessor = new CacheCommandProcessor(provider._Entities, provider._RWL);
            _Delegator = delegator;
            _Entities = provider._Entities;
            _Scalars = provider._Scalars;
            _Loads = provider._Loads;
            _RWL = provider._RWL;
        }

        ICommandProcessor _CommandProcessor;
        public ICommandProcessor CommandProcessor
        {
            get { return _CommandProcessor; }
            set { _CommandProcessor = value; }
        }

        private IPersistenceEngine _Delegator;
        public IPersistenceEngine Delegator
        {
            get { return _Delegator; }
            set { _Delegator = value; }
        }

        public void EnsureDelegator()
        {
            if (_Delegator == null)
                throw new Exception("A delegated Persistence Engine must be specified for the Cache Persistence Engine");
        }

        public override void LoadReference(string reference, IEnumerable<Entity> entities, NLinqQuery query)
        {
            _RWL.AcquireReaderLock(Timeout.Infinite);

            try
            {
                foreach (Entity entity in entities)
                {
                    State tempState = entity.State;

                    entity.RemoveReference(reference);

                    Entity local = _Entities.GetValue(GetCacheKey(entity));

                    if (local != null)
                    {
                        ArrayList unknownIds = new ArrayList();

                        // _Counter.Increment();

                        /// TODO: Create only one call to the Delegator by specifying the list of the references to load
                        // Tries to get the references from the cache
                        if (local.GetValue(reference) != null)
                        {
                            unknownIds.Clear();

                            Entry ee = local[reference];
                            if (ee.IsMultiple)
                                // Adds all the entities which are still in the cache
                                foreach (var e in ((MultipleEntry)local[reference]).TypedValue)
                                {
                                    Entity eLocal = _Entities.GetValue(GetCacheKey(e.TypedValue));
                                    if (eLocal != null)
                                        entity.Add(reference, eLocal.Clone(new string[0]), true, State.UpToDate);
                                    else
                                        unknownIds.Add(local.Id);
                                }

                            // Requests all the unknown ids to the delegator
                            if (unknownIds.Count > 0)
                            {
                                string[] ids = (string[])unknownIds.ToArray(typeof(string));
                                IEnumerable<Entity> unknowns = _Delegator.LoadWithId(Factory.Model.GetReference(local.Type, reference, true).ChildType, ids);
                                foreach (Entity unknown in unknowns)
                                    entity.Add(reference, unknown.Clone(new string[0]), ee.IsMultiple, State.UpToDate);

                                UpdateMap(unknowns);
                            }

                        }
                        else
                            _Delegator.LoadReference(reference, entity);

                        entity.State = tempState;
                    }
                    else
                    {
                        /// TODO: If all the entities don't have the reference, use a call to the collection of entities
                        _Delegator.LoadReference(reference, entity);
                        UpdateMap(new Entity[] { entity });
                    }

                    // mark the reference as loaded for each parentEntity
                        if (!entity.InferredReferences.Contains(reference))
                            entity.InferredReferences.Add(reference);
                }
            }
            finally
            {
                _RWL.ReleaseReaderLock();
            }
        }

        //public override EntitySet LoadWithId(string type, string[] ids, string[] attributes)
        //{
        //    _RWL.AcquireReaderLock(Timeout.Infinite);

        //    try
        //    {
        //        EntitySet result = new EntitySet();

        //        foreach (string id in ids)
        //        {
        //            Entity local = _Entities.GetValue(Utils.GetCacheKey(type, id));
        //            if (local != null)
        //            {
        //                // _Counter.Increment();

        //                Entity entity = local.Clone(attributes);

        //                entity.MetaData = Entity.LoadMetaData.AttributesLoaded;
        //                entity.State = State.UpToDate;
        //                result.Add(entity);
        //            }
        //            else
        //            {
        //                /// TODO: Optimize so that only the entities that are not in cache are loaded from the delegator

        //                EntitySet es = _Delegator.LoadWithId(type, id);
        //                UpdateMap(es.ToArray());

        //                // Removes unwanted attributes
        //                if (attributes == null || attributes.Length > 0)
        //                    foreach (Entity e in es)
        //                    {
        //                        for (int i = e.EntityEntries.Count - 1; i >= 0; i--)
        //                        {
        //                            Entry ee = e.EntityEntries[i] as Entry;
        //                            if (!ee.IsEntity && (attributes == null || Array.IndexOf(attributes, ee.Name) == -1))
        //                                e.Remove(ee.Name);
        //                        }
        //                        e.State = State.UpToDate;
        //                    }

        //                foreach (Entity e in es)
        //                    result.Add(e);
        //            }
        //        }

        //        return result;
        //    }
        //    finally
        //    {
        //        _RWL.ReleaseReaderLock();
        //    }
        //}

        public override T LoadScalar<T>(NLinqQuery nlinq)
        {
            lock (_Scalars)
            {
                object oscalar;
                if (_Scalars.TryGetValue(nlinq.Expression.ToString(), out oscalar))
                    return (T)oscalar;

                T scalar = _Delegator.LoadScalar<T>(nlinq);
                _Scalars.Add(nlinq.Expression.ToString(), scalar);

                return scalar;
            }
        }

        public override void BeforeProcessCommands(Transaction tx)
        {
            _RWL.AcquireWriterLock(Timeout.Infinite);

            try
            {
                _Delegator.BeforeProcessCommands(tx);
            }
            catch
            {
                // Releases the lock if an error occured
                _RWL.ReleaseWriterLock();
                throw;
            }
        }

        public override void ProcessCommands(Transaction tx)
        {
            try
            {
                _Delegator.ProcessCommands(tx);

                foreach (Command command in tx.PendingCommands)
                    _CommandProcessor.Visit(command);

                // Invalidates the Scalar cache
                _Scalars.Clear();
            }
            catch
            {
                // Releases the lock if an error occured
                _RWL.ReleaseWriterLock();
                throw;
            }
        }

        public override void AfterProcessCommands(Transaction tx)
        {
            try
            {
                _Delegator.AfterProcessCommands(tx);
            }
            finally
            {
                // Releases the lock if an error occured
                _RWL.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// Adds the clone of an entityset to the cache entries
        /// </summary>
        /// <param name="entityset">The entityset to clone</param>
        private void UpdateMap(IEnumerable<Entity> entities)
        {
            lock (_Entities)
            {
                RecursiveUpdateMap(entities);
            }
        }

        private void RecursiveUpdateMap(IEnumerable<Entity> entities)
        {
            foreach (Entity e in entities)
            {
                Entity local = _Entities.GetValue(GetCacheKey(e));

                // If the entity was found there is nothing to do
                if (local == null)
                {
                    Entity clone = e.Clone(new string[0]);

                    _Entities.Add(GetCacheKey(clone), clone);

                    foreach (Entry ee in e)
                    {
                        if (ee.IsEntity)
                        {
                            Entity sub = (Entity)ee.Value;
                            // Adds the entries to references
                            clone.Add(ee.Name, sub.Clone(), typeof(Entity), State.UpToDate);
                            RecursiveUpdateMap(new Entity[] { sub });
                        }
                    }

                    clone.State = State.UpToDate;
                }
            }
        }

        public override void InitializeRepository()
        {
            _RWL.AcquireWriterLock(Timeout.Infinite);
            try
            {
                EnsureDelegator();
                _Delegator.InitializeRepository();
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }

            Initialize();
        }

        public override void Initialize()
        {
            _RWL.AcquireWriterLock(Timeout.Infinite);
            try
            {
                EnsureDelegator();
                _Delegator.Initialize();
                _Entities.Clear();
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }

        }

        internal static string GetCacheKey(string type, string id)
        {
            return type + "." + id;
        }

        protected override T LoadMany<T, U>(NLinqQuery query)
        {
            _RWL.AcquireReaderLock(Timeout.Infinite);

            IList<Entity> entityset = _Delegator.Load(query);
            UpdateMap(entityset);

            try
            {
                return (T)entityset;
            }
            finally
            {
                _RWL.ReleaseReaderLock();
            }
        }

        internal static string GetCacheKey(Entity entity)
        {
            return GetCacheKey(entity.Type, entity.Id);
        }

        protected override T LoadSingle<T>(NLinqQuery query)
        {
            throw new NotImplementedException();
        }

        public override IList<Entity> LoadWithId(string type, string[] id)
        {
            throw new NotImplementedException();
        }

        public override void CreateId(Entity e)
        {
            e.Id = Guid.NewGuid().ToString();
        }
    }
}
