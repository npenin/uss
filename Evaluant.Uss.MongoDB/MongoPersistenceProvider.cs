using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.MongoDB.Connections;

namespace Evaluant.Uss.MongoDB
{
    public class MongoPersistenceProvider : PersistenceEngine.Contracts.PersistenceProviderImplementation
    {
        public string ConnectionString { get; set; }

        public override void InitializeConfiguration()
        {

        }

        public override Evaluant.Uss.PersistenceEngine.Contracts.IPersistenceEngine CreatePersistenceEngine()
        {
            return new MongoPersistenceEngine(this,ConnectionFactory.GetConnection(ConnectionString));
        }
    }
}
