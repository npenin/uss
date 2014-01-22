using System;
using System.Collections;
using SQLObject;
using SQLObject.Renderer;

namespace Evaluant.Uss.SqlMapper.SqlObjectModel.LDD
{
	/// <summary>
	/// The CREATE TABLE command allows to create a table with columns of a data type specified. 
	/// </summary>
	/// <example>
	/// <code>
	/// CreateTableCommand cmd = new CreateTableCommand("persons");
	/// cmd.Columns.Add(new CreateColumn("personId", SqlType.CharacterData, 36)));
	/// cmd.Columns.Add(new CreateColumn("name", SqlType.CharacterData, 255)));
	/// cmd.Columns.Add(new CreateColumn("year", SqlType.NumericData));
	/// </code>
	/// </example>
	/// 
	[Serializable]
	public class CreateTableSQLCommand : ISQLExpression
	{
		protected ITagMapping _TagMapping;
        protected SortedList _ColumnDefinitions;
        protected string _TableName;
        protected string _Schema;

        protected PrimaryKey _primaryKey;
        protected ArrayList _foreignKeys = new ArrayList();

		/// <summary>
		/// Creates a new <see cref="CreateTableSQLCommand"/> instance.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		public CreateTableSQLCommand(ITagMapping tag, string tableName)
		{
			_TableName = tableName;
			_ColumnDefinitions = new SortedList();
			_TagMapping = tag; 
		}

        public CreateTableSQLCommand(ITagMapping tag, string schema, string tableName)
            : this(tag, tableName)
        {
            _Schema = schema;
        }

		/// <summary>
		/// Gets the collection of <see cref="ColumnDefinition"/>.
		/// </summary>
		/// <value></value>
		public SortedList ColumnDefinitions
		{
			get { return _ColumnDefinitions; }
		}

		/// <summary>
		/// Gets the collection of <see cref="PrimaryKey"/>.
		/// </summary>
		/// <value></value>
		public PrimaryKey PrimaryKey
		{
			get { return _primaryKey; }
			set { _primaryKey = value; }
		}

		/// <summary>
		/// Gets the collection of <see cref="ForeignKey"/>.
		/// </summary>
		/// <value></value>
		public ArrayList ForeignKeys
		{
			get { return _foreignKeys; }
		}

		/// <summary>
		/// Gets the name of the table.
		/// </summary>
		/// <value></value>
		public string TableName
		{
			get { return _TableName; } 
		}

        private ArrayList _ForeignKeyConstraints = new ArrayList();

        public ArrayList ForeignKeyConstraints
        {
            get { return _ForeignKeyConstraints; }
            set { _ForeignKeyConstraints = value; }
        }


		public ITagMapping TagMapping
		{
			get { return _TagMapping; }
		}

		public void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
