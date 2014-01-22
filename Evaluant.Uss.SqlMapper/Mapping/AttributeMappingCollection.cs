using System;
using System.Collections;

using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper
{
	[Serializable]
	public class AttributeMappingCollection : CollectionBase
	{
        private string typeName;

        public string TypeName
        {
            get { return typeName; }
            set { typeName = value; }
        }

		public int Add(AttributeMapping value)
		{
			return base.List.Add(value as object);
		}

		public void Remove(AttributeMapping value)
		{
			base.List.Remove( (object)value);
		}

		public void Insert(int index, AttributeMapping value)
		{
			base.List.Insert(index, (object)value);
		}

		public bool Contains(AttributeMapping value)
		{
			return base.List.Contains( (object)value);
		}

		public AttributeMapping this[int index]
		{
			get { return ( (AttributeMapping)base.List[index]); }
		}

        public AttributeMapping this[string name]
        {
            get
            {
                foreach (AttributeMapping a in base.List)
                    if (a.Name == name)
                        return a;

                foreach (AttributeMapping a in base.List)
                    if (a.Name == "*")
                        return a;
                return null;
            }
        }

        public AttributeMapping FindByField(string field)
        {
            foreach (AttributeMapping a in base.List)
                if (a.Field == field)
                    return a;
            return null;
        }

        public AttributeMapping this[string name, bool strict]
        {
            get
            {
                AttributeMapping mapping = this[name];

                if(strict && mapping == null)
                    throw new MappingNotFoundException(String.Format("Attribute Mapping [{0}] not found in type [{1}]", name, typeName));

                return mapping;
            }
        }
	}
}
