using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using System.Data;

namespace Evaluant.Uss.SqlExpressions
{
    public class Constant : ValueExpression, IDbExpression
    {
        public Constant(object value, DbType type)
            : base(value, GetTypeCode(type))
        {
            this.Type = type;
        }

        #region IDbExpression Members

        public DbExpressionType DbExpressionType
        {
            get { return DbExpressionType.Value; }
        }

        public DbType Type { get; set; }

        #endregion

        public static TypeCode GetTypeCode(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return TypeCode.String;
                case DbType.Binary:
                    return TypeCode.Object;
                case DbType.Boolean:
                    return TypeCode.Boolean;
                case DbType.Byte:
                    return TypeCode.Byte;
                case DbType.Currency:
                    return TypeCode.Decimal;
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                case DbType.Time:
                    return TypeCode.DateTime;
                case DbType.Decimal:
                    return TypeCode.Decimal;
                case DbType.Double:
                    return TypeCode.Double;
                case DbType.Guid:
                    return TypeCode.Object;
                case DbType.Int16:
                    return TypeCode.Int16;
                case DbType.Int32:
                    return TypeCode.Int32;
                case DbType.Int64:
                    return TypeCode.Int64;
                case DbType.Object:
                    return TypeCode.Object;
                case DbType.SByte:
                    return TypeCode.SByte;
                case DbType.Single:
                    return TypeCode.Single;
                case DbType.UInt16:
                    return TypeCode.UInt16;
                case DbType.UInt32:
                    return TypeCode.UInt32;
                case DbType.UInt64:
                    return TypeCode.UInt64;
                case DbType.VarNumeric:
                    return TypeCode.Decimal;
                case DbType.Xml:
                    return TypeCode.String;
                default:
                    return TypeCode.Object;
            }
        }
    }
}
