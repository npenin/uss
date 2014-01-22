using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace Evaluant.Uss.SqlMapper.Drivers
{
    public class MySql : Driver
    {
        public MySql()
            : this(DbProviderFactories.GetFactory("MySql.Data.MySqlClient"))
        {
        }

        public MySql(DbProviderFactory factory)
            : base(factory)
        {
        }

    }
}
