using System;
using System.Collections.Specialized;
using System.Data;
using SQLObject;
using Evaluant.Uss.SqlMapper.SqlObjectModel.LDD;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de AccessDialect.
	/// </summary>
	public class AccessDialect : DBDialect
    {
        
        #region Ctor

        public AccessDialect()
		{
        }

        #endregion

        public override ForeignKeyScope GetDisableForeignKeyScope()
        {
            return ForeignKeyScope.None;
        }

        #region Parameters

        public override SqlType TypeName(DbType dbType, int length)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    if (length == 0)
                        return new SqlType("MEMO");
                    else
                        return new SqlType("TEXT", length);

                case DbType.AnsiStringFixedLength:
                    if (length == 0)
                        return new SqlType("MEMO");
                    else
                        return new SqlType("TEXT", length);

                case DbType.String:
                    if (length == 0)
                        return new SqlType("MEMO");
                    else
                        return new SqlType("TEXT", length);

                case DbType.StringFixedLength:
                    if (length == 0)
                        return new SqlType("MEMO");
                    else
                        return new SqlType("TEXT", length);

                case DbType.Boolean:
                    return new SqlType("YESNO");

                case DbType.Byte:
                case DbType.SByte:
                    return new SqlType("BYTE");


                case DbType.Int16:
                case DbType.UInt16:
                    return new SqlType("INTEGER");

                case DbType.Int32:
                case DbType.UInt32:
                    return new SqlType("LONG");

                case DbType.Int64:
                case DbType.UInt64:
                    return new SqlType("DOUBLE");

                case DbType.Single:
                    return new SqlType("SINGLE");

                case DbType.Double:
                    return new SqlType("DOUBLE");

                case DbType.Decimal:
                    return new SqlType("DOUBLE");

                case DbType.Currency:
                    return new SqlType("CURRENCY");

                case DbType.Binary:
                case DbType.Object:
                    return new SqlType("LONGBINARY");

                case DbType.Guid:
                    return new SqlType("GUID");

                case DbType.Date:
                case DbType.DateTime:
                    return new SqlType("DATETIME");
            }

            return null;
        }

		protected bool IsJoinedTable(ISQLExpression expression)
		{
			if (expression.GetType() == typeof(JoinedTable))
				return true;
			return false;
		}

        public override void Visit(DisableForeignKey dfkCommand)
        {
        }

        public override void Visit(EnableForeignKey dfkCommand)
        {
        }

        public override string NativeColumnString(ColumnDefinition column)
        {
            return String.Format("IDENTITY({0},{1})", column.Seed, column.Increment);
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
            get { return string.Empty; }
        }

		public override string IdentityColumnString
		{
			get { return "IDENTITY "; }
		}

		public override string DefaultValuestString
		{
			get { return "DEFAULT VALUES "; }
		}

		public override int MaxAnsiStringSize
		{
			get { return 255; }
		}

		public override int MaxBinarySize
		{
			get { return 500; }
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
			get { return 255; }
		}

		public override DbType GetDbTypeToNativeGenerator(StringDictionary parameters)
		{
			return (DbType) Enum.Parse(typeof(DbType), parameters[DBTYPE]);
		}

		public override string FormatAttribute(string name)
		{
			return String.Concat("[", name, "]");
		}

        public override object PreProcessValue(object value)
        {
            //  Access cannot handle milliseconds in DataTime => Remove it
            if (value is DateTime)
            {
                DateTime dt = (DateTime)value;
                return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);
            }
            
            return base.PreProcessValue(value);
        }

        #endregion

        #region Format

        public override string FormatValue(string text, DbType type)
        {
			if (type == DbType.Boolean)
			{
				if (text.ToLower() == "false")
					return "0";
				else
					return "-1";
			}

            return base.FormatValue(text, type);
        }

        public override string FormatTableAlias(string tableAlias)
        {
            if (tableAlias != String.Empty)
                return String.Concat("AS [", tableAlias, "]");
            else
                return String.Empty;
        }

        #endregion

        #region Visit

        public override void Visit(Constant constant)
        {
            if (constant.DbType == DbType.DateTime || constant.DbType == DbType.Time || constant.DbType == DbType.Date)
            {
                DateTime date = DateTime.Parse((string)constant.Value);
                _Query.AppendFormat("#{0} {1}#", date.ToString(Culture.DateTimeFormat.ShortDatePattern), date.ToString(Culture.DateTimeFormat.LongTimePattern));
            }
            else
                base.Visit(constant);
        }

        public override void Visit(JoinedTable table)
        {
            _Query.Append("( ");

            if (IsSelectStatement(table.LeftTable))
                _Query.Append("( ");

            table.LeftTable.Accept(this);

            if (IsSelectStatement(table.LeftTable))
            {
                _Query.Append(" ) ");
                if (table.LeftTable.TableAlias != String.Empty)
                    _Query.Append(String.Format("AS {0} ", table.LeftTable.TableAlias));
            }

            _Query.Append(" ) ");

            Visit(table.Type);

            if (IsJoinedTable(table.RigthTable))
                _Query.Append(" ( ");

            if (IsSelectStatement(table.RigthTable))
                _Query.Append("( ");

            table.RigthTable.Accept(this);

            if (IsSelectStatement(table.RigthTable))
            {
                _Query.Append(" ) ");
                if (table.RigthTable.TableAlias != String.Empty)
                    _Query.Append(String.Format("AS {0} ", table.RigthTable.TableAlias));
            }

            if (IsJoinedTable(table.RigthTable))
                _Query.Append(" ) ");

            foreach(ILogicExpression expression in table.SearchConditions)
            {
                _Query.Append("ON ");
                expression.Accept(this);
            }
        }

        public override void Visit(CastExpression expression)
        {
            string function;
            switch (expression.DbType)
            {
                case DbType.Boolean:
                    function = "CBool";
                    break;
                case DbType.Byte:
                    function = "CByte";
                    break;
                case DbType.Date:
                case DbType.DateTime:
                    function = "CDate";
                    break;
                case DbType.Double:
                    function = "CDbl";
                    break;
                case DbType.Int32:
                    function = "CInt";
                    break;
                case DbType.Int64:
                    function = "CLng";
                    break;
                case DbType.Single:
                    function = "CSng";
                    break;
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    function = "CStr";
                    break;
                default:
                    throw new NotImplementedException(
                        "Cast to type " + expression.DbType + "is not implemented");
            }

            _Query.Append(function);
            _Query.Append("(");
            expression.Expression.Accept(this);
            _Query.Append(")");
        }

        public override void Visit(SelectStatement select_statement)
        {
            _Query.Append("SELECT ");

            if (select_statement.IsDistinct)
                _Query.Append("DISTINCT ");

            if (select_statement.Limit > 0)
                _Query.Append(String.Format("TOP {0} ", select_statement.Limit));

            if (select_statement.SelectedAllColumns)
                _Query.Append("* ");

            if (select_statement.SelectedAllColumns && select_statement.SelectList.Count != 0)
                _Query.Append(", ");

            foreach (ISQLExpression item in select_statement.SelectList)
            {
                item.Accept(this);
                if (select_statement.SelectList.IndexOf(item) != select_statement.SelectList.Count - 1)
                    _Query.Append(", ");
            }

            select_statement.FromClause.Accept(this);

            if (select_statement.WhereClause != null)
                select_statement.WhereClause.Accept(this);

            if (select_statement.OrderByClause != null)
                select_statement.OrderByClause.Accept(this);
        }


        public override void Visit(CaseExpression expression)
        {
            IIfStatement(expression, 0);

            if (expression.Alias != null && expression.Alias != string.Empty)
                _Query.Append(FormatTableAlias(expression.Alias)).Append(" ");

        }

        private void IIfStatement(CaseExpression expression, int expNumber)
        {
            _Query.Append(" IIF( ");

            expression.ExpressionToEval.Accept(this);
            _Query.Append(" = ");
            expression.TestExpressions[expNumber].TestExpression.Accept(this);
            
            _Query.Append(", ");
            
            expression.TestExpressions[expNumber].TestResult.Accept(this);
            
            _Query.Append(", ");

            if (expNumber < expression.TestExpressions.Count - 1)
                IIfStatement(expression, expNumber + 1);
            else if(expression.DefaultResult != null)
                expression.DefaultResult.Accept(this);

            _Query.Append(") ");
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
                //        SELECT TOP {TopFirstPlusMax} e1.Type, e1.Id, {SortSelect}
                //        FROM 
                //              ( 
                //               SELECT Type, Id, {SortSelect} 
                //               FROM {OPathQuery without order by} 
                //              ) e1 
                //        {SortOrderBy}
                //    )x
                //    {SortOrderBy_REV}
                //)y
                //{SortOrderBy}
                SelectStatement selectY = new SelectStatement(null);
                SelectStatement selectX = new SelectStatement(null);

                // put in a subquery because access cannot order with alias in select
                SelectStatement selectZ = new SelectStatement(null);
                foreach (ISQLExpression exp in select.SelectList)
                {
                    if (exp is Column)
                        selectZ.SelectList.Add(new SelectItem(null, ((Column)exp).Alias == String.Empty ? ((Column)exp).ColumnName : ((Column)exp).Alias, ""));
                }

				selectZ.SelectedAllColumns = select.SelectedAllColumns;
                selectZ.FromClause.Add(select as Table);
                selectZ.OrderByClause = select.OrderByClause;
                select.OrderByClause = new OrderByClause(select);
				select = selectZ;

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
            }
            else
            {
                // only sort, limit possible

                //SELECT [TOP pageSize | TOP 100 PERCENT] XXX
                //FROM (select x from y where z) as x WHERE XXX
                //ORDER BY [orderByClause]

                // put in a subquery because access cannot order with alias in select
                SelectStatement selectX = new SelectStatement(null);
                foreach (ISQLExpression exp in select.SelectList)
                {
                    if (exp is Column)
                        selectX.SelectList.Add(new SelectItem(null, ((Column)exp).Alias == String.Empty ? ((Column)exp).ColumnName : ((Column)exp).Alias, ""));
                }

                if (select.SelectList.Count == 0)
                    selectX.SelectedAllColumns = true;

                selectX.Limit = pageSize;
                selectX.FromClause.Add(select as Table);
                selectX.OrderByClause = select.OrderByClause;

                select.OrderByClause = new OrderByClause(select);

                select = selectX;
            }

            return select;
        }

		public override void Visit(OrderByClause order_by_clause)
		{
			//	Access doesn't manage column alias in order by clause
			//	Use columns positions instead (positions : from 1 to n)

			if (order_by_clause.Count != 0)
			{
				_Query.Append(ORDERBY).Append(SPACE);
				foreach(OrderByClauseColumn column in order_by_clause)
				{
					if(column.ColumnName == null || column.ColumnName == string.Empty)
						continue;

					int pos = 2;

					SelectStatement sel = order_by_clause.Select;
					if(sel as UnionStatement != null && (sel as UnionStatement).SelectExpressions.Count > 0)
						sel = (sel as UnionStatement).SelectExpressions[0];

					while(sel.SelectList.Count == 0 && sel.FromClause.Count > 0 && sel.FromClause[0] as SelectStatement != null)
					{
						sel = sel.FromClause[0] as SelectStatement;
						if(sel as UnionStatement != null && (sel as UnionStatement).SelectExpressions.Count > 0)
							sel = (sel as UnionStatement).SelectExpressions[0];
					}

					for(int i=0; i<sel.SelectList.Count; i++)
					{
						ISQLExpression cur = sel.SelectList[i];
						if(cur as Column == null)
							continue;

						if((cur as Column).ColumnName == column.ColumnName || (cur as Column).Alias == column.ColumnName)
						{
							pos = i+1;
							break;
						}
					}

					if(pos != 0)
						_Query.Append(string.Concat(Convert.ToString(pos), " "));

					if(column.Desc)
						_Query.Append(DESC);

					if (order_by_clause.IndexOf(column) != order_by_clause.Count -1 )
						_Query.Append(COMMA);
				}
			}
		}


        #endregion
    }
}
