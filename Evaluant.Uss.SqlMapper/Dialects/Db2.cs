using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;
using Evaluant.Uss.SqlExpressions;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.SqlMapper.Dialects
{
    public class Db2 : Dialect
    {
        private const string SMALLINT = "SMALLINT";
        private const string INTEGER = "INTEGER";
        private const string BIGINT = "BIGINT";
        private const string REAL = "REAL";
        private const string DOUBLE_PRECISION = "DOUBLE PRECISION";
        private const string FLOAT = "FLOAT";
        private const string DECIMAL = "DECIMAL";
        private const string DATE = "DATE";
        private const string TIME = "TIME";
        private const string TIMESTAMP = "TIMESTAMP";
        private const string CHAR = "CHAR";
        private const string VARCHAR = "VARCHAR";
        private const string LONG_VARCHAR = "LONG VARCHAR";
        private const string CHAR_FOR_BIT_DATA = "CHAR FOR BIT DATA";
        private const string VARCHAR_FOR_BIT_DATA = "VARCHAR FOR BIT DATA";
        private const string LONG_VARCHAR_FOR_BIT_DATA = "LONG VARCHAR FOR BIT DATA";
        private const string GRAPHIC = "GRAPHIC";
        private const string VARGRAPHIC = "VARGRAPHIC";
        private const string LONG_GRAPHIC = "LONG GRAPHIC";
        private const string CLOB = "CLOB";
        private const string BLOB = "BLOB";
        private const string DBCLOB = "DBCLOB";
        private const string FETCHFIRST = "FETCH FIRST ";
        private const string ROW = "ROW";
        private const string ONLY = " ONLY";
        private const string CAST = "CAST";

        //public override int MaxAnsiStringSize
        //{
        //    get { return 32700; }
        //}

        //public override int MaxBinarySize
        //{
        //    get { return 32700; }
        //}

        //public override int MaxBinaryBlobSize
        //{
        //    get { return 2147483647; }
        //}

        //public override int MaxStringClobSize
        //{
        //    get { return 32700; }
        //}

        //public override int MaxStringSize
        //{
        //    get { return 32700; }
        //}

        //public override int GetMaxIdentifierLength()
        //{
        //    return 21;
        //}

        public override void Visit(SqlExpressions.Mapping.Field field)
        {
            if (field.DbType == DbType.Guid)
                field.Size = 36;
            writer.Write(beginEscape);
            writer.Write(field.ColumnName);
            writer.Write(endEscape);
            writer.Write(" ");
            writer.Write(beginEscape);
            TypeName(field.DbType, field.Size);
            writer.Write(endEscape);
            if (field.Precision != 0 && field.Scale != 0)
                writer.Write("({0}, {1}) ", field.Precision, field.Scale);
            if (field.Size != 0)
                writer.Write("({0})", field.Size);
            if (field.IsIdentity)
                writer.Write(" IDENTITY(START WITH {0}, INCREMENT BY {1}) ", 1, 1);
            writer.Write(field.IsNullable ? "NULL" : "NOT NULL");
        }

        private void TypeName(System.Data.DbType dbType, int length)
        {
            switch (dbType)
            {
                case DbType.String:
                case System.Data.DbType.AnsiString:
                    if (length == 0)
                        writer.Write(CLOB);
                    writer.Write(VARCHAR);
                    break;
                case System.Data.DbType.AnsiStringFixedLength:
                case System.Data.DbType.StringFixedLength:
                    writer.Write(CHAR);
                    break;
                case System.Data.DbType.Byte:
                case System.Data.DbType.Binary:
                    if (length == 0)
                        writer.Write(BLOB);
                    if (length <= 254)
                        writer.Write(CHAR_FOR_BIT_DATA);
                    writer.Write(BLOB);
                    break;
                case System.Data.DbType.Boolean:
                    writer.Write(CHAR_FOR_BIT_DATA);
                    break;
                case System.Data.DbType.Currency:
                    writer.Write(DECIMAL);
                    break;
                case System.Data.DbType.Date:
                    writer.Write(DATE);
                    break;
                case System.Data.DbType.DateTime:
                    writer.Write(TIMESTAMP);
                    break;
                case System.Data.DbType.Decimal:
                case DbType.Single:
                    writer.Write(FLOAT);
                    break;
                case System.Data.DbType.Double:
                    writer.Write(DOUBLE_PRECISION);
                    break;
                case System.Data.DbType.Guid:
                    writer.Write(CHAR);
                    break;
                case System.Data.DbType.Int16:
                    writer.Write(SMALLINT);
                    break;
                case System.Data.DbType.Int32:
                    writer.Write(INTEGER);
                    break;
                case System.Data.DbType.Int64:
                    writer.Write(BIGINT);
                    break;
                case System.Data.DbType.Object:
                    writer.Write(BLOB);
                    break;
                case System.Data.DbType.Time:
                    writer.Write(TIME);
                    break;
            }
        }

        public override string NullColumnString
        {
            get { return "NOT NULL "; }
        }

        public override bool SupportsIdentityColumns
        {
            get { return true; }
        }

        public override string GetIdentitySelect(string tableAlias)
        {
            return "SELECT @@IDENTITY ";
        }

        public override string GetDummyTableForScalarResults
        {
            get { return " FROM SYSIBM.SYSDUMMY1"; }
        }

        public override string IdentityColumnString
        {
            get { return "IDENTITY "; }
        }

        public override string DefaultValuestString
        {
            get { return "DEFAULT VALUES "; }
        }

        protected override void Top(int top)
        {
            writer.Write(FETCHFIRST);
            if (top > 1)
                writer.Write(top + " ");
            writer.Write(ROW);
            if (top > 1)
                writer.Write("S");
            writer.Write(ONLY);
        }

        public override Expression Visit(Constant constant)
        {
            if (constant.Value == DBNull.Value || constant.Value == null)
            {
                writer.Write("CAST (NULL AS ");
                TypeName(constant.Type, 0);
                writer.Write(")");
            }
            else
            {
                switch (constant.Type)
                {
                    case DbType.Date:
                        writer.Write('\'');
                        writer.Write(DateTime.Parse(constant.Value.ToString()).ToString("yyyy-MM-dd", Culture));
                        writer.Write('\'');
                        break;
                    case DbType.Time:
                        writer.Write('\'');
                        writer.Write(DateTime.Parse(constant.Value.ToString()).ToString("HH.mm.ss", Culture));
                        writer.Write('\'');
                        break;

                    default:

                        base.Visit(constant);
                        break;
                }

            }
        }

        public override AliasedExpression Visit(Exists predicate)
        {
            if (predicate.Parameters[0] is UnionStatement)
            {
                writer.Write('(');

                bool firstElement = true;

                UnionStatement union = predicate.Parameters[0] as UnionStatement;
                foreach (SelectStatement select in union.SelectExpressions)
                {
                    if (!firstElement)
                        writer.Write(" OR ");
                    writer.Write("EXISTS (");

                    Visit(select);
                    writer.Write(')');
                    firstElement = false;
                }
                writer.Write(')');
            }
            else
            {
                writer.Write(" EXISTS (");
                VisitEnumerable(predicate.Parameters, Visit);
                writer.Write(')');
            }
        }

        public override void Visit(AggregateFunction function)
        {
            if (function.Type == AggregateFunctionEnum.Avg)
            {
                _Query.Append(AVG).Append(OPENBRACE);
                _Query.Append(CAST).Append(OPENBRACE);
                function.ValueExpression.Accept(this);
                _Query.Append(AS).Append(DOUBLE_PRECISION);
                _Query.Append(CLOSEBRACE).Append(CLOSEBRACE);
            }
            else
                base.Visit(function);
        }

        public override SqlExpressions.IDbExpression FindFK(string schema, string fk, SqlExpressions.Mapping.Table table)
        {
            if (string.IsNullOrEmpty(table.Schema))
                return new SqlExpressions.HardCodedExpression(
                @"SELECT 1 
                  FROM syscat.references
                  WHERE object_id = OBJECT_ID(N'[" + schema + "].[" + fk + @"]') 
                  AND parent_object_id=OBJECT_ID(N'" + table.TableName + "')");

            return new SqlExpressions.HardCodedExpression(
            @"SELECT 1 
                  FROM syscat.references
                  WHERE fk_name = '" + fk + @"'  
                  AND tabname='" + table.TableName + "'");

        }

        public override SqlExpressions.IDbExpression SchemaExists(string schema)
        {
            throw new NotImplementedException();
        }

        public override NLinq.Expressions.Expression FindTable(SqlExpressions.Mapping.Table table)
        {
            throw new NotImplementedException();
        }
    }
}
