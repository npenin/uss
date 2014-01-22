using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Evaluant.Uss.SqlExpressions
{
    public class DbParameter : NLinq.Expressions.Parameter, IDbExpression
    {
        public DbType Type { get; set; }
        public ParameterDirection Direction { get; set; }

        public DbParameter(string name, DbType type, ParameterDirection direction)
            : base(name)
        {
            Type = type;
            Direction = direction;
        }

        #region IDbExpression Members

        public DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Parameter; }
        }

        #endregion

        public byte Precision { get; set; }

        public byte Scale { get; set; }

        public int Size { get; set; }
    }
}
