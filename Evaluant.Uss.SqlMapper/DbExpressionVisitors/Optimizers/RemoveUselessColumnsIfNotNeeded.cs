using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Optimizers
{
    class RemoveUselessColumnsIfNotNeeded : DbExpressionVisitor
    {
        private bool inSelect;
        SqlExpressions.IAliasedExpression lastColumn = null;
        public override SqlExpressions.IAliasedExpression Visit(SqlExpressions.SelectStatement item)
        {
            bool wasInSelect = inSelect;
            if (!inSelect)
            {
                int n = 0;
                foreach (var column in item.Columns)
                {
                    n++;
                    lastColumn = column;
                }
                if (n == 1 && (lastColumn.DbExpressionType == SqlExpressions.DbExpressionType.Aggregate ||lastColumn.DbExpressionType==DbExpressionType.Function))
                {
                    inSelect = true;
                    if (item.Where != null || item.From.Count != 1 || item.From[0].DbExpressionType != DbExpressionType.Select || ((SelectStatement)item.From[0]).Top > 0)
                        lastColumn = null;
                    item = (SqlExpressions.SelectStatement)base.Visit(item);
                    if (lastColumn != null)
                        return item.From[0];
                    inSelect = wasInSelect;
                }
                return item;
            }
            else
            {
                List<SqlExpressions.IAliasedExpression> columns = new List<SqlExpressions.IAliasedExpression>();
                foreach (var column in item.Columns)
                {
                    if (column.DbExpressionType == SqlExpressions.DbExpressionType.Column)
                    {
                        if (column is SqlExpressions.ComplexColumnExpression)
                            columns.Add(column);
                    }
                }
                if (columns.Count <= 1)
                {
                    if (columns.Count == 1 && ((SqlExpressions.IDbExpression)((SqlExpressions.ComplexColumnExpression)columns[0]).Expression).DbExpressionType == SqlExpressions.DbExpressionType.Case)
                    {
                        columns.RemoveAt(0);
                    }
                    if (lastColumn != null)
                        columns.Add(lastColumn);
                    else
                        columns.Add(new ComplexColumnExpression(null, new Constant(1, System.Data.DbType.Int32), "a"));
                }
                return updater.Update(item, columns, item.From, item.Where, item.OrderBy, item.Alias);
            }
        }
    }
}
