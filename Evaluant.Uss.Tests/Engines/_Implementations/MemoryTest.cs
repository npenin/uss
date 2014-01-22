using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Evaluant.Uss.Linq;
using Evaluant.Uss.Tests.Model;
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

        protected override Evaluant.Uss.ObjectContext.Contracts.IObjectContext CreateContext()
        {
            if (os == null)
            {
                IPersistenceProvider provider = new MemoryProvider();
                provider.RegisterMetaData(MetaData.LightMetaDataFactory.FromAssembly(GetType().Assembly, "Evaluant.Uss.Tests.Model"));
                provider.InitializeConfiguration();
                os = new ObjectService(provider);
                os.ObjectContextType = typeof(EntityResolver.Proxy.Dynamic.ObjectContext).AssemblyQualifiedName;
            }
            return (IPersistenceEngineObjectContext)os.CreateObjectContext();
        }

        [TestMethod]
        public override void TestPaging()
        {
            IObjectContext c = GetContext();
            if (c == null)
                return;
            c.InitializeRepository();
            c.BeginTransaction();
            c.Serialize(new Employee() { FirstName = "Bob", Friends = { new Employee() { FirstName = "John", Address = new Address() { City = "Strasbourg" } } }, Address = new Address() { City = "Mulhouse" } });
            c.CommitTransaction();

            c = CreateContext();

            Assert.AreEqual(1, c.Cast<Employee>().OrderBy(e => e.Id).Skip(1).Take(1).Count());
            Assert.AreEqual("John", c.Cast<Employee>().OrderBy(e => e.FirstName).Skip(1).Take(1).FirstOrDefault().FirstName);
            Assert.AreEqual(1, c.Cast<Employee>().OrderBy(e => e.FirstName).Take(1).Count());
            Assert.AreEqual("Bob", c.Cast<Employee>().OrderBy(e => e.FirstName).Take(1).FirstOrDefault().FirstName);
            Assert.AreEqual(0, c.Cast<Employee>().OrderBy(e => e.FirstName).Skip(2).Count());

        }
    }
}
