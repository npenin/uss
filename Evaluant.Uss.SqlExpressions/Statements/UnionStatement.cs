using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions.Statements
{
    public class UnionStatement : DbStatement
    {
        public UnionStatement(AliasedExpression[] selects)
        {
            this.SelectStatements = selects;
        }
        public override System.Data.StatementType StatementType
        {
            get { return System.Data.StatementType.Select; }
        }

        public override DbExpressionType DbExpressionType
        {
            get { return SqlExpressions.DbExpressionType.Union; }
        }

        public AliasedExpression[] SelectStatements { get; set; }
    }
}
