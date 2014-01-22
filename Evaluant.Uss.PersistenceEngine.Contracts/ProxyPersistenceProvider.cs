using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public class ProxyPersistenceProvider : ProxyPersistenceProviderAsync, IPersistenceProvider
    {
        private IPersistenceProvider innerProvider;

        public IPersistenceProvider InnerProvider
        {
            get { return innerProvider; }
            set
            {
                innerProvider = value;
                InnerProviderAsync = value;
            }
        }

        public virtual IPersistenceEngine CreatePersistenceEngine()
        {
            return new ProxyPersistenceEngine(InnerProvider.CreatePersistenceEngine(), this);
        }
    }
}
