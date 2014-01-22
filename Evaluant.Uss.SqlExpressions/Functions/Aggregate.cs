using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public enum AggregateFunctionType { Unknown = 0, Avg, Max, Min, Sum, Count }

    public class Aggregate : Function, IDbExpression
    {
        public Aggregate(AggregateFunctionType type, params Expression[] expression)
            : this(GetMethodName(type), expression)
        {

        }

        public Aggregate(Identifier methodName, params Expression[] expression)
            : base(methodName, expression)
        {
        }

        public Aggregate(TableAlias alias, Identifier methodName, params Expression[] expression)
            : base(alias, methodName, expression)
        {
        }

        public static readonly Identifier Avg = new Identifier("AVG");
        public static readonly Identifier Max = new Identifier("MAX");
        public static readonly Identifier Min = new Identifier("MIN");
        public static readonly Identifier Sum = new Identifier("SUM");
        public static readonly Identifier Count = new Identifier("COUNT");

        public static Identifier GetMethodName(AggregateFunctionType type)
        {
            switch (type)
            {
                case AggregateFunctionType.Avg:
                    return Avg;
                case AggregateFunctionType.Max:
                    return Max;
                case AggregateFunctionType.Min:
                    return Min;
                case AggregateFunctionType.Sum:
                    return Sum;
                case AggregateFunctionType.Count:
                    return Count;
            }
            return null;
        }

        #region IDbExpression Members

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Aggregate; }
        }

        #endregion

        public override FunctionType FunctionType
        {
            get { return FunctionType.Aggregate; }
        }
    }
}
