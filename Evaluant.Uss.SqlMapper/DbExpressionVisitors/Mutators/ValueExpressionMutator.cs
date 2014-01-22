using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.CommonVisitors;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.SqlMapper;
using System.Data;
using Evaluant.Uss.SqlExpressions.Visitors;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators
{
    public class ValueExpressionMutator : DbExpressionVisitor
    {
        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.ValueExpression item)
        {
            return new Constant(item.Value, DbType.String);
        }
    }
}
