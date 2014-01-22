using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace Evaluant.Uss.SqlMapper.Drivers
{
    public class Oracle : Driver
    {
        public Oracle()
            : this(DbProviderFactories.GetFactory("Oracle.DataAccess.Client"))
        {
        }

        public Oracle(DbProviderFactory factory)
            : base(factory)
        {
        }

    }
}
