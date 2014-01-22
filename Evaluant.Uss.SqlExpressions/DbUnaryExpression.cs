using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public abstract class DbUnaryExpression : UnaryExpression, IDbExpression, IAliasedExpression
    {
        public DbUnaryExpression(UnaryExpressionType type, Expression operand) : base(type, operand) { }

        public abstract DbExpressionType DbExpressionType { get; }

        public new IDbExpression Expression
        {
            get { return (IDbExpression)base.Expression; }
            set { base.Expression = (Expression)value; }
        }

        public TableAlias Alias
        {
            get;
            set;
        }
    }

    public class Not : DbUnaryExpression
    {
        public Not(IDbExpression expression) : this(null, expression) { }

        public Not(TableAlias alias, IDbExpression expression)
            : base(UnaryExpressionType.Not, (Expression)expression)
        {
            Alias = alias;
        }

        public override DbExpressionType DbExpressionType
        {
            get { return SqlExpressions.DbExpressionType.Not; }
        }
    }
}
