using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.Tests.Model;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.Linq;

namespace Evaluant.Uss.Tests
{
    public partial class EngineTest
    {
        [TestMethod]
        public void TestLoadInherited()
        {
            IObjectContext context = GetContext();
            if (context == null)
                return;
            context.InitializeRepository();

            context.BeginTransaction();
            context.Serialize(new Person());
            context.Serialize(new Employee());
            context.CommitTransaction();

            Assert.AreEqual(2, (from Person p in context select p).Count());

            var people = (from Person p in context orderby p.Id select p).ToList();
            Employee e1 = people[0] as Employee;
            Person p1 = people[1];
            if (e1 == null)
            {
                e1 = people[1] as Employee;
                p1 = people[0];
            }

            Assert.AreEqual(typeof(Person).Name + "Proxy", p1.GetType().Name);
            Assert.AreEqual(typeof(Employee).Name + "Proxy", e1.GetType().Name);

            Assert.AreEqual(1, (from Employee p in context select p).Count());
        }

        [TestMethod]
        public void TestLoadReferenceToMany()
        {
            IObjectContext context = GetContext();
            if (context == null)
                return;
            context.InitializeRepository();

            context.BeginTransaction();
            context.Serialize(new Person() { LastName = "b", Friends = { new Employee() { LastName = "a" } } });
            context.CommitTransaction();

            var friends = from Person p in context
                          from Person friend in p.Friends
                          orderby p.LastName
                          select friend;

            //Assert.AreEqual(1, friends.Count());
            Assert.AreEqual(typeof(Employee).Name + "Proxy", friends.First().GetType().Name);
        }
    }
}
