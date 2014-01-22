using System.Collections;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de UpdateCommand.
	/// </summary>
	public class UpdateCommand : SQLCommand
	{
		private SortedList _ColumnValueCollection = new SortedList();
		private WhereClause _WhereClause = new WhereClause(BinaryLogicOperator.And);
		
		public UpdateCommand(ITagMapping tag, string table_name)  : base (tag, table_name)
		{
		}

		/// TODO : Change the collection's type to prevent from ordering as it breaks Access driver
		public SortedList ColumnValueCollection
		{
			get { return _ColumnValueCollection; }
			set { _ColumnValueCollection = value; }
		}

		public WhereClause WhereClause
		{
			get { return _WhereClause; }
			set { _WhereClause = value; }
		}


		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
