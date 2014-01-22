using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.NLinqImprovements;
using Evaluant.Uss.SqlExpressions.Visitors;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    public class AggregateMutator : DbExpressionVisitor
    {
        private Stack<Function> newAggregates = new Stack<Function>();

        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.MethodCall item)
        {
            if (item.Identifier.Text == "Count")
            {
                if (item.LambdaExpression == null)
                    newAggregates.Push(new Aggregate(AggregateFunctionType.Count));
                else
                    newAggregates.Push(new Aggregate(AggregateFunctionType.Count, Visit(item.LambdaExpression)));
                return newAggregates.Peek();
            }
            if (item.Identifier.Text == "Any")
            {
                if (item.LambdaExpression == null)
                    newAggregates.Push(new Exists(null));
                else
                    newAggregates.Push(new Exists(Visit(item.LambdaExpression)));
                return newAggregates.Peek();
            }
            return base.VisitMethodCall(item);
        }

        //public override MethodCall VisitMethodCall(MethodCall call)
        //{
        //    if (call.MethodName.Text == "Count")
        //    {
        //        newAggregates.Push(new Aggregate(AggregateFunctionType.Count, call.Target));
        //        return newAggregates.Peek();
        //    }
        //    return base.VisitMethodCall(call);
        //}

        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.MemberExpression expression)
        {
            //Evaluant.NLinq.Expressions.Expression previous = null;
            //if (expression.Previous != null)
            //    previous = Visit(expression.Previous);
            if (expression.Statement is NLinq.Expressions.MethodCall)
            {
                NLinq.Expressions.Expression exp = Visit(expression.Statement);
                IDbExpression dbExp = exp as IDbExpression;
                if (dbExp != null && dbExp.DbExpressionType == DbExpressionType.Aggregate)
                    return new SelectStatement(new TableAlias(), new IAliasedExpression[] { new ComplexColumnExpression(null, exp) }, new FromClause(new ComplexColumnExpression(null, Visit(expression.Previous))), null, null);
                return new SelectStatement(new TableAlias(), new IAliasedExpression[] { new ComplexColumnExpression(null, new Constant("true", System.Data.DbType.Boolean)) }, new FromClause(new ComplexColumnExpression(null, Visit(expression.Previous))), null, new NLinq.Expressions.WhereClause(exp));
            }
            else
            {
                NLinq.Expressions.Expression statement = Visit(expression.Statement);
                return updater.Update(expression, expression.Previous, statement);
            }
        }
    }
}
