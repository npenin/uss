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
	public class SQLiteDialect : DBDialect
    {
        
        #region Ctor

        public SQLiteDialect()
		{
        }

        #endregion

        public override ForeignKeyScope GetDisableForeignKeyScope()
        {
            return ForeignKeyScope.None;
        }


        public override SqlType TypeName(DbType dbType, int length)
        {
            switch (dbType)
            {
                case DbType.Object: 
                case DbType.Binary: return new SqlType("BLOB");
                case DbType.Byte: 
                case DbType.Int16: 
			    case DbType.Int32: 
			    case DbType.Int64: 
			    case DbType.SByte: 
			    case DbType.UInt16: 
			    case DbType.UInt32: 
			    case DbType.UInt64: return new SqlType("INTEGER");
			    case DbType.Currency: 
			    case DbType.Decimal: 
			    case DbType.Double: 
			    case DbType.Single: 
			    case DbType.VarNumeric: return new SqlType("NUMERIC");
                case DbType.AnsiString:
                case DbType.String:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength: return new SqlType("TEXT");
                case DbType.Date:
                case DbType.DateTime:
                case DbType.Time: return new SqlType("DATETIME");
			    case DbType.Boolean: return new SqlType("INTEGER");
                case DbType.Guid: return new SqlType("UNIQUEIDENTIFIER");
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
            return (DbType)Enum.Parse(typeof(DbType), parameters[DBTYPE]);
		}

		public override string FormatAttribute(string name)
		{
			return String.Concat("[", name, "]");
		}

        #region Format

        public override string FormatTableAlias(string tableAlias)
        {
            if (tableAlias != String.Empty)
                return String.Concat("AS [", tableAlias, "]");
            else
                return String.Empty;
        }

        #endregion

        #region Visit

        public override void Visit(SelectStatement select_statement)
        {
            _Query.Append("SELECT ");

            if (select_statement.IsDistinct)
                _Query.Append("DISTINCT ");

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

            if (select_statement.Limit > 0)
                _Query.Append(String.Format("LIMIT {0} ", select_statement.Limit));

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


        public override void Visit(JoinedTable table)
        {
            if (IsSelectStatement(table.LeftTable))
                _Query.Append(OPENBRACE);

            table.LeftTable.Accept(this);

            if (IsSelectStatement(table.LeftTable))
            {
                _Query.Append(CLOSEBRACE);
                if (table.LeftTable.TableAlias != String.Empty)
                    _Query.Append(SPACE).Append(FormatTableAlias(table.LeftTable.TableAlias)).Append(SPACE);
            }

            Visit(table.Type);

            if (IsSelectStatement(table.RigthTable))
                _Query.Append(OPENBRACE);

            if (table.RigthTable is JoinedTable)
            {
                ((JoinedTable)table.RigthTable).LeftTable.Accept(this);

                foreach(ILogicExpression expression in table.SearchConditions)
                {
                    _Query.Append(ON);
                    expression.Accept(this);
                }

                this.VisitSubJoin(table.RigthTable as JoinedTable);
            }
            else
            {
                table.RigthTable.Accept(this);

                foreach(ILogicExpression expression in table.SearchConditions)
                {
                    _Query.Append(ON);
                    expression.Accept(this);
                }
            }

            if (IsSelectStatement(table.RigthTable))
            {
                _Query.Append(CLOSEBRACE);
                if (table.RigthTable.TableAlias != String.Empty)
                    _Query.Append(SPACE).Append(FormatTableAlias(table.RigthTable.TableAlias)).Append(SPACE);
            }

        }

		public void VisitSubJoin(JoinedTable table)
		{
			if (IsSelectStatement(table.LeftTable))
				_Query.Append(OPENBRACE);

			// table.LeftTable.Accept(this);

			if (IsSelectStatement(table.LeftTable))
			{
				_Query.Append(CLOSEBRACE);
				if (table.LeftTable.TableAlias != String.Empty)
					_Query.Append(SPACE).Append(FormatTableAlias(table.LeftTable.TableAlias)).Append(SPACE);
			}

			Visit(table.Type);

			if (IsSelectStatement(table.RigthTable))
				_Query.Append(OPENBRACE);

			if (table.RigthTable is JoinedTable)
			{
				((JoinedTable)table.RigthTable).LeftTable.Accept(this);

                foreach (ILogicExpression expression in table.SearchConditions)
				{
					_Query.Append(ON);
					expression.Accept(this);
				}

				this.VisitSubJoin(table.RigthTable as JoinedTable);
			}
			else
				table.RigthTable.Accept(this);

			if (IsSelectStatement(table.RigthTable))
			{
				_Query.Append(CLOSEBRACE);
				if (table.RigthTable.TableAlias != String.Empty)
					_Query.Append(SPACE).Append(FormatTableAlias(table.RigthTable.TableAlias)).Append(SPACE);
			}

            foreach (ILogicExpression expression in table.SearchConditions)
			{
				_Query.Append(ON);
				expression.Accept(this);
			}
		}


        #endregion
    }
}
