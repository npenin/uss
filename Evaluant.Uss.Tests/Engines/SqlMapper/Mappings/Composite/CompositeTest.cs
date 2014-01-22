using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.SqlMapper.Mapping;
using System.IO;
using Evaluant.Uss.Linq;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.ObjectContext.Contracts;

namespace Evaluant.Uss.Tests.Engines.SqlMapper.Mappings.Composite
{
    [TestClass]
    public class CompositeTest : MappingTests
    {
        public override string Namespace
        {
            get
            {
                return "Evaluant.Uss.Tests.Engines.SqlMapper.Mappings.Composite";
            }
        }

        [TestMethod]
        public override void ShouldLoadMappingFile()
        {
            Mapping mapping = Mapping.Load(Path.Combine(mappingFolder, "Composite/mapping.xml"));
            Assert.AreEqual(2, mapping.Entities.Count);
            Assert.AreEqual(mapping.Entities[typeof(Person).FullName].Table, mapping.Entities[typeof(Address).FullName].Table);
        }

        [TestMethod]
        public override void ShouldLoadEntities()
        {
            Mapping mapping = Mapping.Load(Path.Combine(mappingFolder, "Composite/mapping.xml"));

            ObjectService os = new ObjectService(GetProvider(mapping));
            IObjectContext oc = os.CreateObjectContext();

            oc.InitializeRepository();

            oc.BeginTransaction();
            oc.Serialize(new Person() { FirstName = "test", Address = new Address() { City = "test" } });
            oc.CommitTransaction();

            oc.Clear();

            IList<Person> person = ((IInferrable<Person>)from Person p in oc select p).ToList();

            Assert.AreEqual(1, person.Count);

            Assert.IsNotNull(person.FirstOrDefault());

            Assert.AreEqual("test", person.First().FirstName);

            Assert.AreEqual(person.First().FirstName, person.First().LastName);

            Assert.IsNotNull(person.First().Address);

            Assert.AreEqual("test", person.First().Address.City);
        }

    }
}
