using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.Tests.Model;
using Evaluant.Uss.Linq;
using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.UnitTesting;

namespace Evaluant.Uss.Tests
{
    public partial class EngineTest : SilverlightTest
    {
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

        [TestMethod, Asynchronous]
        public virtual void TestTrace()
        {
            IObjectContextAsync c = CreateContext();
            if (c == null)
                return;
            var wi = EnqueueCallback(res =>
                {
                    c.EndInitializeRepository(res);
                    var wi2 = EnqueueCallback(res2 => Assert.AreEqual(0, c.EndLoadSingle<int>(res2)));
                    EnqueueTestComplete();
                    ((AsyncQueryableUss<Employee>)(from Employee p in c where p.FirstName == "Bob" select p)).Count(wi2.SetAsyncResult);

                });
            c.BeginInitializeRepository(wi.SetAsyncResult);
        }

        [TestMethod, Asynchronous]
        public virtual void TestBasics()
        {
            IObjectContextAsync c = CreateContext();
            if (c == null)
                return;
            EnqueueCallback(() =>
                {
                    c.BeginInitializeRepository();
                    c.BeginTransaction();
                    c.Serialize(new Employee() { FirstName = "Scott", Friends = { new Employee() { FirstName = "Steeve" } } });
                    var wi = EnqueueCallback(res =>
                        {
                            c.EndCommitTransaction(res);

                            var wi1 = EnqueueCallback(res1 => Assert.AreEqual(2, c.EndLoadSingle<int>(res1)));
                            new AsyncQueryableUss<Person>(c).Count(wi1.SetAsyncResult);

                            var wi2 = EnqueueCallback(res1 => Assert.AreEqual(1, c.EndLoadSingle<int>(res1)));
                            ((AsyncQueryableUss<Employee>)(from Employee p in c where p.FirstName == "Scott" select p)).Count(wi2.SetAsyncResult);

                            var wi3 = EnqueueCallback(res1 => Assert.AreEqual("Scott", c.EndLoadSingle<string>(res1)));
                            ((AsyncQueryableUss<string>)(from Employee p in c where p.Friends != null && p.Friends.Count() == 1 select p.FirstName)).FirstOrDefault(wi3.SetAsyncResult);

                            var wi4 = EnqueueCallback(res1 => Assert.AreEqual("Steeve", c.EndLoadSingle<string>(res1)));
                            ((AsyncQueryableUss<string>)(from Employee p in c where p.FirstName == "Steeve" select p.FirstName)).FirstOrDefault(wi4.SetAsyncResult);
                            EnqueueTestComplete();

                        });
                    c.BeginCommitTransaction(wi.SetAsyncResult);
                });
        }

        private IObjectContextAsync context;

        [Obsolete]
        protected IObjectContextAsync GetContext()
        {
            if (context == null)
                context = CreateContext();
            return context;
        }

        protected virtual IObjectContextAsync CreateContext()
        {
            return null;
        }

        [TestMethod, Asynchronous]
        public virtual void TestCount()
        {
            IObjectContextAsync c = CreateContext();
            if (c == null)
                return;

            EnqueueCallback(() =>
            {
                c.BeginInitializeRepository();
                c.BeginTransaction();
                c.Serialize(new Employee() { FirstName = "Bob", Friends = { new Employee() { FirstName = "John" } } });

                var wi = EnqueueCallback(res =>
                {
                    var wi1 = EnqueueCallback(res1 => Assert.AreEqual(2, c.EndLoadSingle<int>(res1)));
                    ((AsyncQueryableUss<Employee>)(from Employee p in c select p)).Count(wi1.SetAsyncResult);

                    var wi2 = EnqueueCallback(res1 => Assert.AreEqual(1, c.EndLoadSingle<int>(res1)));
                    ((AsyncQueryableUss<Employee>)(from Employee p in c where p.FirstName == "Bob" select p)).Count(wi2.SetAsyncResult);
                    EnqueueTestComplete();

                });

                c.BeginCommitTransaction(wi.SetAsyncResult);
            });
        }

        [TestMethod, Asynchronous]
        public virtual void TestPropertySelection()
        {
            IObjectContextAsync c = CreateContext();
            if (c == null)
                return;
            EnqueueCallback(() =>
            {
                var wi = EnqueueCallback(r =>
                {
                    c.BeginTransaction();
                    c.Serialize(new Employee() { FirstName = "Bob", Friends = { new Employee() { FirstName = "John" } } });
                    var wi1 = EnqueueCallback(res =>
                    {
                        c.EndCommitTransaction(res);

                        var wi2 = EnqueueCallback(res1 => Assert.AreEqual("John", c.EndLoadSingle<string>(res1)));
                        ((AsyncQueryableUss<string>)(from Employee p in c where p.Friends != null from friend in p.Friends select friend.FirstName)).FirstOrDefault(wi2.SetAsyncResult);

                        var wi3 = EnqueueCallback(res1 => Assert.AreEqual("Bob", c.EndLoadSingle<string>(res1)));
                        ((AsyncQueryableUss<string>)(from Employee p in c where p.FirstName == "Bob" select p.FirstName)).FirstOrDefault(wi3.SetAsyncResult);
                        EnqueueTestComplete();

                    });
                    c.BeginCommitTransaction(wi1.SetAsyncResult);
                });
                c.BeginInitializeRepository(wi.SetAsyncResult);
            });
        }

        [TestMethod, Asynchronous]
        public virtual void TestPropertyConstraint()
        {
            IObjectContextAsync c = CreateContext();
            if (c == null)
                return;
            EnqueueCallback(() =>
                {
                    var wi = EnqueueCallback(r =>
                                {
                                    c.BeginTransaction();
                                    c.Serialize(new Employee() { FirstName = "Bob", Friends = { new Employee() { FirstName = "John", Address = new Address() { City = "Strasbourg" } } }, Address = new Address() { City = "Mulhouse" } });
                                    var wi1 = EnqueueCallback(res =>
                                    {
                                        var wi2 = EnqueueCallback(res1 => Assert.AreEqual("John", c.EndLoadSingle<string>(res1)));
                                        ((AsyncQueryableUss<string>)(from Employee p in c from friend in p.Friends where p.Address.City == "Mulhouse" select friend.FirstName)).FirstOrDefault(wi2.SetAsyncResult);

                                        var wi3 = EnqueueCallback(res1 => Assert.AreEqual("John", c.EndLoadSingle<string>(res1)));
                                        ((AsyncQueryableUss<string>)(from Employee p in c from friend in p.Friends where friend.Address.City == "Strasbourg" select friend.FirstName)).FirstOrDefault(wi3.SetAsyncResult);

                                        var wi4 = EnqueueCallback(res1 => Assert.AreEqual("Bob", c.EndLoadSingle<string>(res1)));
                                        ((AsyncQueryableUss<string>)(from Employee p in c where p.FirstName == "Bob" select p.FirstName)).FirstOrDefault(wi4.SetAsyncResult);
                                        TestComplete();

                                    });
                                    c.BeginCommitTransaction(wi1.SetAsyncResult);
                                });
                    c.BeginInitializeRepository(wi.SetAsyncResult);
                });
        }

        public AsyncWorkItem EnqueueCallback(AsyncCallback callback)
        {
            var wi = new AsyncWorkItem(callback, ((AsyncUnitTestHarness)UnitTestHarness).EventRunDispatcher.DispatchRun);
            EnqueueWorkItem(wi);
            return wi;
        }
    }
}
