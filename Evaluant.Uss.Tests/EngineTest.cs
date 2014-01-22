using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Evaluant.Uss.Metadata;
using Evaluant.Uss.Tests.Model;
using Evaluant.Uss.Linq;
using System.Linq;
//using System.Linq.Expressions;
using Evaluant.Uss.PersistenceEngine;
//using Evaluant.Uss.Providers.EUSS;
using Evaluant.Uss;
//using Evaluant.Uss.Memory;
using Evaluant.Uss.Domain;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Configuration;
using Evaluant.Uss.ObjectContext;
#if !SILVERLIGHT
using Evaluant.Uss.SqlMapper;
using Evaluant.Uss.SqlMapper.Dialects;
using Evaluant.Uss.PersistenceEngine.Contracts.Instrumentation;
#endif
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.EntityResolver;
using System.Linq.Expressions;

namespace Evaluant.Uss.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public partial class EngineTest
    {
        public EngineTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        //[AssemblyInitialize]
        //public static void AssemblyInitialize(TestContext context)
        //{
        //    TraceHelper.AddListener(new UnitTestTraceListener()/* { Filter = new EventIdFilter(i => i == (int)Evaluant.Uss.SqlExpressions.DbExpressionType.Select) }*/);
        //}

        [TestInitialize]
        public void EngineTestInitialize()
        {
#if !SILVERLIGHT
            TraceHelper.AddListener(listener = new UnitTestListener());
            TraceHelper.AddListener(new UnitTestTraceListener(TestContext));
#endif
        }

#if !SILVERLIGHT
        protected UnitTestListener listener;
#endif

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

        [TestMethod]
        public virtual void TestTrace()
        {
            IObjectContext c = GetContext();
            if (c == null)
                return;
            c.InitializeRepository();
            Assert.AreEqual(0, (from Employee p in c where p.FirstName == "Bob" select p).Count());
        }

        [TestMethod]
        public virtual void TestBasics()
        {
            IObjectContext c = GetContext();
            if (c == null)
                return;
            c.InitializeRepository();
            c.BeginTransaction();
            c.Serialize(new Employee() { FirstName = "Scott", Friends = { new Employee() { FirstName = "Steeve" } } });
            c.CommitTransaction();

            Assert.AreEqual(2, (from Person p in c select p).Count());

            Assert.AreEqual(1, (from Person p in c where p.FirstName == "Scott" select p).Count());

            Assert.AreEqual("Scott", (from Employee p in c where p.Friends != null && p.Friends.Count() == 1 select p.FirstName).FirstOrDefault());

            Assert.AreEqual("Steeve", (from Employee p in c where p.FirstName == "Steeve" select p.FirstName).FirstOrDefault());
        }

        [TestMethod]
        public virtual void TestStringOperations()
        {
            IObjectContext c = GetContext();
            if (c == null)
                return;
            c.InitializeRepository();
            c.BeginTransaction();
            c.Serialize(new Employee() { FirstName = "Scott", LastName = "Hanselmann", Friends = { new Employee() { FirstName = "Scott", LastName = "Guthrie" } } });
            c.CommitTransaction();

            Assert.AreEqual(2, (from Person p in c where p.FirstName.StartsWith("Sco") select p).Count());
            Assert.AreEqual(2, (from Person p in c where p.FirstName.EndsWith("ott") select p).Count());
        }

        private IObjectContext context;

        protected IObjectContext GetContext()
        {
            if (context == null)
                context = CreateContext();
            return context;
        }

        protected virtual IObjectContext CreateContext()
        {
            return null;
        }

        [TestMethod]
        public virtual void TestCount()
        {
            IObjectContext c = GetContext();
            if (c == null)
                return;
            c.InitializeRepository();
            c.BeginTransaction();
            c.Serialize(new Employee() { FirstName = "Bob", Friends = { new Employee() { FirstName = "John" } } });
            c.CommitTransaction();
            Assert.AreEqual(2, (from Employee p in c select p).Count());

            Assert.AreEqual(1, (from Employee p in c where p.FirstName == "Bob" select p).Count());
        }

        [TestMethod]
        public virtual void TestPropertySelection()
        {
            IObjectContext c = GetContext();
            if (c == null)
                return;
            c.InitializeRepository();
            c.BeginTransaction();
            c.Serialize(new Employee() { FirstName = "Bob", Friends = { new Employee() { FirstName = "John" } } });
            c.CommitTransaction();

            Assert.AreEqual("John", (from Employee p in c where p.Friends != null from friend in p.Friends select friend.FirstName).FirstOrDefault());

            Assert.AreEqual("Bob", (from Employee p in c where p.FirstName == "Bob" select p.FirstName).FirstOrDefault());
        }

        [TestMethod]
        public virtual void TestPropertyConstraint()
        {
            IObjectContext c = GetContext();
            if (c == null)
                return;
            c.InitializeRepository();
            c.BeginTransaction();
            c.Serialize(new Employee() { FirstName = "Bob", Friends = { new Employee() { FirstName = "John", Address = new Address() { City = "Strasbourg" } } }, Address = new Address() { City = "Mulhouse" } });
            c.CommitTransaction();

            Assert.AreEqual("John", (from Employee p in c from friend in p.Friends where p.Address.City == "Mulhouse" select friend.FirstName).FirstOrDefault());
            Assert.AreEqual("John", (from Employee p in c from friend in p.Friends where friend.Address.City == "Strasbourg" select friend.FirstName).FirstOrDefault());

            Assert.AreEqual("Bob", (from Employee p in c where p.FirstName == "Bob" select p.FirstName).FirstOrDefault());
        }

        [TestMethod]
        public void ShouldWorkWithSameTypeAndDifferentParameterNames()
        {
            var oc = GetContext();
            if (oc == null)
                return;
            oc.InitializeRepository();
            Assert.AreEqual(0, oc.Cast<Person>().SelectMany(p => p.Friends).Where(p => p.FirstName == null).OrderBy(person => person.Id).ToList().Count);
            Assert.AreEqual(0, oc.Cast<Person>().Where(p => p.FirstName == null).SelectMany(p => p.Friends).OrderBy(person => person.Id).ToList().Count);
        }

        [TestMethod]
        public void ShouldWorkWithEmptyParameterName()
        {
            var oc = GetContext();
            if (oc == null)
                return;
            oc.InitializeRepository();
            var q = oc.Cast<Person>().SelectMany(p => p.Friends).Where(p => p.FirstName == null);
            //Reproducing a DynamicLinq query
            ParameterExpression[] parameters = new ParameterExpression[] {
                Expression.Parameter(q.ElementType, "") };
            q.Provider.CreateQuery<Person>(Expression.Call(typeof(Queryable), "OrderBy", new Type[] { typeof(Person), typeof(Guid) }, q.Expression, Expression.Quote(Expression.Lambda(Expression.Property(parameters[0], "Id"), parameters))));
            Assert.AreEqual(0, q.ToList().Count);
            Assert.AreEqual(0, q.Count());

            q = oc.Cast<Person>().Where(p => p.FirstName == null).SelectMany(p => p.Friends);
            q.Provider.CreateQuery<Person>(Expression.Call(typeof(Queryable), "OrderBy", new Type[] { typeof(Person), typeof(Guid) }, q.Expression, Expression.Quote(Expression.Lambda(Expression.Property(parameters[0], "Id"), parameters))));
            Assert.AreEqual(0, q.ToList().Count);
            Assert.AreEqual(0, q.Count());

            q = oc.Cast<Person>().Where(p => p.Address.City == "Mulhouse");
            q.Provider.CreateQuery<Person>(Expression.Call(typeof(Queryable), "OrderBy", new Type[] { typeof(Person), typeof(Guid) }, q.Expression, Expression.Quote(Expression.Lambda(Expression.Property(parameters[0], "Id"), parameters))));
            Assert.AreEqual(0, q.Count());
            Assert.AreEqual(0, q.ToList().Count);

            q = oc.Cast<Person>().Where(p => p.Address.City == "Mulhouse").SelectMany(p => p.Friends);
            q.Provider.CreateQuery<Person>(Expression.Call(typeof(Queryable), "OrderBy", new Type[] { typeof(Person), typeof(Guid) }, q.Expression, Expression.Quote(Expression.Lambda(Expression.Property(parameters[0], "Id"), parameters))));
            Assert.AreEqual(0, q.Count());
            Assert.AreEqual(0, q.ToList().Count);


            q = oc.Cast<Person>().SelectMany(p => p.Friends).Where(p => p.Address.City == "Mulhouse");
            q.Provider.CreateQuery<Person>(Expression.Call(typeof(Queryable), "OrderBy", new Type[] { typeof(Person), typeof(Guid) }, q.Expression, Expression.Quote(Expression.Lambda(Expression.Property(parameters[0], "Id"), parameters))));
            Assert.AreEqual(0, q.Count());
            Assert.AreEqual(0, q.ToList().Count);

            var q2 = oc.Cast<Person>().SelectMany(p => p.Cars).Where(p => p.Constructor.Name == "Opel");
            parameters = new ParameterExpression[] {
                Expression.Parameter(q2.ElementType, "") };
            q2.Provider.CreateQuery<Car>(Expression.Call(typeof(Queryable), "OrderBy", new Type[] { typeof(Car), typeof(string) }, q2.Expression, Expression.Quote(Expression.Lambda(Expression.Property(parameters[0], "Name"), parameters))));
            Assert.AreEqual(0, q.Count());
            Assert.AreEqual(0, q.ToList().Count);

        }

        [TestMethod]
        public virtual void TestPaging()
        {
            IObjectContext c = GetContext();
            if (c == null)
                return;
            c.InitializeRepository();
            c.BeginTransaction();
            c.Serialize(new Employee() { FirstName = "Bob", Friends = { new Employee() { FirstName = "John", Address = new Address() { City = "Strasbourg" } } }, Address = new Address() { City = "Mulhouse" } });
            c.CommitTransaction();

            c = CreateContext();
            try
            {
                c.Cast<Employee>().Skip(1).Take(1).Count();
                Assert.Fail("A Not supported exception should have been raised claiming a Skip needs an order by");
            }
            catch (NotSupportedException)
            {
            }
            Assert.AreEqual(1, c.Cast<Employee>().OrderBy(e => e.Id).Skip(1).Take(1).Count());
            Assert.AreEqual("John", c.Cast<Employee>().OrderBy(e => e.FirstName).Skip(1).Take(1).FirstOrDefault().FirstName);
            Assert.AreEqual(1, c.Cast<Employee>().OrderBy(e => e.FirstName).Take(1).Count());
            Assert.AreEqual("Bob", c.Cast<Employee>().OrderBy(e => e.FirstName).Take(1).FirstOrDefault().FirstName);
            Assert.AreEqual(0, c.Cast<Employee>().OrderBy(e => e.FirstName).Skip(2).Count());
        }

        [TestMethod]
        public void TestAnonymousNew()
        {
            IObjectContext c = GetContext();
            if (c == null)
                return;
            c.InitializeRepository();
            c.BeginTransaction();
            c.Serialize(new Employee() { FirstName = "Bob", Friends = { new Employee() { FirstName = "John", Address = new Address() { City = "Strasbourg" } } }, Address = new Address() { City = "Mulhouse" } });
            c.CommitTransaction();

            var personCities = c.Cast<Employee>().Select(e => new { FullName = e.FirstName, City = e.Address.City }).ToList();
            Assert.AreEqual(2, personCities.Count);
            Assert.IsTrue(personCities.Any(p => p.FullName == "John"));
            Assert.IsTrue(personCities.Any(p => p.FullName == "Bob"));
            Assert.AreEqual("Strasbourg", personCities.Where(p => p.FullName == "John").FirstOrDefault().City);
            Assert.AreEqual("Mulhouse", personCities.Where(p => p.FullName == "Bob").FirstOrDefault().City);

            Assert.AreEqual("Mulhouse", c.Cast<Employee>().Select(e => new { FullName = e.FirstName, City = e.Address.City }).Where(e => e.FullName == "Bob").FirstOrDefault().City);
            Assert.AreEqual("Strasbourg", c.Cast<Employee>().Select(e => new { FullName = e.FirstName, City = e.Address.City }).Where(e => e.FullName == "John").FirstOrDefault().City);
        }

        [TestMethod]
        public void TestAny()
        {
            IObjectContext c = GetContext();
            if (c == null)
                return;
            c.InitializeRepository();
            c.BeginTransaction();
            c.Serialize(new Employee() { FirstName = "Bob", Friends = { new Employee() { FirstName = "John", Address = new Address() { City = "Strasbourg" } } }, Address = new Address() { City = "Mulhouse" } });
            c.CommitTransaction();

            var personCities = c.Cast<Employee>();
            Assert.IsTrue(personCities.Any());
            Assert.IsTrue(personCities.Any(p => p.Friends.Any()));
        }
    }
}
