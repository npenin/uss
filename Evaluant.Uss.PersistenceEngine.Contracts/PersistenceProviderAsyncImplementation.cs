using System;
using System.Globalization;
using Evaluant.Uss.Serializer;
using Evaluant.Uss.Model;
using Evaluant.Uss.MetaData;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public abstract class PersistenceProviderAsyncImplementation : IPersistenceProviderAsync
    {
        protected CultureInfo _Culture;

        public PersistenceProviderAsyncImplementation()
        {
            Model = new Model.Model();
            //TypeResolver = new TypeResolver.TypeResolverImpl();
        }

        public abstract void InitializeConfiguration();

        public abstract IPersistenceEngineAsync CreatePersistenceEngineAsync();

        public virtual void RegisterMetaData(params Evaluant.Uss.MetaData.IMetaData[] metadata)
        {
            ModelMetaDataVisitor visitor = new ModelMetaDataVisitor(Model);
            foreach(IMetaData md in metadata)
                md.Accept(visitor);
        }

        public Model.Model Model { get; protected set; }

        //public ITypeResolver TypeResolver { get; set; }
        public ISerializer Serializer { get; set; }

        public CultureInfo Culture
        {
            get { return _Culture; }
            set { _Culture = value; }
        }


        #region IPersistenceProvider Members

        protected bool initialized;

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

        #endregion
    }
}
