using System;
using System.Collections;

using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper
{
	[Serializable]
	public class RuleMappingCollection : CollectionBase
	{

		public int Add(RuleMapping value)
		{
			return base.List.Add(value as object);
		}

		public void Remove(RuleMapping value)
		{
			base.List.Remove( (object)value);
		}

		public void Insert(int index, RuleMapping value)
		{
			base.List.Insert(index, (object)value);
		}

		public bool Contains(RuleMapping value)
		{
			return base.List.Contains( (object)value);
		}

		public RuleMapping this[int index]
		{
			get { return ( (RuleMapping)base.List[index]); }
		}
	}
}
