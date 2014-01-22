using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.SqlMapper.Dialects
{
    public class TraceDialect : DialectProxy
    {
        public TraceDialect(IDialect dialect)
            : base(dialect)
        {
        }

        public override string Render(Evaluant.NLinq.Expressions.Expression expression)
        {
            string renderedCommand = base.Render(expression);
            Trace.WriteLine(renderedCommand);
            return renderedCommand;
        }
    }
}
