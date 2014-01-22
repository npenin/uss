using System;
using Evaluant.NLinq.Expressions;
using System.Collections.Generic;

namespace Evaluant.Uss.ObjectContext.Contracts.Visitors
{
    public class InferRelationShips : NLinqVisitors.NLinqExpressionVisitor
    {
        public InferRelationShips()
        {
            RelationShips = new List<MemberExpression>();
        }

        public List<MemberExpression> RelationShips { get; set; }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.MethodCall item)
        {
            if (item.Identifier.Text == "Infer")
            {
                RelationShips.Add((MemberExpression)Visit(item.Parameters[0]));
                return null;
            }
            return base.Visit(item);
        }

        public override Expression Visit(MemberExpression expression)
        {
            Expression previous = expression.Previous;
            if (previous != null)
                previous = Visit(previous);
            Expression statement = Visit(expression.Statement);
            if (statement == null)
                return previous;
            return updater.Update(expression, previous, statement);
        }
    }
}
