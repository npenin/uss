using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.CommonVisitors;
using System.Diagnostics;

namespace Evaluant.Uss.NLinqVisitors
{
    public class NLinqExpressionVisitor : NLinqExpressionVisitor<NLinqExpressionUpdater>
    {
        public NLinqExpressionVisitor() : base(new NLinqExpressionUpdater()) { }
    }

    [DebuggerStepThrough]
    public class NLinqExpressionVisitor<T> : ExpressionVisitor<T>, INLinqVisitor
        where T : NLinqExpressionUpdater
    {
        public NLinqExpressionVisitor(T updater) : base(updater) { }

        public override Expression Visit(Expression exp)
        {
            if (exp == null)
                return null;
            switch (exp.ExpressionType)
            {
                case ExpressionTypes.MemberAccess:
                    return Visit((MemberExpression)exp);
                case ExpressionTypes.Unary:
                    return Visit((UnaryExpression)exp);
                case ExpressionTypes.Ternary:
                    return Visit((TernaryExpression)exp);
                case ExpressionTypes.TypedNew:
                    return Visit((TypedNew)exp);
                case ExpressionTypes.AnonymousNew:
                    return Visit((AnonymousNew)exp);
                case ExpressionTypes.Quote:
                    return Visit((QueryExpression)exp);
                case ExpressionTypes.Binary:
                    return Visit((BinaryExpression)exp);
                case ExpressionTypes.Constant:
                    return Visit((ValueExpression)exp);
                case ExpressionTypes.Call:
                    return Visit((MethodCall)exp);
                case ExpressionTypes.AnonymousParameter:
                    return Visit((AnonymousParameter)exp);
                case ExpressionTypes.Parameter:
                    return Visit((Parameter)exp);
                case ExpressionTypes.Indexer:
                    return Visit((Indexer)exp);
                case ExpressionTypes.Identifier:
                    return Visit((Identifier)exp);
            }
            return base.Visit(exp);
        }

        public virtual QueryBodyClause Visit(FromClause expression)
        {
            Identifier identifier = (Identifier)Visit(expression.Identifier);
            Expression exp = expression.Expression;
            if (exp != null)
                exp = Visit(expression.Expression);
            return updater.Update(expression, identifier, exp, expression.Type);
        }

        public virtual QueryBodyClause Visit(JoinClause expression)
        {
            Expression eq = Visit(expression.Eq);
            Identifier identifier = (Identifier)Visit(expression.Identifier);
            Expression inIdentifier = Visit(expression.InIdentifier);
            Identifier into = (Identifier)Visit(expression.Into);
            Expression on = Visit(expression.On);
            return updater.Update(expression, eq, identifier, inIdentifier, into, on, expression.Type);

        }




        public virtual QueryBodyClause Visit(LetClause expression)
        {
            Identifier left = (Identifier)Visit(expression.Left);
            Expression right = Visit(expression.Right);
            return updater.Update(expression, left, right);
        }
        public virtual Expression Visit(AnonymousNew expression)
        {
            IEnumerable<AnonymousParameter> parameters = VisitAnonymousParameters(expression.Parameters);
            return updater.Update(expression, parameters, expression.Type);
        }

        public virtual QueryBody Visit(QueryBody expression)
        {
            IEnumerable<QueryBodyClause> clauses = VisitEnumerable(expression.Clauses, Visit);
            QueryContinuation continuation = Visit(expression.Continuation);
            SelectOrGroupClause sogc = Visit(expression.SelectOrGroup);
            return updater.Update(expression, clauses, continuation, sogc);
        }

        public SelectOrGroupClause Visit(SelectOrGroupClause selectOrGroupClause)
        {
            if (selectOrGroupClause is SelectClause)
                return Visit((SelectClause)selectOrGroupClause);
            if (selectOrGroupClause is GroupClause)
                return Visit((GroupClause)selectOrGroupClause);
            throw new NotSupportedException();

        }
        public virtual QueryBodyClause Visit(QueryBodyClause expression)
        {
            if (expression is FromClause)
                return Visit((FromClause)expression);
            if (expression is JoinClause)
                return Visit((JoinClause)expression);
            if (expression is LetClause)
                return Visit((LetClause)expression);
            if (expression is OrderByClause)
                return Visit((OrderByClause)expression);
            if (expression is WhereClause)
                return Visit((WhereClause)expression);
            throw new NotSupportedException();

        }
        public virtual QueryContinuation Visit(QueryContinuation expression)
        {
            if (expression == null)
                return expression;
            Identifier identifier = (Identifier)Visit(expression.Identifier);
            QueryBody body = Visit(expression.QueryBody);
            return updater.Update(expression, identifier, body);
        }
        public virtual Expression Visit(QueryExpression expression)
        {
            FromClause from = (FromClause)Visit(expression.From);
            QueryBody body = Visit(expression.QueryBody);
            return updater.Update(expression, from, body);
        }


        public virtual SelectOrGroupClause Visit(GroupClause expression)
        {
            Expression exp = Visit(expression.Expression);
            Identifier identifier = (Identifier)Visit(expression.Identifier);
            return updater.Update(expression, exp, identifier);
        }
        public virtual SelectOrGroupClause Visit(SelectClause expression)
        {
            Expression exp = Visit(expression.Expression);
            return updater.Update(expression, exp);
        }
        public virtual QueryBodyClause Visit(WhereClause expression)
        {
            if (expression == null)
                return null;
            Expression exp = Visit(expression.Expression);
            if (exp == null)
                return null;
            return updater.Update(expression, exp);
        }
        public virtual Expression Visit(Identifier identifier)
        {
            return VisitIdentifier(identifier);
        }
        public virtual Expression Visit(Indexer expression)
        {
            Expression inner = Visit(expression.InnerExpression);
            Expression parameter = Visit(expression.Parameter);
            return updater.Update(expression, inner, parameter);
        }
        public virtual Expression Visit(MemberExpression expression)
        {
            Expression previous = expression.Previous;
            if (previous != null)
                previous = Visit(previous);
            Expression statement = Visit(expression.Statement);
            return updater.Update(expression, previous, statement);
        }
        public virtual Expression Visit(Parameter parameter)
        {
            return VisitParameter(parameter);
        }

        #region IVisitor<AnonymousParameter,Expression> Members

        public virtual Expression Visit(AnonymousParameter item)
        {
            return base.VisitAnonymousParameter(item);
        }

        #endregion

        #region IVisitor<BinaryExpression,Expression> Members

        public virtual Expression Visit(BinaryExpression item)
        {
            return VisitBinary(item);
        }

        #endregion

        #region IVisitor<MethodCall,Expression> Members

        public virtual Expression Visit(MethodCall item)
        {
            return VisitMethodCall(item);
        }

        #endregion

        #region IVisitor<OrderByClause,Expression> Members

        public virtual QueryBodyClause Visit(OrderByClause item)
        {
            return VisitOrderBy(item);
        }

        #endregion

        #region IVisitor<OrderByCriteria,Expression> Members

        public virtual OrderByCriteria Visit(OrderByCriteria item)
        {
            return item;
        }

        #endregion

        #region IVisitor<TernaryExpression,Expression> Members

        public virtual Expression Visit(TernaryExpression item)
        {
            return VisitTernary(item);
        }

        #endregion

        #region IVisitor<TypedNew,Expression> Members

        public virtual Expression Visit(TypedNew item)
        {
            return VisitNew(item);
        }

        #endregion

        #region IVisitor<UnaryExpression,Expression> Members

        public virtual Expression Visit(UnaryExpression item)
        {
            return VisitUnary(item);
        }

        #endregion

        #region IVisitor<ValueExpression,Expression> Members

        public virtual Expression Visit(ValueExpression item)
        {
            return item;
        }

        #endregion
    }
}
