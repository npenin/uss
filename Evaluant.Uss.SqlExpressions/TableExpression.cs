using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions
{
    public class TableExpression : AliasedExpression
    {
        public TableExpression(TableAlias alias)
            : base(alias)
        {
        }

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Table; }
        }
    }
}
