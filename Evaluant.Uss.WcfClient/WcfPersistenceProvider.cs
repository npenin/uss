using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;

namespace Evaluant.Uss.WcfClient
{
    public class WcfPersistenceProvider : PersistenceProviderImplementation
    {
        public override void InitializeConfiguration()
        {
        }

        public override PersistenceEngine.Contracts.IPersistenceEngine CreatePersistenceEngine()
        {
            return new WcfPersistenceEngine(this);
        }
    }
}
