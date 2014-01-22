using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.OData.UriExpressions;

namespace Evaluant.Uss.OData.Visitors
{
    class NLinqVisitor : NLinqVisitors.NLinqExpressionVisitor
    {
        public NLinqVisitor(Edm.Metadata metadata)
        {
            this.metadata = metadata;
            identifiers = new Dictionary<string, PropertyExpression>();
        }

        bool inQueryBodyClauses = false;
        RootContainerExpression uri;
        private Edm.Metadata metadata;
        private IDictionary<string, PropertyExpression> identifiers;
        private StringBuilder constraintBuilder;
        private bool inFrom;
        PropertyExpression currentProperty;
        private bool inWhere;


        public override NLinq.Expressions.QueryBodyClause Visit(NLinq.Expressions.FromClause expression)
        {
            if (!inQueryBodyClauses)
            {
                uri = new RootContainerExpression() { Type = expression.Type };
                identifiers.Add(expression.Identifier.Text, uri);
            }
            else
            {
                inFrom = true;
                currentProperty = new PropertyExpression();
                identifiers.Add(expression.Identifier.Text, currentProperty);
                Visit(expression.Expression);
                inFrom = false;
            }
            return expression;
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.MemberExpression expression)
        {
            if (inWhere)
            {
                Visit(expression.Previous);
                Visit(expression.Statement);

            }
            else if (inFrom)
            {
                Visit(expression.Previous);
                if (currentProperty.Target != null)
                    throw new NotSupportedException();
                currentProperty.Target = new PropertyExpression();
                currentProperty = currentProperty.Target;
                Visit(expression.Statement);
            }
            else
            {
                if (uri.Filter != null || uri.Expand != null || uri.InLineCount != null || uri.Top != null || uri.Skip != null || uri.Select != null)
                    return expression;

                currentProperty.Target = new PropertyExpression();
                currentProperty = currentProperty.Target;
                Visit(expression.Statement);
            }

            return expression;
        }

        protected override NLinq.Expressions.Identifier VisitIdentifier(NLinq.Expressions.Identifier identifier)
        {
            if (inWhere)
            {
                PropertyExpression currentProperty;
                if (identifiers.TryGetValue(identifier.Text, out currentProperty))
                    this.currentProperty = currentProperty;
                else
                {
                    constraintBuilder.Append(identifier.Text);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(currentProperty.Name))
                    currentProperty.Name = identifier.Text;
                else
                    throw new NotSupportedException();
            }
            return identifier;
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.QueryExpression expression)
        {
            base.Visit(expression);
            return uri;
        }

        public override NLinq.Expressions.QueryBodyClause Visit(NLinq.Expressions.WhereClause expression)
        {
            constraintBuilder = new StringBuilder();
            inWhere = true;
            base.Visit(expression);
            inWhere = false;
            uri.Filter = constraintBuilder.ToString();
            return expression;
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.BinaryExpression item)
        {
            Visit(item.LeftExpression);
            switch (item.Type)
            {
                case Evaluant.NLinq.Expressions.BinaryExpressionType.And:
                    constraintBuilder.Append(" and ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Or:
                    constraintBuilder.Append(" or ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.NotEqual:
                    constraintBuilder.Append(" neq ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.LesserOrEqual:
                    constraintBuilder.Append(" lte ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.GreaterOrEqual:
                    constraintBuilder.Append(" gte ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Lesser:
                    constraintBuilder.Append(" lt ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Greater:
                    constraintBuilder.Append(" gt ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Equal:
                    constraintBuilder.Append(" eq ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Minus:
                    constraintBuilder.Append(" min ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Plus:
                    constraintBuilder.Append(" add ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Modulo:
                    constraintBuilder.Append(" mod ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Div:
                    constraintBuilder.Append(" div ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Times:
                    constraintBuilder.Append(" mul ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Pow:
                    constraintBuilder.Append(" pow ");
                    break;
                case Evaluant.NLinq.Expressions.BinaryExpressionType.Unknown:
                    break;
                default:
                    break;
            }
            Visit(item.RightExpression);
            return item;
            //return base.Visit(item);
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.ValueExpression c)
        {
            if (inWhere)
            {
                constraintBuilder.Append(c.Value);
                return c;
            }

            return base.VisitConstant(c);
        }
    }
}
