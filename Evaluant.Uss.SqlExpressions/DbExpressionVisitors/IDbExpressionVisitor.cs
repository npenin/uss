using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.CommonVisitors;
using Evaluant.Uss.SqlExpressions;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.NLinqImprovements;
using Evaluant.Uss.Domain;
using Evaluant.Uss.SqlExpressions.Statements;
using Evaluant.Uss.SqlExpressions.Functions;

namespace Evaluant.Uss.SqlExpressions.Visitors
{
    public interface IDbExpressionVisitor :
        IVisitor<Evaluant.Uss.SqlExpressions.FromClause, Evaluant.Uss.SqlExpressions.FromClause>,
        IVisitor<RowNumber, IAliasedExpression>,
        IVisitor<Aggregate, IAliasedExpression>,
        IVisitor<Cast, IAliasedExpression>,
        IVisitor<Exists, IAliasedExpression>,
        IVisitor<InPredicate, IAliasedExpression>,
        IVisitor<IsNull, IAliasedExpression>,
        IVisitor<Like, IAliasedExpression>,
        IVisitor<SelectStatement, IAliasedExpression>,
        IVisitor<InsertStatement, IDbStatement>,
        IVisitor<DeleteStatement, IDbStatement>,
        IVisitor<UpdateStatement, IDbStatement>,
        IVisitor<CreateTableStatement, IDbStatement>,
        IVisitor<SchemaStatement, IDbStatement>,
        IVisitor<DropTableStatement, IDbStatement>,
        IVisitor<IfStatement, IDbStatement>,
        IVisitor<CaseExpression, IAliasedExpression>,
        IVisitor<ColumnExpression, IAliasedExpression>,
        IVisitor<ComplexColumnExpression, IAliasedExpression>,
        IVisitor<Constant, IDbExpression>,
        IVisitor<JoinedTableExpression, IAliasedExpression>,
        IVisitor<Parameter, Expression>,
        IVisitor<TableSourceExpression, IAliasedExpression>,
        IVisitor<Mapping.EntitySourceExpression, IAliasedExpression>,
        IVisitor<TableAlias>,
        IVisitor<Mapping.LazyTableAlias, TableAlias>,
        IVisitor<Function, IAliasedExpression>,
        IVisitor<OrderByClause, QueryBodyClause>,
        IVisitor<IAliasedExpression>,
        IVisitor<EntityExpression, IDbExpression>,
        //IVisitor<PropertyReferenceExpression, AliasedExpression>,
        //IVisitor<Evaluant.Uss.NLinqImprovements.MethodCall, Expression>,
        IVisitor<EntityReferenceExpression, IDbExpression>,
        IVisitor<EntityIdentifier, Identifier>,
        IVisitor<TupleExpression, IDbExpression>,
        IVisitor<AlterTableStatement>,
        IVisitor<AlterTableColumnStatement>,
        IVisitor<AlterTableAddStatement>,
        IVisitor<HardCodedExpression, IDbExpression>,
        IVisitor<Not, IAliasedExpression>,
        IVisitor<Exec, IAliasedExpression>,
        IVisitor<Lower, IAliasedExpression>,
        IVisitor<Upper, IAliasedExpression>,
        IVisitor<DatePart, IAliasedExpression>,
        IVisitor<DateAdd, IAliasedExpression>
    {
    }
}
