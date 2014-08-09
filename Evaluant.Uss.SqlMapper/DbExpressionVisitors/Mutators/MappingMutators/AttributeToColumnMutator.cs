using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.NLinqImprovements;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators.MappingMutators
{
    public class AttributeToColumnMutator : DbExpressionVisitor
    {
        Mapping.Mapping mapping;
        Mapping.Entity currentEntity = null;
        TableAlias currentAlias = null;

        public AttributeToColumnMutator(Mapping.Mapping mapping)
        {
            this.mapping = mapping;
        }

        public override Evaluant.NLinq.Expressions.Expression Visit(NLinq.Expressions.MemberExpression item)
        {
            if (item.Statement is Evaluant.NLinq.Expressions.MethodCall)
                return base.Visit(item);
            if (item.Previous is SelectStatement)
            {
                Visit(item.Statement);
                if (item.Statement is Evaluant.NLinq.Expressions.Identifier)
                {
                    ((SelectStatement)item.Previous).Columns = new ColumnExpression[]{
                        new ColumnExpression(((IAliasedExpression)item.Previous).Alias,currentEntity.Attributes[((NLinq.Expressions.Identifier)item.Statement).Text].ColumnName)
                    };
                    return item.Previous;
                }
            }
            Visit(item.Previous);
            Mapping.Attribute attribute;
            if (currentEntity != null && currentEntity.Attributes.TryGetValue(((NLinq.Expressions.Identifier)item.Statement).Text, out attribute))
            {
                if (visitingColumns)
                    return new ColumnExpression(currentAlias, attribute.ColumnName, attribute.Name);
                return new ColumnExpression(currentAlias, attribute.ColumnName);
            }
            else
                return new ColumnExpression(currentAlias, ((NLinq.Expressions.Identifier)item.Statement).Text);
        }

        public override IAliasedExpression Visit(ComplexColumnExpression item)
        {
            if (item.Expression == null)
                return null;
            if (item.Expression.ExpressionType == ExpressionTypes.MemberAccess)
            {
                bool wasVisitingColumns = visitingColumns;
                visitingColumns = true;
                ColumnExpression column = Visit(item.Expression) as ColumnExpression;
                visitingColumns = wasVisitingColumns;
                if (column != null)
                {
                    if (visitingColumns)
                        return new ColumnExpression(item.Alias, column.ColumnAlias, column.ColumnAlias);
                    else
                        return new ColumnExpression(item.Alias, column.ColumnAlias);
                }
            }
            return base.Visit(item);
        }

        public override Identifier Visit(EntityIdentifier identifier)
        {
            if (visitingColumns)
            {
                currentAlias = identifier.Entity.Alias;
                return identifier;
            }
            return base.Visit(identifier);
        }

        public override Expression Visit(AnonymousParameter item)
        {
            if (!visitingColumns)
                throw new NotSupportedException();
            return new ColumnExpression(currentAlias, (Identifier)((MemberExpression)item.Expression).Statement, item.Identifier);
        }

        public override IDbExpression Visit(EntityExpression item)
        {
            if (item.Type != null)
                currentEntity = mapping.Entities[item.Type];
            currentAlias = item.Alias;
            if (visitingColumns)
            {
                if (item.Expression == null)
                    return new ColumnExpression(currentAlias, ColumnExpression.AllColumns);
                return (IAliasedExpression)Visit(item.Expression);
            }
            if (inWhere)
            {
                TupleExpression ids = new TupleExpression();
                if (currentEntity != null)
                    foreach (Mapping.Attribute id in currentEntity.Ids.Values)
                        ids.Add(new ColumnExpression(currentAlias, id.Field.ColumnName));
                return ids;
            }
            return base.Visit(item);
        }

        bool inWhere = false;
        bool visitingColumns = false;

        public override Evaluant.NLinq.Expressions.QueryBodyClause Visit(Evaluant.NLinq.Expressions.WhereClause expression)
        {
            inWhere = true;
            Evaluant.NLinq.Expressions.QueryBodyClause result = base.Visit(expression);
            inWhere = false;
            return result;
        }

        public override IAliasedExpression Visit(SelectStatement item)
        {
            bool wasInWhere = inWhere;
            inWhere = false;
            Evaluant.Uss.SqlExpressions.FromClause from = Visit(item.From);
            //SelectStatement select = (SelectStatement)base.Visit(item);
            inWhere = wasInWhere;
            visitingColumns = true;
            //IEnumerable<AliasedExpression> columns = VisitEnumerable(item.Columns, Visit);
            List<IAliasedExpression> columns;
            if (item.Columns == null)
                columns = new List<IAliasedExpression>(){ new ComplexColumnExpression(
                                    null,
                                    new Constant(1, System.Data.DbType.Int32))};
            else
                columns = new List<IAliasedExpression>(VisitEnumerable(item.Columns, Visit));
            if (columns.Count >= 2 && columns[1].DbExpressionType == DbExpressionType.Column && ((ColumnExpression)columns[1]).ColumnName == ColumnExpression.AllColumns
             || columns.Count == 1 && columns[0].DbExpressionType == DbExpressionType.Column && ((ColumnExpression)columns[0]).ColumnName == ColumnExpression.AllColumns)
            {
                columns.Clear();
                columns.Add(new ComplexColumnExpression(null, new CaseExpression(currentAlias, null, new Constant(currentEntity.Type, System.Data.DbType.String), mapping.Mapper.TestCases(currentEntity.Type, currentAlias)), "EntityType"));
                foreach (var attribute in currentEntity.Attributes)
                    columns.Add(new ColumnExpression(currentAlias, attribute.Value.ColumnName, attribute.Value.Name));
            }
            visitingColumns = false;
            Evaluant.NLinq.Expressions.OrderByClause orderby = VisitOrderBy((Evaluant.NLinq.Expressions.OrderByClause)item.OrderBy);
            Evaluant.NLinq.Expressions.WhereClause where = item.Where;
            if (item.Where != null)
                where = (WhereClause)Visit(item.Where);
            var select = (SelectStatement)updater.Update(item, columns, from, where, orderby, item.Alias);

            //TODO : See What it is used for. Select could be returned directly. In some cases, it removes the EntityType column
            //if (columns.Count >= 2 && columns[1] is ColumnExpression)
            //{
            //    ColumnExpression mainColumn = (ColumnExpression)columns[0];
            //    if (mainColumn.ColumnName != ColumnExpression.AllColumns && mainColumn.ColumnAlias != null && mainColumn.ColumnAlias.Text != "EntityType")
            //        columns.RemoveAt(0);
            //}
            //else
            return select;
            //return updater.Update(select, columns, select.From, select.Where, select.OrderBy, select.Alias);
        }
    }
}
