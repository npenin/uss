using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlExpressions.Statements;

namespace Evaluant.Uss.SqlExpressions
{
    public class FluentDbExpression
    {
        public static IfStatement If(IDbExpression condition, IDbStatement then)
        {
            return If(condition, then);
        }
        public static IfStatement If(IDbExpression condition, IDbStatement then, IDbStatement @else)
        {
            return new IfStatement(condition, then, @else);
        }

        public static Not Not(IDbExpression booleanToBeInversed)
        {
            return new Not(booleanToBeInversed);
        }
    }
}
