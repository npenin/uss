using System;
using System.Collections.Generic;
using System.Text;
using SQLObject;
using Evaluant.Uss.SqlMapper.SqlObjectModel.LDD;
using System.Data;
using System.Text.RegularExpressions;
using System.Collections;

namespace Evaluant.Uss.SqlMapper
{
    public class Db2Dialect : DBDialect
    {
        protected static string SMALLINT = "SMALLINT";
        protected static string INTEGER = "INTEGER";
        protected static string BIGINT = "BIGINT";
        protected static string REAL = "REAL";
        protected static string DOUBLE_PRECISION = "DOUBLE PRECISION";
        protected static string FLOAT = "FLOAT";
        protected static string DECIMAL = "DECIMAL";
        protected static string DATE = "DATE";
        protected static string TIME = "TIME";
        protected static string TIMESTAMP = "TIMESTAMP";
        protected static string CHAR = "CHAR";
        protected static string VARCHAR = "VARCHAR";
        protected static string LONG_VARCHAR = "LONG VARCHAR";
        protected static string CHAR_FOR_BIT_DATA = "CHAR FOR BIT DATA";
        protected static string VARCHAR_FOR_BIT_DATA = "VARCHAR FOR BIT DATA";
        protected static string LONG_VARCHAR_FOR_BIT_DATA = "LONG VARCHAR FOR BIT DATA";
        protected static string GRAPHIC = "GRAPHIC";
        protected static string VARGRAPHIC = "VARGRAPHIC";
        protected static string LONG_GRAPHIC = "LONG GRAPHIC";
        protected static string CLOB = "CLOB";
        protected static string BLOB = "BLOB";
        protected static string DBCLOB = "DBCLOB";
        protected static string FETCHFIRST = "FETCH FIRST ";
        protected static string ROWS = "ROW";
        protected static string ONLY = " ONLY";
        protected static string CAST = "CAST";

        public override int MaxAnsiStringSize
        {
            get { return 32700; }
        }

        public override int MaxBinarySize
        {
            get { return 32700; }
        }

        public override int MaxBinaryBlobSize
        {
            get { return 2147483647; }
        }

        public override int MaxStringClobSize
        {
            get { return 32700; }
        }

        public override int MaxStringSize
        {
            get { return 32700; }
        }

        public override int GetMaxIdentifierLength()
        {
            return 21;
        }

        public override SQLObject.SqlType TypeName(System.Data.DbType dbType, int length)
        {
            switch (dbType)
            {
                case DbType.String:
                case System.Data.DbType.AnsiString:
                    if (length == 0)
                        return new SqlType(CLOB);
                    return new SqlType(VARCHAR,length);
                case System.Data.DbType.AnsiStringFixedLength:
                case System.Data.DbType.StringFixedLength:
                    return new SqlType(CHAR,length);
                case System.Data.DbType.Byte:
                case System.Data.DbType.Binary:
                    if(length==0)
                        return new SqlType(BLOB);
                    if (length <= 254)
                        return new SqlType(CHAR_FOR_BIT_DATA);
                    return new SqlType(BLOB);
                case System.Data.DbType.Boolean:
                    return new SqlType(CHAR_FOR_BIT_DATA);
                case System.Data.DbType.Currency:
                    return new SqlType(DECIMAL,38,38);
                case System.Data.DbType.Date:
                    return new SqlType(DATE);
                case System.Data.DbType.DateTime:
                    return new SqlType(TIMESTAMP);
                case System.Data.DbType.Decimal:
                case DbType.Single:
                    return new SqlType(FLOAT);
                case System.Data.DbType.Double:
                    return new SqlType(DOUBLE_PRECISION);
                case System.Data.DbType.Guid:
                    return new SqlType(CHAR,36);
                case System.Data.DbType.Int16:
                    return new SqlType(SMALLINT);
                case System.Data.DbType.Int32:
                    return new SqlType(INTEGER);
                case System.Data.DbType.Int64:
                    return new SqlType(BIGINT);
                case System.Data.DbType.Object:
                    return new SqlType(BLOB);
                case System.Data.DbType.Time:
                    return new SqlType(TIME);
            }

            return null;
        }

        public override System.Data.DbType GetDbTypeToNativeGenerator(System.Collections.Specialized.StringDictionary parameters)
        {
            return (DbType)Enum.Parse(typeof(System.Data.DbType), parameters[DBTYPE]);
        }

        public override string NativeColumnString(Evaluant.Uss.SqlMapper.SqlObjectModel.LDD.ColumnDefinition column)
        {
            return String.Format("IDENTITY(START WITH {0}, INCREMENT BY {1})", column.Seed, column.Increment);
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

        public override string FormatAttribute(string name)
        {
            return name;
        }

        public override void Visit(DisableForeignKey dfkCommand)
        {
        }

        public override void Visit(EnableForeignKey dfkCommand)
        {
        }

        public override DBDialect.ForeignKeyScope GetDisableForeignKeyScope()
        {
            return ForeignKeyScope.None;
        }

        public override DbType GetDbTypeToPrimaryKey(GeneratorMapping generator)
        {
            if (generator.Name == GeneratorMapping.GeneratorType.guid)
                return DbType.String;
            return base.GetDbTypeToPrimaryKey(generator);
        }

        public override SQLObject.ISQLExpression Page(SQLObject.ISQLExpression sqlExp, SQLObject.OrderByClause orderby, int topPage, int pageSize)
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

        public override void Visit(SelectStatement select_statement)
        {
            _Query.Append(DBDialect.SELECT);

            if (select_statement.IsDistinct)
                _Query.Append(DBDialect.DISTINCT);

            // Used by SqlPersistenceEngine to apply a TOP 100 PERCENT statement
            /*if (select_statement.Limit == -1)
                _Query.Append(DBDialect.TOP).Append(DBDialect.HUNDRED).Append(DBDialect.PERCENT);*/

            if (select_statement.SelectedAllColumns)
                _Query.Append(DBDialect.MULT);

            if (select_statement.SelectedAllColumns && select_statement.SelectList.Count != 0)
                _Query.Append(DBDialect.COMMA);

            foreach (ISQLExpression item in select_statement.SelectList)
            {
                item.Accept(this);
                if (select_statement.SelectList.IndexOf(item) != select_statement.SelectList.Count - 1)
                    _Query.Append(DBDialect.COMMA);
            }

            _Query.Append(SPACE);
            select_statement.FromClause.Accept(this);

            if (select_statement.WhereClause != null)
                select_statement.WhereClause.Accept(this);

            if (select_statement.OrderByClause != null)
                select_statement.OrderByClause.Accept(this);


            if (select_statement.Limit > 0)
            {
                _Query.Append(Db2Dialect.FETCHFIRST);
                if(select_statement.Limit>1)
                    _Query.Append(select_statement.Limit).Append(SPACE);
                _Query.Append(Db2Dialect.ROWS);
                if (select_statement.Limit > 1)
                    _Query.Append("S");
                _Query.Append(Db2Dialect.ONLY);
            }
        }

        public override void Visit(Constant constant)
        {
            if (constant.Value == DBNull.Value)
            {
                _Query.Append(Db2Dialect.CAST)
                    .Append(Db2Dialect.OPENBRACE)
                .Append(Db2Dialect.NULL)
                .Append(Db2Dialect.AS)
                .Append(TypeName(constant.DbType, 0).FormatType())
                .Append(Db2Dialect.CLOSEBRACE);

                if (!string.IsNullOrEmpty(constant.Alias))
                    _Query.Append(SPACE).Append(AS).Append(FormatAttribute(constant.Alias)).Append(SPACE);
            }
            else
            {
                switch (constant.DbType)
                {
                    case DbType.Date :
                        _Query.Append(SINGLEQUOTE).Append(DateTime.Parse(constant.Value.ToString()).ToString("yyyy-MM-dd", base.Culture)).Append(SINGLEQUOTE);
                        break;
                    case DbType.Time :
                        _Query.Append(SINGLEQUOTE).Append(DateTime.Parse(constant.Value.ToString()).ToString("HH.mm.ss", base.Culture)).Append(SINGLEQUOTE);
                        break;
                   
                    default:

                        base.Visit(constant);
                        break;
                }
               
            }
        }

        public override void Visit(ExistsPredicate predicate)
        {
            if (predicate.SubQuery is UnionStatement)
            {
                _Query.Append(OPENBRACE);
                bool firstElement = true;

                UnionStatement union = predicate.SubQuery as UnionStatement;
                foreach (SelectStatement select in union.SelectExpressions)
                {
                    if (!firstElement)
                        _Query.Append(OR);
                    _Query.Append(EXISTS).Append(OPENBRACE);
                    select.Accept(this);
                    _Query.Append(CLOSEBRACE);
                    firstElement = false;
                }
                _Query.Append(CLOSEBRACE);
            }
            else
            {
                _Query.Append(EXISTS).Append(OPENBRACE);
                predicate.SubQuery.Accept(this);
                _Query.Append(CLOSEBRACE);
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
    }
}
