using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Evaluant.Uss.SqlExpressions
{
    public class ValuedParameter : DbParameter
    {
        public virtual object Value { get; set; }

        public ValuedParameter(string name, object value)
            : this(name, value, DbType.String, ParameterDirection.Input)
        {
        }

        public ValuedParameter(string name, object value, DbType dbType)
            : this(name, value, dbType, ParameterDirection.Input)
        {
        }

        public ValuedParameter(string name, object value, ParameterDirection direction)
            : this(name, value, DbType.String, direction)
        {
        }

        public ValuedParameter(string name, object value, DbType dbType, ParameterDirection direction)
            : base(name, dbType, direction)
        {
            if (value != null)
                Value = value;
        }

    }
}
