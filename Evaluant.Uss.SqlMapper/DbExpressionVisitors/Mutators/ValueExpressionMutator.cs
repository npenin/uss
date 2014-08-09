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
        private IDriver driver;
        public ValueExpressionMutator(IDriver driver)
        {
            if (driver == null)
                driver = new Drivers.Driver(null);
            this.driver = driver;
        }

        public override Evaluant.NLinq.Expressions.Expression Visit(Evaluant.NLinq.Expressions.ValueExpression item)
        {
            if (item.Value != null && driver != null)
                return new Constant(item.Value, driver.GetDbType(item.Value.GetType()));
            return new Constant(item.Value, DbType.String);
        }
    }
}
