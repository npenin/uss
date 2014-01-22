using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions
{
    public class TableAlias
    {
        public static TableAlias All = new TableAlias();

        public override string ToString()
        {
            return "t" + GetHashCode();
        }
    }
}
