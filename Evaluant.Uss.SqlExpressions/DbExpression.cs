using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlExpressions.Statements;

namespace Evaluant.Uss.SqlExpressions
{
    public abstract class DbExpression : Expression, IDbExpression
    {
        #region IDbExpression Members

        public abstract DbExpressionType DbExpressionType { get; }

        #endregion

        public override void Accept(NLinqVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public override Evaluant.NLinq.Expressions.ExpressionTypes ExpressionType
        {
            get { return Evaluant.NLinq.Expressions.ExpressionTypes.Unknown; }
        }

        //public override string ToString()
        //{
        //    return new Evaluant.Uss.SqlExpressions.Visitors.DbExpressionWriter().Render(this);
        //}
    }
}
