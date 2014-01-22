using System;
using System.Collections;
using SQLObject.Renderer;

namespace Evaluant.Uss.SqlMapper.SqlObjectModel.LDD
{
	/// <summary>
	/// Description résumée de PrimaryKeyConstraint.
	/// </summary>
	public class PrimaryKey : ICloneable
	{
		private string _name;
		private ArrayList _columns = new ArrayList();

		public PrimaryKey(string name) : this(name, new ColumnDefinition[0])
		{
		}
		
		public PrimaryKey(ColumnDefinition[] columns) : this(null, columns)
		{
		}
		
		public PrimaryKey(string name, ColumnDefinition[] columns)
		{
			if(columns == null)
				throw new ArgumentNullException("columns");

			_name = name;
			_columns.AddRange(columns);
		}

		public ArrayList Columns
		{
			get { return _columns; }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

        #region ICloneable Members

        public object Clone()
        {
            ColumnDefinition[] colDef = this.Columns.ToArray(typeof(ColumnDefinition)) as ColumnDefinition[];
            return new PrimaryKey(this.Name, colDef);
        }

        #endregion
    }
}
