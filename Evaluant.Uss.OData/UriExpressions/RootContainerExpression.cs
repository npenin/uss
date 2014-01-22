using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.OData.UriExpressions
{
    class RootContainerExpression : PropertyExpression
    {
        public string Type { get; set; }

        public string Filter { get; set; }
        public string Top { get; set; }
        public string Skip { get; set; }
        public string Expand { get; set; }
        public string Format { get; set; }
        public string Select { get; set; }

        public InLineCount? InLineCount { get; set; }
    }
}
