using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.SqlExpressions
{
    public class DropTableStatement : DbStatement, IVisitable<DropTableStatement>
    {
        public TableSourceExpression Table { get; private set; }

        public DropTableStatement(Mapping.Table table)
        {
            Table = new TableSourceExpression(null, table);
        }


        #region IVisitable<DropTableStatement> Members

        public void Accept(IVisitor<DropTableStatement> visitor)
        {
            visitor.Visit(this);
        }

        #endregion

        public override System.Data.StatementType StatementType
        {
            get { return System.Data.StatementType.Batch; }
        }

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Drop; }
        }
    }
}
