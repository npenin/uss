using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Xml.Expressions
{
    public enum XmlExpressionTypes
    {
        Query,
        Fragment,
    }

    public interface IXmlExpression
    {
        XmlExpressionTypes Type { get; }
    }
}
