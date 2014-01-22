using System;
using System.Collections.Specialized;
using System.Data;
using SQLObject;
using Evaluant.Uss.SqlMapper.SqlObjectModel.LDD;

namespace Evaluant.Uss.SqlMapper
{
    /// <summary>
    /// Description résumée de SqlServerDialect.
    /// </summary>
    public class MsSqlDialect : DBDialect
    {
        public MsSqlDialect()
        {
        }

        protected static string NOCHECK = "NOCHECK ";
        protected static string CHECK = "CHECK ";
        protected static string ALL = "ALL ";

        protected static string TEXT = "TEXT";
        protected static string VARCHAR = "VARCHAR";
        protected static string CHAR = "CHAR";
        protected static string NTEXT = "NTEXT";
        protected static string NVARCHAR = "NVARCHAR";
        protected static string NCHAR = "NCHAR";
        protected static string BIT = "BIT";
        protected static string TINYINT = "TINYINT";
        protected static string SMALLINT = "SMALLINT";
        protected static string INT = "INT";
        protected static string BIGINT = "BIGINT";
        protected static string REAL = "REAL";
        protected static string FLOAT = "FLOAT";
        protected static string DECIMAL = "DECIMAL";
        protected static string MONEY = "MONEY";
        protected static string IMAGE = "IMAGE";
        protected static string SQL_VARIANT = "SQL_VARIANT";
        protected static string UNIQUEIDENTIFIER = "UNIQUEIDENTIFIER";
        protected static string DATETIME = "DATETIME";
        protected static string CAST = "CAST";

        public override ForeignKeyScope GetDisableForeignKeyScope()
        {
            return ForeignKeyScope.Table;
        }

        public override void Visit(DisableForeignKey dfkCommand)
        {
            _Query.Append(ALTER).Append(TABLE).Append(FormatAttribute(dfkCommand.Table)).Append(SPACE).Append(NOCHECK).Append(CONSTRAINT).Append(ALL);
        }

        public override void Visit(EnableForeignKey dfkCommand)
        {
            _Query.Append(ALTER).Append(TABLE).Append(FormatAttribute(dfkCommand.Table)).Append(SPACE).Append(CHECK).Append(CONSTRAINT).Append(ALL);
        }

        #region Parameters

        private static int MAXVARCHAR = 4 * 1024 - 1;

        public override SqlType TypeName(DbType dbType, int length)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    if (length == 0 || length > MAXVARCHAR)
                        return new SqlType(TEXT);
                    else
                        return new SqlType(VARCHAR, length);

                case DbType.AnsiStringFixedLength:
                    if (length == 0 || length > MAXVARCHAR)
                        return new SqlType(TEXT);
                    else
                        return new SqlType(CHAR, length);

                case DbType.String:
                    if (length == 0 || length > MAXVARCHAR)
                        return new SqlType(NTEXT);
                    else
                        return new SqlType(NVARCHAR, length);

                case DbType.StringFixedLength:
                    if (length == 0 || length > MAXVARCHAR)
                        return new SqlType(NTEXT);
                    else
                        return new SqlType(NCHAR, length);

                case DbType.Boolean:
                    return new SqlType(BIT);

                case DbType.Byte:
                case DbType.SByte:
                    return new SqlType(TINYINT);


                case DbType.Int16:
                case DbType.UInt16:
                    return new SqlType(SMALLINT);

                case DbType.Int32:
                case DbType.UInt32:
                    return new SqlType(INT);

                case DbType.Int64:
                case DbType.UInt64:
                    return new SqlType(BIGINT);

                case DbType.Single:
                    return new SqlType(REAL);

                case DbType.Double:
                    return new SqlType(FLOAT);

                case DbType.Decimal:
                    return new SqlType(DECIMAL, 38, 38);

                case DbType.Currency:
                    return new SqlType(MONEY);

                case DbType.Binary:
                    return new SqlType(IMAGE);

                case DbType.Object:
                    return new SqlType(SQL_VARIANT);

                case DbType.Guid:
                    return new SqlType(UNIQUEIDENTIFIER);

                case DbType.Date:
                case DbType.DateTime:
                    return new SqlType(DATETIME);
            }

            return null;
        }

        public override string NativeColumnString(ColumnDefinition column)
        {
            return String.Format("IDENTITY({0},{1})", column.Seed, column.Increment);
        }

        public override string NullColumnString
        {
            get { throw new NotImplementedException(); }
        }

        public override bool SupportsIdentityColumns
        {
            get { throw new NotImplementedException(); }
        }

        public override string GetIdentitySelect(string tableAlias)
        {
            return "SELECT @@IDENTITY ";
        }

        public override string GetDummyTableForScalarResults
        {
            get { return string.Empty; }
        }

        public override string IdentityColumnString
        {
            get { throw new NotImplementedException(); }
        }

        public override string DefaultValuestString
        {
            get { throw new NotImplementedException(); }
        }

        public override int MaxAnsiStringSize
        {
            get { return 8000; }
        }

        public override int MaxBinarySize
        {
            get { return 8000; }
        }

        public override int MaxBinaryBlobSize
        {
            get { throw new NotImplementedException(); }
        }

        public override int MaxStringClobSize
        {
            get { throw new NotImplementedException(); }
        }

        public override int MaxStringSize
        {
            get { return 4000; }
        }

        public override DbType GetDbTypeToNativeGenerator(StringDictionary parameters)
        {
            return (DbType)Enum.Parse(typeof(DbType), parameters[DBTYPE]);
        }

        public override string FormatAttribute(string name)
        {
            return String.Concat(OPENBRACKET, name, CLOSEBRACKET);
        }

        #endregion

        #region Format

        public override string FormatValue(string text, DbType type)
        {
            if (type == DbType.Boolean)
                if (text == "false")
                    return "0";
                else
                    return "1";

            return base.FormatValue(text, type);
        }

        public override string FormatTableAlias(string tableAlias)
        {
            if (tableAlias != String.Empty)
                return String.Concat(AS, OPENBRACKET, tableAlias, CLOSEBRACKET);
            else
                return String.Empty;
        }

        public override void Visit(Constant constant)
        {
            switch (constant.DbType)
            {
                case DbType.Binary:
                    // Format string for binary content in queries (e.g., 0xC9CBBBCCCEB9C8CABCCCCEB9C9CBBB )
                    _Query.Append("0x");
                    foreach (byte b in (byte[])constant.Value)
                    {
                        _Query.Append(Convert.ToString(b, 16));
                    }
                    break;
                default:
                    base.Visit(constant);
                    break;
            }

        }

        #endregion

        #region Visit

        public override void Visit(SelectStatement select_statement)
        {
            _Query.Append(DBDialect.SELECT);

            if (select_statement.IsDistinct)
                _Query.Append(DBDialect.DISTINCT);

            if (select_statement.Limit > 0)
                _Query.Append(DBDialect.TOP).Append(select_statement.Limit).Append(SPACE);

            // Used by SqlPersistenceEngine to apply a TOP 100 PERCENT statement
            if (select_statement.Limit == -1)
                _Query.Append(DBDialect.TOP).Append(DBDialect.HUNDRED).Append(DBDialect.PERCENT);

            if (select_statement.SelectedAllColumns)
                _Query.Append(DBDialect.MULT);

            if (select_statement.SelectedAllColumns && select_statement.SelectList.Count != 0)
                _Query.Append(DBDialect.COMMA);

            bool first = true;
            foreach (ISQLExpression item in select_statement.SelectList)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    _Query.Append(DBDialect.COMMA);
                }

                item.Accept(this);
            }

            select_statement.FromClause.Accept(this);

            if (select_statement.WhereClause != null)
                select_statement.WhereClause.Accept(this);

            if (select_statement.OrderByClause != null)
                select_statement.OrderByClause.Accept(this);
        }


        public override void Visit(AggregateFunction function)
        {
            switch (function.Type)
            {
                // cast to float (because if values are integer, result will be an integer)
                case AggregateFunctionEnum.Avg:
                    _Query.Append(AVG).Append(OPENBRACE).Append(CAST).Append(OPENBRACE);
                    function.ValueExpression.Accept(this);
                    _Query.Append(SPACE).Append(AS).Append(FLOAT).Append(CLOSEBRACE).Append(CLOSEBRACE);
                    break;

                default:
                    base.Visit(function);
                    break;
            }
        }

        #endregion

        #region Sort / Page

        public override ISQLExpression Page(ISQLExpression sqlExp, OrderByClause orderby, int topPage, int pageSize)
        {
            SelectStatement select = CheckAndAdjustExpression(sqlExp, orderby);

            // Has paging ?
            if (topPage != 1)
            {
                // paging

                //SELECT TOP 100 PERCENT y.* FROM 
                //(
                //    SELECT TOP {TopMax} x.*
                //    FROM
                //    (
                //        SELECT TOP {TopFirstPlusMax} e1.Type, e1.Id
                //        {SortSelect}
                //        FROM ( {OPathQuery} ) e1 
                //        {SortFrom}
                //        {SortOrderBy}
                //    )x
                //    {SortOrderBy_REV}
                //)y
                //{SortOrderBy}
                SelectStatement selectY = new SelectStatement(null);
                SelectStatement selectX = new SelectStatement(null);

                // reverse sort order 
                OrderByClause sortOrderByRev = new OrderByClause(select);
                foreach (OrderByClauseColumn col in select.OrderByClause)
                    sortOrderByRev.Add(new OrderByClauseColumn(col.ColumnName, !col.Desc));

                select.Limit = topPage + pageSize - 1;
                select.TableAlias = "x";

                selectX.Limit = pageSize;
                selectX.SelectedAllColumns = true;
                selectX.FromClause.Add(select as Table);
                selectX.OrderByClause = sortOrderByRev;
                selectX.TableAlias = "y";

                selectY.SelectedAllColumns = true;
                selectY.FromClause.Add(selectX as Table);
                selectY.OrderByClause = select.OrderByClause;

                select = selectY;
                select.Limit = -1;
            }
            else
            {
                // only sort, limit possible

                //SELECT [TOP pageSize | TOP 100 PERCENT] XXX
                //FROM XXX WHERE XXX
                //ORDER BY [orderByClause]

                select.Limit = pageSize;
            }

            return select;
        }

        #endregion


    }
}
