using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de PrimaryKeyMapping.
	/// </summary>
	public class PrimaryKeyMapping : ITagMapping
	{
		string _Field;
		GeneratorMapping _Generator;

        EntityMapping _ParentEntity;

		/// <summary>
		/// Initializes a new instance of the <see cref="PrimaryKeyMapping"/> class.
		/// </summary>
		public PrimaryKeyMapping()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PrimaryKeyMapping"/> class.
		/// </summary>
		/// <param name="field">Field.</param>
		public PrimaryKeyMapping(string field, EntityMapping parentEntity)
		{
			_Field = field;
            _ParentEntity = parentEntity;
		}

        public void Initialize()
        {
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
		/// Gets or sets the field.
		/// </summary>
		/// <value></value>
		[XmlAttribute("field", DataType="string")]
		public string Field
		{
			get { return _Field; }
			set { _Field = value; }
		}

		/// <summary>
		/// Gets or sets the generator.
		/// </summary>
		/// <value></value>
		[XmlElementAttribute()]
		public GeneratorMapping Generator
		{
			get { return _Generator; }
			set { _Generator = value; }
		}
	}
}
