using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.CommonVisitors;

namespace Evaluant.Uss.Xml.Expressions
{
    public class XPathQuery : Expression, IXmlExpression
    {
        public ICollection<XPathFragment> Fragments { get; set; }

        public XmlExpressionTypes Type
        {
            get { return XmlExpressionTypes.Query; }
        }

        public override void Accept(NLinqVisitor visitor)
        {
        }

        public override ExpressionTypes ExpressionType
        {
            get { return ExpressionTypes.Unknown; }
        }

        public override string ToString()
        {
            return XPathWriter.Render(this);
        }

        public bool IsRooted { get; set; }
    }
}
