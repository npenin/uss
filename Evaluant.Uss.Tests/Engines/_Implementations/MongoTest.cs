using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.MongoDB;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.PersistentDescriptors;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.PersistenceEngine.Contracts.Instrumentation;

namespace Evaluant.Uss.Tests.Engines
{
    /// <summary>
    /// Summary description for MongoTest
    /// </summary>
    [TestClass]
    [Ignore]
    public class MongoTest : EngineTest
    {
        static ObjectService os;

        protected override Evaluant.Uss.ObjectContext.Contracts.IObjectContext CreateContext()
        {
            if (os == null)
            {
                IPersistenceProvider provider = new MongoPersistenceProvider();
                provider.RegisterMetaData(MetaData.MetaDataFactory.FromAssembly(GetType().Assembly, "Evaluant.Uss.Tests.Model"));
                ((MongoPersistenceProvider)provider).ConnectionString = "Server=127.0.0.1:27017;Database=euss";
                
                provider.InitializeConfiguration();
                os = new ObjectService(provider);
                os.ObjectContextType = typeof(EntityResolver.Proxy.Dynamic.ObjectContext).AssemblyQualifiedName;
            }
            return (IPersistenceEngineObjectContext)os.CreateObjectContext();
        }
    }
}
