using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.SqlExpressions.Visitors;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    public class StringOperations : DbExpressionVisitor
    {
        private Expression percent = new Constant('%', DbType.String);

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.MemberExpression expression)
        {
            if (expression.Statement.ExpressionType == ExpressionTypes.Call)
            {
                MethodCall item = (MethodCall)expression.Statement;
                switch (item.Identifier.Text)
                {
                    case "ToLower":
                        return new SqlExpressions.Functions.Lower(Visit(expression.Previous));
                    case "ToUpper":
                        return new SqlExpressions.Functions.Upper(Visit(expression.Previous));
                    case "StartsWith":
                        return new Like(
                            Visit(expression.Previous),
                                new BinaryExpression(BinaryExpressionType.Plus,
                                    item.Parameters[0],
                                    percent));
                    case "EndsWith":
                        return new Like(
                            Visit(expression.Previous),
                            new BinaryExpression(BinaryExpressionType.Plus,
                                percent,
                                item.Parameters[0]
                                ));
                    case "Contains":
                        return new Like(
                            Visit(expression.Previous),
                            new BinaryExpression(BinaryExpressionType.Plus,
                                percent,
                                new BinaryExpression(BinaryExpressionType.Plus,
                                    item.Parameters[0],
                                    percent)));
                }
            }
            return base.Visit(expression);
        }
    }
}
