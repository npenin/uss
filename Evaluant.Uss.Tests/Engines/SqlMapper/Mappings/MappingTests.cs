using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.PersistenceEngine.Contracts.Instrumentation;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.SqlMapper.Mapping;
using Evaluant.Uss.SqlMapper;
using System.Configuration;
using System.Reflection;

namespace Evaluant.Uss.Tests.Engines.SqlMapper.Mappings
{
    [TestClass]
    public class MappingTests
    {
        [TestInitialize]
        public void MappingTestsInitialize()
        {
            TraceHelper.AddListener(new UnitTestTraceListener(TestContext));
        }

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

        protected string mappingFolder = "../../../Evaluant.Uss.Tests/Engines/SqlMapper/Mappings";

        [TestMethod]
        [Ignore]
        public virtual void ShouldLoadEntities()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        [Ignore]
        public virtual void ShouldLoadMappingFile()
        {
            throw new NotImplementedException();
        }

        protected IPersistenceProvider GetProvider(Mapping mapping)
        {
            SqlMapperProvider engine = new SqlMapperProvider();
            engine.Mapping = mapping;
            engine.RegisterMetaData(Evaluant.Uss.MetaData.MetaDataFactory.FromAssembly(Assembly.GetExecutingAssembly(), Namespace));
            engine.ConnectionString = ConfigurationManager.ConnectionStrings["SqlMapper.SqlServer"].ConnectionString;
            return engine;
        }


        public virtual string Namespace { get { throw new NotImplementedException(); } }
    }
}
