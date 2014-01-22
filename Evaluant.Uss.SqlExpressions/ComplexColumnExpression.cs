using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class ComplexColumnExpression : ColumnExpression
    {
        public ComplexColumnExpression(TableAlias alias, Expression innerExpression)
            : this(alias, innerExpression, (string)null)
        {
        }

        public ComplexColumnExpression(TableAlias alias, Expression innerExpression, string columnAlias)
            : base(alias, null, columnAlias)
        {
            Expression = innerExpression;
        }

        public ComplexColumnExpression(TableAlias alias, Expression innerExpression, Identifier columnAlias)
            : base(alias, null, columnAlias)
        {
            Expression = innerExpression;
        }


        public Expression Expression { get; private set; }
    }
}
