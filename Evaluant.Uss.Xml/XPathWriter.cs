using Evaluant.NLinq.Expressions;
using Evaluant.Uss.CommonVisitors;
using Evaluant.Uss.Xml.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Evaluant.Uss.Xml
{
    public class XPathWriter : ExpressionWriter<ExpressionUpdater>, IXmlExpressionVisitor
    {
        public XPathWriter(TextWriter tw)
            : base(tw)
        {
        }

        public static string Render(IXmlExpression expression)
        {
            StringBuilder sb = new StringBuilder();
            var writer = new XPathWriter(new StringWriter(sb));
            writer.Visit(expression);
            return sb.ToString();
        }

        protected override string GetOperator(BinaryExpressionType type)
        {
            switch (type)
            {
                case BinaryExpressionType.And:
                    return " and ";
                case BinaryExpressionType.Or:
                    return " or ";
                case BinaryExpressionType.NotEqual:
                    return " != ";
                case BinaryExpressionType.LesserOrEqual:
                    return " &lte; ";
                case BinaryExpressionType.GreaterOrEqual:
                    return " &gte; ";
                case BinaryExpressionType.Lesser:
                    return " &lt; ";
                case BinaryExpressionType.Greater:
                    return " &gt; ";
                case BinaryExpressionType.Equal:
                    return " = ";
                case BinaryExpressionType.Minus:
                    return " - ";
                case BinaryExpressionType.Plus:
                    return " + ";
                case BinaryExpressionType.Modulo:
                    return " % ";
                case BinaryExpressionType.Div:
                    return " / ";
                case BinaryExpressionType.Times:
                    return " * ";
                case BinaryExpressionType.Pow:
                case BinaryExpressionType.Unknown:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
        }

        public override Expression Visit(Expression exp)
        {
            if (exp.ExpressionType == ExpressionTypes.Unknown)
            {
                IXmlExpression expression = exp as IXmlExpression;
                if (expression != null)
                    return (Expression)Visit(expression);
            }
            return base.Visit(exp);
        }

        public IXmlExpression Visit(IXmlExpression item)
        {
            switch (item.Type)
            {
                case XmlExpressionTypes.Query:
                    return Visit((XPathQuery)item);
                case XmlExpressionTypes.Fragment:
                    return Visit((XPathFragment)item);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Expressions.XPathQuery Visit(Expressions.XPathQuery item)
        {
            if (item.IsRooted)
                Write("/");
            bool isfirst = true;
            foreach (XPathFragment fragment in item.Fragments)
            {
                if (isfirst)
                    isfirst = false;
                else
                    Write("/");
                Visit(fragment);
            }
        }

        public Expressions.XPathFragment Visit(Expressions.XPathFragment item)
        {
            Write(item.Name);
            if (item.Constraint != null)
                Visit(item.Constraint);
        }
    }
}
