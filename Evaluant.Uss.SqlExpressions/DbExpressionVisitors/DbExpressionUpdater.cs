using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.CommonVisitors;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.NLinqVisitors;
using Evaluant.Uss.NLinqImprovements;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions.Visitors
{
    public class DbExpressionUpdater : NLinqExpressionUpdater
    {
        public virtual FromClause Update(FromClause item, IAliasedExpression[] originalFroms, IEnumerable<IAliasedExpression> froms)
        {
            if (originalFroms != froms)
                return new FromClause(froms);
            return item;
        }

        public virtual IAliasedExpression Update(TableSourceExpression original, TableAlias alias, Mapping.Table table)
        {
            if (original.Alias != alias || original.Table != table)
                return new TableSourceExpression(alias, table);
            return original;
        }

        public virtual IAliasedExpression Update(JoinedTableExpression item, IAliasedExpression leftTable, IAliasedExpression rightTable, JoinType joinType, TableAlias alias, Evaluant.NLinq.Expressions.BinaryExpression on)
        {
            if (item.LeftTable != leftTable || item.RightTable != rightTable || item.JoinType != joinType || item.Alias != alias)
                return new JoinedTableExpression(leftTable, rightTable, joinType, alias, on);
            return item;
        }

        public virtual CaseTestExpression Update(CaseTestExpression test, Evaluant.NLinq.Expressions.Expression exp, Evaluant.NLinq.Expressions.Expression result)
        {
            if (test.TestExpression != exp || test.TestResult != result)
                return new CaseTestExpression(exp, result);
            return test;
        }

        public virtual IAliasedExpression Update(CaseExpression item, Evaluant.NLinq.Expressions.Expression test, IDbExpression result, CaseTestExpression[] tests, TableAlias alias)
        {
            if (item.Test != test || item.DefaultResult != result || item.CaseTests != tests || item.Alias != alias)
                return new CaseExpression(alias, (IDbExpression)test, (IDbExpression)result, tests);
            return item;
        }

        public virtual IDbStatement Update(DeleteStatement item, FromClause from, Evaluant.NLinq.Expressions.WhereClause where)
        {
            if (item.Where != where)
                return new DeleteStatement(from, where);
            return item;
        }

        public virtual KeyValuePair<ColumnExpression, NLinq.Expressions.Expression> Update(KeyValuePair<ColumnExpression, NLinq.Expressions.Expression> item, IAliasedExpression column, Evaluant.NLinq.Expressions.Expression value)
        {
            if (item.Key != column || item.Value != value)
                return new KeyValuePair<ColumnExpression, NLinq.Expressions.Expression>((ColumnExpression)column, value);
            return item;
        }

        public virtual InsertStatement Update(InsertStatement item, TableExpression table, KeyValuePair<ColumnExpression, NLinq.Expressions.Expression>[] original_values, KeyValuePair<ColumnExpression, NLinq.Expressions.Expression>[] values, IDbExpression select)
        {
            if (item.Select != select)
                return new InsertStatement(table, (SelectStatement)select);
            if (original_values != values)
            {
                IDictionary<ColumnExpression, NLinq.Expressions.Expression> dictValues = new Dictionary<ColumnExpression, NLinq.Expressions.Expression>();
                foreach (KeyValuePair<ColumnExpression, NLinq.Expressions.Expression> kvp in values)
                    dictValues.Add(kvp);
                return new InsertStatement(table, dictValues);
            }
            return item;
        }

        public virtual IAliasedExpression Update(SelectStatement item, IEnumerable<IAliasedExpression> columns, FromClause from, Evaluant.NLinq.Expressions.WhereClause where, Evaluant.NLinq.Expressions.OrderByClause orderby, TableAlias alias)
        {
            if (item.Alias != alias || item.Columns != columns || item.From != from || item.OrderBy != orderby || item.Where != where)
                return new SelectStatement(alias, columns, from, orderby, where) { Top = item.Top, Distinct = item.Distinct };
            return item;
        }

        public virtual Like Update(Like item, Evaluant.NLinq.Expressions.Expression[] expressions)
        {
            if (item.Parameters != expressions)
                return new Like(expressions[0], expressions[1]);
            return item;
        }

        public virtual EntityExpression Update(EntityExpression item, Evaluant.NLinq.Expressions.Expression exp, TableAlias alias, string type)
        {
            if (item.Alias != alias || item.Type != type || item.Expression != exp)
                return new EntityExpression(alias) { Type = type, Expression = exp };
            return item;
        }

        //public virtual MethodCall Update(MethodCall call, Evaluant.NLinq.Expressions.Expression target, Evaluant.NLinq.Expressions.Identifier methodName, Evaluant.NLinq.Expressions.Expression[] parameters)
        //{
        //    if (call.Target != target || call.MethodName != methodName || call.Parameters != parameters)
        //        return new MethodCall(target, methodName, parameters);
        //    return call;
        //}

        //public virtual PropertyReferenceExpression Update(PropertyReferenceExpression item, AliasedExpression target, Evaluant.NLinq.Expressions.Identifier propertyName)
        //{
        //    return Update(item, item.Alias, item.Target, item.PropertyName);
        //}

        //public virtual PropertyReferenceExpression Update(PropertyReferenceExpression item, TableAlias alias, AliasedExpression target, Evaluant.NLinq.Expressions.Identifier propertyName)
        //{
        //    if (item.Alias != alias || item.PropertyName != propertyName || item.Target != target)
        //        return new PropertyReferenceExpression(alias, target, propertyName);
        //    return item;
        //}

        //public virtual MethodCall Update(MethodCall item, Evaluant.NLinq.Expressions.Expression target, Evaluant.NLinq.Expressions.Expression[] parameters, Evaluant.NLinq.Expressions.Identifier methodName)
        //{
        //    if (item.MethodName != methodName || item.Parameters != parameters || item.Target != target)
        //        return new MethodCall(target, methodName, parameters);
        //    return item;
        //}

        public virtual EntityReferenceExpression Update(EntityReferenceExpression item, IAliasedExpression target)
        {
            if (item.Target != target)
                return new EntityReferenceExpression(target);
            return item;
        }

        public virtual IAliasedExpression Update(Aggregate item, Evaluant.NLinq.Expressions.Identifier methodName, Evaluant.NLinq.Expressions.Expression[] parameters)
        {
            if (item.MethodName != methodName || item.Parameters != parameters)
                return new Aggregate(methodName, parameters);
            return item;
        }

        public virtual IAliasedExpression Update(ColumnExpression item, TableAlias alias, Evaluant.NLinq.Expressions.Identifier columnName)
        {
            if (item.ColumnName != columnName || item.Alias != alias)
                return new ColumnExpression(alias, columnName);
            return item;
        }

        public virtual IAliasedExpression Update(ComplexColumnExpression item, TableAlias alias, Evaluant.NLinq.Expressions.Expression exp, Evaluant.NLinq.Expressions.Identifier columnAlias)
        {
            if (item.Alias != alias || item.Expression != exp || item.ColumnAlias != columnAlias)
                return new ComplexColumnExpression(alias, exp, columnAlias);
            return item;
        }

        public virtual Evaluant.NLinq.Expressions.Identifier Update(EntityIdentifier entityIdentifier, EntityExpression entity, Evaluant.NLinq.Expressions.Identifier identifier)
        {
            if (entityIdentifier.Entity != entity || entityIdentifier.Identifier != identifier)
                return new EntityIdentifier(identifier, entity);
            return entityIdentifier;
        }

        public virtual TupleExpression Update(TupleExpression item, IEnumerable<Evaluant.NLinq.Expressions.Expression> expressions)
        {
            if (item == expressions)
                return item;
            return new TupleExpression(expressions);
        }

        internal IAliasedExpression Update(RowNumber item, NLinq.Expressions.OrderByClause order)
        {
            if (item.OverOrder != order)
                return new RowNumber(order);
            return item;
        }

        internal IAliasedExpression Update(Exists item, TableAlias alias, NLinq.Expressions.Expression[] parameters)
        {
            if (item.Alias != alias || item.Parameters != parameters)
                return new Exists(parameters[0]);
            return item;
        }

        internal IAliasedExpression Update(Not item, IDbExpression expression)
        {
            if (item.Expression != expression)
                return new Not(expression);
            return item;
        }

        internal Function Update(Functions.Exec item, NLinq.Expressions.Expression[] expression)
        {
            if (item.Parameters != expression)
                return new Functions.Exec((IDbStatement)expression[0]);
            return item;
        }
        public virtual Function Update(Functions.Lower item, NLinq.Expressions.Expression[] expression)
        {
            if (item.Parameters != expression)
                return new Functions.Lower(expression[0]);
            return item;
        }
        public virtual Function Update(Functions.Upper item, NLinq.Expressions.Expression[] expression)
        {
            if (item.Parameters != expression)
                return new Functions.Upper(expression[0]);
            return item;
        }
        public virtual Function Update(Functions.DatePart item, NLinq.Expressions.Expression[] expression)
        {
            if (item.Parameters != expression)
                return new Functions.DatePart((Identifier)expression[0], expression[1]);
            return item;
        }
        public virtual Function Update(Functions.DateAdd item, NLinq.Expressions.Expression[] expression)
        {
            if (item.Parameters != expression)
                return new Functions.DateAdd((Identifier)expression[0], expression[1], expression[2]);
            return item;
        }

        internal IAliasedExpression Update(Union item, TableAlias alias, IAliasedExpression[] aliasedExpression)
        {
            if (item.SelectStatements != aliasedExpression || item.Alias != alias)
                return new Union(alias, aliasedExpression);
            return item;
        }
    }
}
