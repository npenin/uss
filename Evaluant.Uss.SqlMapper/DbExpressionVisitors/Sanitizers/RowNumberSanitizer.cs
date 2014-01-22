using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Sanitizers
{
    class RowNumberSanitizer : DbExpressionVisitor
    {
        private bool removeColumnAlias;
        public override SqlExpressions.IAliasedExpression Visit(SqlExpressions.RowNumber item)
        {
            removeColumnAlias = true;
            var result = base.Visit(item);
            removeColumnAlias = false;
            return result;
        }

        public override SqlExpressions.IAliasedExpression Visit(SqlExpressions.ColumnExpression item)
        {
            if (removeColumnAlias && item.ColumnAlias != null)
            {
                item.ColumnAlias = null;
                return new SqlExpressions.ColumnExpression(item.Alias, item.ColumnName);
            }
            return base.Visit(item);
        }
    }
}
