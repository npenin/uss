using SQLObject.Renderer;

namespace Evaluant.Uss.SqlMapper.SqlObjectModel.LDD
{
	/// <summary>
	/// Description résumée de ColumnConstraint.
	/// </summary>
	public class ColumnConstraint
	{
		private bool _IsNull;

		public ColumnConstraint(bool isNull)
		{
			this._IsNull = isNull;
		}

		public bool IsNull
		{
			get { return _IsNull; }
		}

		public virtual void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
