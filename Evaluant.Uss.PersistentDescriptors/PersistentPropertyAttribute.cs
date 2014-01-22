using System;

namespace Evaluant.Uss.PersistentDescriptors
{

    /// <summary>
    /// Used to indicate the AttributeDescriptor to persist the property
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PersistentPropertyAttribute : Attribute
    {
        private string _FieldName;
        private bool _Composition;
		private Type _Type;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentPropertyAttribute"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="composition">if set to <c>true</c> the reletaionship will be marked as a composition.</param>
        public PersistentPropertyAttribute(string fieldName, Type type, bool composition)
        {
            _FieldName = fieldName;
			_Type = type;
            _Composition = composition;
        }

		public PersistentPropertyAttribute(string fieldName, Type type)
			: this(fieldName, type, false)
		{
		}

		public PersistentPropertyAttribute(string fieldName, bool composition)
			: this(fieldName, null, composition)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentPropertyAttribute"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        public PersistentPropertyAttribute(string fieldName)
            : this(fieldName, false)
        {
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="PersistentPropertyAttribute"/> class.
		/// </summary>
		/// <param name="fieldName">Type of the reference (if reference is IList)</param>
		public PersistentPropertyAttribute(Type type)
		{
			_Type = type;
		}
       
        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        /// <value>The name of the field.</value>
        public string FieldName
        {
            get { return _FieldName; }
        }

        /// <summary>
        /// Gets the composition.
        /// </summary>
        /// <value>The name of the field.</value>
        public bool Composition
        {
            get { return _Composition; }
        }

		/// <summary>
		/// Gets the reference type in the case of property is IList
		/// </summary>
		public Type Type
		{
			get { return _Type; }
		}

        private bool _SerializeAsAttribute = false;

        /// <summary>
        /// Gets or sets a value indicating whether a property must be serialized like an attribute and not a seprate entity.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the property must be serialized as an attribute; otherwise, <c>false</c>.
        /// </value>
        public bool SerializeAsAttribute
        {
            get { return _SerializeAsAttribute; }
            set { _SerializeAsAttribute = value; }
        }

    }
}
