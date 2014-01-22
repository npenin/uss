using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Mapping;
using Evaluant.Uss.SqlExpressions.Statements;

namespace Evaluant.Uss.SqlExpressions
{
    public class CreateTableStatement : DbStatement
    {
        public Table Table { get; private set; }

        public IList<Field> Columns { get; private set; }


        public CreateTableStatement(Table table)
        {
            Table = table;
            Columns = new List<Field>(table.Fields.Values);
            Constraints = new List<Evaluant.NLinq.Expressions.Expression>();
            var primaryKey = new AlterTableAddStatement() { ConstraintName = "PK_" + table.TableName, Table = table, Constraint = new PrimaryKeyConstraint() };
            foreach (Field f in Columns)
            {
                if (!string.IsNullOrEmpty(f.DefaultValue))
                {
                    Constraints.Add(new AlterTableAddStatement() { Table = table, ColumnName = f.ColumnName.Text, Constraint = new DefaultConstraint() { Field = f, DefaultValue = true }, ConstraintName = "DF_" + Table.TableName + '_' + f.ColumnName.Text });
                }
                if (f.IsPrimaryKey)
                    ((PrimaryKeyConstraint)primaryKey.Constraint).Fields.Add(f);
            }

            if (((PrimaryKeyConstraint)primaryKey.Constraint).Fields.Count > 0)
                Constraints.Add(primaryKey);

        }

        public override System.Data.StatementType StatementType
        {
            get { return System.Data.StatementType.Batch; }
        }

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Create; }
        }

        public List<Evaluant.NLinq.Expressions.Expression> Constraints { get; set; }
    }
}
