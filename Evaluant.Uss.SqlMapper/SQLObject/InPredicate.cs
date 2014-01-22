using System;
using SQLObject.Renderer;
using System.Collections;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de InPredicate.
	/// </summary>
	public class InPredicate : LogicExpression
	{
		public InPredicate(Column c, ExpressionCollection expressions)
		{
			_SubQueries = expressions;
			_Column = c;
		}

		public InPredicate(Column c)
		{
			_SubQueries = new ExpressionCollection();
			_Column = c;
		}

		private Column _Column;
		public Column Column
		{
			get { return _Column; }
			set { _Column = value; }
		}

		private ExpressionCollection _SubQueries;
		public ExpressionCollection SubQueries
		{
			get { return _SubQueries; }
			set { _SubQueries = value; }
		}

        [System.Diagnostics.DebuggerStepThrough]
		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(Column);
            sb.Append(" IN (");
            foreach (ISQLExpression e in SubQueries)
                sb.Append(e).Append(", ");
            sb.Remove(sb.Length - 2, 2);
            sb.Append(")");
            return sb.ToString();
        }
	}
}
