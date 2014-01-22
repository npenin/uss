using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description r�sum�e de DeleteCommand.
	/// </summary>
	public class DropTableSQLCommand : SQLCommand
	{
		public DropTableSQLCommand(ITagMapping tag, string table_name): base (tag, table_name)
		{
		}

		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
