using System;
using System.Data;
using System.Text;
using Evaluant.Uss.SqlMapper.SqlObjectModel.LDD;

namespace SQLObject.Renderer
{
	/// <summary>
	/// Description résumée de BaseStringSQLVisitor.
	/// </summary>
	abstract class BaseStringSQLVisitor : ISQLVisitor
	{
		private StringBuilder _Query = new StringBuilder();
		public StringBuilder Query
		{
			get { return _Query; }
		}

		public void Visit(SelectStatement select_statement)
		{
			_Query.Append("SELECT ");

			if (select_statement.IsDistinct)
				_Query.Append("DISTINCT ");

			if (select_statement.SelectedAllColumns)
				_Query.Append("* ");

			if (select_statement.SelectedAllColumns && select_statement.SelectList.Count != 0)
				_Query.Append(", ");

			foreach(ISQLExpression item in select_statement.SelectList)
			{
				item.Accept(this);
				if ( select_statement.SelectList.IndexOf(item) != select_statement.SelectList.Count - 1 )
					_Query.Append(", ");
			}

            select_statement.FromClause.Accept(this);
			
			if (select_statement.WhereClause != null)
				select_statement.WhereClause.Accept(this);
			
			if( select_statement.OrderByClause != null)
				select_statement.OrderByClause.Accept(this);
		}

		public void Visit(FromClause from_clause)
		{
			_Query.Append("FROM ");
			foreach(TableSource table in from_clause)
			{
				table.Accept(this);
				if ( from_clause.IndexOf(table) != from_clause.Count - 1 )
					_Query.Append(", ");
			}
		}

		public void Visit(WhereClause where_clause)
		{
			_Query.Append("WHERE ");

//			where_clause.SearchCondition.Accept(this);
		}

		public void Visit(OrderByClause order_by_clause)
		{
			_Query.Append("ORDER BY ");
			foreach(Column column in order_by_clause)
			{
				column.Accept(this);
				if (order_by_clause.IndexOf(column) != order_by_clause.Count -1 )
					_Query.Append(", ");
			}
		}

		public void Visit(UnionStatement unionStatement)
		{
			if(unionStatement.TableAlias != null && unionStatement.TableAlias != string.Empty)
				_Query.Append(" ( ");

			foreach(SelectStatement query in unionStatement.SelectExpressions)
			{
				query.Accept(this);

				if (unionStatement.SelectExpressions.IndexOf(query) != unionStatement.SelectExpressions.Count - 1)
					_Query.Append("UNION ");
			}

			if(unionStatement.TableAlias != null && unionStatement.TableAlias != string.Empty)
				_Query.Append(String.Concat(" ) AS ", unionStatement.TableAlias, " "));
		}

		public void Visit(BinaryLogicOperator op)
		{
			switch(op)
			{
				case BinaryLogicOperator.Equals :
					_Query.Append("= ");
					break;

				case BinaryLogicOperator.Greater :
					_Query.Append("> ");
					break;

				case BinaryLogicOperator.Lesser :
					_Query.Append("< ");
					break;

				case BinaryLogicOperator.GreaterOrEquals :
					_Query.Append(">= ");
					break;

				case BinaryLogicOperator.LesserOrEquals :
					_Query.Append("<= ");
					break;

				case BinaryLogicOperator.NotEquals :
					_Query.Append("<> ");
					break;

				default :
					_Query.AppendFormat("{0} ",op.ToString());
					break;

			}
		}

		public abstract void Visit(InsertCommand insertCommand);
		public abstract void Visit(UpdateCommand updateCommand);
		public abstract void Visit(DeleteCommand deleteCommand);
		public abstract void Visit(Parameter param);
		public abstract void Visit(DropTableSQLCommand command);
		public abstract void Visit(CreateTableSQLCommand command);
		public abstract void Visit(ColumnDefinition column);

		public abstract void Visit(ColumnConstraint constraint);
		public abstract void Visit(DefaultConstraint constraint);
		public abstract void Visit(CheckConstraint constraint);
		public abstract void Visit(ForeignKeyConstraint constraint);
		public abstract void Visit(PrimaryKeyConstraint constraint);
		public abstract void Visit(UniqueConstraint constraint);


		public void Visit(Column column)
		{
			if (column.TableName != String.Empty)
				_Query.AppendFormat("{0}.", column.TableName);

			_Query.AppendFormat("{0} ",column.ColumnName);

			if(column.Alias != null && column.Alias != string.Empty)
				_Query.Append(String.Concat("AS ", column.Alias, " "));
		}

		public void Visit(Constant constant)
		{
			if (constant.DbType == DbType.AnsiString)
				_Query.AppendFormat("'{0}' ", constant.Value);
			else
				_Query.AppendFormat("{0} ", constant.Value);

			if(constant.Alias != string.Empty)
				_Query.Append(" as " + constant.Alias + " ");
		}

		public abstract void Visit(UnaryLogicExpression expression);
		public abstract void Visit(AggregateFunction function);
		public abstract void Visit(CastExpression expression);

		public void Visit(BinaryLogicExpression expression)
		{
			expression.LeftOperand.Accept(this);
			Visit(expression.Operator);
			expression.RightOperand.Accept(this);
		}

		public void Visit(LikePredicate predicate)
		{
			predicate.MatchValue.Accept(this);
			_Query.AppendFormat("LIKE '{0}' ", predicate.Pattern);

			if (!Char.IsWhiteSpace(predicate.CharacterEscape))
				_Query.AppendFormat("ESCAPE '{0}' ", predicate.CharacterEscape);
		}

		public void Visit(ExistsPredicate predicate)
		{
			_Query.Append("EXISTS ( ");
			predicate.SubQuery.Accept(this);
			_Query.Append(") ");
		}

		public void Visit(InPredicate predicate)
		{
			predicate.Column.Accept(this);

			_Query.Append( " IN (");

			if(predicate.SubQueries[0] is SelectStatement)
				predicate.SubQueries[0].Accept(this);
			
			if(predicate.SubQueries[0] is ValueExpression)
			{
				foreach(ValueExpression val in predicate.SubQueries)
				{
					val.Accept(this);
					_Query.Append(" ,");
				}
			}

			if(_Query[_Query.Length - 1] == ',')
				_Query.Remove(_Query.Length - 1, 1);
			_Query.Append(" )");
		}

		public abstract void Visit(IsNullPredicate predicate);
		
		public void Visit(SelectItem item)
		{
			if (item.ColumnAlias != String.Empty)
				_Query.AppendFormat("AS {0} ", item.ColumnAlias);
		}

		public void Visit(TableSource table)
		{
			_Query.AppendFormat("{0} ", table.TableName);
			
			if (table.TableAlias != String.Empty)
				_Query.AppendFormat("AS {0} ", table.TableAlias);
		}

		public void Visit(JoinedTable table)
		{
			Visit(table.Type);
			table.RigthTable.Accept(this);

			if (table.SearchCondition != null)
			{
				_Query.Append("ON ");
				table.SearchCondition.Accept(this);
			}
			
		}

		public void Visit(TypeJoinedTable type)
		{
			switch(type)
			{
				case TypeJoinedTable.Inner:
					_Query.Append("INNER JOIN ");
					break;

				case TypeJoinedTable.LeftOuter:
					_Query.Append("LEFT OUTER ");
					break;

				case TypeJoinedTable.RightOuter:
					_Query.Append("RIGHT OUTER ");
					break;
			}
		}


	}
}
