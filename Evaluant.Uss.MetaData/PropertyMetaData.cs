using System;
using System.Runtime.Serialization;
//using Evaluant.Uss.Metadata;
//using Evaluant.Uss.Common;

namespace Evaluant.Uss.MetaData
{
    //[DataContract]
    public class PropertyMetaData : IMetaData
    {
        private string _Type;
        private string _PropertyName;
        private Type _PropertyType;
        private string[] _Values = new string[0];
        private bool _Ignore = false;
        private bool _IsId = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMetaData"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        public PropertyMetaData(string type, string propertyName, Type propertyType, bool isId)
        {
            _Type = type;
            _PropertyName = propertyName;
            _PropertyType = propertyType;
            _IsId = isId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyMetaData"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        public PropertyMetaData(Type type, string propertyName, Type propertyType, bool isId)
        {
            _Type = TypeResolver.ConvertNamespaceDomainToEuss(type);
            _PropertyName = propertyName;
            _PropertyType = propertyType;
            _IsId = isId;
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName
        {
            get { return _PropertyName; }
            set
            {
                if (value == String.Empty)
                    throw new ArgumentException("The value cannot be empty", "PropertyName");

                if (value == null)
                    throw new ArgumentNullException("PropertyType");

                _PropertyName = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        public Type PropertyType
        {
            get
            {
                return _PropertyType;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("PropertyType");

                _PropertyType = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        public string[] Values
        {
            get
            {
                return _Values;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("Values");

                _Values = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PropertyMetadata"/> is ignored.
        /// </summary>
        /// <value><c>true</c> if ignored; otherwise, <c>false</c>.</value>
        public bool Ignore
        {
            get { return _Ignore; }
            set { _Ignore = value; }
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        void IMetaData.Accept(IMetaDataVisitor visitor)
        {
            visitor.Process(this);
        }

    }
}
