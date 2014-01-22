using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.NLinqImprovements
{
    public class MethodCall : Expression, IDbExpression
    {
        public MethodCall(Expression target, Identifier methodName, Expression[] parameters)
        {
            this.Target = target;
            this.MethodName = methodName;
            Parameters = parameters;
        }

        public virtual DbExpressionType ExpressionType
        {
            get { return DbExpressionType.Function; }
        }

        public Expression Target { get; set; }

        public Identifier MethodName { get; set; }

        public Expression[] Parameters { get; set; }

        public override void Accept(NLinqVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}
