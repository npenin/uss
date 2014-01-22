using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Evaluant.Uss.SqlExpressions;
using System.Xml;
using Evaluant.Uss.SqlExpressions.Mapping;
using System.Transactions;
using System.Data.Common;

namespace Evaluant.Uss.SqlMapper.Drivers
{
    public class SqlServer : Driver
    {
        public SqlServer()
            : this(DbProviderFactories.GetFactory("System.Data.SqlClient"))
        {
        }

        public SqlServer(DbProviderFactory factory)
            : base(factory)
        {
        }
    }
}
