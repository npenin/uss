using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions.Visitors
{
    public class DefaultAliasAsigner : DbExpressionVisitor
    {
        private TableAlias defaultAlias;
        private bool alone;

        public DefaultAliasAsigner(TableAlias defaultAlias)
        {
            this.defaultAlias = defaultAlias;
            alone = true;
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.MemberExpression expression)
        {
            alone = false;
            var result=base.Visit(expression);
            alone = true;
            return result;
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.Identifier identifier)
        {
            if (alone)
                return new ColumnExpression(defaultAlias, identifier);
            return base.Visit(identifier);
        }

        public override TableAlias Visit(TableAlias item)
        {
            if(item==null)
                return defaultAlias;
            return item;
        }
    }
}
