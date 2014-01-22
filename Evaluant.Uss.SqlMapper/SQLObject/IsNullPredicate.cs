using System;
using SQLObject.Renderer;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de ExistsPredicat.
	/// </summary>
	public class IsNullPredicate : LogicExpression
	{
		private ISQLExpression _Expression;

		public IsNullPredicate(ISQLExpression expression)
		{
			_Expression = expression;
		}

		public ISQLExpression Expression
		{
			get { return _Expression; }
		}


		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
