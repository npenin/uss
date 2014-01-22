using System;
using System.Collections;
using Evaluant.Uss.Utility;
using System.Collections.Generic;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Cache
{
    public class IdentityMap : IEnumerable<Entity>
    {
        protected IDictionary<string, WeakReference> innerHash;

        public IdentityMap()
        {
            innerHash = new Dictionary<string, WeakReference>();
        }

        #region Implementation of IDictionary
        public IEnumerator<Entity> GetEnumerator()
        {
            foreach (KeyValuePair<string, WeakReference> item in innerHash)
            {
                if (item.Value.IsAlive != null)
                    yield return (Entity)item.Value.Target;
            }
        }

        public bool Remove(Entity e)
        {
            return innerHash.Remove(CacheEngine.GetCacheKey(e));
        }

        public void Remove(string key)
        {
            innerHash.Remove(key);
        }

        public virtual bool Contains(string key)
        {
            if (!innerHash.ContainsKey(key))
                return false;

            Entity o = this[key];
            return o != null;
        }

        public bool Contains(object key)
        {
            return Contains(key as string);
        }

        public void Clear()
        {
            innerHash.Clear();
        }


        public virtual void Add(string key, Entity value)
        {
            innerHash.Add(key, new WeakReference(value));
        }

        public bool IsReadOnly
        {
            get
            {
                return innerHash.IsReadOnly;
            }
        }

        public Entity this[string key]
        {
            get
            {
                return GetValue(key);
            }
            set
            {
                innerHash[key] = new WeakReference(value);
            }
        }

        #endregion

        #region Implementation of ICollection
        public void CopyTo(Entity[] array, int index)
        {
            int i = index;
            foreach (KeyValuePair<string, WeakReference> item in innerHash)
            {
                if (item.Value.IsAlive)
                    array[i] = (Entity)item.Value.Target;
                i++;
            }
            //innerHash.CopyTo(array, index);
        }

        public int Count
        {
            get
            {
                return innerHash.Count;
            }
        }

        #endregion


        #region "HashTable Methods"

        /// <summary>
        /// Gets an object already registered
        /// </summary>
        /// <param name="key">The key of the object to retrieve</param>
        /// <returns>Null if the object is garbage collected or has never been referenced</returns>
        /// <remarks>
        /// It can't be called inside a foreach loop as it can change the collection.
        /// </remarks>
        public Entity GetValue(string key)
        {
            if (!innerHash.ContainsKey(key))
                return null;

            WeakReference weakref = innerHash[key] as WeakReference;

            // Get a reference to block its releasing
            Entity target = (Entity)weakref.Target;

            if (weakref.IsAlive)
                return target;
            else
                innerHash.Remove(key);

            return null;
        }

        public virtual bool ContainsValue(Entity value)
        {
            foreach (WeakReference o in innerHash.Values)
                if (o != null)
                {
                    if (o.IsAlive)
                    {
                        if (o.Target == value)
                            return true;
                    }
                }

            return false;
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }


}