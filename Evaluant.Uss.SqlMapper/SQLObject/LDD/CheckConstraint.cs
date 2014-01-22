using System;
using SQLObject.Renderer;

namespace Evaluant.Uss.SqlMapper.SqlObjectModel.LDD
{
	/// <summary>
	/// Description résumée de CheckConstraint.
	/// </summary>
	public class CheckConstraint : ColumnConstraint
	{
		private string _Predicat;

		public CheckConstraint(string predicat, bool isNull) : base (isNull)
		{
			this._Predicat = predicat;
		}

		public string Predicat
		{
			get { return _Predicat; }
		}

		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}

	}
}
