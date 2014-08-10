using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.NLinqImprovements;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.PersistenceEngine.Contracts;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    public class EntityToSelectMutator : DbExpressionVisitor
    {
        public EntityToSelectMutator(SqlMapperEngine engine)
        {
            this.engine = engine;
        }

        SqlMapperEngine engine;

        private SelectStatement select;

        public override Evaluant.NLinq.Expressions.QueryBodyClause Visit(NLinq.Expressions.FromClause expression)
        {
            TableAlias alias;
            if (entityIdentifierAliases.TryGetValue(expression.Identifier.Text, out alias))
            {
                if (alias != ((EntityIdentifier)expression.Identifier).Entity.Alias)
                    throw new NotSupportedException("The same identifier refers to a different Alias");
            }
            else
                entityIdentifierAliases.Add(expression.Identifier.Text, ((EntityIdentifier)expression.Identifier).Entity.Alias);
            if (expression.Type != null)
            {
                IAliasedExpression src;
                if (expression.Type.StartsWith("System."))
                    src = ((EntityIdentifier)expression.Identifier).Entity;
                else
                    src = new SqlExpressions.Mapping.EntitySourceExpression(((EntityIdentifier)expression.Identifier).Entity.Alias, expression.Type);
                if (select.From == null)
                    select.From = new FromClause(src);
                else
                    select.From = new FromClause(
                        new JoinedTableExpression(
                            select.From[0],
                            src,
                            new Evaluant.NLinq.Expressions.BinaryExpression(
                                  Evaluant.NLinq.Expressions.BinaryExpressionType.Unknown,
                                  expression.Identifier,
                                  Visit(expression.Expression))
                            )
                        );
            }
            else
            {
                SelectStatement currentSelect = select;
                var result = Visit(expression.Expression);
                if (select != currentSelect)
                {
                    select = (SelectStatement)new LazyAliasResolver(new Dictionary<TableAlias, TableAlias> { { select.Alias, ((EntityIdentifier)expression.Identifier).Entity.Alias } }).Visit(select);
                    if (currentSelect.From == null)
                        currentSelect.From = new FromClause(select);
                    else
                        currentSelect.From = new FromClause(
                            new JoinedTableExpression(
                                currentSelect.From[0],
                                select,
                                new NLinq.Expressions.BinaryExpression(
                                    NLinq.Expressions.BinaryExpressionType.Unknown,
                                    expression.Identifier,
                                    result)
                                )
                            );
                }
                select = currentSelect;
            }
            return expression;
        }

        public override Evaluant.NLinq.Expressions.QueryBodyClause Visit(Evaluant.NLinq.Expressions.WhereClause expression)
        {
            if (select.Where == null)
                select.Where = (Evaluant.NLinq.Expressions.WhereClause)base.Visit(expression);
            else
                select.Where = new NLinq.Expressions.WhereClause(new NLinq.Expressions.BinaryExpression(NLinq.Expressions.BinaryExpressionType.And, select.Where.Expression, Visit(expression.Expression)));
            return select.Where;
        }

        public override Evaluant.NLinq.Expressions.QueryBodyClause Visit(Evaluant.NLinq.Expressions.OrderByClause item)
        {
            select.OrderBy = (Evaluant.NLinq.Expressions.OrderByClause)base.Visit(item);
            return select.OrderBy;
        }

        public override Evaluant.NLinq.Expressions.SelectOrGroupClause Visit(Evaluant.NLinq.Expressions.GroupClause expression)
        {
            return base.Visit(expression);
        }
        bool getAlias = false;
        bool getPreviousType = false;
        public override Evaluant.NLinq.Expressions.SelectOrGroupClause Visit(Evaluant.NLinq.Expressions.SelectClause expression)
        {
            expression = (Evaluant.NLinq.Expressions.SelectClause)base.Visit(expression);
            if (expression.Expression.ExpressionType == NLinq.Expressions.ExpressionTypes.AnonymousNew)
            {
                Evaluant.NLinq.Expressions.AnonymousNew anonyNew = (Evaluant.NLinq.Expressions.AnonymousNew)expression.Expression;
                List<IAliasedExpression> columns = new List<IAliasedExpression>();
                foreach (Evaluant.NLinq.Expressions.AnonymousParameter parameter in anonyNew.Parameters)
                {
                    if (parameter.Expression.ExpressionType == NLinq.Expressions.ExpressionTypes.MemberAccess)
                    {
                        getAlias = true;
                        EntityIdentifier entityReference = ((EntityIdentifier)Visit(parameter.Expression));
                        getAlias = false;
                        columns.Add(new EntityExpression(entityIdentifierAliases[entityReference.Text]) { Expression = parameter, Type = entityReference.Entity.Type });
                    }
                    else if (parameter.Expression is IAliasedExpression)
                        columns.Add(parameter.Expression as IAliasedExpression);
                    else
                        throw new NotSupportedException("The anonymous new parameter expression should be an aliased expression");
                }
                select.Columns = columns.ToArray();
            }
            else if (expression.Expression.ExpressionType == NLinq.Expressions.ExpressionTypes.MemberAccess)
            {
                getAlias = true;
                EntityIdentifier entityReference = ((EntityIdentifier)Visit(expression.Expression));
                getAlias = false;

                select.Columns = new IAliasedExpression[] 
                {
                    //new ComplexColumnExpression(null, new CaseExpression(entityIdentifierAliases[entityReference.Text], null, new Constant(entityReference.Entity.Type, System.Data.DbType.AnsiString)),"EntityType"),
                    new EntityExpression(entityIdentifierAliases[entityReference.Text]){ Expression=expression.Expression, Type=entityReference.Entity.Type }
                };
            }
            EntityReferenceExpression entityReferenceExpression = expression.Expression as EntityReferenceExpression;
            if (entityReferenceExpression != null)
            {
                //Evaluant.NLinq.Expressions.Identifier identifier = ((EntityExpression)entityReference.Target).Expression as Evaluant.NLinq.Expressions.Identifier;
                //if (identifier == null)
                //{
                //    getAlias = true;
                //    identifier = (Evaluant.NLinq.Expressions.Identifier)Visit(((EntityExpression)entityReference.Target).Expression);
                //    getAlias = false;
                //}
                //if (!entityIdentifierAliases.ContainsKey(identifier.Text))
                //    entityIdentifierAliases.Add(identifier.Text, new TableAlias());
                if (getAlias)
                {
                    Evaluant.NLinq.Expressions.Identifier identifier = ((EntityExpression)entityReferenceExpression.Target).Expression as Evaluant.NLinq.Expressions.Identifier;
                    if (identifier == null)
                        identifier = (Evaluant.NLinq.Expressions.Identifier)Visit(((EntityExpression)entityReferenceExpression.Target).Expression);
                    return new Evaluant.NLinq.Expressions.SelectClause(identifier);
                }
                var columns = new List<IAliasedExpression>();

                if (((EntityExpression)entityReferenceExpression.Target).Expression.ExpressionType == NLinq.Expressions.ExpressionTypes.AnonymousNew)
                {
                    var finder = new TypeFinderVisitor(engine.Provider.Model, engine.Provider.Mapping);
                    foreach (NLinq.Expressions.AnonymousParameter parameter in ((NLinq.Expressions.AnonymousNew)((EntityExpression)entityReferenceExpression.Target).Expression).Parameters)
                    {
                        finder.Visit(parameter.Expression);
                        columns.Add(new ColumnExpression(entityReferenceExpression.Alias, parameter.Identifier, finder.DbType));
                    }
                }
                else
                {

                    string entityType = ((EntityExpression)entityReferenceExpression.Target).Type;
                    columns.Add(new ComplexColumnExpression(null, new CaseExpression(entityReferenceExpression.Target.Alias, null, new Constant(entityType, System.Data.DbType.AnsiString)), "EntityType"));
                    if (entityType != null)
                    {
                        foreach (var attribute in engine.Factory.Model.GetInheritedAttributes(entityType))
                        {
                            columns.Add(new EntityExpression(entityReferenceExpression.Target.Alias) { Expression = new NLinq.Expressions.MemberExpression(new NLinq.Expressions.Identifier(attribute.Name), new EntityExpression(entityReferenceExpression.Target.Alias) { Type = entityType }) });
                        }
                    }
                }
                select.Columns = columns;
            }
            return expression;
        }

        private Dictionary<string, TableAlias> entityIdentifierAliases = new Dictionary<string, TableAlias>();

        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.MemberExpression expression)
        {
            if (getAlias)
            {
                if (expression.Previous is EntityIdentifier)
                {
                    EntityIdentifier identifier = (EntityIdentifier)expression.Previous;
                    if (!entityIdentifierAliases.ContainsKey(identifier.Text))
                        entityIdentifierAliases.Add(identifier.Text, identifier.Entity.Alias);
                    return identifier;
                }

                if (expression.Previous != null)
                    return Visit(expression.Previous);
                return base.Visit(expression);

            }
            if (getPreviousType)
            {
                if (expression.Previous is EntityIdentifier)
                    return ((EntityIdentifier)expression.Previous).Entity;

                if (expression.Previous != null)
                {
                    EntityExpression entity = (EntityExpression)Visit(expression.Previous);
                    Model.Reference reference = engine.Factory.Model.Entities[entity.Type].References[((Evaluant.NLinq.Expressions.Identifier)expression.Statement).Text];
                    entity = new EntityExpression(new TableAlias()) { Type = reference.ChildType, Expression = expression };
                    return entity;
                }
            }

            if (expression.Statement.ExpressionType == NLinq.Expressions.ExpressionTypes.Call)
            {
                NLinq.Expressions.MethodCall item = (NLinq.Expressions.MethodCall)expression.Statement;
                AggregateFunctionType aggregateType;
                switch (item.Identifier.Text)
                {
                    case "Count":
                        aggregateType = AggregateFunctionType.Count;
                        break;
                    case "Average":
                        aggregateType = AggregateFunctionType.Avg;
                        break;
                    case "Max":
                        aggregateType = AggregateFunctionType.Max;
                        break;
                    case "Min":
                        aggregateType = AggregateFunctionType.Min;
                        break;
                    case "Sum":
                        aggregateType = AggregateFunctionType.Sum;
                        break;
                    default:
                        aggregateType = AggregateFunctionType.Unknown;
                        break;
                }
                if (aggregateType == AggregateFunctionType.Unknown)
                {
                    SelectStatement oldSelect = select;
                    if (item.Identifier.Text == "Take")
                    {
                        Visit(expression.Previous);
                        select.Top = (int)((Constant)item.Parameters[0]).Value;
                        return select;
                    }
                    if (item.Identifier.Text == "Skip")
                    {
                        Visit(expression.Previous);
                        if (Convert.ToInt32(((Constant)item.Parameters[0]).Value) == 0)
                            return select;

                        if (select.OrderBy == null)
                            throw new NotSupportedException("A skip operation cannot be done on a statement which is not ordered");


                        List<IAliasedExpression> columns = new List<IAliasedExpression>(select.Columns);
                        columns.Add(new ComplexColumnExpression(null, new RowNumber(select.OrderBy), "rn" + select.Alias));
                        select.Columns = columns.ToArray();
                        columns.RemoveAt(columns.Count - 1);
                        oldSelect = select;
                        select = new SelectStatement(new TableAlias(), null, new FromClause(select), null, new NLinq.Expressions.WhereClause(new NLinq.Expressions.BinaryExpression(NLinq.Expressions.BinaryExpressionType.Greater, new ColumnExpression(select.Alias, "rn" + select.Alias), item.Parameters[0])));
                        columns = new List<IAliasedExpression>();
                        foreach (IAliasedExpression expr in oldSelect.Columns)
                        {
                            IAliasedExpression column = null;
                            if (expr.DbExpressionType == DbExpressionType.Column)
                            {
                                column = new ColumnExpression(oldSelect.Alias, ((ColumnExpression)expr).ColumnAlias, ((ColumnExpression)expr).ColumnAlias);
                            }
                            else if (expr.DbExpressionType == DbExpressionType.Entity)
                            {
                                EntityExpression entity = expr as EntityExpression;
                                column = new ComplexColumnExpression(oldSelect.Alias, new NLinq.Expressions.MemberExpression(((NLinq.Expressions.MemberExpression)entity.Expression).Statement, entity.Expression));
                            }
                            else
                                throw new NotSupportedException();
                            if (column != null)
                                columns.Add(column);
                        }
                        if (oldSelect.OrderBy != null && oldSelect.OrderBy.Count > 0)
                        {
                            List<Evaluant.NLinq.Expressions.OrderByCriteria> criterias = new List<NLinq.Expressions.OrderByCriteria>();
                            foreach (var criteria in oldSelect.OrderBy.Criterias)
                            {
                                if (criteria.Expression != null)
                                {
                                    if (criteria.Expression.ExpressionType == NLinq.Expressions.ExpressionTypes.MemberAccess)
                                    {
                                        criterias.Add(new NLinq.Expressions.OrderByCriteria(new ColumnExpression(oldSelect.Alias, (NLinq.Expressions.Identifier)((NLinq.Expressions.MemberExpression)criteria.Expression).Statement), criteria.Ascending));
                                    }
                                }
                            }
                            if (criterias.Count > 0)
                                select.OrderBy = new NLinq.Expressions.OrderByClause(criterias);
                        }
                        //Since we include a surrounding select, we need to clone some information with alias changing
                        var aliasChanger = new LazyAliasResolver(new Dictionary<TableAlias, TableAlias> { { TableAlias.All, oldSelect.Alias } });
                        select.Columns = aliasChanger.VisitColumns(columns);

                        select.OrderBy = aliasChanger.VisitOrderBy(select.OrderBy);
                        oldSelect.OrderBy = null;
                        return select;
                    }
                    if (item.Identifier.Text == "First")
                    {
                        Visit(expression.Previous);
                        select.Top = 1;
                        return select;
                    }
                    if (item.Identifier.Text == "Last")
                    {
                        Visit(expression.Previous);
                        if (select.OrderBy != null && select.OrderBy.Criterias != null && select.OrderBy.Criterias.Count > 0)
                        {
                            foreach (NLinq.Expressions.OrderByCriteria criteria in select.OrderBy.Criterias)
                            {
                                criteria.Ascending = !criteria.Ascending;
                            }
                        }
                        else
                            throw new NotSupportedException("To use Last or LastOrDefault, you have to specify an order in your query.");
                        select.Top = 1;
                        return select;
                    }
                    if (item.Identifier.Text == "Any")
                    {
                        if (expression.Previous.ExpressionType != NLinq.Expressions.ExpressionTypes.Quote)
                        {
                            select = new SelectStatement(new TableAlias());
                            select.Columns = new[] { new ComplexColumnExpression(
                                    null,
                                    new Constant(1, System.Data.DbType.Int32)) 
                                };
                            select.Where = new NLinq.Expressions.WhereClause(Visit(expression.Previous));
                            var result = new Exists(select);
                            select = oldSelect;
                            return result;
                        }
                        return new Exists(Visit(expression.Previous));
                    }
                    if (item.Identifier.Text == "Distinct")
                    {
                        Visit(expression.Previous);
                        select.Distinct = true;
                        return select;
                    }
                    return expression;
                    select = new SelectStatement(new TableAlias());
                    Visit(expression.Previous);
                    SelectStatement previousSelect = select;
                    select = oldSelect;
                    if (item.Parameters.Length == 0)
                        return new SelectStatement(new TableAlias(), new IAliasedExpression[] { new Aggregate(item.Identifier, ColumnExpression.AllColumns) }, new FromClause(previousSelect), null, null);
                    else
                        return new SelectStatement(new TableAlias(), new IAliasedExpression[] { new Aggregate(item.Identifier, item.Parameters) }, new FromClause(previousSelect), null, null);
                }
                else
                {
                    IAliasedExpression result;
                    if (expression.Previous is Evaluant.NLinq.Expressions.MemberExpression)
                    {
                        //If we are aggregating results
                        if (select == null)
                        {
                            Visit(expression.Previous);
                            SelectStatement oldSelect = select;
                            select = new SelectStatement(new TableAlias());
                            var columns = new IAliasedExpression[] { new Aggregate(aggregateType, new Constant(1, System.Data.DbType.Int32)) };
                            select.Columns = columns;
                            select.From = new FromClause(oldSelect);
                            //Prevent order by if counting (useless, and causes crashes on SQLServer
                            if (oldSelect.OrderBy != null)
                                oldSelect.OrderBy = null;
                            return select;
                        }
                        else
                        {
                            SelectStatement oldSelect = select;
                            select = new SelectStatement(new TableAlias());
                            //from var veryPrevious in oldSelect.From 
                            //from var a in expression.Previous 
                            //where veryPrevious==oldSelect.Select
                            //select a
                            getAlias = true;
                            EntityIdentifier identifier = (EntityIdentifier)Visit(expression);
                            getAlias = false;
                            getPreviousType = true;
                            EntityExpression previousEntity = (EntityExpression)Visit(expression.Previous);
                            getPreviousType = false;
                            select.From = oldSelect.From;

                            Evaluant.NLinq.Expressions.Identifier generatedIdentifier = new Evaluant.NLinq.Expressions.Identifier("source" + expression.GetHashCode());
                            generatedIdentifier = new EntityIdentifier(generatedIdentifier, new EntityExpression(identifier.Entity.Alias) { Type = previousEntity.Type, Expression = generatedIdentifier });

                            Evaluant.NLinq.Expressions.Identifier generatedTargetIdentifier = new Evaluant.NLinq.Expressions.Identifier("target" + expression.GetHashCode());
                            generatedTargetIdentifier = new EntityIdentifier(generatedTargetIdentifier, new EntityExpression(new TableAlias()) { Type = previousEntity.Type, Expression = generatedTargetIdentifier });

                            //from generatedIdentifier
                            //from generatedTargetIdentifier in expression
                            //where generatedIdentifier==identifier
                            //select generatedTargetIdentifier


                            //Add a join to load the reference on which we want to aggregate
                            Evaluant.NLinq.Expressions.QueryExpression query = new Evaluant.NLinq.Expressions.QueryExpression(
                                new NLinq.Expressions.FromClause(identifier.Entity.Type, generatedIdentifier, null),
                                new Evaluant.NLinq.Expressions.QueryBody(
                                    new Evaluant.NLinq.Expressions.ClauseList{
                            new NLinq.Expressions.FromClause(previousEntity.Type, generatedTargetIdentifier, expression.Previous),
                                    new Evaluant.NLinq.Expressions.WhereClause(
                            new Evaluant.NLinq.Expressions.BinaryExpression(
                                Evaluant.NLinq.Expressions.BinaryExpressionType.Equal,
                                    new EntityReferenceExpression(((EntityIdentifier)generatedIdentifier).Entity),
                                    new EntityReferenceExpression(identifier.Entity)))
                                }, new Evaluant.NLinq.Expressions.SelectClause(
                                        new EntityReferenceExpression(((EntityIdentifier)generatedTargetIdentifier).Entity)),
                                    null));

                            result = (IAliasedExpression)Visit(query);
                            select = oldSelect;
                        }
                    }
                    else
                        result = (IAliasedExpression)Visit(expression.Previous);
                    if (item.Parameters.Length == 0)
                        result = new SelectStatement(new TableAlias(), new IAliasedExpression[] { new Aggregate(aggregateType, ColumnExpression.AllColumns) }, new FromClause(result), null, null);
                    else
                        result = new SelectStatement(new TableAlias(), new IAliasedExpression[] { new Aggregate(aggregateType, item.Parameters) }, new FromClause(result), null, null);
                    return (NLinq.Expressions.Expression)result;
                }
            }
            return base.Visit(expression);

            //throw new NotSupportedException("There should be at least one previous");
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.UnaryExpression item)
        {
            return new Not((IDbExpression)base.Visit(item.Expression));
        }

        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.QueryExpression expression)
        {
            var select = this.select = new SelectStatement(new TableAlias());
            if (getAlias)
                return ((Evaluant.NLinq.Expressions.SelectClause)((Evaluant.NLinq.Expressions.QueryExpression)base.Visit(expression)).QueryBody.SelectOrGroup).Expression;

            base.Visit(expression);
            return select;
        }
    }
}
