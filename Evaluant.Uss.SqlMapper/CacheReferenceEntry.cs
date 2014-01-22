using System.Collections;
using System.Collections.Specialized;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de CacheReferenceEntry.
	/// </summary>
	class CacheReferenceEntry : CacheEntry
	{
		private ArrayList _QueryEntries = new ArrayList();

		public CacheReferenceEntry(ReferenceMapping mapping) : base(mapping)
		{
		}

		public ArrayList QueryEntries
		{
			get { return _QueryEntries; }
		}

        private StringCollection _DisableTable = new StringCollection();

        public StringCollection DisableTable
        {
            get { return _DisableTable; }
            set { _DisableTable = value; }
        }

		public bool ContainsQuery(string tableName)
		{
			foreach(CacheQueryEntry entry in _QueryEntries)
			{
				if (entry.Query.TableName == tableName)
					return true;
			}
			return false;
		}

		public CacheQueryEntry GetQueryFromTableName(string tableName)
		{
			foreach(CacheQueryEntry entry in _QueryEntries)
			{
				if (entry.Query.TableName == tableName)
					return entry;
			}
			return null;
		}
	}
}
