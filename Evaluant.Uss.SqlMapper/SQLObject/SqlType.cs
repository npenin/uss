using System;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de SqlType.
	/// </summary>
	public class SqlType
	{
		private string _Name;

        private int _DefaultSize;
		private byte _DefaultPrecision;
		private byte _DefaultScale;
		
		public SqlType(string name)
		{
			_Name = name;
		}

		public SqlType(string name, int size) : this(name)
		{
			_DefaultSize = size;
		}

		public SqlType(string name, byte precision, byte scale): this(name)
		{
			_DefaultPrecision = precision;
			_DefaultScale = scale;
		}

		public string FormatType()
		{
			return String.Format("{0}", _Name);
		}

		public string FormatType(int size)
		{
			if (_DefaultSize != 0)
				return String.Format("{0}({1})", _Name, size >= _DefaultSize ? _DefaultSize : size);	
			else
				return FormatType();
		}

		public string FormatType(byte precision, byte scale)
		{
			if (_DefaultPrecision != 0 && _DefaultScale != 0)
			{
				precision = precision >=  _DefaultPrecision ? _DefaultPrecision : precision;
				scale = scale >= _DefaultScale ? _DefaultScale : scale;
				return String.Format("{0}({1},{2})", _Name, precision, scale);
			}
			else
				return FormatType();
		}

		public string FormatType(int size, byte precision, byte scale)
		{
			if (_DefaultPrecision != 0 && _DefaultScale != 0)
			{
				precision = precision >=  _DefaultPrecision ? _DefaultPrecision : precision;
				scale = scale >= _DefaultScale ? _DefaultScale : scale;
				return String.Format("{0}({1},{2})", _Name, precision, scale);
			}
			else
				if (_DefaultSize != 0)
					return String.Format("{0}({1})", _Name, size >= _DefaultSize ? _DefaultSize : size);	
				else
					return FormatType();
		}	

		public string FormatType(int max_size, int size, byte precision, byte scale)
		{
			if (_DefaultPrecision != 0 && _DefaultScale != 0)
			{
				precision = precision >=  _DefaultPrecision ? _DefaultPrecision : precision;
				scale = scale >= _DefaultScale ? _DefaultScale : scale;
				return String.Format("{0}({1},{2})", _Name, precision, scale);
			}
			else
				if (_DefaultSize != 0)
				{
					if (size == 0)
						return String.Format("{0}({1})", _Name, _DefaultSize);
						
					return String.Format("{0}({1})", _Name, size >= max_size ? max_size : size);	
				}
				else
					return FormatType();
        }


        #region Properties

        public string Name
        {
            get { return _Name;  }
            set { _Name = value; }
        }

        public int Size
        {
            get { return _DefaultSize; }
            set { _DefaultSize = value; }
        }

        public byte Precision
        {
            get { return _DefaultPrecision; }
            set { _DefaultPrecision = value; }
        }

        public byte Scale
        {
            get { return _DefaultScale; }
            set { _DefaultScale = value; }
        }

        #endregion
    }
}
