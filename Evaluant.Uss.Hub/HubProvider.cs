using Evaluant.Uss.MetaData;
using System.Globalization;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Collections.Generic;

namespace Evaluant.Uss.Hub
{
    /// <summary>
    /// Description résumée de HubProvider.
    /// </summary>
    public class HubProvider : IPersistenceProvider
    {
        public HubProvider()
        {
        }

        #region Membres de IPersistenceProvider

        public void InitializeConfiguration()
        {
        }

        public virtual IPersistenceEngine CreatePersistenceEngine()
        {
            IList<IPersistenceEngine> delegators = new List<IPersistenceEngine>(_Delegators.Length);
            foreach (IPersistenceProvider delegator in _Delegators)
                delegators.Add(delegator.CreatePersistenceEngine());

            HubEngine engine = new HubEngine(delegators);
            engine.Factory = this;
            engine.DefaultEngineIndex = _DefaultEngineIndex;

            return engine;
        }

        #endregion

        private IPersistenceProvider[] _Delegators;
        public IPersistenceProvider[] Delegators
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

        public void RegisterMetaData(IMetaData[] metadata)
        {
            foreach (IPersistenceProvider delegator in _Delegators)
                delegator.RegisterMetaData(metadata);
        }

        // Nothing to do as the culture must be specified in the last engine
        public CultureInfo Culture
        {
            get { return null; }
            set { }
        }

        #region IPersistenceProvider Members


        public Evaluant.Uss.Model.Model Model
        {
            get
            {
                return _Delegators[_DefaultEngineIndex].Model;
            }
        }

        //public TypeResolver.Contracts.ITypeResolver TypeResolver
        //{
        //    get { return _Delegators[_DefaultEngineIndex].TypeResolver; }
        //}

        public void EnsureConfigurationInitialized()
        {
            InitializeConfiguration();
        }

        #endregion

        #region IPersistenceProvider Members


        public Evaluant.Uss.Serializer.ISerializer Serializer
        {
            get { return _Delegators[_DefaultEngineIndex].Serializer; }
        }

        #endregion

        #region IPersistenceProviderAsync Members

        public IPersistenceEngineAsync CreatePersistenceEngineAsync()
        {
            return CreatePersistenceEngine();
        }

        #endregion
    }
}
