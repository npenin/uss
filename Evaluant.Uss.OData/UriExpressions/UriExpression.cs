using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.OData.UriExpressions
{
    abstract class UriExpression : Expression
    {
        public override void Accept(NLinqVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override ExpressionTypes ExpressionType
        {
            get { return ExpressionTypes.Unknown; }
        }
    }
}
