using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions.Statements
{
    public enum AlterMode
    {
        Column,
        Add,
        Drop,
        Check,
        Uncheck,
        Enable,
        Disable,
        Switch,
        Set,
        Rebuilb,
    }
    public abstract class AlterTableStatement : DbStatement
    {
        public override System.Data.StatementType StatementType
        {
            get { return System.Data.StatementType.Update; }
        }

        public override DbExpressionType DbExpressionType
        {
            get { return SqlExpressions.DbExpressionType.Alter; }
        }

        public abstract AlterMode AlterMode { get; }
    }
}
