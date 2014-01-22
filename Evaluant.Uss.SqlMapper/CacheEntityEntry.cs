using System.Collections;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de CacheEntityEntry.
	/// </summary>
	class CacheEntityEntry : CacheEntry
	{
		private ArrayList _InsertCompoundQueryEntries = new ArrayList();
		private ArrayList _UpdateCompoundQueryEntries = new ArrayList();

		private ArrayList _CreateReferenceEntries = new ArrayList();

		public CacheEntityEntry(EntityMapping mapping) : base(mapping)
		{
		}

		public ArrayList InsertCompoundQueryEntries
		{
			get { return _InsertCompoundQueryEntries; }
		}

		public CacheQueryEntry GetInsertCompoundQueryEntries(string tableName)
		{
			foreach(CacheQueryEntry entry in _InsertCompoundQueryEntries)
			{
				if (entry.Query.TableName == tableName)
					return entry;
			}
			return null;
		}

		public bool InsertCompoundQueryEntriesContains(string tableName)
		{
			foreach(CacheQueryEntry entry in _InsertCompoundQueryEntries)
			{
				if (entry.Query.TableName == tableName)
					return true;
			}
			return false;
		}

		public ArrayList UpdateCompoundQueryEntries
		{
			get { return _UpdateCompoundQueryEntries; }
		}

		public CacheQueryEntry GetUpdateCompoundQueryEntries(string tableName)
		{
			foreach(CacheQueryEntry entry in _UpdateCompoundQueryEntries)
			{
				if (entry.Query.TableName == tableName)
					return entry;
			}
			return null;
		}

		public bool UpdateCompoundQueryEntriesContains(string tableName)
		{
			foreach(CacheQueryEntry entry in _UpdateCompoundQueryEntries)
			{
				if (entry.Query.TableName == tableName)
					return true;
			}
			return false;
		}

		public ArrayList CreateReferenceEntries
		{
			get { return _CreateReferenceEntries; }
		}

		public CacheReferenceEntry GetCreateReferenceEntries(string name)
		{
			foreach(CacheReferenceEntry entry in _CreateReferenceEntries)
			{
				if (((ReferenceMapping)entry.Mapping).Name == name)
					return entry;
			}
			return null;
		}

		public CacheReferenceEntry GetCreateReferenceEntries(string name, string parentType, string childType)
		{
			foreach(CacheReferenceEntry entry in _CreateReferenceEntries)
			{
				ReferenceMapping rm = (ReferenceMapping) entry.Mapping;
				if (rm.Name == name && rm.EntityParent.Type == parentType && rm.EntityChild == childType)
					return entry;
			}
			return null;
		}

		public bool CreateReferenceEntriesContains(string name)
		{
			foreach(CacheReferenceEntry entry in _CreateReferenceEntries)
			{
				if (((ReferenceMapping)entry.Mapping).Name == name)
					return true;
			}
			return false;
		}

	}
}
