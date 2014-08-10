using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions
{
    public class Union : AliasedExpression
    {
        public Union(TableAlias alias, params IAliasedExpression[] selects)
            :base(alias)
        {
            this.SelectStatements = selects;
        }

        public override DbExpressionType DbExpressionType
        {
            get { return SqlExpressions.DbExpressionType.Union; }
        }

        public IAliasedExpression[] SelectStatements { get; set; }
    }
}
