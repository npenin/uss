using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using SQLObject;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper.SqlObjectModel.LDD;
using LDD = Evaluant.Uss.SqlMapper.SqlObjectModel.LDD;
using System.Globalization;

using UniqueConstraint = Evaluant.Uss.SqlMapper.SqlObjectModel.LDD.UniqueConstraint;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de Dialect.
	/// </summary>
	public abstract class DBDialect : ISQLVisitor
	{
        // Queries separator in order to include multiple queries in the resulting string
        public static string ENDQUERY = "$ENDQUERY$";


        #region SQL Keywords
        public static string ADD = "ADD ";
        public static string AND = "AND ";
        public static string ASC = "ASC ";
        public static string ALTER = "ALTER ";
        public static string AS = "AS ";
        public static string AVG = "AVG ";
        public static string BEFORE = "BEFORE ";
        public static string BEGIN = "BEGIN ";
        public static string CASE = "CASE ";
        public static string CONVERT = "CONVERT ";
        public static string CLUSTERED = "CLUSTERED ";
        public static string CONSTRAINT = "CONSTRAINT ";
        public static string COUNT = "COUNT ";
        public static string CREATE = "CREATE ";
        public static string DECODE = "DECODE ";
        public static string DEFAULT = "DEFAULT ";
        public static string DELETE = "DELETE ";
        public static string DESC = "DESC ";
        public static string DISTINCT = "DISTINCT ";
        public static string DROP = "DROP ";
        public static string EACH = "EACH";
        public static string END = "END ";
        public static string ELSE = "ELSE ";
        public static string ESCAPE = "ESCAPE ";
        public static string EXISTS = "EXISTS ";
        public static string FOR = "FOR ";
        public static string FOREIGN = "FOREIGN ";
        public static string FROM = "FROM ";
        public static string HUNDRED = "100 ";
        public static string IN = "IN ";
        public static string INNER = "INNER ";
        public static string INSERT = "INSERT ";
        public static string INSERTINTO = "INSERT INTO ";
        public static string INTO = "INTO ";
        public static string IS = "IS ";
        public static string JOIN = "JOIN ";
        public static string KEY = "KEY ";
        public static string LEFT = "LEFT ";
        public static string LIMIT = "LIMIT";
        public static string LIKE = "LIKE ";
        public static string MAX = "MAX ";
        public static string MIN = "MIN ";
        public static string NOT = "NOT ";
        public static string NULL = "NULL ";
        public static string OFFSET = "OFFSET";
        public static string ON = "ON ";
        public static string OR = "OR ";
        public static string ORDERBY = "ORDER BY ";
        public static string PERCENT = "PERCENT ";
        public static string PRIMARY = "PRIMARY ";
        public static string REFERENCES = "REFERENCES ";
        public static string REPLACE = "REPLACE ";
        public static string ROW = "ROW ";
        public static string RIGHT = "RIGHT ";
        public static string SELECT = "SELECT ";
        public static string SEQUENCE = "SEQUENCE ";
        public static string SET = "SET ";
        public static string SUM = "SUM ";
        public static string TABLE = "TABLE ";
        public static string TEST = "TEST ";
        public static string TOP = "TOP ";
        public static string THEN = "THEN ";
        public static string TRIGGER = "TRIGGER ";
        public static string UNIONALL = "UNION ALL ";
        public static string UPDATE = "UPDATE ";
        public static string VALUES = "VALUES ";
        public static string WHEN = "WHEN ";
        public static string WHERE = "WHERE ";
        #endregion

        public static string SPACE = " ";
        public static string COMMA = ", ";
        public static string COLON = ":";
        public static string SEMICOLON = ";";
        public static string OPENBRACE = "(";
        public static string CLOSEBRACE = ") ";
        public static string CLOSEBRACKET = "] ";
        public static string OPENBRACKET = "[";

        public static string EQUAL = "= ";
        public static string LESSER = "< ";
        public static string GREATER = "> ";
        public static string LESSEREQ = "<= ";
        public static string GREATEREQ = ">= ";
        public static string NOTEQUAL = "<> ";

        public static string PLUS = "+ ";
        public static string MINUS = "- ";
        public static string MULT = "* ";
        public static string STAR = "*";
        public static string DIV = "/ ";
        public static string MOD = "% ";
        public static string DOT = ".";

        public static string DBTYPE = "dbType";
        public static string QUOTES = "\"";
        public static string SINGLEQUOTE = "'";
        public static string DOUBLESINGLEQUOTE = "''";

		protected IDriver _Driver;

		#region Visitor
		protected StringBuilder _Query = new StringBuilder();

        public StringBuilder QueryBuilder
        {
            get { return _Query; }
            set { _Query = value; }
        }
		
		private CultureInfo _Culture;
		public CultureInfo Culture
		{
			get { return _Culture; }
			set { _Culture = value; }
		}

        protected string[] options = new string[0];

        public string[] Options
        {
            get { return options; }
            set { options = value; }
        }

		public virtual string FormatTableAlias(string tableAlias)
		{
			if(tableAlias != String.Empty)
				return String.Concat(AS, tableAlias);
			else
				return String.Empty;
		}

        //protected string schema;

        //public virtual string Schema
        //{
        //    get { return schema; }
        //    set { schema = value; }
        //}

        public virtual int GetMaxSchemaLength()
        {
            return int.MaxValue;
        }

		public virtual void Visit(SelectStatement select_statement)
		{
			_Query.Append(SELECT);

			if (select_statement.IsDistinct)
				_Query.Append(DISTINCT);

			if (select_statement.SelectedAllColumns)
				_Query.Append(MULT);

			if (select_statement.SelectedAllColumns && select_statement.SelectList.Count != 0)
				_Query.Append(COMMA);

			foreach(ISQLExpression item in select_statement.SelectList)
			{
				item.Accept(this);
				if ( select_statement.SelectList.IndexOf(item) != select_statement.SelectList.Count - 1 )
					_Query.Append(COMMA);
			}

			select_statement.FromClause.Accept(this);
			
			if (select_statement.WhereClause != null)
				select_statement.WhereClause.Accept(this);
			
			if( select_statement.OrderByClause != null)
				select_statement.OrderByClause.Accept(this);
		}

		public virtual void Visit(FromClause from_clause)
		{
            if(from_clause.Count > 0)
			    _Query.Append(FROM);

			foreach(Table table in from_clause)
			{
				if (IsSelectStatement(table))
					_Query.Append(OPENBRACE);

				table.Accept(this);

				if (IsSelectStatement(table))
				{
					_Query.Append(CLOSEBRACE).Append(FormatTableAlias(table.TableAlias)).Append(SPACE);
				}

				if ( from_clause.IndexOf(table) != from_clause.Count - 1 )
					_Query.Append(COMMA);
			}
		}

        public virtual void Visit(LogicExpressionCollection collection)
        {
            bool firstElement = true;
            foreach (LogicExpression expression in collection)
            {
                if (!firstElement)
                    _Query.Append(AND);
                expression.Accept(this);
                firstElement = false;
            }
        }

		public virtual void Visit(WhereClause whereClause)
		{
			if (whereClause.SearchCondition.Count != 0)
			{
				_Query.Append(WHERE);
				foreach(ILogicExpression exp in whereClause.SearchCondition)
				{
					_Query.Append(OPENBRACE);
					exp.Accept(this);
					_Query.Append(CLOSEBRACE);
					if (whereClause.SearchCondition.IndexOf(exp) != whereClause.SearchCondition.Count - 1)
						Visit(whereClause.DefaultOperator);
				}
			}
		}

		public virtual void Visit(OrderByClause order_by_clause)
		{
			if (order_by_clause.Count != 0)
			{
				_Query.Append(ORDERBY).Append(SPACE);
				foreach(OrderByClauseColumn column in order_by_clause)
				{
					column.Accept(this);
					if (order_by_clause.IndexOf(column) != order_by_clause.Count -1 )
						_Query.Append(COMMA);
				}
			}
		}

		public virtual void Visit(UnionStatement unionStatement)
		{
			foreach(SelectStatement query in unionStatement.SelectExpressions)
			{
				query.Accept(this);

				if (unionStatement.SelectExpressions.IndexOf(query) != unionStatement.SelectExpressions.Count - 1)
					_Query.Append(UNIONALL);
			}

			if( unionStatement.OrderByClause != null)
				unionStatement.OrderByClause.Accept(this);
		}

		public virtual void Visit(BinaryLogicOperator op)
		{
			switch(op)
			{
				case BinaryLogicOperator.Equals :
					_Query.Append(EQUAL);
					break;

				case BinaryLogicOperator.Greater :
					_Query.Append(GREATER);
					break;

				case BinaryLogicOperator.Lesser :
					_Query.Append(LESSER);
					break;

				case BinaryLogicOperator.GreaterOrEquals :
					_Query.Append(GREATEREQ);
					break;

				case BinaryLogicOperator.LesserOrEquals :
					_Query.Append(LESSEREQ);
					break;

				case BinaryLogicOperator.NotEquals :
					_Query.Append(NOTEQUAL);
					break;

                // Arithmetic operators

                case BinaryLogicOperator.Plus:
                    _Query.Append(PLUS);
                    break;

                case BinaryLogicOperator.Minus:
                    _Query.Append(MINUS);
                    break;

                case BinaryLogicOperator.Times:
                    _Query.Append(MULT);
                    break;

                case BinaryLogicOperator.Div:
                    _Query.Append(DIV);
                    break;

                case BinaryLogicOperator.Modulo:
                    _Query.Append(MOD);
                    break;

				default :
					_Query.Append(op.ToString()).Append(SPACE);
					break;
			}
		}

		public virtual void Visit(InsertCommand insertCommand)
		{
			//INSERT [INTO] nom_de_la_table_cible [(liste_des_colonnes_visées)] 
			//{VALUES (liste_des_valeurs) | requête_select | DEFAULT VALUES }

            //if (!string.IsNullOrEmpty(schema))
            //    _Query.Append(INSERTINTO).Append(FormatAttribute(schema + DOT + insertCommand.TableName)).Append(SPACE).Append(OPENBRACE);
            //else
                _Query.Append(INSERTINTO).Append(FormatAttribute(insertCommand.TableName)).Append(SPACE).Append(OPENBRACE);
			ArrayList coll = insertCommand.ColumnValueCollection;
			foreach(DictionaryEntry entry in coll)
			{
				_Query.Append(FormatAttribute((string)entry.Key));
				if (coll.Count - 1 != coll.IndexOf(entry))
					_Query.Append(COMMA);
			}
			_Query.Append(SPACE).Append(CLOSEBRACE).Append(VALUES).Append(OPENBRACE);

			foreach(DictionaryEntry entry in coll)
			{
				((ISQLExpression)entry.Value).Accept(this);
				if (coll.Count - 1 != coll.IndexOf(entry))
					_Query.Append(COMMA);
			}
			_Query.Append(" ) ");
		}

		public virtual void Visit(UpdateCommand updateCommand)
		{
            if (updateCommand.ColumnValueCollection.Count != 0)
            {

                _Query.Append(UPDATE).Append(FormatAttribute(updateCommand.TableName)).Append(SPACE).Append(SET);
                SortedList coll = updateCommand.ColumnValueCollection;
                foreach (string key in coll.Keys)
                {
                    _Query.Append(FormatAttribute(key)).Append(" = ");
                    ((ISQLExpression)coll[key]).Accept(this);
                    if (coll.Keys.Count - 1 != coll.IndexOfKey(key))
                        _Query.Append(COMMA);
                }
                updateCommand.WhereClause.Accept(this);
            }
		}

		public void Visit(DeleteCommand deleteCommand)
		{
			//DELETE FROM nom_de_la_table_cible 
			//WHERE (liste_des_valeurs) | requête_select | DEFAULT VALUES 
            //if (!string.IsNullOrEmpty(schema))
            //    _Query.Append(DELETE).Append(FROM).Append(FormatAttribute(schema + DBDialect.DOT + deleteCommand.TableName)).Append(SPACE);
            //else
                _Query.Append(DELETE).Append(FROM).Append(FormatAttribute(deleteCommand.TableName)).Append(SPACE);
			deleteCommand.Condition.Accept(this);
		}

		public virtual void Visit(Parameter param)
		{
			_Query.Append(SPACE).Append(_Driver.FormatParameter(param.Name)).Append(SPACE);
		}

		public virtual void Visit(UnaryLogicOperator op)
		{
			switch(op)
			{
				case UnaryLogicOperator.Not :
					_Query.Append(NOT);
					break;

				case UnaryLogicOperator.Minus :
					_Query.Append(MINUS);
					break;

				default :
					_Query.Append(op.ToString()).Append(SPACE);
					break;

			}
		}

		public virtual void Visit(DropTableSQLCommand command)
		{
            // "DROP TABLE {0} "
            //if (!string.IsNullOrEmpty(schema))
            //    _Query.Append(DROP).Append(TABLE).Append(FormatAttribute(schema + DBDialect.DOT + command.TableName)).Append(SPACE);
            //else
                _Query.Append(DROP).Append(TABLE).Append(FormatAttribute(command.TableName)).Append(SPACE);
		}

		public virtual void Visit(CreateTableSQLCommand command)
		{
            // "CREATE TABLE {0} ("
            //if (string.IsNullOrEmpty(schema))
            _Query.Append(CREATE).Append(TABLE).Append(FormatAttribute(command.TableName)).Append(SPACE).Append(OPENBRACE);
            //else
            //    _Query.Append(CREATE).Append(TABLE).Append(FormatAttribute(schema + DBDialect.DOT + command.TableName)).Append(SPACE).Append(OPENBRACE);
			SortedList columns = command.ColumnDefinitions;
			foreach(ColumnDefinition column in columns.Values)
			{
				column.Accept(this);
				if (columns.Count-1 != columns.IndexOfValue(column) || command.PrimaryKey != null)
					_Query.Append(COMMA);
			}

			if(command.PrimaryKey != null)
			{
				ArrayList columnNames = new ArrayList();
				foreach(ColumnDefinition column in command.PrimaryKey.Columns)
					columnNames.Add(FormatAttribute(column.ColumnName));

                _Query.Append(CONSTRAINT);

                //if(!string.IsNullOrEmpty(schema))
                //    _Query.Append(schema)
                //        .Append(DBDialect.DOT);

                _Query.Append(FormatAttribute(command.PrimaryKey.Name))
                    .Append(SPACE).Append(PRIMARY).Append(KEY).Append(OPENBRACE)
                    .Append(String.Join(COMMA, (string[])columnNames.ToArray(typeof(string))))
                    .Append(CLOSEBRACE);
			}


			_Query.Append(CLOSEBRACE);
		}

        public void Visit(AlterTableSQLCommand command)
        {
            _Query.Append(ALTER).Append(TABLE).Append(FormatAttribute(command.TableName)).Append(SPACE);
            

            switch (command.AlterType)
            {
                case AlterTypeEnum.AlterColumn:
                    throw new NotImplementedException();

                case AlterTypeEnum.Add:
                    _Query.Append(SPACE).Append(ADD);
                    break;

                case AlterTypeEnum.Drop:
                    _Query.Append(SPACE).Append(DROP);
                    break;
            }

            ArrayList alterList = new ArrayList();

            if(command.PrimaryKey != null)
            {
                string[] pkFields = new string[command.PrimaryKey.Columns.Count];
                for (int i = 0; i < pkFields.Length; i++)
                    pkFields[i] = FormatAttribute(((ColumnDefinition)command.PrimaryKey.Columns[i]).ColumnName);

                string constr = String.Concat(SPACE, CONSTRAINT, FormatAttribute(command.PrimaryKey.Name), SPACE);

                if (command.AlterType == AlterTypeEnum.Add)
                {
                    constr += String.Concat(SPACE, PRIMARY, KEY, CLUSTERED, OPENBRACE, string.Join(COMMA, pkFields), CLOSEBRACE);
                }
                

                alterList.Add(constr);
            }

            foreach (ForeignKey fk in command.ForeignKeys)
            {
                string constr = CONSTRAINT;
                
                if(command.AlterType == AlterTypeEnum.Add)
                {
                    string[] foreignFields = (string[])fk.ForeignKeys.Clone();
                    string[] referenceFields = (string[])fk.ReferenceKeys.Clone();

                    for(int i=0; i<foreignFields.Length; i++)
                    {
                        foreignFields[i] = FormatAttribute(foreignFields[i]);
                        referenceFields[i] = FormatAttribute(referenceFields[i]);
                    }

                    // " {0} FOREIGN KEY ( {1} ) REFERENCES {2} ( {3} )"
                    constr += string.Concat(SPACE,
                        FormatAttribute(fk.Name),
                        SPACE,
                        FOREIGN, KEY, OPENBRACE, SPACE,
                        string.Join(COMMA, foreignFields), SPACE,
                        CLOSEBRACE, REFERENCES, FormatAttribute(fk.ParentTable), SPACE,
                        OPENBRACE, SPACE, string.Join(COMMA, referenceFields), SPACE, CLOSEBRACE);
                }
                else if(command.AlterType == AlterTypeEnum.Drop)
                {
                    constr += FormatAttribute(fk.Name);
                }
                else
                    throw new ArgumentException("ForeignKeys");

                alterList.Add(constr);
            }

            _Query.Append(String.Join(COMMA, (string[])alterList.ToArray(typeof(string))));
        }

		public virtual void Visit(ColumnDefinition column)
		{
            _Query.Append(FormatAttribute(column.ColumnName)).Append(SPACE).Append(ConvertDbTypeToString(column.Type, column.Size, column.Precision, column.Scale)).Append(SPACE);
			if (column.IsAutoIncrement)
                _Query.Append(this.NativeColumnString(column)).Append(SPACE);

            if(column.ColumnConstraint != null)
				column.ColumnConstraint.Accept(this	);
		}

		public virtual void Visit(ColumnConstraint constraint)
		{
			if (!constraint.IsNull)
				_Query.Append(NOT).Append(NULL);
		}

        public virtual void Visit(DefaultConstraint constraint)
        {
            this.Visit((ColumnConstraint)constraint);
            if (constraint.DefaultValue != null)
            {
                _Query.Append(DEFAULT).Append(OPENBRACE);
                constraint.DefaultValue.Accept(this);
                _Query.Append(CLOSEBRACE);
            }
        }

        public virtual void Visit(CheckConstraint constraint)
		{
			this.Visit((ColumnConstraint)constraint);
		}

		public virtual void Visit(UniqueConstraint constraint)
		{
			this.Visit((ColumnConstraint)constraint);
		}

		public virtual void Visit(Column column)
		{
            if (column.TableName != String.Empty)
            {
                //if(!string.IsNullOrEmpty(schema))
                //    _Query.Append(FormatAttribute(schema + DOT + column.TableName)).Append(".");
                //else
                    _Query.Append(FormatAttribute(column.TableName)).Append(".");
            }
			
			_Query.Append(FormatAttribute(column.ColumnName));
            _Query.Append(SPACE);

			if(column.Alias != null && column.Alias != string.Empty)
                _Query.Append(AS).Append(FormatAttribute(column.Alias));

		}

        public virtual void Visit(SystemObject column)
        {
            if (column.TableName != null && column.TableName != string.Empty)
            {
                //if (!string.IsNullOrEmpty(schema))
                //    _Query.Append(FormatAttribute(schema + DOT + column.TableName)).Append(DOT);
                //else
                _Query.Append(FormatAttribute(column.TableName)).Append(DOT);
            }

            _Query.Append(column.ColumnName).Append(SPACE);

            if (column.Alias != null && column.Alias != string.Empty)
                _Query.Append(AS).Append(FormatAttribute(column.Alias)).Append(SPACE);
        }

        public virtual void Visit(CaseExpression expression)
        {
            _Query.Append(CASE);

            expression.ExpressionToEval.Accept(this);

            foreach (CaseTest cs in expression.TestExpressions)
            {
                _Query.Append(SPACE).Append(WHEN);
                cs.TestExpression.Accept(this);
                _Query.Append(SPACE).Append(THEN);
                cs.TestResult.Accept(this);
            }

            if (expression.DefaultResult != null)
            {   
                _Query.Append(SPACE).Append(ELSE);
                expression.DefaultResult.Accept(this);
            }

            _Query.Append(SPACE).Append(END);

            if (expression.Alias != null && expression.Alias != string.Empty)
                _Query.Append(FormatTableAlias(expression.Alias)).Append(SPACE);

        }

        public virtual void Visit(OrderByClauseColumn column)
        {
            if (!string.IsNullOrEmpty(column.TableName))
            {
                //if (!string.IsNullOrEmpty(schema))
                //    _Query.Append(FormatAttribute(schema + DOT + column.TableName)).Append(DOT);
                //else
                    _Query.Append(FormatAttribute(column.TableName)).Append(DOT);
            }

            _Query.Append(FormatAttribute(column.ColumnName)).Append(SPACE);

            if (column.ColumnName != string.Empty && column.Desc)
                _Query.Append(DESC);
        }

		public virtual void Visit(Constant constant)
		{
			string result = String.Empty;

			if(constant.Value != DBNull.Value)
			{
				switch(constant.DbType)
				{
					case DbType.AnsiStringFixedLength:
					case DbType.StringFixedLength:
						if (constant.Value.ToString() == STAR)
							_Query.Append (MULT);
						else 
                        {
							result = constant.Value.ToString();
							result = result.Replace (SINGLEQUOTE, DOUBLESINGLEQUOTE);
                            _Query.Append(SINGLEQUOTE).Append(result).Append(SINGLEQUOTE);
						}
						break;
                    case DbType.Guid:
                    case DbType.AnsiString:
					case DbType.String:
						result = (string) constant.Value;
                        result = result.Replace(SINGLEQUOTE, DOUBLESINGLEQUOTE);
                        _Query.Append(SINGLEQUOTE).Append(result).Append(SINGLEQUOTE);
						break;

					case DbType.Date:
					case DbType.DateTime:
						_Query.Append(SINGLEQUOTE).Append(DateTime.Parse(constant.Value.ToString()).ToString("s", _Culture)).Append(SINGLEQUOTE);
						break;
					case DbType.Boolean:
                        _Query.Append(FormatValue(constant.Value.ToString(), DbType.Boolean));
                        break;
					case DbType.Byte: /// TODO: Comment the reason of this case (if it is necessary)
						_Query.Append((byte)constant.Value);
						break;
					
					case DbType.Int32:
					case DbType.Binary:
					case DbType.Currency:
					case DbType.Decimal:
					case DbType.Double:
					case DbType.Int16:
					case DbType.Int64:
					case DbType.Object:
					case DbType.SByte:
					case DbType.Single:
					case DbType.UInt16:
					case DbType.UInt32:
					case DbType.UInt64:
					case DbType.VarNumeric:
						_Query.Append(Convert.ToString(constant.Value, _Culture));
						break;
				}
			}
			else
			{
				_Query.Append(NULL);
			}

            _Query.Append(SPACE);
            if (constant.Alias != null && constant.Alias != string.Empty)
                _Query.Append(AS).Append(FormatAttribute(constant.Alias)).Append(SPACE);
				
		}

		
		public virtual void Visit(UnaryLogicExpression expression)
		{
			Visit(expression.Operator);
            //if (IsSelectStatement(expression.Operand))
				_Query.Append(OPENBRACE);
				
			expression.Operand.Accept(this);

            //if (IsSelectStatement(expression.Operand))
				_Query.Append(CLOSEBRACE);
		}

		// don't add a space between the function name and the
		// opening parentesis, as it breaks on mysql
		public virtual void Visit(AggregateFunction function)
		{
			switch(function.Type)
			{
				case AggregateFunctionEnum.Count:
					_Query.Append(COUNT).Append(OPENBRACE);
					function.ValueExpression.Accept(this);
					_Query.Append(CLOSEBRACE);
					break;
				case AggregateFunctionEnum.Max:
					_Query.Append(MAX).Append(OPENBRACE);
					function.ValueExpression.Accept(this);
					_Query.Append(CLOSEBRACE);
					break;
				case AggregateFunctionEnum.Min:
					_Query.Append(MIN).Append(OPENBRACE);
					function.ValueExpression.Accept(this);
					_Query.Append(CLOSEBRACE);
					break;
				case AggregateFunctionEnum.Avg:
					_Query.Append(AVG).Append(OPENBRACE);
					function.ValueExpression.Accept(this);
					_Query.Append(CLOSEBRACE);
					break;
				case AggregateFunctionEnum.Sum:
					_Query.Append(SUM).Append(OPENBRACE);
					function.ValueExpression.Accept(this);
					_Query.Append(CLOSEBRACE);
					break;
				default : 
					throw new NotImplementedException();
			}
		}

		public virtual void Visit(CastExpression expression)
		{
			string typeName = ConvertDbTypeToString(expression.DbType, expression.Size, expression.Precision, expression.Scale);
			_Query.Append(CONVERT).Append(OPENBRACE).Append(typeName).Append(COMMA);
			expression.Expression.Accept(this);
			_Query.Append(CLOSEBRACE);
		}

		public virtual void Visit(BinaryLogicExpression expression)
		{
            bool BinaryIsFirst = _Query.Length == SELECT.Length;

            _Query.Append(OPENBRACE);

			if (IsSelectStatement(expression.LeftOperand))
				_Query.Append(OPENBRACE);
				
			expression.LeftOperand.Accept(this);

			if (IsSelectStatement(expression.LeftOperand))
				_Query.Append(CLOSEBRACE);

			Visit(expression.Operator);

			if (IsSelectStatement(expression.RightOperand))
				_Query.Append(OPENBRACE);

			expression.RightOperand.Accept(this);

			if (IsSelectStatement(expression.RightOperand))
				_Query.Append(CLOSEBRACE);

            _Query.Append(CLOSEBRACE);

            if (BinaryIsFirst)
                _Query.Append(GetDummyTableForScalarResults);
        }

		public virtual void Visit(LikePredicate predicate)
		{
			predicate.MatchValue.Accept(this);
            _Query.Append(LIKE).Append(FormatValue(predicate.Pattern, DbType.String)).Append(SPACE);

			if (!Char.IsWhiteSpace(predicate.CharacterEscape))
                _Query.Append(ESCAPE).Append(SINGLEQUOTE).Append(predicate.CharacterEscape).Append(SINGLEQUOTE).Append(SPACE);
		}

		public virtual void Visit(ExistsPredicate predicate)
		{
            _Query.Append(EXISTS).Append(OPENBRACE);
			predicate.SubQuery.Accept(this);
			_Query.Append(CLOSEBRACE);
		}

		public virtual void Visit(InPredicate predicate)
		{
			predicate.Column.Accept(this);
            _Query.Append(SPACE);
			_Query.Append(IN).Append(OPENBRACE);

			if(predicate.SubQueries.Count > 0)
			{
				if(predicate.SubQueries[0] is SelectStatement)
					predicate.SubQueries[0].Accept(this);
			
				if(predicate.SubQueries[0] is Constant)
				{
					foreach(Constant val in predicate.SubQueries)
					{
						val.Accept(this);
						_Query.Append(COMMA);
					}

                    _Query.Remove(_Query.Length - 2, 2);
				}
			}
			
            _Query.Append(CLOSEBRACE);
		}

		public virtual void Visit(IsNullPredicate predicate)
		{
			if (IsSelectStatement(predicate.Expression))
				_Query.Append(OPENBRACE);
			
			predicate.Expression.Accept(this);

			if (IsSelectStatement(predicate.Expression))
				_Query.Append(CLOSEBRACE);

			_Query.Append(IS).Append(NULL);
		}

		public virtual void Visit(SelectItem item)
		{
			if (item.ColumnAlias != String.Empty)
				_Query.Append(AS).Append(item.ColumnAlias).Append(SPACE);
		}

		public virtual void Visit(TableSource table)
		{
            //if (string.IsNullOrEmpty(schema))
            _Query.Append(FormatAttribute(table.TableName)).Append(SPACE);
            //else
                //_Query.Append(FormatAttribute(schema + DBDialect.DOT + table.TableName)).Append(SPACE);
			
			if (table.TableAlias != String.Empty)
                _Query.Append(SPACE).Append(FormatTableAlias(table.TableAlias)).Append(SPACE);
		}

		protected bool IsSelectStatement(ISQLExpression expression)
		{
            return expression as SelectStatement != null;
		}

		public virtual void Visit(JoinedTable table)
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

			table.RigthTable.Accept(this);

			if (IsSelectStatement(table.RigthTable))
			{
				_Query.Append(CLOSEBRACE);
				if (table.RigthTable.TableAlias != String.Empty)
					_Query.Append(SPACE).Append(FormatTableAlias(table.RigthTable.TableAlias)).Append(SPACE);
			}

            _Query.Append(ON);

            bool firsElement = true;
            //if (table.SearchConditions.Count > 0)
            //{
            //    table.SearchConditions.Accept(this);
            //    firsElement = false;
            //}

            foreach (ILogicExpression expression in table.SearchConditions)
            {
                if (!firsElement)
                    _Query.Append(AND);
                expression.Accept(this);
                firsElement = false;
            }

		}

		public virtual void Visit(TypeJoinedTable type)
		{
			switch(type)
			{
				case TypeJoinedTable.Inner:
					_Query.Append(INNER).Append(JOIN);
					break;

				case TypeJoinedTable.LeftOuter:
					_Query.Append(LEFT).Append(JOIN);
					break;

				case TypeJoinedTable.RightOuter:
					_Query.Append(RIGHT).Append(JOIN);
					break;
			}
		}

		#endregion

        /// <summary>
        /// Gets the length of the max identifier.
        /// </summary>
        /// <returns></returns>
        public virtual int GetMaxIdentifierLength()
        {
            return int.MaxValue;
        }

		public string[] RenderQueries(ISQLExpression expression, IDriver driver)
		{
            if (driver != null)
			    _Driver = driver;

			_Query = new StringBuilder();
			expression.Accept(this);

            ArrayList queries = new ArrayList();
            string query = _Query.ToString();

            int startIndex = 0, endIndex = -1;
            while ((endIndex = query.IndexOf(ENDQUERY, startIndex)) != -1)
            {
                queries.Add(query.Substring(startIndex, endIndex - startIndex));
                startIndex = endIndex + ENDQUERY.Length;
            }

            queries.Add(query.Substring(startIndex, query.Length - startIndex));

            return (string[])queries.ToArray(typeof(string));
		}

        public string[] RenderQueries(ISQLExpression expression)
        {
            return RenderQueries(expression, null);
        }

		public abstract int MaxAnsiStringSize { get; }
		public abstract int MaxBinarySize { get; }
		public abstract int MaxBinaryBlobSize { get; }
		public abstract int MaxStringClobSize { get; }
		public abstract int MaxStringSize { get; }

        public abstract SqlType TypeName(DbType dbType, int length);

		public string DropTable(DataTable table)
		{
            //if(string.IsNullOrEmpty(schema))
			    return String.Concat(DROP, TABLE, FormatAttribute(table.TableName));
            //return string.Concat(DROP, TABLE, FormatAttribute(schema + DOT + table.TableName));
		}

        /// <summary>
        /// Returns a formatted value (cloned) when inserting value in DB if necessary
        /// </summary>
        /// <param name="value">The value to format</param>
        /// <returns>A cloned formatted value if necessary, else value</returns>
        public virtual object PreProcessValue(object value)
        {
            return value;
        }

        /// <summary>
        /// Returns an unformatted value when extracting value from the DB
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual object PostProcessValue(object value)
        {
            return value;
        }

		public abstract DbType GetDbTypeToNativeGenerator(StringDictionary parameters);
		public abstract string NativeColumnString(ColumnDefinition column);
        
		public abstract string NullColumnString { get; }
		public abstract bool SupportsIdentityColumns { get; }
		public abstract string GetIdentitySelect(string tableAlias);
        public abstract string GetDummyTableForScalarResults { get;}
		public abstract string IdentityColumnString { get; }
		public abstract string DefaultValuestString { get; }

		public virtual DbType GetDbTypeToPrimaryKey(GeneratorMapping generator)
		{
			switch(generator.Name)
			{
				case GeneratorMapping.GeneratorType.assigned:
					ParamMapping param = generator.GetParam(DBTYPE);
					return (DbType) Enum.Parse(typeof(DbType), param.Value);

				case GeneratorMapping.GeneratorType.business:
                    ParamMapping pbiz = generator.GetParam(DBTYPE);
					return (DbType) Enum.Parse(typeof(DbType), pbiz.Value);

				case GeneratorMapping.GeneratorType.guid:
					return DbType.AnsiString;

				case GeneratorMapping.GeneratorType.native:
					StringDictionary dico = new StringDictionary();
					foreach(ParamMapping p in generator.Params)
					{
						dico.Add(p.Name, p.Value);
					}
					return GetDbTypeToNativeGenerator(dico);

				case GeneratorMapping.GeneratorType.inherited:
					GeneratorMapping tmp = generator;
					if(generator.Params == null)
						return DbType.AnsiString;
					if(generator.Params.Length == 1)
						tmp.Name = GeneratorMapping.GeneratorType.assigned;
					if(generator.Params.Length > 1)
						tmp.Name = GeneratorMapping.GeneratorType.native;
					return GetDbTypeToPrimaryKey(tmp);
			}

			return DbType.Object;
		}

        public int GetSizeToPrimaryKey(GeneratorMapping generator)
        {
            switch (generator.Name)
            {
                case GeneratorMapping.GeneratorType.assigned:
                    ParamMapping param = generator.GetParam("size");
                    if (param == null)
                        throw new SqlMapperException("Property 'size' is missing in Id Generator Mapping");
                    return Int32.Parse(param.Value);
                case GeneratorMapping.GeneratorType.guid:
                    return 36;
				case GeneratorMapping.GeneratorType.native:
					param = generator.GetParam("size");
					if (param != null)
						return Int32.Parse(param.Value);
					else
						return 36;
				case GeneratorMapping.GeneratorType.business:
					param = generator.GetParam("size");
					if (param != null)
						return Int32.Parse(param.Value);
					else
						return 0;
				case GeneratorMapping.GeneratorType.inherited:
                    GeneratorMapping tmp = generator;
                    if (generator.Params == null)
                        return 36;
                    if (generator.Params.Length == 1)
                        tmp.Name = GeneratorMapping.GeneratorType.assigned;
                    if (generator.Params.Length > 1)
                        tmp.Name = GeneratorMapping.GeneratorType.native;
                    return GetSizeToPrimaryKey(tmp);
            }

            return 0;
        }
	
        public virtual string ConvertDbTypeToString(DbType dbtype, int size, byte precision, byte scale)
        {
            SqlType sqlType = TypeName(dbtype, size);

            if (sqlType == null)
                throw new SqlMapperException(String.Format("Mapping data type unknown [{0}]", dbtype.ToString()));

            switch (dbtype)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    return sqlType.FormatType(MaxAnsiStringSize, size, precision, scale);
                case DbType.String:
                case DbType.StringFixedLength:
                    return sqlType.FormatType(MaxStringSize, size, precision, scale);
                case DbType.Binary:
                    return sqlType.FormatType(MaxBinarySize, size, precision, scale);
                default:
                    return sqlType.FormatType(size == 0 ? sqlType.Size : size, precision == 0 ? sqlType.Precision : precision, scale == 0 ? sqlType.Scale : scale);
            }
        }

		public abstract string FormatAttribute(string name);

		public virtual string FormatValue(string text, DbType type)
		{
			switch(type)
			{
				case DbType.AnsiStringFixedLength:
				case DbType.String:
				case DbType.StringFixedLength:
				case DbType.AnsiString:
                    return String.Concat(SINGLEQUOTE, text.Replace(SINGLEQUOTE, DOUBLESINGLEQUOTE), SINGLEQUOTE);

                case DbType.Boolean:
                        if (text == "false")
                            return "0";
                        else
                            return "1";
				default:
					return text;
			}
        }

        public abstract void Visit(DisableForeignKey dfkCommand);
        public abstract void Visit(EnableForeignKey dfkCommand);

        /// <summary>
        /// Gets the disable foreign key scope.
        /// </summary>
        /// <returns></returns>
        public abstract ForeignKeyScope GetDisableForeignKeyScope();

        #region Sort / Page

        public abstract ISQLExpression Page(ISQLExpression sqlExp, OrderByClause orderby, int topPage, int pageSize);

		protected SelectStatement CheckAndAdjustExpression(ISQLExpression sqlExp, OrderByClause orderby)
		{
			SelectStatement select = null;

			if(sqlExp is UnionStatement)
			{
				// put expression in a select statement

				UnionStatement union = (UnionStatement)sqlExp;
				union.TableAlias = "DERIVEDTBL";

				select = new SelectStatement(null);
				select.SelectedAllColumns = true;

				select.FromClause.Add(union as Table);

				foreach(OrderByClauseColumn oc in orderby)
					select.OrderByClause.Add(oc);

				if(union.OrderByClause.Count > 0)
					union.OrderByClause.Clear();
			}
			else if (sqlExp is SelectStatement)
			{
				((SelectStatement)sqlExp).OrderByClause = orderby;
				select = (SelectStatement)sqlExp;
			}

			return select;
		}

        #endregion

        #region Query2String

        public string Command2String(IDbCommand command)
        {
            string query = command.CommandText;

            foreach (IDbDataParameter param in command.Parameters)
            {
                query = query.Replace(param.ParameterName, FormatValue(param.Value.ToString(), param.DbType));
            }

            return query;
        }

        #endregion


        public enum ForeignKeyScope
        {
            Constraint,
            Table,
            DataBase,
            None
        }
    }
}
