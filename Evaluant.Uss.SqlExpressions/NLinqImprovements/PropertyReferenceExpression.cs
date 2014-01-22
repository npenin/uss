using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.NLinqImprovements
{
    public class PropertyReferenceExpression : EntityReferenceExpression
    {
        public PropertyReferenceExpression(AliasedExpression target, Identifier propertyName)
            : base(target)
        {
            this.PropertyName = propertyName;
        }

        public PropertyReferenceExpression(TableAlias alias, AliasedExpression target, Identifier propertyName)
            : base(alias, target)
        {
            this.PropertyName = propertyName;
        }

        public Identifier PropertyName { get; set; }

        public override Evaluant.Uss.SqlExpressions.DbExpressionType ExpressionType
        {
            get
            {
                return Evaluant.Uss.SqlExpressions.DbExpressionType.Property;
            }
        }

    }
}
