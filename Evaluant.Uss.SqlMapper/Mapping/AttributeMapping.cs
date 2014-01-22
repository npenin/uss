using System;
using System.Data;
using System.Reflection;
using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de AttributeMapping.
	/// </summary>
	/// 
	public enum SerializableType { NotDefined, BinarySerialization, StringSerialization, String, Standard, Int};

	public class AttributeMapping : ITagMapping
	{
		protected IDbDataParameter _Parameter;
		protected DbType _DbType;
		protected byte _Scale;
		protected byte _Precision;
		protected int _Size;

		protected string _Name;
		protected string _Table;
		protected Type _Type;
		protected string _Field;
		protected string _ParentField;

		protected string _DefaultValue;
		protected bool _IsNotNull = false;

		protected string _Discriminator;
		protected string _DiscriminatorValue;

		EntityMapping _ParentEntity;

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeMapping"/> class.
		/// </summary>
		public AttributeMapping()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeMapping"/> class.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="type">Type.</param>
		/// <param name="parentEntity">Parent entity.</param>
		public AttributeMapping(string name, Type type, EntityMapping parentEntity)
		{
			this._Name = name;
			this._Type = type;
			this._ParentEntity = parentEntity;
		}

		/// <summary>
		/// Gets or sets the parent entity.
		/// </summary>
		/// <value></value>
		[XmlIgnoreAttribute()]
		public EntityMapping ParentEntity
		{
			get { return _ParentEntity; }
			set { _ParentEntity = value; }
		}	

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value></value>
		[XmlAttribute("name", DataType="string")]
		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}
		
		/// <summary>
		/// Gets or sets the table.
		/// </summary>
		/// <value></value>
		[XmlAttribute("table", DataType="string")]
		public string Table
		{
			get
			{
				if (_Table == null)
					return _ParentEntity.Table;
				return _Table;
			}
			set { _Table = value; }
		}

		/// <summary>
		/// Gets or sets the field.
		/// </summary>
		/// <value></value>
		[XmlAttribute("field", DataType="string")]
		public string Field
		{
			get
			{
				if (_Field == null)
                    return _Name;
				return _Field;

			}
			set { _Field = value; }
		}

		public void Initialize()
		{
		}

		/// <summary>
		/// Gets or sets the type of the db.
		/// </summary>
		/// <value></value>
		[XmlAttribute("db-type")]
		public DbType DbType
		{
			get { return _DbType; }
			set { _DbType = value; }
		}

		/// <summary>
		/// Gets or sets the size.
		/// </summary>
		/// <value></value>
		[XmlAttribute("size", DataType="int")]
		public int Size
		{
			get { return _Size; }
			set	{ _Size = value;}
		}

		/// <summary>
		/// Gets or sets the scale.
		/// </summary>
		/// <value></value>
		[XmlAttribute("scale", DataType="unsignedByte")]
		public byte Scale
		{
			get { return _Scale; }
			set	{ _Scale = value; }
		}

		/// <summary>
		/// Gets or sets the precision.
		/// </summary>
		/// <value></value>
		[XmlAttribute("precision", DataType="unsignedByte")]
		public byte Precision
		{
			get { return _Precision; }
			set	{ _Precision = value;}
		}

		/// <summary>
		/// Gets or sets the parent field.
		/// </summary>
		/// <value></value>
		[XmlAttribute("parentField", DataType="string")]
		public string ParentField
		{
			get { return _ParentField; }
			set { _ParentField = value; }
		}

        [XmlIgnore()]
        public string[] ParentFields
        {
            get { return _ParentField.Split(SqlMapperProvider.IDSEP); }
        }

		/// <summary>
		/// Gets or sets the discriminator.
		/// </summary>
		/// <value></value>
		[XmlAttribute("discriminator", DataType="string")]
		public string Discriminator
		{
			get { return _Discriminator; }
			set { _Discriminator = value; }
		}

		/// <summary>
		/// Gets or sets the discriminator_ value.
		/// </summary>
		/// <value></value>
		[XmlAttribute("discriminator-value", DataType="string")]
		public string DiscriminatorValue
		{
			get
			{
				if (_DiscriminatorValue == "*")
					return _Name;
				return _DiscriminatorValue;
			}
			set { _DiscriminatorValue = value; }
		}

		[XmlAttribute("default-value", DataType="string")]
		public string DefaultValue
		{
			get { return _DefaultValue; }
			set { _DefaultValue = value; }
		}

		[XmlAttribute("not-null")]
		public bool IsNotNull
		{
			get { return _IsNotNull; }
			set { _IsNotNull = value; }
		}

		/// <summary>
		/// Creates the db data parameter.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="value">Value.</param>
		/// <returns></returns>
		public IDbDataParameter CreateDbDataParameter(string name, object value)
		{
			IDriver driver = ParentEntity.ParentMapping.Driver;
			IDbDataParameter param;
			
			if (_Precision != 0 || _Scale != 0)
                param = driver.CreateParameter(name, value, _DbType, _Precision, _Scale);
			else
				param = driver.CreateParameter(name, value, _DbType, _Size);
			return param;
		}

		/// <summary>
		/// Creates the db data parameter.
		/// </summary>
		/// <returns></returns>
		public IDbDataParameter CreateDbDataParameter()
		{
			IDriver driver = ParentEntity.ParentMapping.Driver;
			IDbDataParameter param;
			if (_Precision != 0 || _Scale != 0)
                param = driver.CreateParameter(String.Empty, null, _DbType, _Precision, _Scale);
			else
                param = driver.CreateParameter(String.Empty, null, _DbType, _Size);
			return param;
		}

		/// <summary>
		/// Gets or sets the type.
		/// </summary>
		/// <value></value>
		[XmlIgnore()]
		public Type Type
		{
			set { _Type = value; }
			get { return _Type; }
		}

		private string _XmlType;
		//[XmlAttribute("type", DataType="string")]
		public string XmlType
		{
			get { return _XmlType; }
			set { _XmlType = value; }
		}

		/// <summary>
		/// Gets the type of the serialization.
		/// </summary>
		/// <param name="dbType">Type in the database.</param>
		/// <param name="type">Type.</param>
		/// <returns></returns>
		public SerializableType GetSerializableType(DbType dbType, Type type)
		{
			if (type == null)
				return SerializableType.NotDefined;
			
			if ((type.IsValueType || type.IsEnum) && (dbType == DbType.AnsiString || dbType == DbType.String || dbType==DbType.Guid))
				return SerializableType.String;
			
			if ((type == typeof(bool) || type.IsEnum) && (dbType == DbType.Int32))
				return SerializableType.Int;

            if (type != typeof(string) && (dbType == DbType.AnsiString || dbType == DbType.String))
				return SerializableType.StringSerialization;

            if (type != typeof(byte[]) &&  (dbType == DbType.Binary || dbType == DbType.Object) )
				return SerializableType.BinarySerialization;
			
			return SerializableType.Standard;
		}
			
	}
}
