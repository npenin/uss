using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.SqlExpressions.Visitors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    class ArrayToUnionAll : DbExpressionVisitor
    {
        private IDriver driver;
        private bool inFrom;
        private bool inWhere;
        public ArrayToUnionAll(IDriver driver)
        {
            this.driver = driver;
        }

        public override SqlExpressions.FromClause Visit(SqlExpressions.FromClause item)
        {
            bool wasInWhere = inWhere, wasInFrom = inFrom;
            inWhere = false;
            inFrom = true;
            var result = base.Visit(item);
            inWhere = wasInWhere;
            inFrom = wasInFrom;
            return result;
        }

        public override QueryBodyClause Visit(WhereClause expression)
        {
            bool wasInWhere = inWhere, wasInFrom = inFrom;
            inWhere = true;
            inFrom = false;
            var result = base.Visit(expression);
            inWhere = wasInWhere;
            inFrom = wasInFrom;
            return result;
        }

        public override Identifier Visit(NLinqImprovements.EntityIdentifier identifier)
        {
            if (inWhere && identifier.Entity.Type != null && identifier.Entity.Type.StartsWith("System."))
                return new Identifier("b");
            return base.Visit(identifier);
        }

        public override SqlExpressions.IDbExpression Visit(SqlExpressions.EntityExpression item)
        {
            if (item.Type != null && item.Type.StartsWith("System."))
            {
                if (item.Expression.ExpressionType == NLinq.Expressions.ExpressionTypes.Constant)
                {
                    IEnumerable list = ((ValueExpression)item.Expression).Value as IEnumerable;
                    List<IAliasedExpression> selects = new List<IAliasedExpression>();
                    foreach (var value in list)
                    {
                        var select = new SelectStatement(null);
                        select.Columns = new[]{
                            new ComplexColumnExpression(null, new Constant(value, driver.GetDbType(value.GetType())), new Identifier("b"))
                        };
                        selects.Add(select);
                    }
                    if (selects.Count == 1)
                        return selects[0];
                    return new EntityExpression(null) { Expression = new Union(new TableAlias(), selects.ToArray()) };
                }
            }
            return base.Visit(item);
        }
    }
}
