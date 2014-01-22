using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.NLinqImprovements
{
    public class EntityReferenceExpression : AliasedExpression
    {
        public EntityReferenceExpression(IAliasedExpression target)
            : base(target.Alias)
        {
            this.Target = target;
        }

        public EntityReferenceExpression(TableAlias alias, IAliasedExpression target)
            : base(alias)
        {
            this.Target = target;
        }

        public IAliasedExpression Target { get; set; }

        public override void Accept(NLinqVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Entity; }
        }
    }
}
