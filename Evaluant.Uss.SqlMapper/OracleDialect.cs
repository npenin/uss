using System;
using System.Collections.Specialized;
using System.Data;
using SQLObject;
using Evaluant.Uss.SqlMapper.SqlObjectModel.LDD;

namespace Evaluant.Uss.SqlMapper
{
    /// <summary>
    /// Description résumée de OracleDialect
    /// </summary>
    public class OracleDialect : DBDialect
    {
        private static int MAXVARCHAR = 4 * 1024 - 1;

        public static string CLOB = "CLOB";
        public static string VARCHAR2 = "VARCHAR2";
        public static string CHAR = "CHAR";
        public static string DOUBLEPRECISION = "DOUBLE PRECISION";
        public static string NCLOB = "NCLOB";
        public static string NVARCHAR2 = "NVARCHAR2";
        public static string NCHAR = "NCHAR";
        public static string NUMBER = "NUMBER";
        public static string FLOAT = "FLOAT";
        public static string BLOB = "BLOB";
        public static string DATE = "DATE";
        public static string FOREACHROWWHEN = "FOR EACH ROW WHEN ";
        public static string NEXTVAL = "NEXTVAL ";
        public static string NEW = "new";
        public static string DUAL = "DUAL ";

        #region Ctor

        public OracleDialect()
        {
        }

        #endregion

        #region Parameters

        public override SqlType TypeName(DbType dbType, int length)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    if (length == 0 || length > MAXVARCHAR)
                        return new SqlType(CLOB);
                    else
                        return new SqlType(VARCHAR2, length);

                case DbType.AnsiStringFixedLength:
                    if (length == 0 || length > MAXVARCHAR)
                        return new SqlType(CLOB);
                    else
                        return new SqlType(CHAR, length);

                case DbType.String:
                    if (length == 0 || length > MAXVARCHAR)
                        return new SqlType(NCLOB);
                    else
                        return new SqlType(NVARCHAR2, length);

                case DbType.StringFixedLength:
                    if (length == 0 || length > MAXVARCHAR)
                        return new SqlType(NCLOB);
                    else
                        return new SqlType(NCHAR, length);

                case DbType.Boolean:
                    return new SqlType(NUMBER, 1, 0);

                case DbType.Byte:
                case DbType.SByte:
                    return new SqlType(NUMBER, 3, 0);


                case DbType.Int16:
                case DbType.UInt16:
                    return new SqlType(NUMBER, 5, 0);

                case DbType.Int32:
                case DbType.UInt32:
                    return new SqlType(NUMBER, 10, 0);

                case DbType.Int64:
                case DbType.UInt64:
                    return new SqlType(NUMBER, 20, 0);

                case DbType.Single:
                    return new SqlType(FLOAT);

                case DbType.Double:
                    return new SqlType(DOUBLEPRECISION);

                case DbType.Decimal:
                    return new SqlType(NUMBER);  

                case DbType.Currency:
                    return new SqlType(NUMBER, 19, 1);

                case DbType.Binary:
                case DbType.Object:
                    return new SqlType(BLOB);

                case DbType.Guid:
                    return new SqlType(NVARCHAR2, 36);

                case DbType.Date:
                case DbType.DateTime:
                    return new SqlType(DATE);
            }

            return null;
        }

        public override string NativeColumnString(ColumnDefinition column)
        {
            return String.Format(String.Empty);
        }

        public static string SEQPREFIX = "_SEQ";
        public string FormatSequenceName(string tablename)
        {
            return FormatAttribute(string.Concat(tablename, SEQPREFIX));
        }

        public static string TRIGGERPREFIX = "_BIR";
        public string FormatTriggerName(string tablename)
        {
            return FormatAttribute(string.Concat(tablename, TRIGGERPREFIX));
        }

        public override string NullColumnString
        {
            get { throw new NotImplementedException(); }
        }

        public override bool SupportsIdentityColumns
        {
            get { return false; }
        }

        public override string GetIdentitySelect(string tableAlias)
        {
            return String.Concat("SELECT \"", tableAlias, "_SEQ\".CURRVAL", GetDummyTableForScalarResults);
        }

        public override string GetDummyTableForScalarResults
        {
            get { return " FROM DUAL"; }
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

        #endregion

        #region Format

        public override string FormatAttribute(string name)
        {
            return String.Concat(QUOTES, name, QUOTES);
        }

        public override string FormatTableAlias(string tableAlias)
        {
            return FormatAttribute(tableAlias);
        }

        public override string FormatValue(string text, DbType type)
        {
            // Oracle can't handle Empty strings
            if (text == String.Empty &&
                (type == DbType.AnsiStringFixedLength ||
                    type == DbType.String ||
                    type == DbType.StringFixedLength ||
                    type == DbType.AnsiString))
                return NULL;

            return base.FormatValue(text, type);
        }

        public override object PreProcessValue(object value)
        {
            string s = value as string;
            if (s != null && s == string.Empty)
                return DBNull.Value;

            return base.PreProcessValue(value);
        }

        #endregion

        #region Visit

        public override void Visit(CreateTableSQLCommand command)
        {
            base.Visit(command);
            if (command.PrimaryKey != null && ((ColumnDefinition)command.PrimaryKey.Columns[0]).IsAutoIncrement)
            {
                _Query.Append(ENDQUERY);
                _Query.Append(DROP).Append(SEQUENCE).Append(FormatSequenceName(command.TableName)).Append(SPACE).Append(ENDQUERY);
                _Query.Append(CREATE).Append(SEQUENCE).Append(FormatSequenceName(command.TableName)).Append(SPACE).Append(ENDQUERY);
                _Query.Append(CREATE).Append(OR).Append(REPLACE).Append(TRIGGER).Append(FormatTriggerName(command.TableName)).Append(SPACE).Append(BEFORE).Append(INSERT).Append(ON).Append(FormatTableAlias(command.TableName)).Append(SPACE).Append(FOREACHROWWHEN).Append(OPENBRACE).Append(NEW).Append(DOT).Append(FormatAttribute(((ColumnDefinition)command.PrimaryKey.Columns[0]).ColumnName)).Append(SPACE).Append(IS).Append(NULL).Append(CLOSEBRACE).Append(BEGIN).Append(SELECT).Append(FormatSequenceName(command.TableName)).Append(DOT).Append(NEXTVAL).Append(INTO).Append(COLON).Append(NEW).Append(DOT).Append(FormatAttribute(((ColumnDefinition)command.PrimaryKey.Columns[0]).ColumnName)).Append(SPACE).Append(FROM).Append(DUAL).Append(SEMICOLON).Append(SPACE).Append(END).Append(SEMICOLON);
            }
        }

        protected static string DISABLE = "DISABLE ";
        protected static string ENABLE = "ENABLE ";

        public override void Visit(DisableForeignKey dfkCommand)
        {
            _Query.Append(ALTER).Append(TABLE).Append(FormatTableAlias(dfkCommand.Table)).Append(SPACE).Append(DISABLE).Append(CONSTRAINT).Append(FormatTableAlias(dfkCommand.Name));
        }

        public override void Visit(EnableForeignKey dfkCommand)
        {
            _Query.Append(ALTER).Append(TABLE).Append(FormatTableAlias(dfkCommand.Table)).Append(SPACE).Append(ENABLE).Append(CONSTRAINT).Append(FormatTableAlias(dfkCommand.Name));
        }

        public override void Visit(Constant constant)
        {

           
            switch (constant.DbType)
            {
                case DbType.Date:
                case DbType.DateTime:
                case DbType.Time:
                    DateTime date = DateTime.Parse((string)constant.Value);
                    _Query.AppendFormat("TO_DATE('{0} {1}', 'DD/MM/YYYY HH24:MI:SS')", date.ToString("dd/MM/yyyy"), date.ToString("HH:mm:ss", Culture));
                    break;

                case DbType.Binary:
                    // Format string for binary content in queries (e.g., 0xC9CBBBCCCEB9C8CABCCCCEB9C9CBBB )
                    _Query.Append(SINGLEQUOTE);
                    foreach (byte b in (byte[])constant.Value)
                    {
                        _Query.Append(Convert.ToString(b, 16));
                    }
                    _Query.Append(SINGLEQUOTE);
                    break;

                default:
                    base.Visit(constant);
                    break;
            }
        }

        public override void Visit(AggregateFunction function)
        {
            switch (function.Type)
            {
                // cast to double to avoid overflow exception since oracle use 128 bit number precision
                case AggregateFunctionEnum.Avg:
                    _Query.Append("Round(Avg(");
                    function.ValueExpression.Accept(this);
                    _Query.Append("), 10)");
                    break;

                default:
                    base.Visit(function);
                    break;
            }
        }

        public override void Visit(UnionStatement unionStatement)
        {
            if (unionStatement.OrderByClause != null && unionStatement.OrderByClause.Count > 0)
                _Query.Append(SELECT).Append(MULT).Append(FROM).Append(OPENBRACE);

            foreach (SelectStatement query in unionStatement.SelectExpressions)
            {
                query.Accept(this);

                if (unionStatement.SelectExpressions.IndexOf(query) != unionStatement.SelectExpressions.Count - 1)
                    _Query.Append(UNIONALL);
            }

            if (unionStatement.OrderByClause != null && unionStatement.OrderByClause.Count > 0)
                _Query.Append(CLOSEBRACE);

            if (unionStatement.OrderByClause != null)
                unionStatement.OrderByClause.Accept(this);
        }

        public override void Visit(OrderByClause order_by_clause)
        {
            base.Visit(order_by_clause);
        }

        public override void Visit(FromClause from_clause)
        {
            if (from_clause.Count > 0)
                base.Visit(from_clause);
            else
                _Query.Append(FROM).Append(DUAL);     // DUAL is a dummy table ever existing
        }

        public override void Visit(CaseExpression expression)
        {
            _Query.Append(SPACE).Append(DECODE).Append(OPENBRACE);

            expression.ExpressionToEval.Accept(this);
            _Query.Append(COMMA);

            foreach (CaseTest cs in expression.TestExpressions)
            {
                cs.TestExpression.Accept(this);
                _Query.Append(COMMA);
                cs.TestResult.Accept(this);
                _Query.Append(COMMA);
            }

            if (expression.DefaultResult != null)
                expression.DefaultResult.Accept(this);
            else
                _Query.Append(NULL);

            _Query.Append(CLOSEBRACE);

            if (expression.Alias != null && expression.Alias != string.Empty)
                _Query.Append(FormatTableAlias(expression.Alias)).Append(SPACE);
        }

        #endregion

        public override int GetMaxIdentifierLength()
        {
            return 30;
        }

        public override ForeignKeyScope GetDisableForeignKeyScope()
        {
            return ForeignKeyScope.Constraint;
        }

        private static readonly string REGEXP_LIKE = "regexp_like";

        public override void Visit(LikePredicate predicate)
        {
            if (Array.IndexOf(options, "INSENSITIVE_LIKE") != -1)
            {
                /// TODO: Test the three cases of % placement to create a ^ and a $ in the regexp
                _Query.Append(REGEXP_LIKE).Append(OPENBRACE);
                predicate.MatchValue.Accept(this);
                _Query.Append(COMMA).Append(SINGLEQUOTE).Append(FormatValue(predicate.Pattern, DbType.String)).Replace("%", String.Empty).Append(SINGLEQUOTE).Append(COMMA).Append("'i'").Append(CLOSEBRACE);

                if (!Char.IsWhiteSpace(predicate.CharacterEscape))
                    _Query.Append(ESCAPE).Append(SINGLEQUOTE).Append(predicate.CharacterEscape).Append(SINGLEQUOTE).Append(SPACE);
            }
            else
                base.Visit(predicate);
        }

        #region Sort / Page

        public override ISQLExpression Page(ISQLExpression sqlExp, OrderByClause orderby, int topPage, int pageSize)
        {
            SelectStatement select = CheckAndAdjustExpression(sqlExp, orderby);

            // Has paging ?
            if (topPage != 1 || pageSize != 0)
            {
                // paging

                //SELECT y.* FROM 
                //(
                //    SELECT x.*, [rownum n]
                //    FROM
                //    (
                //        SELECT e1.*
                //        FROM ( {OPathQuery} ) e1 
                //        ORDER BY [SortOrderBy]
                //    )x
                //)y
                //WHERE n Between [topPage] and [topPage + pageSize - 1]
                // ORDER BY n   <-- add this sort to not loose order when putting this query in another one

                SelectStatement selectY = new SelectStatement(null);
                SelectStatement selectX = new SelectStatement(null);

                select.TableAlias = "x";

                selectX.SelectList.Add(new SystemObject("x", MULT, String.Empty));     // use SystemObject to have the table prefix
                selectX.SelectList.Add(new SystemObject("rownum", "n"));
                selectX.FromClause.Add(select as Table);
                selectX.TableAlias = "y";

                selectY.SelectedAllColumns = true;
                selectY.FromClause.Add(selectX as Table);
                
                if(topPage > 1)
                    selectY.WhereClause.SearchCondition.Add(new BinaryLogicExpression(new Column(null, "n"), BinaryLogicOperator.GreaterOrEquals, new Constant(topPage, DbType.Int32)));
                
                if(pageSize > 0)
                    selectY.WhereClause.SearchCondition.Add(new BinaryLogicExpression(new Column(null, "n"), BinaryLogicOperator.LesserOrEquals, new Constant(topPage + pageSize - 1, DbType.Int32)));

                selectY.OrderByClause.Add(new OrderByClauseColumn("n"));

                select = selectY;
            }

            return select;
        }

        #endregion
    }


}
