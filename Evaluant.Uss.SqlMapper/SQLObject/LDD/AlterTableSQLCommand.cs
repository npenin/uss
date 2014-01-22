using System;
using System.Collections;
using SQLObject;
using SQLObject.Renderer;

namespace Evaluant.Uss.SqlMapper.SqlObjectModel.LDD
{
	/// <summary>
	/// The ALTER TABLE command allows to modify/add/remove column definition or constraint (PrimaryKey, ForeignKey) of a table. 
	/// </summary>
	/// <example>
	/// <code>
	/// AlterTableCommand cmd = new AlterTableCommand("persons");
	/// cmd.DropForeignKeys.Add(new ForeignKey()));
	/// </code>
	/// </example>
	/// 

    public enum AlterTypeEnum { AlterColumn, Add, Drop };

	[Serializable]
	public class AlterTableSQLCommand : ISQLExpression, ICloneable
	{
        

		private ITagMapping _TagMapping;
        private ArrayList _ColumnDefinitions;
        private PrimaryKey _PrimaryKey;
        private ArrayList _ForeignKeys;
        private AlterTypeEnum _AlterType;

		private string _TableName;

		/// <summary>
		/// Creates a new <see cref="CreateTableSQLCommand"/> instance.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
        public AlterTableSQLCommand(ITagMapping tag, string tableName, AlterTypeEnum alterType)
		{
            _AlterType = alterType;
			_TableName = tableName;
            _TagMapping = tag;
			_ColumnDefinitions = new ArrayList();
            _ForeignKeys = new ArrayList();
		}

        public AlterTypeEnum AlterType
        {
            get { return _AlterType; }
            set { _AlterType = value; }
        }

		public string TableName
		{
			get { return _TableName; } 
		}

		public ITagMapping TagMapping
		{
			get { return _TagMapping; }
		}

        public ArrayList ColumnDefinitions
        {
            get { return _ColumnDefinitions; }
            set { _ColumnDefinitions = value; }
        }

        public PrimaryKey PrimaryKey
        {
            get { return _PrimaryKey; }
            set { _PrimaryKey = value; }
        }

        public ArrayList ForeignKeys
        {
            get { return _ForeignKeys; }
            set { _ForeignKeys = value; }
        }

		public void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}

        #region ICloneable Members

        public object Clone()
        {
            AlterTableSQLCommand cmd = new AlterTableSQLCommand(this.TagMapping, this.TableName, this.AlterType);
            cmd.ColumnDefinitions = this.ColumnDefinitions.Clone() as ArrayList;

            foreach (ForeignKey fk in ForeignKeys)
                cmd.ForeignKeys.Add(new ForeignKey(fk.Name, fk.ForeignKeys, fk.ParentTable, fk.ReferenceKeys));

            if (this.PrimaryKey != null)
                cmd.PrimaryKey = this.PrimaryKey.Clone() as PrimaryKey;

            return cmd;
        }

        #endregion
    }
}
