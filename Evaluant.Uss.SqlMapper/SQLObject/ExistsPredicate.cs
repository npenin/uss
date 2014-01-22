using System;
using SQLObject.Renderer;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de ExistsPredicat.
	/// </summary>
	public class ExistsPredicate : LogicExpression
	{
		private SelectStatement _SubQuery;

		public ExistsPredicate(SelectStatement subquery)
		{
			_SubQuery = subquery;
		}

		public SelectStatement SubQuery
		{
			get { return _SubQuery; }
		}

		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
