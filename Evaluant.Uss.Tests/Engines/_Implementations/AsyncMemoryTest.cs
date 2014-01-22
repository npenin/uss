using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.PersistentDescriptors;
using Evaluant.Uss.Memory;
#if !SILVERLIGHT
using Evaluant.Uss.PersistenceEngine.Contracts.Instrumentation;
#endif

namespace Evaluant.Uss.Tests.Engines
{
    [TestClass]
    public class MemoryTest : EngineTest
    {
        static ObjectService os;

        protected override Evaluant.Uss.ObjectContext.Contracts.IObjectContextAsync CreateContext()
        {
            if (os == null)
            {
                IPersistenceProvider provider = new MemoryProvider();
                provider.RegisterMetaData(MetaData.LightMetaDataFactory.FromAssembly(GetType().Assembly, new ReflectionDescriptor(), "Evaluant.Uss.Tests.Model"));
                provider.InitializeConfiguration();
                os = new ObjectService(provider);
                os.ObjectContextType = typeof(EntityResolver.Proxy.Dynamic.ObjectContextAsync).AssemblyQualifiedName;
                os.AddAssembly(GetType().Assembly);
            }
            return os.CreateAsyncObjectContext();
        }
    }
}
