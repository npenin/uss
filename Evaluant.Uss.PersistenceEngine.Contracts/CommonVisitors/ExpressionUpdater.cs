using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.CommonVisitors
{
    public class ExpressionUpdater
    {
        public virtual AnonymousParameter Update(AnonymousParameter original, Identifier identifier, Expression expression)
        {
            if (original.Expression != expression || original.Identifier != identifier)
                return new AnonymousParameter(identifier, expression);
            return original;
        }

        public virtual TernaryExpression Update(TernaryExpression c, Expression test, Expression ifTrue, Expression ifFalse)
        {
            if (test != c.LeftExpression || ifTrue != c.MiddleExpression || ifFalse != c.RightExpression)
            {
                return new TernaryExpression(test, ifTrue, ifFalse);
            }
            return c;
        }

        public virtual MethodCall Update(MethodCall m, Identifier target, Identifier methodName, Expression[] args)
        {
            if (target != m.AnonIdentifier || methodName != m.Identifier || args != m.Parameters)
            {
                return new MethodCall(methodName, (Expression[])args, m.AnonIdentifier, m.IndexIdentifier, m.LambdaExpression);

            }
            return m;
        }

        public virtual TypedNew Update(TypedNew nex, string type, Expression[] args)
        {
            if (args != nex.Parameters || nex.Type != type)
            {
                return new TypedNew(type, args);
            }
            return nex;
        }

        public virtual UnaryExpression Update(UnaryExpression u, Expression operand, UnaryExpressionType unaryExpressionType)
        {
            if (u.Expression != operand || u.Type != unaryExpressionType)
            {
                return new UnaryExpression(unaryExpressionType, operand);
            }
            return u;
        }

        public virtual OrderByClause Update(OrderByClause expression, IEnumerable<OrderByCriteria> criteria)
        {
            if (expression.Criterias != criteria)
                return new OrderByClause(criteria);
            return expression;
        }

        public virtual OrderByCriteria Update(OrderByCriteria criterium, Expression expression, bool ascending)
        {
            if (criterium.Expression != expression || criterium.Ascending != ascending)
                return new OrderByCriteria(expression, ascending);
            return criterium;

        }
    }
}
