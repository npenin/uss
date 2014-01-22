using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.CommonVisitors;
using Evaluant.Uss.SqlExpressions;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.NLinqVisitors;
using Evaluant.Uss.NLinqImprovements;
using Evaluant.Uss.SqlExpressions.Mapping;
using System.Diagnostics;
using Evaluant.Uss.SqlExpressions.Statements;
using Evaluant.Uss.SqlExpressions.Functions;

namespace Evaluant.Uss.SqlExpressions.Visitors
{
    [DebuggerStepThrough]
    public class DbExpressionVisitor : NLinqExpressionVisitor<DbExpressionUpdater>, IDbExpressionVisitor
    {
        public DbExpressionVisitor() : base(new DbExpressionUpdater()) { }

        protected virtual IDbExpression Visit(IDbExpression exp)
        {
            switch (exp.DbExpressionType)
            {
                case DbExpressionType.Table:
                    return Visit((TableExpression)exp);
                case DbExpressionType.Case:
                    return Visit((CaseExpression)exp);
                case DbExpressionType.ClientJoin:
                    throw new NotSupportedException();
                case DbExpressionType.Column:
                    return Visit((ColumnExpression)exp);
                case DbExpressionType.Select:
                    return Visit((SelectStatement)exp);
                case DbExpressionType.Projection:
                    return Visit((TupleExpression)exp);
                case DbExpressionType.Entity:
                    if (exp is EntityExpression)
                        return Visit((EntityExpression)exp);
                    if (exp is EntitySourceExpression)
                        return Visit((EntitySourceExpression)exp);
                    if (exp is EntityReferenceExpression)
                        return Visit((EntityReferenceExpression)exp);
                    break;
                case DbExpressionType.Join:
                    return Visit((JoinedTableExpression)exp);
                case DbExpressionType.Aggregate:
                    return Visit((Aggregate)exp);
                case DbExpressionType.Scalar:
                    break;
                case DbExpressionType.Exists:
                    return Visit((Exists)exp);
                case DbExpressionType.In:
                    return Visit((InPredicate)exp);
                case DbExpressionType.Grouping:
                    break;
                case DbExpressionType.AggregateSubquery:
                    break;
                case DbExpressionType.IsNull:
                    return Visit((IsNull)exp);
                case DbExpressionType.Between:
                    break;
                    break;
                case DbExpressionType.NamedValue:
                    break;
                case DbExpressionType.OuterJoined:
                    break;
                case DbExpressionType.Insert:
                    return Visit((InsertStatement)exp);
                case DbExpressionType.Update:
                    return Visit((UpdateStatement)exp);
                case DbExpressionType.Upsert:
                    break;
                case DbExpressionType.Delete:
                    return Visit((DeleteStatement)exp);
                case DbExpressionType.If:
                    return Visit((IfStatement)exp);
                case DbExpressionType.Function:
                    return Visit((Function)exp);
                case DbExpressionType.Not:
                    return Visit((Not)exp);
                case DbExpressionType.Value:
                    return Visit((Constant)exp);
                case DbExpressionType.Parameter:
                    return Visit((Evaluant.Uss.SqlExpressions.DbParameter)exp);
                case DbExpressionType.Drop:
                    return Visit((DropTableStatement)exp);
                case DbExpressionType.Create:
                    return Visit((CreateTableStatement)exp);
                case DbExpressionType.Alter:
                    return Visit((Statements.AlterTableStatement)exp);
                case DbExpressionType.HardCoded:
                    return Visit((HardCodedExpression)exp);
                case DbExpressionType.Schema:
                    return Visit((SchemaStatement)exp);
            }
            return (IDbExpression)VisitUnknown((Expression)exp);
        }

        public virtual Identifier Visit(EntityIdentifier identifier)
        {
            return updater.Update(identifier, Visit(identifier.Entity) as EntityExpression, identifier.Identifier);
        }

        public override Expression Visit(Identifier identifier)
        {
            if (identifier is EntityIdentifier)
                return Visit((EntityIdentifier)identifier);
            return base.Visit(identifier);
        }

        public override Expression Visit(Expression exp)
        {
            this.visiting = exp;
            if (exp is IDbExpression)
                return (Expression)Visit((IDbExpression)exp);
            return base.Visit(exp);
        }
        #region IVisitor<Function,Function> Members

        public virtual IAliasedExpression Visit(Function item)
        {
            switch (item.FunctionType)
            {
                case FunctionType.RowNumber:
                    return Visit((RowNumber)item);
                case FunctionType.Cast:
                    return Visit((Cast)item);
                case FunctionType.Aggregate:
                    return Visit((Aggregate)item);
                case FunctionType.Exists:
                    return Visit((Exists)item);
                case FunctionType.In:
                    return Visit((InPredicate)item);
                case FunctionType.IsNull:
                    return Visit((IsNull)item);
                case FunctionType.Like:
                    return Visit((Like)item);
                case FunctionType.Exec:
                    return Visit((Exec)item);
                default:
                    throw new NotSupportedException(string.Format("The function '{0}' is not supported", item.GetType().FullName));
            }
        }

        #endregion

        #region IVisitor<FromClause,FromClause> Members

        public virtual Evaluant.Uss.SqlExpressions.FromClause Visit(Evaluant.Uss.SqlExpressions.FromClause item)
        {
            if (item == null)
                return null;
            if (item.Count == 0)
                return item;
            IAliasedExpression[] original_froms = new IAliasedExpression[item.Count];
            item.CopyTo(original_froms, 0);
            IAliasedExpression[] froms = VisitArray(original_froms, Visit);
            return updater.Update(item, original_froms, froms);
        }

        #endregion

        #region IVisitor<Aggregate,Expression> Members

        public virtual IAliasedExpression Visit(Aggregate item)
        {
            if (item.MethodName == Aggregate.Count)
            {
                Identifier methodName = (Identifier)Visit(item.MethodName);
                Expression[] parameters = VisitArray(item.Parameters, Visit);
                return updater.Update(item, methodName, parameters);
            }
            else
                throw new NotImplementedException();
        }

        #endregion

        #region IVisitor<Cast,Expression> Members

        public virtual IAliasedExpression Visit(Cast item)
        {
            return Visit((Function)item);
        }

        #endregion

        #region IVisitor<Exists,Expression> Members

        public virtual IAliasedExpression Visit(Exists item)
        {
            var alias = Visit(item.Alias);
            var parameters = VisitArray(item.Parameters, Visit);
            return updater.Update(item, alias, parameters);

        }

        #endregion

        #region IVisitor<InPredicate,Expression> Members

        public virtual IAliasedExpression Visit(InPredicate item)
        {
            return Visit((Function)item);
        }

        #endregion

        #region IVisitor<IsNull,Expression> Members

        public virtual IAliasedExpression Visit(IsNull item)
        {
            return Visit((Function)item);
        }

        #endregion

        #region IVisitor<Like,Expression> Members

        public virtual IAliasedExpression Visit(Like item)
        {
            return updater.Update(item, VisitArray(item.Parameters, Visit));
        }

        #endregion

        #region IVisitor<SelectStatement,Expression> Members

        public virtual IAliasedExpression Visit(SelectStatement item)
        {
            Evaluant.Uss.SqlExpressions.FromClause from = Visit(item.From);
            IEnumerable<IAliasedExpression> columns = VisitEnumerable(item.Columns, Visit);
            OrderByClause orderby = VisitOrderBy((OrderByClause)item.OrderBy);
            WhereClause where = item.Where;
            if (item.Where != null)
                where = (WhereClause)Visit(item.Where);
            return updater.Update(item, columns, from, where, orderby, Visit(item.Alias));
        }

        #endregion

        #region IVisitor<InsertStatement,Expression> Members

        public virtual IDbStatement Visit(InsertStatement item)
        {
            IDbExpression select = Visit(item.Select);
            KeyValuePair<ColumnExpression, NLinq.Expressions.Expression>[] original_values = new KeyValuePair<ColumnExpression, NLinq.Expressions.Expression>[item.Values.Count];
            item.Values.CopyTo(original_values, 0);
            KeyValuePair<ColumnExpression, NLinq.Expressions.Expression>[] values = (KeyValuePair<ColumnExpression, NLinq.Expressions.Expression>[])VisitArray(original_values, Visit, new KeyValuePairComparer<ColumnExpression, NLinq.Expressions.Expression>());
            return updater.Update(item, item.Table, original_values, values, select);
        }

        public virtual KeyValuePair<ColumnExpression, NLinq.Expressions.Expression> Visit(KeyValuePair<ColumnExpression, NLinq.Expressions.Expression> item)
        {
            IAliasedExpression column = Visit(item.Key);
            Expression value = Visit(item.Value);
            return updater.Update(item, column, value);
        }

        #endregion

        #region IVisitor<DeleteStatement,Expression> Members

        public virtual IDbStatement Visit(DeleteStatement item)
        {
            WhereClause where = (WhereClause)Visit(item.Where);
            Evaluant.Uss.SqlExpressions.FromClause from = Visit(item.From);
            return updater.Update(item, from, where);
        }

        #endregion

        #region IVisitor<CaseExpression,Expression> Members

        public virtual IAliasedExpression Visit(CaseExpression item)
        {
            IDbExpression exp = Visit(item.DefaultResult);
            CaseTestExpression[] tests = (CaseTestExpression[])VisitArray(item.CaseTests, Visit);
            Expression test = null;
            if (item.Test != null)
                test = Visit(test);
            return updater.Update(item, test, exp, tests, item.Alias);
        }

        public virtual CaseTestExpression Visit(CaseTestExpression test)
        {
            Expression exp = Visit(test.TestExpression);
            Expression result = Visit(test.TestResult);
            return updater.Update(test, exp, result);
        }

        #endregion

        #region IVisitor<ColumnExpression,AliasedExpression> Members

        public virtual IAliasedExpression Visit(ColumnExpression item)
        {
            if (item is ComplexColumnExpression)
                return Visit((ComplexColumnExpression)item);
            TableAlias alias = item.Alias;
            if (alias != null)
                alias = Visit(alias);
            return updater.Update(item, alias, item.ColumnName);
        }

        #endregion

        #region IVisitor<Constant,Expression> Members

        public virtual IDbExpression Visit(Constant item)
        {
            return item;
        }

        #endregion

        #region IVisitor<JoinedTableExpression,Expression> Members

        public virtual IAliasedExpression Visit(JoinedTableExpression item)
        {
            IAliasedExpression leftTable = Visit(item.LeftTable);
            IAliasedExpression rightTable = Visit(item.RightTable);
            BinaryExpression on = (BinaryExpression)Visit(item.On);
            return updater.Update(item, leftTable, rightTable, item.JoinType, item.Alias, on);
        }

        #endregion

        #region IVisitor<Parameter,Expression> Members

        public virtual IDbExpression Visit(Evaluant.Uss.SqlExpressions.DbParameter parameter)
        {
            return parameter;
        }

        #endregion

        #region IVisitor<TableSourceExpression,Expression> Members

        public virtual IAliasedExpression Visit(TableSourceExpression item)
        {
            TableAlias alias = Visit(item.Alias);
            return updater.Update(item, alias, item.Table);
        }

        #endregion

        #region IVisitor<TableAlias,TableAlias> Members

        public virtual TableAlias Visit(TableAlias item)
        {
            if (item is Mapping.LazyTableAlias)
                return Visit((Mapping.LazyTableAlias)item);
            return item;
        }

        #endregion

        #region IVisitor<TableExpression,TableExpression> Members

        public virtual IAliasedExpression Visit(IAliasedExpression item)
        {
            if (item is Mapping.EntitySourceExpression)
                return Visit((Mapping.EntitySourceExpression)item);
            if (item is TableSourceExpression)
                return Visit((TableSourceExpression)item);
            if (item is JoinedTableExpression)
                return Visit((JoinedTableExpression)item);
            if (item is ColumnExpression)
                return Visit((ColumnExpression)item);
            if (item is EntityReferenceExpression)
                return (IAliasedExpression)Visit((EntityReferenceExpression)item);
            if (item is EntityExpression)
                return (IAliasedExpression)Visit((EntityExpression)item);
            if (item is SelectStatement)
                return Visit((SelectStatement)item);
            if (item is Aggregate)
                return Visit((Aggregate)item);
            if (item is CaseExpression)
                return Visit((CaseExpression)item);
            return (IAliasedExpression)VisitUnknown((Expression)item);
        }

        #endregion

        #region IVisitor<EntityExpression,Expression> Members

        public virtual IDbExpression Visit(EntityExpression item)
        {
            return updater.Update(item, Visit(item.Expression), Visit(item.Alias), item.Type);
        }

        #endregion

        #region IVisitor<EntityReferenceExpression,Expression> Members

        public virtual IDbExpression Visit(EntityReferenceExpression item)
        {
            IDbExpression target = Visit((IDbExpression)item.Target);
            if (target as IAliasedExpression == null)
                return target;
            return updater.Update(item, (IAliasedExpression)target);

        }

        #endregion

        #region IVisitor<UpdateStatement,Expression> Members

        public virtual IDbStatement Visit(UpdateStatement item)
        {
            WhereClause where = (WhereClause)Visit(item.Where);
            Evaluant.Uss.SqlExpressions.FromClause from = Visit(item.From);
            VisitEnumerable(item.Set, Visit);
            return item;
        }

        public virtual KeyValuePair<ColumnExpression, DbParameter> Visit(KeyValuePair<ColumnExpression, DbParameter> sets)
        {
            IAliasedExpression key = Visit(sets.Key);
            IDbExpression value = Visit(sets.Value);
            if (sets.Key != key || sets.Value != value)
                return new KeyValuePair<ColumnExpression, DbParameter>((ColumnExpression)key, (DbParameter)value);
            return sets;
        }

        #endregion

        #region IVisitor<DropTableStatement,Expression> Members

        public virtual IDbStatement Visit(DropTableStatement item)
        {
            return item;
        }

        #endregion

        #region IVisitor<DropTableIfExistsStatement,Expression> Members

        public virtual IDbStatement Visit(Statements.IfStatement item)
        {
            return item;
        }

        #endregion

        #region IVisitor<EntitySourceExpression,AliasedExpression> Members

        public virtual IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.Mapping.EntitySourceExpression item)
        {
            return item;
        }

        #endregion

        #region IVisitor<LazyTableAlias,TableAlias> Members

        public virtual TableAlias Visit(Evaluant.Uss.SqlExpressions.Mapping.LazyTableAlias item)
        {
            return item;
        }

        #endregion

        #region IVisitor<CreateTableStatement,Expression> Members

        public virtual IDbStatement Visit(CreateTableStatement item)
        {
            return item;
        }

        #endregion

        #region IVisitor<ComplexColumnExpression,AliasedExpression> Members

        public virtual IAliasedExpression Visit(ComplexColumnExpression item)
        {
            TableAlias alias = item.Alias;
            if (item.Alias != null)
                alias = Visit(item.Alias);
            Expression exp = Visit(item.Expression);
            Identifier columnAlias = (Identifier)Visit(item.ColumnAlias);
            return updater.Update(item, alias, exp, columnAlias);
        }

        #endregion

        #region IVisitor<TupleExpression,Expression> Members

        public virtual IDbExpression Visit(TupleExpression item)
        {
            IEnumerable<Expression> expressions = VisitEnumerable(item, Visit);
            return updater.Update(item, expressions);
        }

        #endregion

        #region IVisitor<AlterTableStatement> Members

        public virtual Statements.AlterTableStatement Visit(Statements.AlterTableStatement item)
        {
            switch (item.AlterMode)
            {
                case Evaluant.Uss.SqlExpressions.Statements.AlterMode.Column:
                    return Visit((Statements.AlterTableColumnStatement)item);
                case Evaluant.Uss.SqlExpressions.Statements.AlterMode.Add:
                    return Visit((Statements.AlterTableAddStatement)item);
                case Evaluant.Uss.SqlExpressions.Statements.AlterMode.Drop:
                    return Visit((Statements.AlterTableDropStatement)item);
                case Evaluant.Uss.SqlExpressions.Statements.AlterMode.Check:
                case Evaluant.Uss.SqlExpressions.Statements.AlterMode.Uncheck:
                case Evaluant.Uss.SqlExpressions.Statements.AlterMode.Enable:
                case Evaluant.Uss.SqlExpressions.Statements.AlterMode.Disable:
                case Evaluant.Uss.SqlExpressions.Statements.AlterMode.Switch:
                case Evaluant.Uss.SqlExpressions.Statements.AlterMode.Set:
                case Evaluant.Uss.SqlExpressions.Statements.AlterMode.Rebuilb:
                default:
                    throw new NotSupportedException();
            }
        }

        #endregion

        #region IVisitor<AlterTableColumnStatement> Members

        public virtual Statements.AlterTableColumnStatement Visit(Statements.AlterTableColumnStatement item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVisitor<AlterTableAddStatement> Members

        public virtual Statements.AlterTableAddStatement Visit(Statements.AlterTableAddStatement item)
        {
            return item;
        }

        public virtual Statements.AlterTableDropStatement Visit(Statements.AlterTableDropStatement item)
        {
            return item;
        }

        #endregion

        #region IVisitor<RowNumber,AliasedExpression> Members

        public virtual IAliasedExpression Visit(RowNumber item)
        {
            OrderByClause order = (OrderByClause)Visit(item.OverOrder);
            var partition = VisitEnumerable(item.PartitionBy, Visit);
            return updater.Update(item, order);
        }

        #endregion

        public virtual IDbStatement Visit(SchemaStatement item)
        {
            return item;
        }

        public virtual IDbExpression Visit(HardCodedExpression item)
        {
            return item;
        }

        public virtual IAliasedExpression Visit(Not item)
        {
            return updater.Update(item, Visit(item.Expression));
        }

        public virtual Function Visit(Exec item)
        {
            return updater.Update(item, VisitArray(item.Parameters, Visit));
        }
    }
}
