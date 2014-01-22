using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public class ProxyPersistenceProviderAsync : IPersistenceProviderAsync
    {
        public IPersistenceProviderAsync InnerProviderAsync { get; set; }

        #region IPersistenceProvider Members

        public virtual void InitializeConfiguration()
        {
            InnerProviderAsync.InitializeConfiguration();
        }

        public virtual void EnsureConfigurationInitialized()
        {
            InnerProviderAsync.EnsureConfigurationInitialized();
        }

        public virtual IPersistenceEngineAsync CreatePersistenceEngineAsync()
        {
            return new ProxyPersistenceEngineAsync(InnerProviderAsync.CreatePersistenceEngineAsync(), this);
        }

        public void RegisterMetaData(MetaData.IMetaData[] metadata)
        {
            InnerProviderAsync.RegisterMetaData(metadata);
        }

        public Model.Model Model
        {
            get { return InnerProviderAsync.Model; }
        }

        public System.Globalization.CultureInfo Culture
        {
            get { return InnerProviderAsync.Culture; }
            set { InnerProviderAsync.Culture = value; }
        }

        public Serializer.ISerializer Serializer
        {
            get { return InnerProviderAsync.Serializer; }
        }

        #endregion
    }
}
