using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.CommonVisitors;

namespace Evaluant.Uss.NLinqVisitors
{
    public class NLinqExpressionUpdater : ExpressionUpdater
    {
        public AnonymousNew Update(AnonymousNew expression, IEnumerable<AnonymousParameter> parameters, string type)
        {
            if (expression.Parameters != parameters || expression.Type != type)
                return new AnonymousNew(type, new AnonymousParameterList(parameters));
            return expression;
        }

        public QueryExpression Update(QueryExpression original, FromClause from, QueryBody body)
        {
            if (from != original.From || body != original.QueryBody)
                return new QueryExpression(from, body);
            return original;
        }

        public JoinClause Update(JoinClause expression, Expression eq, Identifier identifier, Expression inIdentifier, Identifier into, Expression on, string type)
        {
            if (eq != expression.Eq || identifier != expression.Identifier || inIdentifier != expression.InIdentifier || into != expression.Into || on != expression.On || expression.Type != type)
                return new JoinClause(type, identifier, inIdentifier, on, eq, into);
            return expression;
        }

        public FromClause Update(FromClause original, Identifier identifier, Expression exp, string type)
        {
            if (identifier != original.Identifier || exp != original.Expression || original.Type != type)
                return new FromClause(type, identifier, exp);
            return original;
        }

        public LetClause Update(LetClause expression, Identifier left, Expression right)
        {
            if (expression.Left != left || expression.Right != right)
                return new LetClause(left, right);
            return expression;
        }

        public QueryBody Update(QueryBody expression, IEnumerable<QueryBodyClause> clauses, QueryContinuation continuation, SelectOrGroupClause sogc)
        {
            if (expression.Clauses != clauses || expression.Continuation != continuation || expression.SelectOrGroup != sogc)
                return new QueryBody(new ClauseList(clauses), sogc, continuation);
            return expression;
        }

        public QueryContinuation Update(QueryContinuation expression, Identifier identifier, QueryBody body)
        {
            if (expression.Identifier != identifier || expression.QueryBody != body)
                return new QueryContinuation(identifier, body);
            return expression;
        }

        public GroupClause Update(GroupClause expression, Expression exp, Identifier identifier)
        {
            if (expression.Expression != exp || expression.Identifier != identifier)
                return new GroupClause(identifier, exp);
            return expression;
        }

        public SelectClause Update(SelectClause expression, Expression exp)
        {
            if (expression.Expression != exp)
                return new SelectClause(exp);
            return expression;
        }

        public WhereClause Update(WhereClause expression, Expression exp)
        {
            if (expression.Expression != exp)
                return new WhereClause(exp);
            return expression;
        }

        public Indexer Update(Indexer expression, Expression inner, Expression parameter)
        {
            if (expression.InnerExpression != inner || expression.Parameter != parameter)
                return new Indexer(inner, parameter);
            return expression;
        }

        public Expression Update(MemberExpression expression, Expression previous, Expression statement)
        {
            if (expression.Statement != statement || expression.Previous != previous)
                return new MemberExpression(statement, previous);
            return expression;
        }
    }
}
