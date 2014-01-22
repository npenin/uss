using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Mapping;

namespace Evaluant.Uss.SqlExpressions.Statements
{
    public class AlterTableAddStatement : AlterTableStatement
    {
        public AlterTableAddStatement()
        {
        }

        public override AlterMode AlterMode
        {
            get { return Statements.AlterMode.Add; }
        }

        public string ColumnName { get; set; }
        public string ConstraintName { get; set; }
        public Mapping.Table Table { get; set; }

        public IConstraint Constraint { get; set; }

    }

    public class AlterTableDropStatement : AlterTableStatement
    {
        public AlterTableDropStatement()
        {
        }

        public override AlterMode AlterMode
        {
            get { return Statements.AlterMode.Drop; }
        }

        public string ColumnName { get; set; }
        public string ConstraintName { get; set; }
        public Mapping.Table Table { get; set; }

    }

    public interface IConstraint
    {
    }

    public class DefaultConstraint : IConstraint
    {
        public bool DefaultValue { get; set; }

        public Field Field { get; set; }
    }

    public class PrimaryKeyConstraint : IConstraint
    {
        public PrimaryKeyConstraint()
        {
            Fields = new List<Mapping.Field>();
        }

        public IList<Mapping.Field> Fields { get; set; }
    }

    public class ForeignKeyConstraint : IConstraint
    {
        public ForeignKeyConstraint()
        {
            Fields = new List<Field>();
        }

        public IList<Mapping.Field> Fields { get; set; }

        public Table ReferencesTable { get; set; }
        public IList<Mapping.Field> References { get; set; }

    }
}
