using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Optimizers
{
    class RemoveUselessRootSelectIfPossible : DbExpressionVisitor
    {
        SqlExpressions.IAliasedExpression columnToReplaceWith;
        private bool inFrom;
        private SqlExpressions.IAliasedExpression replacedSelect;

        public override SqlExpressions.IAliasedExpression Visit(SqlExpressions.SelectStatement item)
        {
            SqlExpressions.SelectStatement oldItem = item;
            var columnToReplaceWith = this.columnToReplaceWith;
            if (this.columnToReplaceWith != null && inFrom && item.Top == 0)
            {
                replacedSelect = item = (SqlExpressions.SelectStatement)updater.Update(item, new SqlExpressions.IAliasedExpression[] { columnToReplaceWith }, item.From, item.Where, item.OrderBy, item.Alias);

            }
            this.columnToReplaceWith = null;
            int n = 0;
            foreach (var column in oldItem.Columns)
            {
                n++;
                this.columnToReplaceWith = column;
            }

            if (n > 1 ||
                this.columnToReplaceWith == null ||
                this.columnToReplaceWith.DbExpressionType != SqlExpressions.DbExpressionType.Aggregate)
            {
                if (this.columnToReplaceWith != null)
                {
                    if (this.columnToReplaceWith.DbExpressionType == SqlExpressions.DbExpressionType.Column)
                    {
                        var column = this.columnToReplaceWith as SqlExpressions.ComplexColumnExpression;
                        if (column != null && ((SqlExpressions.IDbExpression)column.Expression).DbExpressionType != SqlExpressions.DbExpressionType.Value)
                        {
                            this.columnToReplaceWith = null;
                            replacedSelect = null;
                            item = oldItem;
                        }
                        else if (this.columnToReplaceWith.DbExpressionType != SqlExpressions.DbExpressionType.Aggregate)
                        {
                            this.columnToReplaceWith = null;
                            replacedSelect = null;
                            item = oldItem;
                        }
                    }
                    else
                    {
                        this.columnToReplaceWith = null;
                        replacedSelect = null;
                        item = oldItem;
                    }
                }
            }


            var result = base.Visit(item);
            this.columnToReplaceWith = columnToReplaceWith;
            if (replacedSelect != null && item != replacedSelect)
            {
                result = replacedSelect;
                replacedSelect = null;
            }
            return result;
        }

        public override NLinq.Expressions.QueryBodyClause Visit(NLinq.Expressions.WhereClause expression)
        {
            bool wasInFrom = inFrom;
            inFrom = false;
            var result = base.Visit(expression);
            inFrom = wasInFrom;
            return result;
        }

        public override SqlExpressions.FromClause Visit(SqlExpressions.FromClause item)
        {
            bool wasInFrom = inFrom;
            inFrom = true;
            var result = base.Visit(item);
            inFrom = wasInFrom;
            return result;
        }
    }
}
