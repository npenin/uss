using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using System.Data;

namespace Evaluant.Uss.SqlExpressions
{
    public class Cast : Function
    {
        public static readonly Identifier CastIdentifier = new Identifier("CAST");

        public DbType Type { get; set; }

        public Cast(Expression expressionToCast, DbType type)
            : base(CastIdentifier, new Expression[] { expressionToCast })
        {
            Type = type;
        }

        public override FunctionType FunctionType
        {
            get { return FunctionType.Cast; }
        }
    }
}
