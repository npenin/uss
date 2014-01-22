using System;
using System.Data;
using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de CastExpression.
	/// </summary>
	public class CastExpression : ISQLExpression
	{
		private ITagMapping _TagMapping;
		private DbType _DbType; 
		private int _Size;
		private byte _Precision;
		private byte _Scale;
		private ISQLExpression _Expression;

		public CastExpression(ITagMapping tag, ISQLExpression expression, DbType dbType)
		{
			_Expression = expression;
			_DbType = dbType;
			_TagMapping = tag;
		}

		public int Size
		{
			get { return _Size; }
			set { _Size = value; }
		}

		public byte Precision
		{
			get { return _Precision; }
			set { _Precision = value; }
		}

		public byte Scale
		{
			get { return _Scale; }
			set { _Scale = value; }
		}

		public DbType DbType
		{
			get { return _DbType; }
		}

		public ISQLExpression Expression
		{
			get { return _Expression; }
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
