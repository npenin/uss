using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.NLinqImprovements;
using Evaluant.Uss.SqlExpressions.Functions;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    public class DateOperations : DbExpressionVisitor
    {
        private Mapping.Mapping mapping;

        public DateOperations(Mapping.Mapping mapping)
        {
            this.mapping = mapping;
        }

        bool previousIsDateTime;

        public override Expression Visit(Expression exp)
        {
            previousIsDateTime = false;
            return base.Visit(exp);
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.MemberExpression expression)
        {
            var result = Visit(expression.Previous);
            if (previousIsDateTime)
            {
                if (expression.Statement.ExpressionType == ExpressionTypes.Call)
                {
                    var call = ((MethodCall)expression.Statement);
                    switch (call.Identifier.Text)
                    {
                        case "AddDays":
                            return new DateAdd(DatePart.Day, Visit(call.Parameters[0]), result);
                        case "AddHours":
                            return new DateAdd(DatePart.Hour, Visit(call.Parameters[0]), result);
                        case "AddMilliseconds":
                            return new DateAdd(DatePart.Millisecond, Visit(call.Parameters[0]), result);
                        case "AddMinutes":
                            return new DateAdd(DatePart.Minute, Visit(call.Parameters[0]), result);
                        case "AddMonths":
                            return new DateAdd(DatePart.Month, Visit(call.Parameters[0]), result);
                        case "AddSeconds":
                            return new DateAdd(DatePart.Second, Visit(call.Parameters[0]), result);
                        case "AddTicks":
                            return new DateAdd(DatePart.Microsecond, Visit(call.Parameters[0]), result);
                        case "AddYears":
                            return new DateAdd(DatePart.Year, Visit(call.Parameters[0]), result);
                        default:
                            throw new NotSupportedException();
                    }
                }
                else if (expression.Statement.ExpressionType == ExpressionTypes.Identifier)
                {
                    switch (((Identifier)expression.Statement).Text)
                    {
                        case "Value":
                            //Case of nullable
                            return result;
                        case "Second":
                            return new DatePart(DatePart.Second, result);
                        case "Minute":
                            return new DatePart(DatePart.Minute, result);
                        case "Hour":
                            return new DatePart(DatePart.Hour, result);
                        case "Day":
                            return new DatePart(DatePart.Day, result);
                        case "Month":
                            return new DatePart(DatePart.Month, result);
                        case "Year":
                            return new DatePart(DatePart.Year, result);
                        default:
                            throw new NotSupportedException();
                            break;
                    }
                }
            }
            else if (result != null && result.ExpressionType == ExpressionTypes.Identifier)
            {
                var identifier = result as EntityIdentifier;
                if (identifier != null && expression.Statement.ExpressionType == ExpressionTypes.Identifier)
                {
                    if (identifier.Entity.Type != null)
                    {
                        Mapping.Attribute attribute;
                        if (mapping.Entities[identifier.Entity.Type].Attributes.TryGetValue(((Identifier)expression.Statement).Text, out attribute))
                        {
                            previousIsDateTime = attribute.DbType == DbType.DateTime || attribute.DbType == DbType.DateTime2;
                        }
                    }
                    else if(identifier.Entity.Expression!=null)
                    {
                        if(identifier.Entity.Expression.ExpressionType==ExpressionTypes.AnonymousNew)
                        {
                            foreach(var param in ((AnonymousNew)identifier.Entity.Expression).Parameters)
                            {
                                if (param.Identifier.Text == ((Identifier)expression.Statement).Text)
                                {
                                    Visit(param.Expression);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return expression;
        }
    }
}
