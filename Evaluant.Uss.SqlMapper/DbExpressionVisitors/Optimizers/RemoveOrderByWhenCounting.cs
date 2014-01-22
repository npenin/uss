using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Optimizers
{
    class RemoveOrderByWhenCounting : DbExpressionVisitor
    {
        public override SqlExpressions.IAliasedExpression Visit(SqlExpressions.SelectStatement item)
        {
            int n = 0;
            SqlExpressions.IAliasedExpression lastColumn = null;
            foreach (var column in item.Columns)
            {
                n++;
                lastColumn = column;
            }
            if (n == 1 && lastColumn != null && lastColumn.DbExpressionType == SqlExpressions.DbExpressionType.Aggregate)
            {
                item.OrderBy = null;
            }

            return base.Visit(item);
        }
    }
}
