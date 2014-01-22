using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.PersistenceEngine.Contracts.CommonVisitors
{
    public class RelationshipQueryCreator : NLinqVisitors.NLinqExpressionVisitor
    {
        Identifier item = new Identifier("it");
        public RelationshipQueryCreator(Model.Entity target, QueryExpression query)
        {
            this.target = target;
            this.Query = new QueryExpression(
                new FromClause(target.Type, item, query),
                new QueryBody(
                    new ClauseList(),
                    new SelectClause(item),
                    null)
                );
        }

        public QueryExpression Query { get; set; }

        Model.Entity target;
        Model.Entity tempTarget;
        Identifier tempIdentifier;

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.MemberExpression expression)
        {
            bool changeTempTarget = tempTarget == null;
            if (changeTempTarget)
            {
                tempTarget = target;
                tempIdentifier = Query.From.Identifier;
            }
            var result = base.Visit(expression);
            if (changeTempTarget)
            {
                tempTarget = null;
                tempIdentifier = null;
            }
            return result;
        }

        public override Expression Visit(Identifier identifier)
        {
            Model.Reference reference;
            if (!tempTarget.References.TryGetValue(identifier.Text, out reference))
                throw new KeyNotFoundException("The reference " + identifier.Text + " could not be found on type " + tempTarget.Type);
            tempTarget = reference.Target;
            if (reference.Cardinality.IsMany)
            {
                tempIdentifier = new Identifier("e" + Query.QueryBody.Clauses.Count);
                Query.QueryBody.Clauses.Add(new FromClause(reference.Target.Type, tempIdentifier, new MemberExpression(new Identifier(reference.Name), ((SelectClause)Query.QueryBody.SelectOrGroup).Expression)));
                ((SelectClause)Query.QueryBody.SelectOrGroup).Expression = tempIdentifier;
            }
            else
            {
                ((SelectClause)Query.QueryBody.SelectOrGroup).Expression = new MemberExpression(new Identifier(reference.Name), ((SelectClause)Query.QueryBody.SelectOrGroup));
            }
            return base.Visit(identifier);
        }

    }
}
