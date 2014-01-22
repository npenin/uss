using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.SqlMapper.Mapping;
using System.IO;

using SelfReferencingTableWithManyToManyAssociation;

namespace Evaluant.Uss.Tests.Engines.SqlMapper.Mappings.SelfReferencingTableWithManyToManyAssociation
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

        [TestMethod]
        public override void ShouldLoadMappingFile()
        {
            Mapping mapping = Mapping.Load(Path.Combine(mappingFolder, "SelfReferencingTableWithManyToManyAssociation/mapping.xml"));
            Assert.AreEqual(4, mapping.Entities.Count);
            Assert.AreEqual(InheritanceMappings.TablePerClass, mapping[typeof(Article).FullName].Inheritance.Type);
            Assert.AreEqual(InheritanceMappings.TablePerClass, mapping[typeof(Video).FullName].Inheritance.Type);
        }
    }
}
