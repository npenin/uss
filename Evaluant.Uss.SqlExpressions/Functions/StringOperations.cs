using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using System.Data;

namespace Evaluant.Uss.SqlExpressions.Functions
{
    public class Lower : Function
    {
        public static readonly Identifier LowerIdentifier = new Identifier("LOWER");

        public Lower(Expression expressionToLower)
            : base(LowerIdentifier, new Expression[] { expressionToLower })
        {
        }

        public override FunctionType FunctionType
        {
            get { return FunctionType.Lower; }
        }
    }

    public class Upper : Function
    {
        public static readonly Identifier UpperIdentifier = new Identifier("UPPER");

        public Upper(Expression expressionToLower)
            : base(UpperIdentifier, new Expression[] { expressionToLower })
        {
        }

        public override FunctionType FunctionType
        {
            get { return FunctionType.Upper; }
        }
    }
}
