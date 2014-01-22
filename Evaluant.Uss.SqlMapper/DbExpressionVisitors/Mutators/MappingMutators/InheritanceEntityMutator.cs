using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    class InheritanceMappingMutator : DbExpressionVisitor
    {
        public InheritanceMappingMutator(Mapping.Mapping mapping)
        {
            this.mapping = mapping;
        }

        private bool inFrom;
        private Mapping.Mapping mapping;
        public override NLinq.Expressions.QueryBodyClause Visit(NLinq.Expressions.FromClause expression)
        {
            bool wasInFrom = inFrom;
            inFrom = true;
            NLinq.Expressions.QueryBodyClause result = base.Visit(expression);
            inFrom = wasInFrom;
            return result;
        }

        public override SqlExpressions.IAliasedExpression Visit(SqlExpressions.ComplexColumnExpression item)
        {
            if (item.Expression != null && ((SqlExpressions.IDbExpression)item.Expression).DbExpressionType == SqlExpressions.DbExpressionType.Case)
            {
                if (mapping.Model.Entities[(string)((SqlExpressions.Constant)((SqlExpressions.CaseExpression)item.Expression).DefaultResult).Value].Inherit == null)
                {
                    return updater.Update(item, item.Alias, null, item.ColumnAlias);
                }
            }
            return base.Visit(item);
        }
    }
}
