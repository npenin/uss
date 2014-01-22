using Evaluant.Uss.SqlMapper.SqlObjectModel.LDD;

namespace SQLObject.Renderer
{
	/// <summary>
	/// Description résumée de ISQLVisitor.
	/// </summary>
	public interface ISQLVisitor
	{
		void Visit ( TableSource table );

		void Visit ( JoinedTable table );
		void Visit ( SelectStatement select_statement );

		void Visit ( Column column );
		void Visit ( SelectItem item );
        void Visit ( SystemObject item);
		void Visit ( Constant constant );
        void Visit ( CaseExpression expression);

		void Visit ( UnaryLogicExpression expression );
		void Visit ( AggregateFunction function );
		void Visit ( CastExpression expression );

        void Visit (LogicExpressionCollection collection);
		void Visit ( BinaryLogicExpression expression );
		void Visit ( LikePredicate predicate );
		void Visit ( ExistsPredicate predicate );
		void Visit ( InPredicate predicate );
		void Visit ( IsNullPredicate predicate );
		
		void Visit ( FromClause from_clause );
		void Visit ( WhereClause where_clause );
		void Visit ( OrderByClause order_by_clause );
        void Visit ( OrderByClauseColumn col);

		void Visit ( UnionStatement unionStatement );

		void Visit ( BinaryLogicOperator op );
		
		void Visit ( InsertCommand insertCommand );
		void Visit ( UpdateCommand updateCommand );
		void Visit ( DeleteCommand deleteCommand );
		void Visit ( Parameter param );

		void Visit ( DropTableSQLCommand command );
		void Visit ( CreateTableSQLCommand command );
        void Visit ( AlterTableSQLCommand command);
		void Visit ( ColumnDefinition column );

		/// TODO: Consider revising the object model to include all the constraints into a single Constraint class
		
		void Visit ( ColumnConstraint constraint );
		void Visit ( DefaultConstraint constraint );
		void Visit ( CheckConstraint constraint );
        void Visit ( UniqueConstraint constraint);

        void Visit(DisableForeignKey dfkCommand);
        void Visit(EnableForeignKey efkCommand);

	}
}
