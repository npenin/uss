using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.NLinqVisitors;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.PersistenceEngine.Contracts.CommonVisitors
{
    public class InferRelationshipQueryAnalyzer : NLinqExpressionVisitor
    {
        //private bool visitingSelect;
        private Identifier identifierToFind;
        public string Reference { get; set; }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.QueryExpression expression)
        {
            Visit(expression.QueryBody.SelectOrGroup);
            return base.Visit(expression);
        }

        public override NLinq.Expressions.SelectOrGroupClause Visit(NLinq.Expressions.SelectClause expression)
        {
            if (expression.Expression.ExpressionType == NLinq.Expressions.ExpressionTypes.Identifier)
            {
                identifierToFind = (Identifier)expression.Expression;
                return expression;
            }

            //visitingSelect = true;
            base.Visit(expression);
            //visitingSelect = false;
            return expression;
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.Identifier identifier)
        {
            return base.Visit(identifier);
        }
    }
}
