using System;
using System.Globalization;
using System.Collections.Specialized;
using System.Data;
using SQLObject;
using Evaluant.Uss.SqlMapper.SqlObjectModel.LDD;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de MySqlDialect.
	/// </summary>
	public class MySqlDialect : DBDialect
	{
		public MySqlDialect()
		{
        }

        public override ForeignKeyScope GetDisableForeignKeyScope()
        {
            return ForeignKeyScope.DataBase;
        }

        protected static string FOREIGN_KEY_CHECKS_0 = "FOREIGN_KEY_CHECKS = 0";
        protected static string FOREIGN_KEY_CHECKS_1 = "FOREIGN_KEY_CHECKS = 1";

        public override void Visit(DisableForeignKey dfkCommand)
        {
            _Query.Append(SET).Append(FOREIGN_KEY_CHECKS_0);
        }

        public override void Visit(EnableForeignKey dfkCommand)
        {
            _Query.Append(SET).Append(FOREIGN_KEY_CHECKS_1);
        }

        #region Parameters

        private static int MAXVARCHAR = 4 * 1024 - 1;

        public override SqlType TypeName (DbType dbType, int length)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    if (length == 0 || length > MAXVARCHAR)
                        return new SqlType("TEXT");
                    else
                        return new SqlType("VARCHAR", length);

                case DbType.AnsiStringFixedLength:
                    if (length == 0 || length > MAXVARCHAR)
                        return new SqlType("TEXT");
                    else
                        return new SqlType("CHAR", length);

                case DbType.String:
                    if (length == 0 || length > MAXVARCHAR)
                        return new SqlType("NATIONAL TEXT");
                    else
                        return new SqlType("NATIONAL VARCHAR", length);

                case DbType.StringFixedLength:
                    if (length == 0 || length > MAXVARCHAR)
                        return new SqlType("NATIONAL TEXT");
                    else
                        return new SqlType("NATIONAL CHAR", length);

                case DbType.Boolean:
                    return new SqlType("TINYINT(1)");

                case DbType.Byte:
                case DbType.SByte:
                    return new SqlType("TINYINT UNSIGNED");

                case DbType.Int16:
                case DbType.UInt16:
                    return new SqlType("SMALLINT");

                case DbType.Int32:
                case DbType.UInt32:
                    return new SqlType("INTEGER");

                case DbType.Int64:
                case DbType.UInt64:
                    return new SqlType("BIGINT");

                case DbType.Single:
                    return new SqlType("FLOAT");

                case DbType.Double:
                    return new SqlType("DOUBLE");

                case DbType.Decimal:
                    return new SqlType("NUMERIC", 38, 38);

                case DbType.Currency:
                    return new SqlType("MONEY");

                case DbType.Binary:
                    return new SqlType("IMAGE");

                case DbType.Object:
                    return new SqlType("BLOB");

                case DbType.Guid:
                    return new SqlType("VARCHAR(36)");

                case DbType.Date:
                    return new SqlType("DATE");

                case DbType.DateTime:
                    return new SqlType("DATETIME");

                case DbType.Time:
                    return new SqlType("TIME");
            }

            return null;
        }


        public override string NativeColumnString(ColumnDefinition column)
		{
			return String.Format("AUTO_INCREMENT");
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
			return "SELECT LAST_INSERT_ID()";
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
			return String.Concat("`", name, "`");
		}

		private void PrintConstant (Constant constant)
		{
			if (constant.Value == null) {
				_Query.Append ("null");
				return;
			}

			_Query.Append ("'");
			switch (constant.DbType) {
				case DbType.Date :
				case DbType.DateTime :
					_Query.Append (DateTime.Parse(((string) constant.Value), CultureInfo.CurrentCulture).ToString (
						"yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
					break;
				case DbType.Time :
					_Query.Append (TimeSpan.Parse ((string) constant.Value).ToString ());
					break;
			}
			_Query.Append ("' ");
		}

        #endregion

        #region Visit

		public override void Visit (Column column)
		{
			if (column.TableName != String.Empty)
				_Query.Append (column.TableName).Append (".");

			_Query.Append (FormatAttribute (column.ColumnName)).Append (" ");

			if (column.Alias != null && column.Alias != string.Empty)
				_Query.AppendFormat ("AS {0} ", FormatAttribute (column.Alias));
		}

		public override void Visit (Constant constant)
		{
			switch (constant.DbType) {
				case DbType.Date :
				case DbType.DateTime :
				case DbType.Time :
					PrintConstant (constant);
					break;
                case DbType.Binary:
                    // Format string for binary content in queries (e.g., 0xC9CBBBCCCEB9C8CABCCCCEB9C9CBBB )
                    _Query.Append("x").Append(SINGLEQUOTE);
                    foreach (byte b in (byte[])constant.Value)
                    {
                        _Query.Append(Convert.ToString(b, 16));
                    }
                    _Query.Append(SINGLEQUOTE);
                    break;
				default:
					base.Visit (constant);
					break;
			}
		}

        public override void Visit(CastExpression expression)
        {
            _Query.Append(String.Concat("CAST("));
            expression.Expression.Accept(this);
            _Query.Append(" As ");
            _Query.Append(TypeName(expression.DbType, 0));
            _Query.Append(" ) ");
        }

        public override void Visit(JoinedTable table)
        {
            if (IsSelectStatement(table.LeftTable))
                _Query.Append("( ");

            table.LeftTable.Accept(this);

            if (IsSelectStatement(table.LeftTable))
            {
                _Query.Append(" ) ");
                if (table.LeftTable.TableAlias != String.Empty)
                    _Query.Append(String.Format("AS {0} ", table.LeftTable.TableAlias));
            }

            Visit(table.Type);

            _Query.Append("( ");

            if (IsSelectStatement(table.RigthTable))
                _Query.Append("( ");

			if (table.RigthTable is UnionStatement)
				VisitUnion (table.RigthTable as UnionStatement, true);
			else
				table.RigthTable.Accept(this);

            _Query.Append(" ) ");

            if (IsSelectStatement(table.RigthTable))
            {
                _Query.Append(" ) ");
                if (table.RigthTable.TableAlias != String.Empty)
                    _Query.Append(String.Format("AS {0} ", table.RigthTable.TableAlias));
            }

            foreach(ILogicExpression expression in table.SearchConditions )
            {
                _Query.Append("ON (");
                expression.Accept(this);
                _Query.Append(" ) ");
            }
        }

		void VisitUnion (UnionStatement unionStatement, bool parenthesize)
		{
			foreach (SelectStatement query in unionStatement.SelectExpressions) {
				if (parenthesize) {
					_Query.Append (" ( ");
					query.Accept (this);
					_Query.Append (" ) ");
				} else
					query.Accept (this);

				if (unionStatement.SelectExpressions.IndexOf (query) != unionStatement.SelectExpressions.Count - 1)
					_Query.Append ("UNION ");
			}

			if (unionStatement.OrderByClause != null)
				unionStatement.OrderByClause.Accept (this);
		}

		public override void Visit (UnionStatement unionStatement)
		{
			VisitUnion (unionStatement, false);
		}

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

            if (select_statement.OrderByClause != null)
                select_statement.OrderByClause.Accept(this);

            if (select_statement.Limit > 0)
                _Query.Append(String.Format("LIMIT {0} ", select_statement.Limit));

			if (select_statement.Offset > 1)
				_Query.Append (String.Format ("OFFSET {0} ", select_statement.Offset - 1));   // index start at 0
        }

        #endregion

        #region Sort / Page

        public override ISQLExpression Page(ISQLExpression sqlExp, OrderByClause orderby, int topPage, int pageSize)
        {
            SelectStatement select = CheckAndAdjustExpression(sqlExp, orderby);

            //SELECT XXX
            //FROM XXX WHERE XXX 
            //ORDER BY [orderByClause]
            //[LIMIT pageSize OFFSET topPage]
            select.Limit = pageSize;
            select.Offset = topPage;

            return select;
        }

        #endregion

	}
}
