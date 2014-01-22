using System;
using System.Data;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de Constant.
	/// </summary>
	public class Constant : ISQLExpression
	{
		private DbType _DbType;
		private object _Value;
		private string _Alias = string.Empty;

		public string Alias
		{
			get { return _Alias; }
			set { _Alias = value; }
		}

        public Constant(object value, DbType dbtype)
		{
			_Value = value;
			_DbType = dbtype;
		}

        public Constant(object value, DbType dbtype, string alias)
		{
			_Value = value;
			_DbType = dbtype;
			_Alias = alias;
		}

		public DbType DbType
		{
			get { return _DbType; }
		}

        public object Value
		{
			get { return _Value; }
		}


		public ITagMapping TagMapping
		{
			get { throw new NotImplementedException(); }
		}

		public void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}

		public static bool IsNumeric(DbType dbType)
		{
			switch(dbType)
			{
				case DbType.Decimal:
				case DbType.Double:
				case DbType.Int16:
				case DbType.Int32:
				case DbType.Int64:
				case DbType.UInt16:
				case DbType.UInt32:
				case DbType.UInt64:
				case DbType.VarNumeric:
					return true;
				default :
					return false;
			}
		}

        public override string ToString()
        {
            return Value.ToString();
        }
	}
}
