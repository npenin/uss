using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class IsNull : Function
    {
        public static readonly Identifier IsNullIdentifier = new Identifier("ISNULL");

        public IsNull(Expression nullable)
            : base(IsNullIdentifier, nullable)
        {
        }

        public override FunctionType FunctionType
        {
            get { return FunctionType.IsNull; }
        }
    }
}
