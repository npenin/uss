using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.SqlMapper.Mapping;
using System.IO;

namespace Evaluant.Uss.Tests.Engines.SqlMapper.Mappings.Inheritance
{
    /// <summary>
    /// Summary description for Fixtures
    /// </summary>
    [TestClass]
    public class Fixtures : MappingTests
    {
        public Fixtures()
        {
            //
            // TODO: Add constructor logic here
            //
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
        public override void ShouldLoadMappingFile()
        {
            var m1 = Mapping.Load(Path.Combine(mappingFolder, "Inheritance/table-per-class.xml"));
            Assert.AreEqual(4, m1.Entities.Count);

            var m2 = Mapping.Load(Path.Combine(mappingFolder, "Inheritance/table-per-hierarchy.xml"));
            Assert.AreEqual(4, m2.Entities.Count);

            var m3 = Mapping.Load(Path.Combine(mappingFolder, "Inheritance/table-per-concrete-class.xml"));
            Assert.AreEqual(3, m3.Entities.Count);
        }
    }
}
