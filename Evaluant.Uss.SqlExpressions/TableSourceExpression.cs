using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions
{
    public class TableSourceExpression : TableExpression
    {
        public TableSourceExpression(TableAlias alias, Mapping.Table table)
            : base(alias)
        {
            Table = table;
        }

        public Mapping.Table Table { get; set; }
    }
}
