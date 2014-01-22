using System.Collections;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de UpdateCommand.
	/// </summary>
	public class DeleteCommand : SQLCommand
	{
		private WhereClause _Condition = new WhereClause(BinaryLogicOperator.And);

		public DeleteCommand(ITagMapping tag, string table_name): base (tag, table_name)
		{
		}

		public DeleteCommand(ITagMapping tag, string table_name, WhereClause condition): base (tag, table_name)
		{
			_Condition = condition;
		}

		public WhereClause Condition
		{
			get { return _Condition; }
		}


		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}

		
	}
}
