using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace Evaluant.Uss.SqlMapper.Drivers
{
    public class OleDb : Driver
    {
        public OleDb()
            : this(DbProviderFactories.GetFactory("System.Data.OleDb"))
        {
        }

        public OleDb(DbProviderFactory factory)
            : base(factory)
        {
        }

    }
}
