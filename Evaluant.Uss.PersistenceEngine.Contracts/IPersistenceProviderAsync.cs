using System;
using Evaluant.Uss.MetaData;
using System.Globalization;
#if SILVERLIGHT
using Evaluant.Uss.Serializer;
#endif

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public interface IPersistenceProviderAsync
    {
        /// <summary>
        /// Initializes the configuration once all properties are loaded.
        /// </summary>
        void InitializeConfiguration();

        void EnsureConfigurationInitialized();

        /// <summary>
        /// Creates the persistence engine using the properties which are loaded.
        /// </summary>
        /// <returns></returns>
        IPersistenceEngineAsync CreatePersistenceEngineAsync();

        /// <summary>
        /// Registers the meta data.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        void RegisterMetaData(params IMetaData[] metadata);

        Model.Model Model { get; }

        CultureInfo Culture { get; set; }

        //ITypeResolver TypeResolver { get; }
        Serializer.ISerializer Serializer { get; }
    }
}
