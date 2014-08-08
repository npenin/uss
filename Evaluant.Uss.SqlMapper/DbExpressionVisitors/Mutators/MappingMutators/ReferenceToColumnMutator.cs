using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.NLinqImprovements;
using System.Collections.Specialized;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators.MappingMutators
{
    public class ReferenceToColumnMutator : DbExpressionVisitor
    {
        public override IAliasedExpression Visit(SelectStatement item)
        {
            IDictionary<string, TableAlias> oldLoadedReferences = loadedReferences;
            Dictionary<TableAlias, TableAlias> oldAliasesMapping = AliasesMapping;
            FromClause oldFrom = currentFrom;
            loadedReferences = new Dictionary<string, TableAlias>();
            AliasesMapping = new Dictionary<TableAlias, TableAlias>();
            bool wasInFrom = inFrom;
            inFrom = false;
            var oldConstraint = constraint;
            IAliasedExpression select = base.Visit(item);
            inFrom = wasInFrom;
            if (select is SelectStatement)
            {
                SelectStatement sel = (SelectStatement)select;
                if (constraint != null)
                {
                    if (sel.Where == null || sel.Where.Expression == null)
                        sel.Where = new NLinq.Expressions.WhereClause(constraint);
                    else
                        sel.Where = new NLinq.Expressions.WhereClause(new NLinq.Expressions.BinaryExpression(NLinq.Expressions.BinaryExpressionType.And, sel.Where.Expression, constraint));
                    constraint = oldConstraint;
                }
                select = updater.Update(sel, sel.Columns, currentFrom, sel.Where, sel.OrderBy, sel.Alias);
            }
            select = new LazyAliasResolver(AliasesMapping).Visit(select);
            loadedReferences = oldLoadedReferences;
            AliasesMapping = oldAliasesMapping;
            currentFrom = oldFrom;
            return select;
        }

        Mapping.Mapping mapping;
        Mapping.Entity currentEntity;
        EntityExpression currentEntityExpression;

        IDictionary<string, TableAlias> loadedReferences = new Dictionary<string, TableAlias>();
        public Dictionary<TableAlias, TableAlias> AliasesMapping { get; set; }
        StringBuilder referencePath = new StringBuilder();
        TableAlias lastAliasDefined = null;
        //IDictionary<AliasedExpression, TableAlias> registeredAliasedExpression = new Dictionary<AliasedExpression, TableAlias>();

        public ReferenceToColumnMutator(Mapping.Mapping mapping)
        {
            AliasesMapping = new Dictionary<TableAlias, TableAlias>();
            this.mapping = mapping;
        }

        bool inFrom = false;

        public override IAliasedExpression Visit(JoinedTableExpression item)
        {
            if (item.On.RightExpression is NLinq.Expressions.MemberExpression)
            {
                Visit(item.On.LeftExpression);
                referencePath.Length = 0;
                NLinq.Expressions.MemberExpression propertyReference = (NLinq.Expressions.MemberExpression)item.On.RightExpression;
                IAliasedExpression join = (IAliasedExpression)Visit(propertyReference);
                //if (loadedReferences.ContainsKey(((EntityIdentifier)item.On.LeftExpression).Text))
                //{
                //    AliasesMapping.Add(loadedReferences[((EntityIdentifier)item.On.LeftExpression).Text], lastAliasDefined);
                loadedReferences[((EntityIdentifier)item.On.LeftExpression).Text] = lastAliasDefined;
                //}
                //else
                //    loadedReferences.Add(((EntityIdentifier)item.On.LeftExpression).Text, lastAliasDefined);
                if (!AliasesMapping.ContainsKey(item.RightTable.Alias))
                    AliasesMapping.Add(item.RightTable.Alias, join.Alias);
                return join;
            }
            return item;
        }

        public override Evaluant.NLinq.Expressions.QueryBodyClause Visit(Evaluant.NLinq.Expressions.WhereClause expression)
        {
            //Evaluant.NLinq.Expressions.Expression exp = Visit(expression.Expression);
            //if (exp == null)
            //    return null;
            if (constraint != null)
                return updater.Update((NLinq.Expressions.WhereClause)base.Visit(expression), new NLinq.Expressions.BinaryExpression(NLinq.Expressions.BinaryExpressionType.And, expression.Expression, constraint));
            return base.Visit(expression);
        }

        protected override Evaluant.NLinq.Expressions.Identifier VisitIdentifier(Evaluant.NLinq.Expressions.Identifier identifier)
        {
            if (identifier == null)
                return null;
            referencePath.Append(identifier.Text);
            return base.VisitIdentifier(identifier);
        }

        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.BinaryExpression item)
        {
            NLinq.Expressions.Expression exp = base.Visit(item);
            if (exp is NLinq.Expressions.BinaryExpression)
            {
                //item = (NLinq.Expressions.BinaryExpression)exp;
                if (item.Type == Evaluant.NLinq.Expressions.BinaryExpressionType.NotEqual)
                {
                    NLinq.Expressions.Expression otherMember = null;
                    if (item.LeftExpression is Constant && ((Constant)item.LeftExpression).Value == null)
                        otherMember = item.RightExpression;
                    if (item.RightExpression is Constant && ((Constant)item.RightExpression).Value == null)
                        otherMember = item.LeftExpression;
                    var analyzer = new Analyzers.ReferenceAnalyzer(mapping.Model);
                    analyzer.Visit(otherMember);
                    if (analyzer.Reference != null && analyzer.Reference.Cardinality is Era.Cardinality.ToMany)
                        return null;
                }
                else if (item.Type == Evaluant.NLinq.Expressions.BinaryExpressionType.And || item.Type == Evaluant.NLinq.Expressions.BinaryExpressionType.Or)
                {
                    if (item.LeftExpression == null)
                        return item.RightExpression;
                    if (item.RightExpression == null)
                        return item.LeftExpression;
                }
            }
            return exp;
        }

        public override IAliasedExpression Visit(TableSourceExpression item)
        {
            //registeredAliasedExpression.Add(item, item.Alias);
            return base.Visit(item);
        }

        TableAlias currentAlias;

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.MemberExpression item)
        {
            NLinq.Expressions.Expression target = Visit(item.Previous);
            TableAlias entityAlias = currentAlias;
            NLinq.Expressions.Expression statement = Visit(item.Statement);

            if (statement is Evaluant.NLinq.Expressions.Identifier)
            {
                string propertyName = ((Evaluant.NLinq.Expressions.Identifier)statement).Text;
                if (inFrom && currentEntity.References.ContainsKey(propertyName))
                {
                    Mapping.Reference reference = currentEntity.References[propertyName];
                    TableAlias newTableAlias;
                    IAliasedExpression join = mapping.Mapper.Join(reference, out newTableAlias);
                    //registeredAliasedExpression.Add(join, newTableAlias);
                    lastAliasDefined = ((JoinedTableExpression)join).RightTable.Alias;
                    loadedReferences.Add(referencePath.ToString(), lastAliasDefined);
                    AliasesMapping.Add(newTableAlias, entityAlias);
                    return (NLinq.Expressions.Expression)join;
                }
                else
                {
                    string targetReferencePath = referencePath.ToString().Substring(0, referencePath.Length - propertyName.Length);
                    //If this is not a reference leave the treatment to another visitor
                    if (currentEntity != null && currentEntity.References.ContainsKey(propertyName))
                    {
                        bool exists = currentFrom == null;
                        Mapping.Reference reference = currentEntity.References[propertyName];
                        if (!loadedReferences.ContainsKey(referencePath.ToString()))
                        {
                            currentEntity = reference.Target;
                            TableAlias newTableAlias;
                            //New target to reduce the already processed tree
                            target = (NLinq.Expressions.Expression)mapping.Mapper.Join(reference, out newTableAlias);
                            if (reference.IsComposite || loadedReferences.ContainsKey(targetReferencePath))
                            {
                                target = new LazyAliasResolver(new Dictionary<TableAlias, TableAlias>() { { newTableAlias, currentAlias } }).Visit(target);
                                newTableAlias = currentAlias;
                                //AliasesMapping[loadedReferences[targetReferencePath]] = target.Alias;
                            }


                            //Add JoinedTableExpression to load reference in query
                            if (!exists)
                                target = ((JoinedTableExpression)target).Replace(newTableAlias, currentFrom[0]);
                            else
                            {
                                //Case of Exists in where clause
                                Evaluant.NLinq.Expressions.Expression constraint;
                                target = (NLinq.Expressions.Expression)((JoinedTableExpression)target).Replace(newTableAlias, loadedReferences[targetReferencePath], out constraint);

                                if (this.constraint == null)
                                    this.constraint = constraint;
                                else
                                    this.constraint = new NLinq.Expressions.BinaryExpression(NLinq.Expressions.BinaryExpressionType.And, this.constraint, constraint);
                            }

                            //if (target == null)
                            //    throw new NotSupportedException("The alias specified could not be found");
                            if (loadedReferences.ContainsKey(targetReferencePath))
                                target = new LazyAliasResolver(new Dictionary<TableAlias, TableAlias>() { { loadedReferences[targetReferencePath], newTableAlias } }).Visit(target);
                            currentFrom = new FromClause((IAliasedExpression)target);
                            lastAliasDefined = ((JoinedTableExpression)target).RightTable.Alias;
                            loadedReferences.Add(referencePath.ToString(), lastAliasDefined);
                        }
                        else
                        {
                            if (!inFrom)
                                lastAliasDefined = loadedReferences[referencePath.ToString()];
                        }
                        if (exists)
                        {
                            var constraint = this.constraint;
                            this.constraint = null;
                            return constraint;
                        }
                        return new EntityExpression(lastAliasDefined) { Type = reference.Target.Type };
                    }
                    else
                    {
                        if (!loadedReferences.ContainsKey(targetReferencePath))
                            return updater.Update(item, currentEntityExpression, item.Statement);
                        //return new ComplexColumnExpression(currentAlias, target is SelectStatement ? new EntityIdentifier(propertyName, currentEntityExpression) : new Evaluant.NLinq.Expressions.Identifier(propertyName));
                        return updater.Update(item, target, item.Statement);
                        //return new ComplexColumnExpression(currentAlias, target is SelectStatement ? new EntityIdentifier(propertyName, new EntityExpression(loadedReferences[targetReferencePath]) { Type = currentEntity.Type }) : new Evaluant.NLinq.Expressions.Identifier(propertyName));
                        //return updater.Update(item, target, target is SelectStatement ? new EntityIdentifier(propertyName, new EntityExpression(loadedReferences[targetReferencePath]) { Type = currentEntity.Type }) : new Evaluant.NLinq.Expressions.Identifier(propertyName));
                    }
                }
            }
            return updater.Update(item, target, statement);
        }

        public override FromClause Visit(FromClause item)
        {
            bool wasInFrom = inFrom;
            inFrom = true;
            currentFrom = base.Visit(item);
            inFrom = wasInFrom;
            return currentFrom;
        }

        public override TableAlias Visit(TableAlias item)
        {
            if (item == null)
                return null;
            if (AliasesMapping.ContainsKey(item))
                return AliasesMapping[item];
            return item;
        }

        private FromClause currentFrom;
        private NLinq.Expressions.Expression constraint;

        public override IDbExpression Visit(EntityExpression item)
        {
            if (item.Type != null)
                currentEntity = mapping.Entities[item.Type];
            currentEntityExpression = item;
            currentAlias = item.Alias;
            if (item.Expression is NLinq.Expressions.Identifier)
            {
                referencePath = new StringBuilder(((NLinq.Expressions.Identifier)item.Expression).Text);
                if (loadedReferences.ContainsKey(referencePath.ToString()))
                    currentAlias = loadedReferences[referencePath.ToString()];
                else
                    loadedReferences.Add(referencePath.ToString(), item.Alias);
            }
            if (!inFrom && item.Expression is Evaluant.NLinq.Expressions.MemberExpression)
            {
                NLinq.Expressions.Expression expression = Visit(item.Expression);
                if (expression is IAliasedExpression)
                    currentAlias = ((IAliasedExpression)expression).Alias;
                return updater.Update(item, expression, currentAlias, item.Type);
            }
            if (item.Expression != null && item.Expression.ExpressionType == NLinq.Expressions.ExpressionTypes.AnonymousParameter)
            {
                NLinq.Expressions.Expression expression = Visit(item.Expression);
                return updater.Update(item, Visit(expression), currentAlias, item.Type);
            }

            return updater.Update(item, item.Expression, currentAlias, item.Type);
        }

    }
}
