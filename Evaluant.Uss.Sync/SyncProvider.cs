using Evaluant.Uss.MetaData;
using System.Globalization;
using System;
using Evaluant.Uss.PersistenceEngine.Contracts;
//using Evaluant.Uss.TypeResolver.Contracts;

namespace Evaluant.Uss.Sync
{
    /// <summary>
    /// Description résumée de SyncProvider.
    /// </summary>
    public class SyncProvider : IPersistenceProvider
    {

        public SyncProvider()
        {
        }

        #region Membres de IPersistenceProvider

        public void InitializeConfiguration()
        {
            initialized = true;
        }

        public IPersistenceEngine CreatePersistenceEngine()
        {
            return new SyncEngine(provider.CreatePersistenceEngine(),
                metadataProvider.CreatePersistenceEngine(),
                secondaryMetadataProvider != null ? secondaryMetadataProvider.CreatePersistenceEngine() : null,
                clientId, this);
        }

        public IPersistenceEngineAsync CreatePersistenceEngineAsync()
        {
            return CreatePersistenceEngine();
        }

        #endregion

        private IPersistenceProvider provider;

        public IPersistenceProvider Provider
        {
            get { return provider; }
            set { provider = value; }
        }

        private IPersistenceProvider metadataProvider;

        public IPersistenceProvider MetadataProvider
        {
            get { return metadataProvider; }
            set { metadataProvider = value; }
        }

        private IPersistenceProvider secondaryMetadataProvider;

        public IPersistenceProvider SecondaryMetadataProvider
        {
            get { return secondaryMetadataProvider; }
            set { secondaryMetadataProvider = value; }
        }

        public void RegisterMetaData(IMetaData[] metadata)
        {
            provider.RegisterMetaData(metadata);
        }

        private string clientId = Guid.NewGuid().ToString();

        public string ClientId
        {
            get { return clientId; }
            set { clientId = value; }
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
            get;
            set;
        }

        #endregion

        #region IPersistenceProvider Members

        private bool initialized;

        public void EnsureConfigurationInitialized()
        {
            if (!initialized)
            {
                lock (this)
                {
                    if (!initialized)
                        InitializeConfiguration();
                }
            }
        }

        //public ITypeResolver TypeResolver
        //{
        //    get;
        //    set;
        //}

        #endregion

        #region IPersistenceProvider Members


        public Evaluant.Uss.Serializer.ISerializer Serializer
        {
            get;
            set;
        }

        #endregion
    }
}
