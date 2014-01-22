using SQLObject.Renderer;
using SQLObject;

namespace Evaluant.Uss.SqlMapper.SqlObjectModel.LDD
{
	/// <summary>
	/// Description résumée de DefaultConstraint.
	/// </summary>
	public class DefaultConstraint : ColumnConstraint
	{
        private Constant _DefaultValue;

		public DefaultConstraint(Constant default_value, bool isNull) : base ( isNull )
		{
			_DefaultValue = default_value;
		}

        public Constant DefaultValue
		{
			get { return _DefaultValue; }
		}

		public override void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
