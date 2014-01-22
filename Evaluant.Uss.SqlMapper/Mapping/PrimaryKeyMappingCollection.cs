using System;
using System.Collections;

using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper
{
	[Serializable]
	public class PrimaryKeyMappingCollection : CollectionBase
	{
		public int Add(PrimaryKeyMapping value)
		{
			return base.List.Add(value as object);
		}

		public void Remove(PrimaryKeyMapping value)
		{
			base.List.Remove( (object)value);
		}

		public void Insert(int index, PrimaryKeyMapping value)
		{
			base.List.Insert(index, (object)value);
		}

		public bool Contains(PrimaryKeyMapping value)
		{
			return base.List.Contains( (object)value);
		}

        public PrimaryKeyMapping this[int index]
        {
            get { return ((PrimaryKeyMapping)base.List[index]); }
            set { base.List[index] = value; }
        }

        public PrimaryKeyMapping this[string fieldName]
        {
            get
            {
                foreach (PrimaryKeyMapping a in base.List)
                    if (a.Field == fieldName)
                        return a;

                return null;
            }
        }

        public PrimaryKeyMapping this[string name, bool strict]
        {
            get
            {
                PrimaryKeyMapping mapping = this[name];

                if(strict && mapping == null)
                    throw new MappingNotFoundException(String.Format("Attribute [{0}] not found", name));

                return mapping;
            }
        }
	}
}
