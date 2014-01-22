using System;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de TableSource.
	/// </summary>
	public class TableSource : Table
	{
		private string _TableName;

		public TableSource(ITagMapping tag, string tableName) : this(tag, tableName, String.Empty)
		{
		}

		public TableSource(ITagMapping tag, string tableName, string tableAlias) : base (tag, tableAlias)
		{
			_TableName = tableName;
		}

		public string TableName
		{
			get { return _TableName; }
		}

		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}

        public override string ToString()
        {
            if (string.IsNullOrEmpty(TableName))
                return TableAlias;
            if (string.IsNullOrEmpty(TableAlias))
                return TableName;
            return TableName + " AS " + TableAlias;
        }
	}
}
