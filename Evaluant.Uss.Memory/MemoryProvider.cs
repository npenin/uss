using System;
using System.Collections;
using System.Threading;
using Evaluant.Uss.PersistenceEngine.Contracts;

//using Evaluant.Uss.Common;
using Evaluant.Uss.MetaData;
using Evaluant.Uss.Domain;
using Evaluant.Uss.Utility;
using System.Collections.Generic;

namespace Evaluant.Uss.Memory
{
    /// <summary>
    /// Description résumée de MemoryProvider.
    /// </summary>
    public class MemoryProvider : PersistenceProviderImplementation
    {
        public MemoryProvider()
            : base()
        {
        }

        public MemoryProvider(IPersistenceProviderAsync delegatorAsync)
        {
            _DelegatorAsync = delegatorAsync;
        }

        public MemoryProvider(IPersistenceProvider delegator)
            : base()
        {
            _Delegator = delegator;
        }

        public override void InitializeConfiguration()
        {
            _Entities = new Dictionary<string, Entity>(10000);
            _RWL = new ReaderWriterLock();
            InitializeData();
        }

        public override IPersistenceEngine CreatePersistenceEngine()
        {
            if (_Delegator != null)
                return new MemoryEngine(_Delegator.CreatePersistenceEngine(), _Entities, _RWL, _Culture, this);
            else
                return new MemoryEngine(null, _Entities, _RWL, _Culture, this);
        }

        public override void RegisterMetaData(IMetaData[] metadata)
        {
            base.RegisterMetaData(metadata);

            if (_Delegator != null)
                _Delegator.RegisterMetaData(metadata);
        }

        private IPersistenceProvider _Delegator;
        public IPersistenceProvider Delegator
        {
            get { return _Delegator; }
            set { _Delegator = value; }
        }

        private IPersistenceProviderAsync _DelegatorAsync;
        public IPersistenceProviderAsync DelegatorAsync
        {
            get { return _DelegatorAsync; }
            set { _DelegatorAsync = value; }
        }

        protected IDictionary<string, Entity> _Entities;
        protected ReaderWriterLock _RWL;

        protected void InitializeData()
        {
            if (_Delegator == null)
                return;

            // Instantiates the Delegated persistence engine to load all its content
            IPersistenceEngine delegator = _Delegator.CreatePersistenceEngine();

            foreach (Model.Entity me in Model.Entities.Values)
            {
                // Process only super types
                if (me.Inherit == null || me.Inherit == String.Empty)
                {
                    foreach (Entity e in delegator.Load(me.Type))
                        _Entities.Add(MemoryEngine.GetCacheKey(e.Type, e.Id), e);
                }
            }

            // Loads all references of each entity, and try to find the corresponding entity locally
            foreach (Entity entity in _Entities.Values)
            {
                Entity entityClone = entity.Clone();
                foreach (string reference in Model[entity.Type].References.Keys)
                    delegator.LoadReference(reference, entityClone);

                foreach (Entry ee in entityClone)
                {
                    if (ee.IsEntity)
                    {
                        // Find this reference locally
                        Entity localRef = (Entity)_Entities[MemoryEngine.GetCacheKey((Entity)ee.Value)];

                        // Creates the reference
                        if (localRef == null)
                            throw new PersistenceEngineException("Integrity exception");

                        entity.Add(ee.Name, localRef, typeof(Entity), ee.State);
                    }
                }
            }
        }
    }
}
