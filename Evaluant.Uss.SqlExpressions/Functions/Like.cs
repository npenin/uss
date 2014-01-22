using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class Like : Function
    {
        public static readonly Identifier LikeIdentifier = new Identifier("LIKE");

        public Like(Expression lhs, Expression rhs)
            : base(LikeIdentifier, lhs, rhs)
        {
        }

        public override FunctionType FunctionType
        {
            get { return FunctionType.Like; }
        }
    }
}
