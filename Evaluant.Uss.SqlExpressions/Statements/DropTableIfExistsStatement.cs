using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions
{
    public class DropTableIfExistsStatement : DropTableStatement
    {
        public DropTableIfExistsStatement(Mapping.Table table) : base(table) { }

    }
}
