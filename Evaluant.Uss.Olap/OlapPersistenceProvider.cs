using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;

namespace Evaluant.Uss.Olap
{
    public class OlapPersistenceProvider : PersistenceProviderImplementation
    {
        public override void InitializeConfiguration()
        {
        }

        public override IPersistenceEngine CreatePersistenceEngine()
        {
            return new OlapPersistenceEngine(this);
        }
    }
}
