using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.PersistentDescriptors;
using Evaluant.Uss.Memory;
using Evaluant.Uss.Tests.Model;
using Evaluant.Uss.Extensions;
using Microsoft.Silverlight.Testing;

namespace Evaluant.Uss.Tests.Engines
{
    [TestClass]
    public class ODataTest : EngineTest
    {
        private ObjectService os;

        OData.ODataPersistenceProvider provider;

        protected override IObjectContextAsync CreateContext()
        {
            if (provider == null)
            {
                provider = new OData.ODataPersistenceProvider();
                provider.ConnectionString = new Uri("http://localhost:37770/Common.svc/");
                provider.Initialized += ((AsyncUnitTestHarness)UnitTestHarness).EventRunDispatcher.DispatchRun;
                provider.EnsureConfigurationInitialized();
            }
            else if (provider.Metadata != null)
            {
                ((AsyncUnitTestHarness)UnitTestHarness).EventRunDispatcher.DispatchRun();
            }
            os = new ObjectService(provider);
            os.ObjectContextType = typeof(EntityResolver.Proxy.Dynamic.ObjectContextAsync).AssemblyQualifiedName;
            return os.CreateAsyncObjectContext();
        }

        [Asynchronous]
        public override void TestBasics()
        {
            //OData.ODataPersistenceProvider provider = new OData.ODataPersistenceProvider();
            //provider.ConnectionString = new Uri("http://services.odata.org/OData/OData.svc/");
            //IPersistenceEngineAsync pe = null;
            //provider.Initialized += ((AsyncUnitTestHarness)UnitTestHarness).EventRunDispatcher.DispatchRun;

            //EnqueueCallback(() =>
            //{
            //    pe = provider.CreatePersistenceEngineAsync();

            //    var wi = EnqueueCallback(res => Assert.IsNotNull(pe.EndLoadWithId(res)));
            //    pe.BeginLoadWithId(
            //        wi.SetAsyncResult,
            //        "ODataDemo.Category",
            //        "0",
            //        null);

            //    var wi1 = EnqueueCallback(res => Assert.AreEqual("Beverages", pe.EndLoad(res)[0].GetString("Name")));
            //    pe.BeginLoad(
            //        wi1.SetAsyncResult,
            //        "from ODataDemo.Category c in oc where c.ID==1 select c.Name",
            //        null);

            //    var wi2 = EnqueueCallback(res => Assert.AreEqual(1, pe.EndLoadScalar<int>(res)));
            //    pe.BeginLoadScalar<int>(
            //        wi2.SetAsyncResult,
            //        "from ODataDemo.Category c in oc where c.ID==1 select c.ID",
            //        null);



            DataService.uss2Entities dataService = new DataService.uss2Entities(new Uri("http://localhost:37770/Common.svc"));
            var scott = new DataService.Employee() { FirstName = "Scott" };
            var steeve = new DataService.Employee() { FirstName = "Steeve" };
            dataService.AddToPerson(scott);
            dataService.AddToPerson(steeve);
            dataService.AddLink(scott, "Friends", steeve);

            //var wi = EnqueueCallback(res => dataService.EndSaveChanges(res));
            //dataService.BeginSaveChanges(System.Data.Services.Client.SaveChangesOptions.Batch, wi.SetAsyncResult, null);

            base.TestBasics();
            //});

            //provider.EnsureConfigurationInitialized();
        }

        //[Asynchronous]
        //public override void TestCount()
        //{
        //    EnqueueCallback(base.TestCount);
        //}

        //[Asynchronous]
        //public override void TestPropertyConstraint()
        //{
        //    EnqueueCallback(base.TestPropertyConstraint);
        //}

        //[Asynchronous]
        //public override void TestPropertySelection()
        //{
        //    EnqueueCallback(base.TestPropertySelection);
        //}

        //[Asynchronous]
        //public override void TestTrace()
        //{
        //    EnqueueCallback(base.TestTrace);
        //}

        //[Asynchronous]
        //public override void TestLoadInherited()
        //{
        //    EnqueueCallback(base.TestLoadInherited);
        //}

        //[Asynchronous]
        //public override void TestLoadReferenceToMany()
        //{
        //    EnqueueCallback(base.TestLoadReferenceToMany);
        //}

        //[Asynchronous]
        //public override void TestLoadToMany()
        //{
        //    EnqueueCallback(base.TestLoadToMany);
        //}
    }
}
