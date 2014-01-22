using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;
using System.Data;
using System.Collections;

namespace SQLObject
{
	/// <summary>
	/// Description résumée de Parameter.
	/// </summary>
	public class Parameter : ISQLExpression
	{
		private ITagMapping _TagMapping;
		private string _Name;
        private bool _UseParentValue;
		
		public Parameter(ITagMapping tag, string name)
		{
			_Name = name;
			_TagMapping = tag;
            _UseParentValue = false;
		}

		public Parameter Clone()
		{
			return (Parameter) this.MemberwiseClone();
		}

		public string Name
		{
			get { return _Name; }
			set { _Name = value;}
		}

		public ITagMapping TagMapping
		{
			get { return _TagMapping; }
		}

        /// <summary>
        /// True if the instance represents the value of the Parent when it is ambiguous
        /// </summary>
        public bool UseParentValue
        {
            get { return _UseParentValue; }
            set { _UseParentValue = value; }
        }

		public void Accept(ISQLVisitor visitor)
		{
			visitor.Visit(this);
		}

        public override string ToString()
        {
            return Name;
        }
	}

    public class ValuedParameter : Parameter
    {
        public ValuedParameter(string name, object value)
            : this(name, value, DbType.String, ParameterDirection.Input)
        {
        }

        public ValuedParameter(string name, object value, DbType dbType)
            : this(name, value, dbType, ParameterDirection.Input)
        {
        }

        public ValuedParameter(string name, object value, ParameterDirection direction)
            : this(name, value, DbType.String, direction)
        {
        }

        public ValuedParameter(string name, object value, DbType dbType, ParameterDirection direction) : base(null, name)
		{
            this.dbType = dbType;
            this.direction = direction;
            this.value = value;
		}

        protected DbType dbType;

        public DbType DbType
        {
            get { return dbType; }
            set { dbType = value; }
        }


        protected ParameterDirection direction;

        public ParameterDirection Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        protected object value;
        public object Value
        {
            get { return value; }
            set { this.value = value; }
        }
    }

    public class ValuedParameterCollection : CollectionBase
    {
        /// <summary>
        /// Initializes a new empty instance of the ValuedParameterCollection class.
        /// </summary>
        public ValuedParameterCollection()
        {
            // empty
        }

        /// <summary>
        /// Initializes a new instance of the ValuedParameterCollection class, containing elements
        /// copied from an array.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the new ValuedParameterCollection.
        /// </param>
        public ValuedParameterCollection(ValuedParameter[] items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Initializes a new instance of the ValuedParameterCollection class, containing elements
        /// copied from another instance of ValuedParameterCollection
        /// </summary>
        /// <param name="items">
        /// The ValuedParameterCollection whose elements are to be added to the new ValuedParameterCollection.
        /// </param>
        public ValuedParameterCollection(ValuedParameterCollection items)
        {
            this.AddRange(items);
        }

        /// <summary>
        /// Adds the elements of an array to the end of this ValuedParameterCollection.
        /// </summary>
        /// <param name="items">
        /// The array whose elements are to be added to the end of this ValuedParameterCollection.
        /// </param>
        public virtual void AddRange(ValuedParameter[] items)
        {
            foreach (ValuedParameter item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds the elements of another ValuedParameterCollection to the end of this ValuedParameterCollection.
        /// </summary>
        /// <param name="items">
        /// The ValuedParameterCollection whose elements are to be added to the end of this ValuedParameterCollection.
        /// </param>
        public virtual void AddRange(ValuedParameterCollection items)
        {
            foreach (ValuedParameter item in items)
            {
                this.List.Add(item);
            }
        }

        /// <summary>
        /// Adds an instance of type ValuedParameter to the end of this ValuedParameterCollection.
        /// </summary>
        /// <param name="value">
        /// The ValuedParameter to be added to the end of this ValuedParameterCollection.
        /// </param>
        public virtual void Add(ValuedParameter value)
        {
            this.List.Add(value);
        }

        /// <summary>
        /// Determines whether a specfic ValuedParameter value is in this ValuedParameterCollection.
        /// </summary>
        /// <param name="value">
        /// The ValuedParameter value to locate in this ValuedParameterCollection.
        /// </param>
        /// <returns>
        /// true if value is found in this ValuedParameterCollection;
        /// false otherwise.
        /// </returns>
        public virtual bool Contains(ValuedParameter value)
        {
            return this.List.Contains(value);
        }

        /// <summary>
        /// Return the zero-based index of the first occurrence of a specific value
        /// in this ValuedParameterCollection
        /// </summary>
        /// <param name="value">
        /// The ValuedParameter value to locate in the ValuedParameterCollection.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of the _ELEMENT value if found;
        /// -1 otherwise.
        /// </returns>
        public virtual int IndexOf(ValuedParameter value)
        {
            return this.List.IndexOf(value);
        }

        /// <summary>
        /// Inserts an element into the ValuedParameterCollection at the specified index
        /// </summary>
        /// <param name="index">
        /// The index at which the ValuedParameter is to be inserted.
        /// </param>
        /// <param name="value">
        /// The ValuedParameter to insert.
        /// </param>
        public virtual void Insert(int index, ValuedParameter value)
        {
            this.List.Insert(index, value);
        }

        /// <summary>
        /// Gets or sets the ValuedParameter at the given index in this ValuedParameterCollection.
        /// </summary>
        public virtual ValuedParameter this[int index]
        {
            get
            {
                return (ValuedParameter)this.List[index];
            }
            set
            {
                this.List[index] = value;
            }
        }

        public virtual ValuedParameter this[string name]
        {
            get
            {
                foreach (ValuedParameter param in base.List)
                {
                    if (param.Name == name)
                        return param;
                }
                return null;
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific ValuedParameter from this ValuedParameterCollection.
        /// </summary>
        /// <param name="value">
        /// The ValuedParameter value to remove from this ValuedParameterCollection.
        /// </param>
        public virtual void Remove(ValuedParameter value)
        {
            this.List.Remove(value);
        }

        /// <summary>
        /// Type-specific enumeration class, used by ValuedParameterCollection.GetEnumerator.
        /// </summary>
        public class Enumerator : IEnumerator
        {
            private IEnumerator wrapped;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="collection"></param>
            public Enumerator(ValuedParameterCollection collection)
            {
                this.wrapped = ((CollectionBase)collection).GetEnumerator();
            }
            /// <summary>
            /// 
            /// </summary>
            public ValuedParameter Current
            {
                get
                {
                    return (ValuedParameter)(this.wrapped.Current);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return (ValuedParameter)(this.wrapped.Current);
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                return this.wrapped.MoveNext();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Reset()
            {
                this.wrapped.Reset();
            }
        }

        /// <summary>
        /// Returns an enumerator that can iterate through the elements of this ValuedParameterCollection.
        /// </summary>
        /// <returns>
        /// An object that implements System.Collections.IEnumerator.
        /// </returns>        
        public new virtual Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
}
