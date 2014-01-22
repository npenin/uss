using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.CommonVisitors;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.NLinqVisitors
{
    public interface INLinqVisitor :
        IVisitor<AnonymousNew, Expression>,
        IVisitor<AnonymousParameter,Expression>,
        IVisitor<BinaryExpression, Expression>,
        IVisitor<FromClause, QueryBodyClause>,
        IVisitor<GroupClause, SelectOrGroupClause>,
        IVisitor<Identifier, Expression>,
        IVisitor<Indexer, Expression>,
        IVisitor<JoinClause, QueryBodyClause>,
        IVisitor<LetClause, QueryBodyClause>,
        IVisitor<MethodCall, Expression>,
        IVisitor<Expression>,
        IVisitor<OrderByClause, QueryBodyClause>,
        IVisitor<OrderByCriteria, OrderByCriteria>,
        IVisitor<Parameter, Expression>,
        IVisitor<QueryBody>,
        IVisitor<QueryBodyClause, QueryBodyClause>,
        IVisitor<QueryContinuation>,
        IVisitor<QueryExpression, Expression>,
        IVisitor<SelectClause, SelectOrGroupClause>,
        IVisitor<SelectOrGroupClause, SelectOrGroupClause>,
        IVisitor<MemberExpression, Expression>,
        IVisitor<TernaryExpression, Expression>,
        IVisitor<TypedNew, Expression>,
        IVisitor<UnaryExpression, Expression>,
        IVisitor<ValueExpression, Expression>,
        IVisitor<WhereClause, QueryBodyClause>
    {
    }
}
