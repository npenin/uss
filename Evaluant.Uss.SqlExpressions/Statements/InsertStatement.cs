using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlExpressions
{
    public class InsertStatement : DbStatement
    {
        public IDictionary<ColumnExpression, NLinq.Expressions.Expression> Values { get; set; }
        public SelectStatement Select { get; set; }

        private ValuedParameter identity;

        public ValuedParameter Identity
        {
            get { return identity; }
            set
            {
                if (identity != null)
                    Parameters.Remove(identity.Name);
                identity = value;
                if (identity != null)
                    Parameters.Add(identity.Name, identity);
            }
        }


        public InsertStatement(TableExpression table, SelectStatement statement)
            : this(table)
        {
            Select = statement;
        }

        public InsertStatement(TableExpression table, IDictionary<ColumnExpression, NLinq.Expressions.Expression> values)
            : this(table)
        {
            Values = values;
        }

        private InsertStatement(TableExpression table)
        {
            this.Table = table;
        }

        public TableExpression Table { get; set; }

        public override System.Data.StatementType StatementType
        {
            get { return System.Data.StatementType.Insert; }
        }

        public override DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Insert; }
        }
    }
}
