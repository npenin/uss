using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.Xml
{
    class XPathTransformer : NLinqVisitors.NLinqExpressionVisitor
    {
        private Model.Model model;
        private bool isFirstFrom;
        //Cannot use another dictionnary, because the enumeration may not return the items 
        //in the order they were added
        private readonly Dictionary<string, NLinq.Expressions.BinaryExpression> identifiers;
        private bool inWhere;
        private string currentIdentifier;

        public XPathTransformer(Model.Model model)
        {
            this.model = model;
            isFirstFrom = true;
            identifiers = new Dictionary<string, NLinq.Expressions.BinaryExpression>();
        }

        public override NLinq.Expressions.QueryBodyClause Visit(NLinq.Expressions.FromClause expression)
        {
            if (isFirstFrom)
            {
                isFirstFrom = false;
                identifiers.Add(expression.Identifier.Text, null);
            }
            return expression;
        }

        public override NLinq.Expressions.QueryBodyClause Visit(NLinq.Expressions.WhereClause expression)
        {
            bool wasInWhere = inWhere;
            inWhere = true;
            var result = base.Visit(expression);
            inWhere = wasInWhere;
            return result;
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.BinaryExpression item)
        {
            switch (item.Type)
            {
                case BinaryExpressionType.And:
                case BinaryExpressionType.Or:
                    return base.Visit(item);
                default:
                    string oldIdentifier = currentIdentifier;
                    currentIdentifier = null;
                    Visit(item.LeftExpression);
                    if (currentIdentifier == null)
                        Visit(item.RightExpression);

                    BinaryExpression constraint;
                    if (!identifiers.TryGetValue(currentIdentifier, out constraint))
                        identifiers[currentIdentifier] = item;
                    else
                        identifiers[currentIdentifier] = new BinaryExpression(BinaryExpressionType.And, constraint, item);
                    return item;
            }
        }
    }
}
