using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.Tests.Model;
using Evaluant.Uss.Linq;

namespace Evaluant.Uss.Tests
{
    public partial class EngineTest
    {
        [TestMethod]
        public void TestLoadToMany()
        {
            IObjectContext context = GetContext();
            if (context == null)
                return;
            context.InitializeRepository();
            context.BeginTransaction();
            context.Serialize(new Employee { FirstName = "Bill", LastName = "Gates", Friends = { new Employee() { FirstName = "Steve", LastName = "Ballmer" } } });
            context.CommitTransaction();

            Person friend = context.Load<Employee>(string.Format("from {0} p in context from {0} friend in p.Friends select friend", typeof(Employee).FullName)).FirstOrDefault();
            Assert.IsNotNull(friend);
            Assert.AreEqual("Steve", friend.FirstName);

            Assert.AreEqual(0, context.Cast<Employee>().Where(employee => employee.FirstName == "Steve").SelectMany(employee => employee.Friends).Count());
            friend = context.Cast<Employee>().Where(employee => employee.FirstName == "Steve").SelectMany(employee => employee.Friends).FirstOrDefault();
            Assert.IsNull(friend);

            Assert.AreEqual(1,context.Cast<Employee>().Where(employee => employee.FirstName == "Bill").SelectMany(employee => employee.Friends).Count());
            friend = context.Cast<Employee>().Where(employee => employee.FirstName == "Bill").SelectMany(employee => employee.Friends).FirstOrDefault();
            Assert.IsNotNull(friend);
            Assert.AreEqual("Steve", friend.FirstName);

            friend.Cars.Add(new Car() { Name = "Tigra Twin Top" });
            friend.Cars.Add(new Car() { Name = "306 XRD" });
            context.BeginTransaction();
            context.Serialize(friend);
            context.CommitTransaction();

            context.Clear();
            Assert.AreEqual(2, context.Cast<Person>().Where(employee => employee.FirstName == "Steve").SelectMany(employee => employee.Cars).Count());
            var cars = context.Cast<Person>().Where(employee => employee.FirstName == "Steve").SelectMany(employee => employee.Cars).ToList();
            if (cars[0].Name == "306 XRD")
                Assert.AreEqual("Tigra Twin Top", cars[1].Name);
            else
                Assert.AreEqual("306 XRD", cars[1].Name);
        }
    }
}
