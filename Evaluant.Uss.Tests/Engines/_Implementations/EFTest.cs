using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.SqlMapper.EF;

namespace Evaluant.Uss.Tests.Engines._Implementations
{
    [TestClass]
    [Ignore]
    public class EFTest : SqlMapperTest
    {
        public EFTest()
        {
            connectionString = "metadata=res://*/EFSimple.csdl|res://*/EFSimple.ssdl|res://*/EFSimple.msl;provider=System.Data.SqlClient;provider connection string=\"Data Source=.;Initial Catalog=uss2;Integrated Security=True\"";
        }

        protected override Uss.SqlMapper.IDialect GetDialect()
        {
            return new EFDialect();
        }

        protected override Uss.SqlMapper.IDriver GetDriver()
        {
            return new EFDriver();
        }

        [TestMethod]
        [Ignore]
        public override void EnsureQueryExecutedWithNoLock()
        {
            base.EnsureQueryExecutedWithNoLock();
        }
    }
}
