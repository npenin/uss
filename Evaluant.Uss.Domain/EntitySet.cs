using System;
using System.Collections;
using Evaluant.Uss.Collections;
using System.Collections.Generic;

namespace Evaluant.Uss.Domain
{
    /// <summary>
    ///     <para>
    ///       A collection that stores <see cref='Entity'/> objects.
    ///    </para>
    /// </summary>
    /// <seealso cref='EntitySet'/>
#if !SILVERLIGHT
    [Serializable()]
#endif
    public class EntitySet : HashedList<Entity>
    {

        /// <summary>
        ///     <para>
        ///       Initializes a new instance of <see cref='EntitySet'/>.
        ///    </para>
        /// </summary>
        public EntitySet()
            : base()
        {
        }

        /// <summary>
        ///     <para>
        ///       Initializes a new instance of <see cref='EntitySet'/> containing any array of <see cref='Entity'/> objects.
        ///    </para>
        /// </summary>
        /// <param name='val'>
        ///       A array of <see cref='Entity'/> objects with which to intialize the collection
        /// </param>
        public EntitySet(IEnumerable<Entity> val)
        {
            this.AddRange(val);
        }

        ///// <summary>
        ///// Sorts its elements.
        ///// </summary>
        ///// <remarks>The value to use as the sort criterium must be loaded in the object graph</remarks>
        ///// <param name="path">Path.</param>
        //public void Sort(string path)
        //{
        //    Sort(new string[] { path });
        //}

        ///// <summary>
        ///// Sorts its elements.
        ///// </summary>
        ///// <remarks>The value to use as the sort criterium must be loaded in the object graph</remarks>
        ///// <param name="path">Path.</param>
        ///// <param name="ascending">False to sort in a reverse order.</param>
        //public void Sort(string path, bool ascending)
        //{
        //    ArrayList sortedList = new ArrayList(base.Count);
        //    sortedList.AddRange(this);
        //    sortedList.Sort(new NavigationComparer(path, ascending));

        //    base.Clear();
        //    for (int i = 0; i < sortedList.Count; i++)
        //        base.Add(sortedList[i]);
        //}

        ///// <summary>
        ///// Sorts its elements.
        ///// </summary>
        ///// <remarks>The value to use as the sort criterium must be loaded in the object graph</remarks>
        ///// <param name="path">Path.</param>
        ///// <param name="ascending">False to sort in a reverse order.</param>
        //public void Sort(string[] path)
        //{
        //    ArrayList directions = new ArrayList(path.Length);

        //    for (int i = 0; i < path.Length; i++)
        //    {
        //        string[] values = path[i].Trim().Split();
        //        bool ascending = true;

        //        if (values.Length > 2)
        //            throw new ArgumentException("path[" + i + "]");

        //        if (values.Length == 2)
        //        {
        //            ascending = (values[1] == "asc");
        //            path[i] = values[0];
        //        }
        //        directions.Add(ascending);
        //    }

        //    ArrayList sortedList = new ArrayList(base.Count);
        //    sortedList.AddRange(this);
        //    sortedList.Sort(new NavigationComparer(path, (bool[])directions.ToArray(typeof(bool))));

        //    base.Clear();
        //    for (int i = 0; i < sortedList.Count; i++)
        //        base.Add(sortedList[i]);
        //}

        /// <summary>
        /// Copies the elements of the EntitySet to a new Entity array.
        /// </summary>
        /// <returns>An Entity array containing copies of the elements of the EntitySet. </returns>
        public Entity[] ToArray()
        {
            Entity[] entities = new Entity[this.Count];
            this.CopyTo(entities, 0);

            return entities;
        }


        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public EntitySet Clone()
        {
            EntitySet es = new EntitySet();

            foreach (Entity e in this)
                es.Add(e.Clone());

            return es;
        }

        public override string ToString()
        {
            return this.Count.ToString();
        }
    }
}
