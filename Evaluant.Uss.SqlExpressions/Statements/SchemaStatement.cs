using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Evaluant.Uss.SqlExpressions.Statements
{
    public class SchemaStatement : DbStatement
    {
        private StatementType statementType;
        private string p;
        public SchemaStatement(StatementType statementType)
        {
            this.statementType = statementType;
        }

        public SchemaStatement(System.Data.StatementType statementType, string name)
            : this(statementType)
        {
            this.Name = name;
        }

        public override System.Data.StatementType StatementType
        {
            get { return statementType; }
        }

        public override DbExpressionType DbExpressionType
        {
            get { return SqlExpressions.DbExpressionType.Schema; }
        }

        public string Name { get; set; }
    }
}
