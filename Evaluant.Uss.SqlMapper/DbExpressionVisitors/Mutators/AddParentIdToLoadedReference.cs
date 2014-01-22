using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.NLinqVisitors;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    public class AddParentIdToLoadedReference : NLinqExpressionVisitor
    {
        private Model.Entity target;
        private Identifier targetIdentifier;
        public AddParentIdToLoadedReference(Model.Entity target)
        {
            this.target = target;
        }

        public override QueryBodyClause Visit(FromClause expression)
        {
            if (expression.Type == target.Type && targetIdentifier == null)
                targetIdentifier = expression.Identifier;
            return base.Visit(expression);
        }

        public override NLinq.Expressions.SelectOrGroupClause Visit(NLinq.Expressions.SelectClause expression)
        {
            AnonymousParameterList parameters = new AnonymousParameterList();
            foreach (var attribute in target.Attributes)
            {
                if (!attribute.Value.IsId)
                    continue;
                parameters.Add(new AnonymousParameter(attribute.Key, new MemberExpression(new Identifier(attribute.Key), targetIdentifier)));
            }
            parameters.Add(new AnonymousParameter("Entity", expression.Expression));
            return updater.Update(expression, new AnonymousNew(null, parameters));
        }
    }
}
