using System;
using System.Collections;

using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper
{
	[Serializable]
	public class ReferenceMappingCollection : CollectionBase
	{

		public int Add(ReferenceMapping value)
		{
			return base.List.Add(value as object);
		}

		public void Remove(ReferenceMapping value)
		{
			base.List.Remove( (object)value);
		}

		public void Insert(int index, ReferenceMapping value)
		{
			base.List.Insert(index, (object)value);
		}

		public bool Contains(ReferenceMapping value)
		{
			return base.List.Contains( (object)value);
		}

		public ReferenceMapping this[int index]
		{
			get { return ( (ReferenceMapping)base.List[index]); }
		}

        public ReferenceMapping this[string name]
        {
            get
            {
                foreach (ReferenceMapping r in base.List)
                    if (r.Name == name)
                        return r;

                return null;
            }
        }

        public ReferenceMapping this[string name, bool strict]
        {
            get
            {
                ReferenceMapping mapping = this[name];

                if(strict && mapping == null)
                    throw new MappingNotFoundException(String.Format("Attribute [{0}] not found", name));

                return mapping;
            }
        }

        public ReferenceMapping this[string name, string childType]
        {
            get
            {
                foreach (ReferenceMapping r in base.List)
                    if (r.Name == name && r.EntityChild == childType)
                        return r;
                return null;
            }
        }

 
        public ReferenceMapping this[string name, string parentType, string childType]
        {
            get
            {
                foreach (ReferenceMapping r in base.List)
                    if (r.Name == name && r.EntityParent.Type == parentType && r.EntityChild == childType)
                        return r;
                return null;
            }
        }

         public ReferenceMapping this[string name, string parentType, string childType, bool strict]
        {
            get
            {
                ReferenceMapping mapping = this[name, parentType, childType];

                if(strict && mapping == null)
                    throw new MappingNotFoundException(String.Format("Reference mapping [{0}] not found between [{1}] and [{2}]", name, parentType, childType));

                return mapping;
            }
        }
 
	}
}
