using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions
{
    public class HardCodedExpression : DbExpression
    {
        public HardCodedExpression(string sql)
        {
            Sql = sql;
        }

        public override DbExpressionType DbExpressionType
        {
            get { return SqlExpressions.DbExpressionType.HardCoded; }
        }

        public string Sql { get; private set; }
    }
}
