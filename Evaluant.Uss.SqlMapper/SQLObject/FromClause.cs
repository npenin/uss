using System;
using SQLObject.Renderer;

using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de FromClause.
	/// </summary>
	public class FromClause : TableCollection, ISQLExpression
	{
        private ITagMapping _TagMapping;

		public FromClause() : base()
		{
		}

        public ITagMapping TagMapping
        {
            get { return _TagMapping; }
            set { _TagMapping = value; }
        }

		public void Replace(Table oldValue, Table currentValue)
		{
			int index = IndexOf(oldValue);
			if (index != -1)
				List[index] = currentValue;
			else
				List.Add(currentValue);
		}

		public override bool Contains(Table value)
		{
			foreach(Table table in List)
			{
				if (table.TableAlias != String.Empty && table.TableAlias == value.TableAlias)
					return true;
			}
			return base.Contains(value);
		}

		public override int IndexOf(Table value)
		{
			for(int index = 0; index < List.Count; index++)
			{
				if (((Table)List[index]).TableAlias != String.Empty && ((Table)List[index]).TableAlias == value.TableAlias)
					return index;
			}
			return base.IndexOf (value);
		}



		public void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
