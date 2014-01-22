using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.Tests.Model;
using Microsoft.Silverlight.Testing;

namespace Evaluant.Uss.Tests
{
    public partial class EngineTest
    {
        [TestMethod, Asynchronous]
        public virtual void TestLoadToMany()
        {
            IObjectContextAsync context = CreateContext();
            if (context == null)
                return;
            EnqueueCallback(() =>
            {

                context.BeginInitializeRepository();
                context.BeginTransaction();
                context.Serialize(new Employee { FirstName = "Bill", LastName = "Gates", Friends = { new Employee() { FirstName = "Steve", LastName = "Ballmer" } } });
                var wi = EnqueueCallback(res =>
                {
                    var wi2 = EnqueueCallback(res2 =>
                        {
                            Employee friend = context.EndLoad<Employee>(res2).FirstOrDefault();
                            Assert.IsNotNull(friend);
                            Assert.AreEqual("Steve", friend.FirstName);
                            TestComplete();
                        });
                    context.BeginLoad<Employee>(wi2.SetAsyncResult,
                        string.Format("from {0} p in context from {0} friend in p.Friends select friend", typeof(Employee).FullName));
                });
                context.BeginCommitTransaction(wi.SetAsyncResult);
            });
        }
    }
}
