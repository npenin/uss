using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.PersistentDescriptors;
using Evaluant.Uss.Memory;
using Evaluant.Uss.PersistenceEngine.Contracts.Instrumentation;
using Evaluant.Uss.Tests.Model;

namespace Evaluant.Uss.Tests.Engines
{
    [TestClass]
    public class ODataTest : EngineTest
    {
        private ObjectService os;

        protected override IObjectContext CreateContext()
        {
            OData.ODataPersistenceProvider provider = new OData.ODataPersistenceProvider();
            provider.ConnectionString = new Uri("http://localhost:37770/Common.svc/");
            os = new ObjectService(provider);
            os.ObjectContextType = typeof(EntityResolver.Proxy.Dynamic.ObjectContext).AssemblyQualifiedName;
            return os.CreateObjectContext();
        }

        [TestMethod]
        public override void TestBasics()
        {
            var context = GetContext();
            context.BeginTransaction();
            context.Serialize(new Person() { Address = new Address() { City = "Mulhouse" }, FirstName = "Nicolas" });
            context.CommitTransaction();

            OData.ODataPersistenceProvider provider = new OData.ODataPersistenceProvider();
            provider.ConnectionString = new Uri("http://services.odata.org/OData/OData.svc/");
            var pe = provider.CreatePersistenceEngine();

            Assert.AreEqual(1, pe.LoadWithId("ODataDemo.Category", new string[] { "0" }).Count);

            Assert.AreEqual("Beverages", pe.Load("from ODataDemo.Category c in oc where c.ID==1 select c.Name")[0].GetString("Name"));

            Assert.AreEqual(1, pe.LoadScalar<int>("from ODataDemo.Category c in oc where c.ID==1 select c.ID"));
        }
    }
}
