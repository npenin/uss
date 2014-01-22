using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class RowNumber : WindowFunction.RankingWindowFunction
    {
        private static Identifier Name = new Identifier("row_number");

        public RowNumber(OrderByClause order)
            :base(Name, order)
        {

        }

        public override FunctionType FunctionType
        {
            get { return SqlExpressions.FunctionType.RowNumber; }
        }
    }
}
