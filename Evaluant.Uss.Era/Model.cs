using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Evaluant.Uss.Era
{
    public class Model<TEntity, TAttribute, TReference> : IEnumerable<TEntity>
        where TEntity : Entity<TEntity, TAttribute, TReference>
        where TAttribute : Attribute<TEntity, TAttribute, TReference>
        where TReference : Reference<TEntity, TAttribute, TReference>
    {
        public Model()
        {
            Entities = new Dictionary<string, TEntity>();
        }

        [XmlElement("Entity")]
        public Dictionary<string, TEntity> Entities { get; set; }

        public TEntity this[string type]
        {
            get
            {
                return this[type, false];
            }
        }

        public TEntity this[string type, bool strict]
        {
            get
            {
                TEntity result;
                if (Entities.TryGetValue(type, out result))
                    return result;
                if (strict)
                    OnEntityNotFound(type);
                return null;
            }
        }

        protected virtual void OnEntityNotFound(string key)
        {
            throw new KeyNotFoundException(String.Format("Type [{0}] not found", key));
        }


        public void Add(TEntity item)
        {
            Entities.Add(item.Type, item);
        }

        #region IEnumerable<TEntity> Members

        public IEnumerator<TEntity> GetEnumerator()
        {
            return Entities.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
