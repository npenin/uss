using System;
using System.Globalization;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public abstract class PersistenceProviderImplementation : PersistenceProviderAsyncImplementation, IPersistenceProvider
    {
        public abstract IPersistenceEngine CreatePersistenceEngine();

        public override IPersistenceEngineAsync CreatePersistenceEngineAsync()
        {
            return (IPersistenceEngineAsync)CreatePersistenceEngine();
        }
    }
}
