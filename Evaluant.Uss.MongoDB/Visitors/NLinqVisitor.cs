using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.NLinqVisitors;
using Evaluant.Uss.Domain;
using Evaluant.Uss.MongoDB.Protocol;

namespace Evaluant.Uss.MongoDB.Visitors
{
    public class NLinqVisitor : NLinqExpressionVisitor
    {
        private string database;

        public NLinqVisitor(string database, Model.Model model)
        {
            this.database = database;
            this.model = model;
            identifiers = new Dictionary<string, Entity>();
        }

        bool inWhere;

        Model.Model model;
        Dictionary<string, Entity> identifiers;
        Entity currentEntity;
        Entry currentEntry;
        bool inSelect;
        public QueryMessage Query { get; private set; }

        public override Evaluant.NLinq.Expressions.QueryBodyClause Visit(Evaluant.NLinq.Expressions.FromClause expression)
        {
            Query = new QueryMessage();
            Query.NumberToReturn = 1;
            if (currentEntity == null)
            {
                currentEntity = new Entity(expression.Type);
                identifiers.Add(expression.Identifier.Text, currentEntity);
            }
            else
            {
                currentEntity = new Entity(expression.Type);
                identifiers.Add(expression.Identifier.Text, currentEntity);
            }
            return expression;
        }

        public override Evaluant.NLinq.Expressions.SelectOrGroupClause Visit(Evaluant.NLinq.Expressions.SelectClause expression)
        {
            inSelect = true;
            base.Visit(expression);
            inSelect = false;
            return expression;
        }

        public override Evaluant.NLinq.Expressions.QueryBodyClause Visit(Evaluant.NLinq.Expressions.WhereClause expression)
        {
            inWhere = true;
            Evaluant.NLinq.Expressions.QueryBodyClause result = base.Visit(expression);
            inWhere = false;
            return result;
        }

        protected override Evaluant.NLinq.Expressions.Identifier VisitIdentifier(Evaluant.NLinq.Expressions.Identifier identifier)
        {
            Entity current = currentEntity;
            if (!identifiers.TryGetValue(identifier.Text, out currentEntity))
            {
                if (inSelect)
                {
                    Model.Reference reference;
                    Model.Attribute attribute;
                    if (model.Entities[current.Type].References.TryGetValue(identifier.Text, out reference))
                    {
                        Entry entry = current[identifier.Text];
                        if (entry != null && entry.IsEntity)
                        {
                            currentEntity = (Entity)entry.Value;
                        }
                        else
                        {
                            currentEntity = new Entity(reference.ChildType);
                            current.Add(identifier.Text, currentEntity);
                        }
                    }
                    else if (model.Entities[current.Type].Attributes.TryGetValue(identifier.Text, out attribute))
                    {
                        Query.ReturnFieldSelector = new Entity(current.Type);
                        Query.ReturnFieldSelector.Add(identifier.Text, 1);
                    }
                    else
                        throw new NotSupportedException();
                }
                else
                {
                    Model.Reference reference;
                    if (model.Entities[current.Type].References.TryGetValue(identifier.Text, out reference))
                    {

                        Entry entry;
                        if (current.TryGet(identifier.Text, out entry))
                        {
                            currentEntity = (Entity)entry.Value;
                        }
                        else
                        {
                            currentEntity = new Entity(reference.ChildType);
                            current.Add(identifier.Text, currentEntity);
                        }
                    }
                    else
                    {
                        Model.Attribute attribute;
                        if (model.Entities[current.Type].Attributes.TryGetValue(identifier.Text, out attribute))
                        {
                            if (!current.TryGet(identifier.Text, out currentEntry))
                            {
                                currentEntry = Entry.Create(identifier.Text, State.New, null, attribute.Type);
                                current.Add(currentEntry);
                            }
                        }
                        else
                            throw new NotSupportedException();
                    }
                }
            }
            else if (inSelect)
            {
                Query.Query = currentEntity;
                Query.FullCollectionName = database + "." + currentEntity.Type;
            }
            return identifier;
        }

        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.MethodCall item)
        {
            if (item.Identifier.Text == "First" || item.Identifier.Text == "FirstOrDefault")
            {
                Query.NumberToReturn = 1;
                return item;
            }
            if (item.Identifier.Text == "Count")
            {
                if (inWhere)
                {
                    currentEntry = Entry.Create("$size", State.New, 0, typeof(int));
                    currentEntity.EntityEntries.Add(currentEntry);
                }
                else
                {
                    currentEntity = Query.Query;
                    Query.Query = new DynamicEntity();
                    Query.FullCollectionName = database + ".$cmd";
                    Query.Query.Add(Entry.Create("count", State.New, currentEntity.Type));
                    if (currentEntity.EntityEntries.Count > 0)
                        Query.Query.Add(Entry.Create("query", State.New, currentEntity));
                }
            }
            return base.Visit(item);
        }

        protected override Evaluant.NLinq.Expressions.Expression VisitBinary(Evaluant.NLinq.Expressions.BinaryExpression b)
        {
            object value = null;
            switch (b.Type)
            {
                case Evaluant.NLinq.Expressions.BinaryExpressionType.And:
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Or:
                    return base.VisitBinary(b);
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Equal:
                    if (b.RightExpression is Evaluant.NLinq.Expressions.ValueExpression)
                    {
                        Visit(b.LeftExpression);
                        value = ((Evaluant.NLinq.Expressions.ValueExpression)b.RightExpression).Value;
                    }
                    if (b.LeftExpression is Evaluant.NLinq.Expressions.ValueExpression)
                    {
                        Visit(b.RightExpression);
                        value = ((Evaluant.NLinq.Expressions.ValueExpression)b.LeftExpression).Value;
                    }

                    if (value == null)
                    {
                        currentEntity.Add("$exists", false);
                    }

                    currentEntry.Value = value;
                    return b;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.NotEqual:
                    if (b.RightExpression is Evaluant.NLinq.Expressions.ValueExpression)
                    {
                        Visit(b.LeftExpression);
                        value = ((Evaluant.NLinq.Expressions.ValueExpression)b.RightExpression).Value;
                    }
                    if (b.LeftExpression is Evaluant.NLinq.Expressions.ValueExpression)
                    {
                        Visit(b.RightExpression);
                        value = ((Evaluant.NLinq.Expressions.ValueExpression)b.LeftExpression).Value;
                    }
                    if (value == null)
                    {
                        currentEntity.Add("$exists", true);
                    }
                    return b;
                //    break;
                //case Evaluant.NLinq.Expressions.BinaryExpressionType.LesserOrEqual:
                //    break;
                //case Evaluant.NLinq.Expressions.BinaryExpressionType.GreaterOrEqual:
                //    break;
                //case Evaluant.NLinq.Expressions.BinaryExpressionType.Lesser:
                //    break;
                //case Evaluant.NLinq.Expressions.BinaryExpressionType.Greater:
                //    break;
                //case Evaluant.NLinq.Expressions.BinaryExpressionType.Minus:
                //    break;
                //case Evaluant.NLinq.Expressions.BinaryExpressionType.Plus:
                //    break;
                //case Evaluant.NLinq.Expressions.BinaryExpressionType.Modulo:
                //    break;
                //case Evaluant.NLinq.Expressions.BinaryExpressionType.Div:
                //    break;
                //case Evaluant.NLinq.Expressions.BinaryExpressionType.Times:
                //    break;
                //case Evaluant.NLinq.Expressions.BinaryExpressionType.Pow:
                //    break;
                //case Evaluant.NLinq.Expressions.BinaryExpressionType.Unknown:
                //    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
