using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.Tests.ObjectContext.Relationships.Model;
using Evaluant.Uss.Linq;
//using Microsoft.Silverlight.Testing;

namespace Evaluant.Uss.Tests.ObjectContext
{
    /// <summary>
    /// Summary description for Relationships
    /// </summary>
    public partial class ObjectContextTest
    {

        [TestMethod]
        public void ShouldSaveOneToOneRelationship()
        {
            var context = CreateObjectContext();

            context.InitializeRepository();

            Person p = new Person() { Name = "Nick" };
            p.Pet = new Pet() { Name = "Titus" };

            context.BeginTransaction();
            context.Serialize(p);
            context.CommitTransaction();

            // get with same context
            var pets = context.Load<Pet>();
            Assert.AreEqual(1, pets.Count);

            // get with new context
            context = CreateObjectContext();
            pets = context.Load<Pet>();
            Assert.AreEqual(1, pets.Count);
        }

        [TestMethod]
        public void ShouldLoadOneToOneRelationship()
        {
            var context = CreateObjectContext();
            context.InitializeRepository();

            Person p = new Person() { Name = "Nick" };
            p.Pet = new Pet() { Name = "Titus" };

            context.BeginTransaction();
            context.Serialize(p);
            context.CommitTransaction();

            // using nlinq
            Assert.AreEqual(1, context.LoadScalar<int>("(from Evaluant.Uss.Tests.ObjectContext.Relationships.Model.Pet pet in context select pet).Count()"));
            Assert.AreEqual(1, context.LoadScalar<int>("(from Evaluant.Uss.Tests.ObjectContext.Relationships.Model.Person person in context select person).Count()"));

            // using LINQ
            Assert.AreEqual(1, (from Evaluant.Uss.Tests.ObjectContext.Relationships.Model.Pet pet in context select pet).Count());
            Assert.AreEqual(1, (from Evaluant.Uss.Tests.ObjectContext.Relationships.Model.Person person in context select person).Count());

        }

        [TestMethod]
        public void ShouldInferRelationship()
        {
            var context = CreateObjectContext();
            context.InitializeRepository();

            Person p = new Person() { Name = "Nick" };
            p.Pet = new Pet() { Name = "Titus" };

            context.BeginTransaction();
            context.Serialize(p);
            context.CommitTransaction();


            //Lazyloading
            context = CreateObjectContext();

            p = context.Cast<Person>().FirstOrDefault();
            Assert.IsNotNull(p);
            Assert.IsNotNull(p.Pet);

            //no lazyLoading
            context = CreateObjectContext();
            context.IsLazyLoadingEnabled = false;
            p = context.Cast<Person>().FirstOrDefault();
            Assert.IsNotNull(p);
            Assert.IsNull(p.Pet);

            //only infer when specified
            context = CreateObjectContext();
            p = context.Cast<Person>().Infer(q => q.Pet).FirstOrDefault();
            context.IsLazyLoadingEnabled = false;
            Assert.IsNotNull(p);
            Assert.IsNotNull(p.Pet);
        }


        [TestMethod]
        public void ShouldInferRelationshipToMany()
        {
            var context = CreateObjectContext();
            context.InitializeRepository();

            Person p = new Person() { Name = "Nick" };
            p.Addresses = new List<Address> { new Address { City = "Mulhouse" }, new Address { City = "Colmar" } };

            context.BeginTransaction();
            context.Serialize(p);
            context.CommitTransaction();


            //Lazyloading
            context = CreateObjectContext();

            p = context.Cast<Person>().FirstOrDefault();
            Assert.IsNotNull(p);
            Assert.IsNotNull(p.Addresses);
            Assert.AreEqual(2, p.Addresses.Count);

            //no lazyLoading
            context = CreateObjectContext();
            context.IsLazyLoadingEnabled = false;
            p = context.Cast<Person>().FirstOrDefault();
            Assert.IsNotNull(p);
            Assert.AreEqual(0, p.Addresses.Count);


            //only infer when specified
            context = CreateObjectContext();
            p = context.Cast<Person>().Infer(q => q.Addresses).FirstOrDefault();
            context.IsLazyLoadingEnabled = false;
            Assert.IsNotNull(p);
            Assert.IsNotNull(p.Addresses);
            Assert.AreEqual(2, p.Addresses.Count);
        }
    }
}
