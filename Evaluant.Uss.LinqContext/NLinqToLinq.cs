using System;
using Evaluant.NLinq.Expressions;
using Linq = System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Evaluant.Uss.LinqContext
{
    public class NLinqToLinq : NLinqVisitors.NLinqExpressionVisitor
    {
        static NLinqToLinq()
        {
            SelectMany = typeof(Queryable).GetMethods().Where(m => m.Name == "SelectMany" && m.GetParameters().Count() == 2).First();
            Where = typeof(Queryable).GetMethods().Where(m => m.Name == "Where" && m.GetParameters().Count() == 2).First();

        }

        private static MethodInfo SelectMany;
        private static MethodInfo Where;

        Dictionary<string, NLinq.Expressions.Identifier> identifiers = new Dictionary<string, NLinq.Expressions.Identifier>();
        Linq.Expression result;

        bool inQueryBody;

        public override QueryBodyClause Visit(FromClause expression)
        {
            if (!inQueryBody)
            {
                Visit(expression.Expression);
                identifiers.Add(expression.Identifier.Text, expression.Identifier);
            }
            else
            {
                var oldResult = result;
                Visit(expression.Expression);
                identifiers.Add(expression.Identifier.Text, expression.Identifier);
                result = Linq.Expression.Call(null, SelectMany, oldResult, Linq.Expression.Quote(result));
            }
            return expression;
        }

        public override QueryBodyClause Visit(WhereClause expression)
        {
            var query=result;
            Visit(expression.Expression);
            
            return expression;
        }
    }
}
