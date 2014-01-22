using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class EntityExpression : AliasedExpression
    {
        public EntityExpression(TableAlias alias) : base(alias) { }

        public Expression Expression { get; set; }

        public string Type { get; set; }

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Entity; }
        }
    }
}
