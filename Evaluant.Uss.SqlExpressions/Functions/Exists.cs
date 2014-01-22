using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class Exists : Function
    {
        public static readonly Identifier ExistsIdentifier = new Identifier("EXISTS");

        public Exists(Expression expression)
            : base(ExistsIdentifier, expression)
        {
        }

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Function; }
        }



        public override FunctionType FunctionType
        {
            get { return FunctionType.Exists; }
        }
    }
}
