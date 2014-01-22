using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.OData.UriExpressions
{
    class PropertyExpression : UriExpression
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public PropertyExpression Target { get; set; }

    }
}
