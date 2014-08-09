using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.NLinqImprovements;
using Evaluant.Uss.SqlExpressions.Visitors;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators.MappingMutators
{
    public class EntityToTableMutator : DbExpressionVisitor
    {
        Mapping.Mapping mapping;
        IDriver driver;

        public EntityToTableMutator(Mapping.Mapping mapping, IDriver driver)
        {
            this.mapping = mapping;
            this.driver = driver;
        }

        bool visitingSelect = false;

        IList<NLinq.Expressions.WhereClause> additionnalClauses = new List<NLinq.Expressions.WhereClause>();

        public override IAliasedExpression Visit(Evaluant.Uss.SqlExpressions.Mapping.EntitySourceExpression item)
        {
            Mapping.Entity em = mapping.Entities[item.EntityType];
            if (em.Inherit != null && em.Inheritance.DiscriminatorExpression != null)
            {
                additionnalClauses.Add(new NLinq.Expressions.WhereClause(new DiscriminatorMutator(item).Visit(em.Inheritance.DiscriminatorExpression)));
            }
            return new TableSourceExpression(item.Alias, mapping.Entities[item.EntityType].Table);
        }


        public override IAliasedExpression Visit(SelectStatement item)
        {
            bool wasVisitingSelect = visitingSelect;
            visitingSelect = false;
            var additionnalClauses = this.additionnalClauses;
            this.additionnalClauses = new List<NLinq.Expressions.WhereClause>();
            FromClause from = Visit(item.From);
            visitingSelect = true;
            IEnumerable<IAliasedExpression> columns = VisitEnumerable(item.Columns, Visit);
            visitingSelect = false;
            Evaluant.NLinq.Expressions.OrderByClause orderby = VisitOrderBy((Evaluant.NLinq.Expressions.OrderByClause)item.OrderBy);
            Evaluant.NLinq.Expressions.WhereClause where = (Evaluant.NLinq.Expressions.WhereClause)Visit(item.Where);
            if (this.additionnalClauses.Count == 1)
            {
                if (where != null)
                    where = new NLinq.Expressions.WhereClause(new NLinq.Expressions.BinaryExpression(NLinq.Expressions.BinaryExpressionType.And, where.Expression, this.additionnalClauses[0].Expression));
                else
                    where = this.additionnalClauses[0];
            }
            visitingSelect = wasVisitingSelect;
            this.additionnalClauses = additionnalClauses;
            return updater.Update(item, columns, from, where, orderby, item.Alias);
        }

        public override IAliasedExpression Visit(TableSourceExpression item)
        {
            return new TableSourceExpression(item.Alias, item.Table);
        }

        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.MemberExpression expression)
        {
            if (visitingSelect && expression.Previous != null)
                return expression;
            return base.Visit(expression);
        }

        public override IDbExpression Visit(EntityExpression item)
        {
            if (visitingSelect && item.Expression is Evaluant.NLinq.Expressions.Identifier)
                return new ColumnExpression(item.Alias, ColumnExpression.AllColumns);
            return base.Visit(item);
        }

        public override IAliasedExpression Visit(CaseExpression item)
        {
            if (item.CaseTests == null || item.CaseTests.Length == 0)
            {
                if (((Constant)item.DefaultResult).Value == null)
                    return null;
                item.CaseTests = mapping.Mapper.TestCases((string)((Constant)item.DefaultResult).Value, item.Alias);
            }

            item = (CaseExpression)new ValueExpressionMutator(driver).Visit(item);
            return base.Visit(item);
        }
    }
}
