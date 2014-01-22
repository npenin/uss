using System;
using System.Collections;
using System.Globalization;
using System.Threading;

using Evaluant.Uss.Commands;
using Evaluant.Uss.Domain;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Collections.Generic;
using Evaluant.NLinq;
using Evaluant.NLinq.Memory;
using Evaluant.Uss.PersistenceEngine.Contracts.CommonVisitors;
using Evaluant.Uss.Utility;
using System.ComponentModel;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.Memory
{
    /// <summary>
    /// Loads everything into memory for better performances. If no _Delegator is provided, nothing is persisted.
    /// </summary>
    /// <remarks>
    /// This engine is thread safe as all the entities are shared among several engines using the same factory.
    /// The factory owns the entities.
    /// </remarks>
    public class MemoryEngine : PersistenceEngineImplementation
    {
        protected IDictionary<string, Entity> _Entities;
        protected ReaderWriterLock _RWL;

        public MemoryEngine(IPersistenceEngine delegator, IDictionary<string, Entity> entities, ReaderWriterLock rwl, CultureInfo culture, IPersistenceProvider provider)
            : base(provider)
        {
            _Delegator = delegator;
            _Entities = entities;
            _RWL = rwl;
            //_Model = model;
            Culture = culture;
            _CommandProcessor = new MemoryCommandProcessor(_Entities, _RWL);
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

        public override void InitializeRepository()
        {
            _RWL.AcquireWriterLock();
            try
            {
                if (_Delegator != null)
                    _Delegator.InitializeRepository();

                _Entities.Clear();
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }
        }

        public override void Initialize()
        {
            if (_Delegator != null)
                _Delegator.Initialize();
        }

        internal static string GetCacheKey(string type, string id)
        {
            return type + "." + id;
        }
        internal static string GetCacheKey(Entity entity)
        {
            return GetCacheKey(entity.Type, entity.Id);
        }

        //public override object LoadScalar(string opath)
        //{
        //    _RWL.AcquireReaderLock(Timeout.Infinite);

        //    try
        //    {
        //        opath = string.Format("eval({0})", opath);
        //        OPathQuery query = new OPathQuery(opath, OPathQueryTypeEnum.Expression);
        //        query.Compile();

        //        if (query.HasErrors)
        //            throw new OPathException(query);

        //        EntitySet roots = new EntitySet();
        //        foreach (Entity entity in _Entities.Values)
        //            roots.Add(entity);

        //        NavigationVisitor eval = new NavigationVisitor(roots, Model, true);
        //        eval.Visit(query.Expression);

        //        object res = eval._NodeValue;

        //        // Ensures the result in an Int32 in case of a count()
        //        if (opath.Trim().StartsWith("eval(count"))
        //            res = Convert.ToInt32(res);

        //        return res;
        //    }
        //    finally
        //    {
        //        _RWL.ReleaseReaderLock();
        //    }
        //}

        public override void BeforeProcessCommands(Transaction tx)
        {
            _RWL.AcquireWriterLock();

            try
            {
                base.BeforeProcessCommands(tx);

                if (_Delegator != null)
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
                foreach (Command command in tx.PendingCommands)
                    _CommandProcessor.Visit(command);

                if (_Delegator != null)
                    _Delegator.ProcessCommands(tx);
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
                base.AfterProcessCommands(tx);

                if (_Delegator != null)
                    _Delegator.AfterProcessCommands(tx);
            }
            finally
            {
                // Releases the lock if an error occured
                _RWL.ReleaseWriterLock();
            }
        }

        public override IList<Entity> LoadWithId(string type, string[] ids)
        {
            _RWL.AcquireReaderLock();
            try
            {
                EntitySet result = new EntitySet();

                foreach (string id in ids)
                {
                    foreach (string subtype in Factory.Model.GetTreeAsArray(type))
                    {
                        bool found = false;
                        if (_Entities.ContainsKey(GetCacheKey(subtype, id)))
                        {
                            Entity local = _Entities[GetCacheKey(subtype, id)] as Entity;
                            Entity entity = local.Clone(new string[0]);

                            entity.MetaData = Entity.LoadMetaData.AttributesLoaded;
                            entity.State = State.UpToDate;
                            result.Add(entity);

                            found = true;
                        }

                        if (found)
                            break;
                    }
                }

                return result;
            }
            finally
            {
                _RWL.ReleaseReaderLock();
            }
        }

        public override void LoadReference(string reference, IEnumerable<Entity> entities, NLinqQuery query)
        {
            _RWL.AcquireReaderLock();
            try
            {
                foreach (Entity entity in entities)
                {
                    State tempState = entity.State;

                    entity.RemoveReference(reference);

                    Entity local = _Entities[GetCacheKey(entity)] as Entity;

                    foreach (Entry ee in local)
                    {
                        if (ee.IsEntity)
                        {
                            if (ee.Name == reference)
                            {
                                if (ee.IsMultiple)
                                {
                                    MultipleEntry me = new MultipleEntry(ee.Name);
                                    entity.Add(me);
                                    foreach (Entry eee in ((MultipleEntry)ee).TypedValue)
                                    {
                                        Entity refEntity = ((Entity)eee.Value).Clone(new string[0]);
                                        me.Add(Entry.Create<Entity>(me.Name, State.UpToDate, refEntity));

                                        refEntity.State = State.UpToDate;
                                    }
                                }
                                else
                                {
                                    Entity refEntity = ((Entity)ee.Value).Clone(new string[0]);
                                    entity.Add(ee.Name, refEntity, typeof(Entity), State.UpToDate);

                                    refEntity.State = State.UpToDate;
                                }
                            }
                        }
                    }

                    entity.State = tempState;

                    // mark references as loaded for each entity
                    if (!entity.InferredReferences.Contains(reference))
                        entity.InferredReferences.Add(reference);
                }
            }
            finally
            {
                _RWL.ReleaseReaderLock();
            }
        }

        protected override T LoadSingle<T>(NLinqQuery query)
        {
            _RWL.AcquireReaderLock();

            try
            {
                query = new NLinqQuery(new FromInMutator().Visit(query.Expression), query.Parameters);
                LinqToMemory linq = new LinqToMemory(query, new EntityPropertyGetter(this));
                TypeFilterVisitor typeFilter = new TypeFilterVisitor(Factory.Model, _Entities.Values, "#context");
                typeFilter.Visit(query.Expression);
                linq.SetParameter("#context", typeFilter.Entities);

                object result = linq.Evaluate();
                if (result is Entity)
                    return (T)Convert.ChangeType((result as Entity).Clone(), typeof(T), Culture);
                if (result is Entry<Entity>)
                    return (T)Convert.ChangeType(((Entry<Entity>)result).TypedValue.Clone(), typeof(T), Culture);

                return (T)result;
            }
            finally
            {
                _RWL.ReleaseReaderLock();
            }
        }

        protected override T LoadMany<T, U>(NLinqQuery query)
        {
            _RWL.AcquireReaderLock();
            try
            {
                query = new NLinqQuery(new FromInMutator().Visit(query.Expression), query.Parameters);
                if (typeof(U) == typeof(Entity))
                {
                    return ReadEntities(query) as T;
                }
                LinqToMemory linq = new LinqToMemory(query, new EntityPropertyGetter(this));
                linq.SetParameter("#context", _Entities);

                return linq.Evaluate<T>();
            }
            catch
            {
                throw;
            }
            finally
            {
                _RWL.ReleaseReaderLock();
            }
        }

        private IList<Entity> ReadEntities(NLinqQuery query)
        {
            EntitySet entities = new EntitySet();
            LinqToMemory linq = new LinqToMemory(query, new EntityPropertyGetter(this));

            // extract only the entities of the From type to set a source (context)
            TypeFilterVisitor typeFilter = new TypeFilterVisitor(Factory.Model, _Entities.Values, "#context");
            typeFilter.Visit(query.Expression);
            linq.SetParameter("#context", typeFilter.Entities);
            foreach (object item in linq.Enumerate())
            {
                Entity entity = item as Entity;
                if (entity == null)
                {
                    Variant var = item as Variant;
                    if (var != null)
                    {
                        entity = new Entity();
                        foreach (string property in var.PropertyNames)
                        {
                            entity.Add(property, var[property]);
                        }
                        entities.Add(entity);
                        continue;
                    }
                    if (((Entry)item).IsMultiple)
                    {
                        foreach (Entry<Entity> it in ((MultipleEntry)item))
                        {
                            var en = it.TypedValue.Clone();
                            en.State = State.UpToDate;
                            entities.Add(en);
                        }
                        continue;
                    }
                    else
                        entity = ((Entry<Entity>)item).TypedValue;
                }
                Entity e = entity.Clone();
                e.State = State.UpToDate;
                entities.Add(e);
            }

            return entities;
        }

        public override void CreateId(Entity e)
        {
            _RWL.AcquireReaderLock();

            try
            {
                if (_Entities.Contains(new KeyValuePair<string, Entity>(GetCacheKey(e), e)))
                {
                    return;
                }
                //else
                //{
                //    _Entities.Add(MemoryEngine.GetCacheKey(e), e);
                //}
            }
            finally
            {
                _RWL.ReleaseReaderLock();
            }

        }
    }
}
