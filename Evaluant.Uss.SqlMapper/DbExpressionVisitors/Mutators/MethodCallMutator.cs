using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.NLinqImprovements;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.SqlExpressions.Visitors;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    public class MethodCallMutator : DbExpressionVisitor
    {
        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.MemberExpression expression)
        {
            if (expression.Previous == null)
                return Visit(expression.Statement);



            List<Evaluant.NLinq.Expressions.Expression> expressions = new List<Evaluant.NLinq.Expressions.Expression>(expression.Expressions);
            Evaluant.NLinq.Expressions.Expression exp = Visit(expressions[1]);
            if (exp is NLinq.Expressions.MethodCall)
            {
                NLinq.Expressions.MethodCall oldCall = exp as NLinq.Expressions.MethodCall;
                expressions.RemoveAt(1);
                MethodCall call = new MethodCall(Visit(expressions[0]), oldCall.Identifier, oldCall.Parameters);
                if (expressions.Count == 1)
                    return call;
                expressions[0] = call;
                return Visit(updater.Update(expression, expressions.ToArray()));
            }
            return base.Visit(expression);
        }
    }
}
