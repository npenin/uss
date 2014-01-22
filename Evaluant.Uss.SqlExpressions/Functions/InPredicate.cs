using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class InPredicate : Function
    {
        public static readonly Identifier InPredicateIdentifier = new Identifier("IN");

        public InPredicate(Expression[] values)
            : base(InPredicateIdentifier, values)
        {
        }

        public override DbExpressionType DbExpressionType { get { return DbExpressionType.In; } }

        public override FunctionType FunctionType
        {
            get { return FunctionType.In; }
        }
    }
}
