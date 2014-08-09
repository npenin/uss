using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.NLinqImprovements;
using Evaluant.Uss.SqlExpressions.Visitors;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    public class IdentifierToEntityMutator : DbExpressionVisitor, IEqualityComparer<Evaluant.NLinq.Expressions.Identifier>
    {
        IDictionary<Evaluant.NLinq.Expressions.Identifier, EntityExpression> entities;

        bool inFrom = false;

        public IdentifierToEntityMutator()
        {
            entities = new Dictionary<Evaluant.NLinq.Expressions.Identifier, EntityExpression>(this);
        }

        public override Evaluant.NLinq.Expressions.SelectOrGroupClause Visit(Evaluant.NLinq.Expressions.SelectClause expression)
        {
            Evaluant.NLinq.Expressions.SelectClause select = (Evaluant.NLinq.Expressions.SelectClause)base.Visit(expression);
            Evaluant.NLinq.Expressions.Identifier identifier = select.Expression as Evaluant.NLinq.Expressions.Identifier;
            if (identifier != null)
            {
                if (!entities.ContainsKey(identifier))
                    throw new NotSupportedException(string.Format("No entity identified by {0} could be found", identifier.Text));
                select.Expression = new EntityReferenceExpression(entities[identifier]);
            }
            return select;
        }

        public override Evaluant.NLinq.Expressions.QueryBodyClause Visit(Evaluant.NLinq.Expressions.FromClause expression)
        {
            inFrom = true;
            EntityExpression entity = new EntityExpression(new TableAlias());
            if (!string.IsNullOrEmpty(expression.Type))
                entity.Type = expression.Type;
            entities.Add(expression.Identifier, entity);
            Evaluant.NLinq.Expressions.FromClause from = (Evaluant.NLinq.Expressions.FromClause)base.Visit(expression);
            if (expression.Expression == null)
                entity.Expression = expression.Identifier;
            else if (from.Expression.ExpressionType != NLinq.Expressions.ExpressionTypes.Quote)
                entity.Expression = from.Expression;
            else
                entity.Expression = ((NLinq.Expressions.SelectClause)((NLinq.Expressions.QueryExpression)from.Expression).QueryBody.SelectOrGroup).Expression;
            //entity.Expression = from.Expression;
            inFrom = false;
            return from;
        }

        protected override NLinq.Expressions.AnonymousParameter VisitAnonymousParameter(NLinq.Expressions.AnonymousParameter parameter)
        {
            return updater.Update(parameter, parameter.Identifier, Visit(parameter.Expression));
        }

        protected override Evaluant.NLinq.Expressions.Identifier VisitIdentifier(Evaluant.NLinq.Expressions.Identifier identifier)
        {
            if (entities.ContainsKey(identifier))
                return new EntityIdentifier(identifier, entities[identifier]);
            return base.VisitIdentifier(identifier);
        }

        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.MemberExpression expression)
        {
            if (expression.Previous != null)
            {
                if (expression.Statement.ExpressionType == NLinq.Expressions.ExpressionTypes.Identifier)
                {
                    var previous = expression.Previous;
                    previous = Visit(previous);
                    //if (!inFrom && previous != null && previous.ExpressionType == NLinq.Expressions.ExpressionTypes.Identifier && previous is EntityIdentifier)
                    //    return updater.Update(expression, new EntityReferenceExpression(((EntityIdentifier)previous).Entity), expression.Statement);
                    return updater.Update(expression, previous, expression.Statement);
                }

                return updater.Update(expression, Visit(expression.Previous), Visit(expression.Statement));
            }
            return Visit(expression.Statement);
        }

        #region IEqualityComparer<Identifier> Members

        public bool Equals(Evaluant.NLinq.Expressions.Identifier x, Evaluant.NLinq.Expressions.Identifier y)
        {
            return EqualityComparer<string>.Default.Equals(x.Text, y.Text);
        }

        public int GetHashCode(Evaluant.NLinq.Expressions.Identifier obj)
        {
            return obj.Text.GetHashCode();
        }

        #endregion
    }
}
