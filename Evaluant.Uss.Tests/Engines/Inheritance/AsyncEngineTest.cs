using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.Tests.Model;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.Linq;
using Microsoft.Silverlight.Testing;

namespace Evaluant.Uss.Tests
{
    public partial class EngineTest
    {
        [TestMethod, Asynchronous]
        public virtual void TestLoadInherited()
        {
            IObjectContextAsync context = CreateContext();
            if (context == null)
                return;
            EnqueueCallback(() =>
            {
                context.BeginInitializeRepository();
                context.BeginTransaction();
                context.Serialize(new Person());
                context.Serialize(new Employee());
                var wi1 = EnqueueCallback(res1 =>
                    {
                        context.EndCommitTransaction(res1);

                        var wi2 = EnqueueCallback(res2 => Assert.AreEqual(2, context.EndLoadSingle<int>(res2)));
                        ((AsyncQueryableUss<Person>)(from Person p in context select p)).Count(wi2.SetAsyncResult);

                        var wi3 = EnqueueCallback(res2 =>
                            {
                                var people = context.EndLoad<Person>(res2);
                                Employee e1 = people[0] as Employee;
                                Person p1 = people[1];
                                if (e1 == null)
                                {
                                    e1 = people[1] as Employee;
                                    p1 = people[0];
                                }

                                Assert.AreEqual(typeof(Person).Name + "Proxy", p1.GetType().Name);
                                Assert.AreEqual(typeof(Employee).Name + "Proxy", e1.GetType().Name);
                            });
                        ((AsyncQueryableUss<Person>)(from Person p in context orderby p.Id select p)).ToList(wi3.SetAsyncResult);

                        var wi4 = EnqueueCallback(res2 => Assert.AreEqual(1, context.EndLoadSingle<int>(res2)));
                        ((AsyncQueryableUss<Employee>)(from Employee p in context select p)).Count(wi4.SetAsyncResult);
                    });
                context.BeginCommitTransaction(wi1.SetAsyncResult);
            });
        }

        [TestMethod, Asynchronous]
        public virtual void TestLoadReferenceToMany()
        {
            IObjectContextAsync context = CreateContext();
            if (context == null)
                return;
            EnqueueCallback(() =>
                {
                    var wi = EnqueueCallback(res =>
                                {
                                    context.EndInitializeRepository(res);
                                    context.BeginTransaction();
                                    context.Serialize(new Person() { Friends = { new Employee() } });
                                    var wi2 = EnqueueCallback(res2 =>
                                    {
                                        context.EndCommitTransaction(res2);

                                        var friends = from Person p in context
                                                      from Person friend in p.Friends
                                                      orderby p.Id
                                                      select friend;

                                        var wi3 = EnqueueCallback(res3 => Assert.AreEqual(1, context.EndLoadSingle<int>(res3)));
                                        ((AsyncQueryableUss<Person>)friends).Count(wi3.SetAsyncResult);

                                        var wi4 = EnqueueCallback(res3 => Assert.AreEqual(typeof(Employee).Name + "Proxy", context.EndLoadSingle<Person>(res3).GetType().Name));
                                        ((AsyncQueryableUss<Person>)friends).First(wi4.SetAsyncResult);
                                        EnqueueTestComplete();
                                    });
                                    context.BeginCommitTransaction(wi2.SetAsyncResult);
                                });
                    context.BeginInitializeRepository(wi.SetAsyncResult);
                });
        }
    }
}
