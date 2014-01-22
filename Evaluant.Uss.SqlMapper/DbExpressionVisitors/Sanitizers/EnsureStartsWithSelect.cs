using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Sanitizers
{
    public class EnsureStartsWithSelect : DbExpressionVisitor
    {
        private bool hasSelect;
        public override SqlExpressions.IAliasedExpression Visit(SqlExpressions.Exists item)
        {
            if (hasSelect)
                return base.Visit(item);
            return new SelectStatement(new TableAlias(), new IAliasedExpression[] { new ComplexColumnExpression(null, new Constant(1, System.Data.DbType.Int32)) }, null, null, new NLinq.Expressions.WhereClause(item)) { Top = 1 };
        }

        public override SqlExpressions.IAliasedExpression Visit(SqlExpressions.SelectStatement item)
        {
            hasSelect = true;
            return base.Visit(item);
        }
    }
}
