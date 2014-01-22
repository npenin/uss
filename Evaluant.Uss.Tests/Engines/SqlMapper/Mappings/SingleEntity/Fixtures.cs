using System;
using System.Text;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.ObjectContext;
using Evaluant.NLinq;
using Evaluant.Uss.SqlMapper.Mapping;
using Evaluant.Uss.SqlMapper;
using System.Reflection;
using System.IO;
using System.Configuration;

namespace Evaluant.Uss.Tests.Engines.SqlMapper.Mappings.SingleEntity
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
            Mapping mapping = Mapping.Load(Path.Combine(mappingFolder, "SingleEntity/mapping.xml"));

            SqlMapperProvider engine = new SqlMapperProvider();
            engine.Mapping = mapping;
            engine.RegisterMetaData(Evaluant.Uss.MetaData.MetaDataFactory.FromAssembly(Assembly.GetExecutingAssembly(), "SingleEntity"));
            engine.ConnectionString = ConfigurationManager.ConnectionStrings["SqlMapper.SqlServer"].ConnectionString;
            engine.CreatePersistenceEngine().InitializeRepository();

            engine.CreatePersistenceEngine().Load(new NLinqQuery(
                @"  from SingleEntity.Person p in context
                    where p.FirstName == 'toto'
                    select p"
            ));

        }
    }
}
