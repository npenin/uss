using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.PersistentDescriptors;
using Evaluant.Uss.Tests.Model;
using Evaluant.Uss.Domain;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.Linq;
using System.Linq;

namespace Evaluant.Uss.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class Basics
    {
        public Basics()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void ShouldCRUD()
        {
            IPersistenceProvider provider = new SqlMapper.SqlMapperProvider();
            ((SqlMapper.SqlMapperProvider)provider).ConnectionString = "Data Source=.;Initial Catalog=uss2;Integrated Security=true";
            ((SqlMapper.SqlMapperProvider)provider).UseDefaultMapping = true;
            provider.RegisterMetaData(MetaData.MetaDataFactory.FromAssembly(GetType().Assembly, "Evaluant.Uss.Tests.Model"));
            provider.InitializeConfiguration();
            IPersistenceEngine engine = provider.CreatePersistenceEngine();
            engine.InitializeRepository();
            Transaction t = new Transaction(provider.Model);
            Entity a = new Entity(typeof(Address).FullName);
            //a.Model = provider.Model.Entities[typeof(Evaluant.Uss.Tests.Model.Address).FullName];
            a.Add("City", "Mulhouse");
            t.Serialize(a);
            t.Commit(engine);
            IList<Domain.Entity> entities = engine.Load("from Evaluant.Uss.Tests.Model.Address a in context select a");
            if (entities.Count > 0)
            {
                Assert.AreEqual("Evaluant.Uss.Tests.Model.Address", entities[0].Type);
                Assert.AreEqual("Mulhouse", entities[0].GetString("City"));
            }
        }

        [TestMethod]
        public void ShouldWorkWithHighLayer()
        {
            IPersistenceProvider provider = new SqlMapper.SqlMapperProvider();
            ((SqlMapper.SqlMapperProvider)provider).ConnectionString = "Data Source=.;Initial Catalog=uss2;Integrated Security=true";
            ((SqlMapper.SqlMapperProvider)provider).UseDefaultMapping = true;
            provider.RegisterMetaData(MetaData.MetaDataFactory.FromAssembly(GetType().Assembly, "Evaluant.Uss.Tests.Model"));
            provider.InitializeConfiguration();
            ObjectService os = new ObjectService(provider);
            os.ObjectContextType = typeof(EntityResolver.Proxy.Dynamic.ObjectContext).AssemblyQualifiedName;
            IObjectContext context = os.CreateObjectContext();
            context.InitializeRepository();
            context.BeginTransaction();
            context.Serialize(new Address() { City = "Mulhouse" });
            context.CommitTransaction();

            context = os.CreateObjectContext();
            var addresses = context.Load<Address>();

            if (addresses.Count > 0)
            {
                Address a = addresses.First();
                Assert.AreEqual("Mulhouse", a.City);
            }

            var addresses2 = (from Address a in context select a).ToList();

            Assert.AreEqual(addresses.FirstOrDefault(), addresses2.FirstOrDefault());
        }
    }
}
