using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using Evaluant.Uss.EntityResolver.Proxy;
using Evaluant.Uss.Memory;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.ObjectContext.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Evaluant.Uss.Tests.ObjectContext._Impl
{
    [TestClass]
    public class PersistenceEngineObjectContext : ObjectContextTest
    {
        static PersistenceEngineObjectContext()
        {
            IPersistenceProvider provider = new MemoryProvider();
            provider.RegisterMetaData(MetaData.LightMetaDataFactory.FromAssembly(typeof(ObjectContextTest).Assembly));
            provider.InitializeConfiguration();
            os = new ObjectService(provider);
            os.ObjectContextType = typeof(EntityResolver.Proxy.Dynamic.ObjectContext).AssemblyQualifiedName;
        }
    }
}
