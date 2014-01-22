using Evaluant.NLinq.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Xml.Expressions
{
    public class XPathFragment : Expression, IXmlExpression
    {
        public string Name { get; set; }

        public BinaryExpression Constraint { get; set; }

        public override void Accept(NLinqVisitor visitor)
        {
        }

        public override ExpressionTypes ExpressionType
        {
            get { return ExpressionTypes.Unknown; }
        }

        public XmlExpressionTypes Type
        {
            get { return XmlExpressionTypes.Fragment; }
        }

        public override string ToString()
        {
            return XPathWriter.Render(this);
        }
    }
}
