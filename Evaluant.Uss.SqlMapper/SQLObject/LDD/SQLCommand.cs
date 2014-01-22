using System;
using SQLObject;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de ILDDCommand.
	/// </summary>
	public abstract class SQLCommand : ISQLExpression
	{
		private ITagMapping _TagMapping;
		private string _TableName;

		public SQLCommand(ITagMapping tag, string tableName)
		{
			_TagMapping = tag;
			_TableName = tableName;
		}

		public ITagMapping TagMapping
		{
			get { return _TagMapping; }
		}

		public abstract void Accept(ISQLVisitor visitor);

		public string TableName
		{
			get { return _TableName; }
		}
	}
}
