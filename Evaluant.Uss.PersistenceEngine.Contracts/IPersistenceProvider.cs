using System;
using System.Globalization;
using Evaluant.Uss.MetaData;
//using Evaluant.Uss.TypeResolver.Contracts;
using Evaluant.Uss.Serializer;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    /// <summary>
    /// Description résumée de IPersistenceProvider.
    /// </summary>
    public interface IPersistenceProvider : IPersistenceProviderAsync
    {
        /// <summary>
        /// Creates the persistence engine using the properties which are loaded.
        /// </summary>
        /// <returns></returns>
        IPersistenceEngine CreatePersistenceEngine();
    }
}
