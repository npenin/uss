using SQLObject.Renderer;
using System.Data;

namespace Evaluant.Uss.SqlMapper.SqlObjectModel.LDD
{
	/// <summary>
	/// Description résumée de ColumnDefinition.
	/// </summary>
	public class ColumnDefinition
	{
		private string _ColumnName;
		private DbType _Type;
        private int _Size;
        private byte _Precision;
        private byte _Scale;

        private bool _IsAutoIncrement = false;
        private int _Seed = 1;
        private int _Increment = 1;

		private ColumnConstraint _ColumnConstraints;

        /// <summary>
        /// Creates a new <see cref="ColumnDefinition"/> instance.
        /// </summary>
        /// <param name="column_name">Column_name.</param>
        /// <param name="type">Type.</param>
        public ColumnDefinition(string column_name, DbType type)
            : this(column_name, type, 0, 0, 0, null)
        { 
        }

        /// <summary>
        /// Creates a new <see cref="ColumnDefinition"/> instance.
        /// </summary>
        /// <param name="column_name">Column_name.</param>
        /// <param name="type">Type.</param>
        /// <param name="size">Size.</param>
        public ColumnDefinition(string column_name, DbType type, int size)
            : this(column_name, type, size, 0, 0, null)
        { 
        }

		/// <summary>
		/// Creates a new <see cref="ColumnDefinition"/> instance.
		/// </summary>
		/// <param name="column_name">Column_name.</param>
		/// <param name="type">Type.</param>
        /// <param name="size">Size.</param>
        /// <param name="precision">Precision.</param>
        /// <param name="scale">Scale.</param>
		public ColumnDefinition(string column_name, DbType type, int size, byte precision, byte scale) 
            : this(column_name, type, size, precision, scale, null)
		{
		}

        /// <summary>
        /// Creates a new <see cref="ColumnDefinition"/> instance.
        /// </summary>
        /// <param name="column_name">Column_name.</param>
        /// <param name="type">Type.</param>
        /// <param name="column_constraint">Column_constraint.</param>
        public ColumnDefinition(string column_name, DbType type, ColumnConstraint column_constraint)
            : this(column_name, type, 0, 0, 0, column_constraint)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ColumnDefinition"/> instance.
        /// </summary>
        /// <param name="column_name">Column_name.</param>
        /// <param name="type">Type.</param>
        /// <param name="size">Size.</param>
        /// <param name="column_constraint">Column_constraint.</param>
        public ColumnDefinition(string column_name, DbType type, int size, ColumnConstraint column_constraint)
            : this(column_name, type, size, 0, 0, column_constraint)
        {
        }


		/// <summary>
		/// Creates a new <see cref="ColumnDefinition"/> instance.
		/// </summary>
		/// <param name="column_name">Column_name.</param>
		/// <param name="type">Type.</param>
        /// <param name="size">Size.</param>
        /// <param name="precision">Precision.</param>
        /// <param name="scale">Scale.</param>
		/// <param name="column_constraint">Column_constraint.</param>
		public ColumnDefinition(string column_name, DbType type, int size, byte precision, byte scale, ColumnConstraint column_constraint) 
		{
			this._ColumnName = column_name;
			this._Type = type;
			this._ColumnConstraints = column_constraint;
            this._Size = size;
            this._Precision = precision;
            this._Scale = scale;
		}

		/// <summary>
		/// Gets the name of the column.
		/// </summary>
		/// <value></value>
		public string ColumnName
		{
			get { return _ColumnName; }
		}

		/// <summary>
		/// Gets the type of the column.
		/// </summary>
		/// <value></value>
		public DbType Type
		{
			get { return _Type; }
		}

        /// <summary>
        /// Gets the size of the column.
        /// </summary>
        /// <value></value>
        public int Size
        {
            get { return _Size; }
        }

        /// <summary>
        /// Gets the precision of the column.
        /// </summary>
        /// <value></value>
        public byte Precision
        {
            get { return _Precision; }
        }

        /// <summary>
        /// Gets the scale of the column.
        /// </summary>
        /// <value></value>
        public byte Scale
        {
            get { return _Scale; }
        }

        /// <summary>
        /// Gets the increment of the column.
        /// </summary>
        /// <value></value>
        public int Increment
        {
            get { return _Increment; }
        }

        /// <summary>
        /// Gets the seed of the column.
        /// </summary>
        /// <value></value>
        public int Seed
        {
            get { return _Seed; }
        }

        public bool IsAutoIncrement
        {
            get { return _IsAutoIncrement; }
        }

        public void EnableAutoIncrement(int increment, int seed)
        {
            this._IsAutoIncrement = true;
            this._Seed = seed;
            this._Increment = increment;
        }

        public void DisableAutoIncrement()
        {
            this._IsAutoIncrement = true;
            this._Seed = 1;
            this._Increment = 1;
        }


		/// <summary>
		/// Gets the <see cref="ColumnConstraint"/>
		/// </summary>
		/// <value></value>
		public ColumnConstraint ColumnConstraint
		{
			get { return _ColumnConstraints; }
            set { _ColumnConstraints = value; }
		}

		public void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
