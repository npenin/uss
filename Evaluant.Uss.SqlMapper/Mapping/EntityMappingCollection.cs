using System;
using System.Collections;

using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper
{
    [Serializable]
    public class EntityMappingCollection : CollectionBase
    {

        public int Add(EntityMapping value)
        {
            if (this[value.Type] == null)
                return base.List.Add(value as object);

            return 0;
        }

        public void Remove(EntityMapping value)
        {
            base.List.Remove((object)value);
        }

        public void Insert(int index, EntityMapping value)
        {
            base.List.Insert(index, (object)value);
        }

        public bool Contains(EntityMapping value)
        {
            return base.List.Contains((object)value);
        }

        public EntityMapping this[int index]
        {
            get { return (EntityMapping)base.List[index]; }
        }

        public EntityMapping FindByTableName(string table)
        {
            foreach (EntityMapping e in base.List)
            {
                if (e.Table == table)
                    return e;
            }
            return null;
        }

        public EntityMapping FindByTableNameOrType(string tableOrType)
        {
            foreach (EntityMapping e in base.List)
            {
                if (e.Table == tableOrType || e.Type == tableOrType)
                    return e;
            }
            return null;
        }

        public EntityMapping this[string type]
        {
            get
            {
                foreach (EntityMapping e in base.List)
                    if (e.Type == type)
                        return e;
                return null;
            }
        }

        public EntityMapping this[string type, bool strict]
        {
            get
            {
                EntityMapping mapping = this[type];

                if (strict && mapping == null)
                    throw new MappingNotFoundException(string.Format("Type [{0}] not found", type));

                return mapping;
            }
        }
    }
}
