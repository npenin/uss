using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions.Mapping
{
    public class LazyTableAlias : TableAlias
    {
        public LazyTableAlias(Table table)
        {
            TargetTable = table;
        }

        public Table TargetTable { get; set; }
    }
}
