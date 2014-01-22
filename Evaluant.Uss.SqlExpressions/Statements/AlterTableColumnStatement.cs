using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions.Statements
{
    public class AlterTableColumnStatement : AlterTableStatement
    {
        public override AlterMode AlterMode
        {
            get { return Statements.AlterMode.Column; }
        }
    }
}
