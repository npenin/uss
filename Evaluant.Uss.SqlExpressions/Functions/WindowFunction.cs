using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public enum WindowFunctionType
    {
        Ranking,
        Aggregate,
    }

    public abstract class WindowFunction : Function
    {
        public abstract class RankingWindowFunction : WindowFunction
        {
            protected RankingWindowFunction(Identifier identifier, OrderByClause order)
                : base(identifier, order, null)
            {

            }

            public override WindowFunctionType WindowType
            {
                get { return WindowFunctionType.Ranking; }
            }
        }

        public abstract class AggreggateWindowFunction : WindowFunction
        {

            public AggreggateWindowFunction(Identifier identifier, OrderByClause order, IEnumerable<IAliasedExpression> partition)
                : base(identifier, order, partition)
            {

            }

            public override WindowFunctionType WindowType
            {
                get { return WindowFunctionType.Aggregate; }
            }

            public override FunctionType FunctionType
            {
                get { throw new NotImplementedException(); }
            }
        }

        public abstract WindowFunctionType WindowType { get; }

        public OrderByClause OverOrder { get; private set; }

        private List<IAliasedExpression> partitionBy;

        public WindowFunction(Identifier identifier, OrderByClause order, IEnumerable<IAliasedExpression> partition)
            : base(identifier)
        {
            OverOrder = order;
            if (partition != null)
                partitionBy = new List<IAliasedExpression>(partition);
        }

        public List<IAliasedExpression> PartitionBy
        {
            get
            {
                if (WindowType != WindowFunctionType.Ranking)
                    throw new NotSupportedException("Another window type than ranking does not support partitions");
                if (partitionBy == null)
                    partitionBy = new List<IAliasedExpression>();
                return partitionBy;
            }
        }
    }
}
