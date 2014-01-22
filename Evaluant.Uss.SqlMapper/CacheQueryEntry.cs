using System.Collections;
using SQLObject;

namespace Evaluant.Uss.SqlMapper
{
	class CacheQueryEntry
	{
		private bool _IsGenericQuery = false;
		private SQLCommand _Query;
		private ArrayList _Parameters = new ArrayList();

		public bool IsAttributeGenericQuery
		{
			get { return _IsGenericQuery; }
			set { _IsGenericQuery = value; }
		}

		public CacheQueryEntry(SQLCommand query)
		{
			_Query = query;
		}

		public SQLCommand Query
		{
			get { return _Query; }
		}

		public ArrayList Parameters
		{
			get { return _Parameters; }
			set { _Parameters = value; }
		}

		public bool ContainsParameter(string name)
		{
			foreach(Parameter param in _Parameters)
			{
				if(param.Name == name)
					return true;
			}
			return false;
		}

		public Parameter GetParameterFromName(string name)
		{
			foreach(Parameter param in _Parameters)
			{
				if(param.Name == name)
					return param;
			}
			return null;
		}

		public virtual CacheQueryEntry Clone()
		{
			CacheQueryEntry queryEntry = new CacheQueryEntry(_Query);
			foreach(Parameter cloneParam in _Parameters)
			{
				queryEntry.Parameters.Add(cloneParam.Clone());
			}
		
			return queryEntry;
		}
	}
}
