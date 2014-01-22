using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    public class SqlValidator : DbExpressionVisitor
    {
        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.BinaryExpression item)
        {
            //NLinq.Expressions.Expression left = Visit(item.LeftExpression);
            //NLinq.Expressions.Expression right = Visit(item.RightExpression);
            return base.Visit(item);
        }
    }
}
