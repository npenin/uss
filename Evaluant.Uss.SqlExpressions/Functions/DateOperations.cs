using Evaluant.NLinq.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evaluant.Uss.SqlExpressions.Functions
{
    public class DatePart : Function
    {
        public static readonly Identifier DatePartIdentifier = new Identifier("DATEPART");
        public static readonly Identifier Year = new Identifier("yy");
        public static readonly Identifier Quarter = new Identifier("qq");
        public static readonly Identifier Month = new Identifier("mm");
        public static readonly Identifier DayOfYear = new Identifier("dy");
        public static readonly Identifier Day = new Identifier("dd");
        public static readonly Identifier Week = new Identifier("wk");
        public static readonly Identifier WeekDay = new Identifier("dw");
        public static readonly Identifier Hour = new Identifier("hh");
        public static readonly Identifier Minute = new Identifier("mi");
        public static readonly Identifier Second = new Identifier("ss");
        public static readonly Identifier Millisecond = new Identifier("ms");
        public static readonly Identifier Microsecond = new Identifier("mcs");
        public static readonly Identifier Nanosecond = new Identifier("ns");

        public DatePart(Identifier part, Expression expressionToCast)
            : base(DatePartIdentifier, new Expression[] { part, expressionToCast })
        {
        }

        public override FunctionType FunctionType
        {
            get { return FunctionType.DatePart; }
        }
    }
    public class DateAdd : Function
    {
        public static readonly Identifier DateAddIdentifier = new Identifier("DATEADD");

        public DateAdd(Identifier identifier, Expression expression, Expression result)
            : base(DateAddIdentifier, identifier, expression, result)
        {
        }

        public override FunctionType FunctionType
        {
            get { return FunctionType.DateAdd; }
        }
    }
}
