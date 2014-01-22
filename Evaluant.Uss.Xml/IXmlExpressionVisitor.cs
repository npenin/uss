using Evaluant.Uss.Domain;
using Evaluant.Uss.Xml.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Xml
{
    public interface IXmlExpressionVisitor : 
        IVisitor<IXmlExpression>,
        IVisitor<XPathQuery>,
        IVisitor<XPathFragment>
    {
    }
}
