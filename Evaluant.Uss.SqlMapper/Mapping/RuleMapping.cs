using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de RuleMapping.
	/// </summary>
	public class RuleMapping : ITagMapping
	{
        string _ParentSchema;
		string _ParentTable;
		string _ParentField;
        string _ParentDefaultValue = string.Empty;

        string _ChildSchema;
        string _ChildTable;
		string _ChildField;
        string _ChildDefaultValue = string.Empty;

        string _Constraint;

		bool _IsParentId;



		ReferenceMapping _ParentReference;

		/// <summary>
		/// Initializes a new instance of the <see cref="RuleMapping"/> class.
		/// </summary>
		public RuleMapping()
		{}

		/// <summary>
		/// Initializes a new instance of the <see cref="RuleMapping"/> class.
		/// </summary>
		/// <param name="fieldParent">Field parent.</param>
		/// <param name="fieldChild">Field child.</param>
		/// <param name="parentReference">Parent reference.</param>
		public RuleMapping(string fieldParent, string fieldChild, ReferenceMapping parentReference)
		{
			_ParentField = fieldParent;
			_ChildField = fieldChild;
			_ParentReference = parentReference;
		}

		[XmlIgnore()]
		public bool IsParentId
		{
			get { return _IsParentId; }
			set { _IsParentId = value; }
		}

        /// <summary>
        /// Gets or sets the schema of the parent table.
        /// </summary>
        /// <value></value>
        [XmlAttribute("parentSchema", DataType = "string")]
        public string ParentSchema
        {
            get { return _ParentSchema; }
            set { _ParentSchema = value; }
        }
		
		/// <summary>
		/// Gets or sets the parent table.
		/// </summary>
		/// <value></value>
        [XmlAttribute("parentTable", DataType = "string")]
		public string ParentTable
		{
			get { return _ParentTable; }
			set { _ParentTable = value; }
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

        [XmlAttribute("parentDefaultValue", DataType = "string")]
        public string ParentDefaultValue
        {
            get { return _ParentDefaultValue; }
            set { _ParentDefaultValue = value; }
        }

		private static string PREFIX_ID = "pk";

		public string GetParentFieldAs()
		{
			return string.Concat(PREFIX_ID, _ParentField);
		}

		/// <summary>
		/// Gets or sets the parent reference.
		/// </summary>
		/// <value></value>
		[XmlIgnoreAttribute()]
		public ReferenceMapping ParentReference
		{
			get { return _ParentReference; }
			set { _ParentReference = value; }
		}

        /// <summary>
        /// Gets or sets the schema of the child table.
        /// </summary>
        /// <value></value>
        [XmlAttribute("childSchema", DataType = "string")]
        public string ChildSchema
        {
            get { return _ChildSchema; }
            set { _ChildSchema = value; }
        }

		/// <summary>
		/// Gets or sets the child table.
		/// </summary>
		/// <value></value>
		[XmlAttribute("childTable", DataType="string")]
		public string ChildTable
		{
			get { return _ChildTable; }
			set { _ChildTable = value; }
		}

		/// <summary>
		/// Gets or sets the child field.
		/// </summary>
		/// <value></value>
		[XmlAttribute("childField", DataType="string")]
		public string ChildField
		{
			get { return _ChildField; }
			set { _ChildField = value; }
		}

        [XmlAttribute("childDefaultValue", DataType = "string")]
        public string ChildDefaultValue
        {
            get { return _ChildDefaultValue; }
            set { _ChildDefaultValue = value; }
        }

        /// <summary>
        /// Gets or sets the constraint.
        /// </summary>
        /// <value>The name of the constraint.</value>
        [XmlAttribute("constraint")]
        public string Constraint
        {
            get { return _Constraint; }
            set { _Constraint = value; }
        }

        [XmlIgnore()]
        public string[] ParentFields
        {
            get { return ParentField.Split(SqlMapperProvider.IDSEP); }
        }

        [XmlIgnore()]
        public string[] ChildFields
        {
            get { return ChildField.Split(SqlMapperProvider.IDSEP); }
        }

        [XmlIgnore()]
        public string[] ParentDefaultValues
        {
            get { return ParentDefaultValue.Split(SqlMapperProvider.IDSEP); }
        }

        [XmlIgnore()]
        public string[] ChildDefaultValues
        {
            get { return ChildDefaultValue.Split(SqlMapperProvider.IDSEP); }
        }

	}
}
