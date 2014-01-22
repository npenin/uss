using System;
using System.Collections;
using System.Data;
using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper
{
    /// <summary>
    /// Description résumée de EntityMapping.
    /// </summary>
    public class EntityMapping : ITagMapping
    {
        public static readonly string PREFIX_ID = "pk";

        protected string _Type;
        protected string _Table;
        protected string _Schema;
        protected string _DiscriminatorValue;
        protected string _DiscriminatorField;

        protected PrimaryKeyMappingCollection _Ids;
        protected AttributeMappingCollection _Attributes;
        protected ReferenceMappingCollection _References;

        protected string _CopyNode = String.Empty;

        protected Mapping _ParentMapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMapping"/> class.
        /// </summary>
        public EntityMapping()
        {
            _Ids = new PrimaryKeyMappingCollection();
            _Attributes = new AttributeMappingCollection();
            _References = new ReferenceMappingCollection();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityMapping"/> class.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="parentMapping">Parent mapping.</param>
        public EntityMapping(string type, Mapping parentMapping)
        {
            _Type = type;
            _Ids = new PrimaryKeyMappingCollection();
            _Attributes = new AttributeMappingCollection();
            _Attributes.TypeName = type;
            _References = new ReferenceMappingCollection();
            _ParentMapping = parentMapping;
        }

        public void Initialize()
        {
            foreach (PrimaryKeyMapping pkm in _Ids)
            {
                // Browse any Business Id to map the field the data must be loaded from
                if (pkm.Generator.Name == GeneratorMapping.GeneratorType.business)
                {
                    foreach (AttributeMapping a in _Attributes)
                    {
                        if (a.Field == GetIdField(pkm))
                        {
                            ArrayList ps = new ArrayList();
                            ps.Add(new ParamMapping("name", a.Name));
                            ps.Add(new ParamMapping("dbType", a.DbType.ToString()));
                            ps.Add(new ParamMapping("size", a.Size.ToString()));
                            pkm.Generator.Params = (ParamMapping[])ps.ToArray(typeof(ParamMapping));
                        }
                    }
                }
                pkm.ParentEntity = this;
                pkm.Initialize();
            }
            if (!string.IsNullOrEmpty(_DiscriminatorField) && Ids[0].Generator.Name == GeneratorMapping.GeneratorType.business)
            {
                PrimaryKeyMapping discriminatorPkm = new PrimaryKeyMapping(_DiscriminatorField, this);
                _Ids.Add(discriminatorPkm);
                discriminatorPkm.Generator = new GeneratorMapping();
                discriminatorPkm.Generator.Name = GeneratorMapping.GeneratorType.business;
                ArrayList ps = new ArrayList();
                ps.Add(new ParamMapping("dbType", DbType.AnsiString.ToString()));
                ps.Add(new ParamMapping("size", "255"));
                discriminatorPkm.Generator.Params = (ParamMapping[])ps.ToArray(typeof(ParamMapping));
                discriminatorPkm.Initialize();
            }

            foreach (AttributeMapping a in _Attributes)
            {
                a.ParentEntity = this;
                a.Initialize();
            }

            foreach (ReferenceMapping r in _References)
            {
                r.EntityParent = this;
                r.Initialize();
            }
        }

        /// <summary>
        /// Gets or sets the parent mapping.
        /// </summary>
        /// <value>The parent mapping.</value>
        [XmlIgnore()]
        public Mapping ParentMapping
        {
            get { return _ParentMapping; }
            set { _ParentMapping = value; }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value></value>
        [XmlAttribute("type", DataType = "string")]
        public string Type
        {
            get { return _Type; }
            set 
            { 
                _Type = value;
                _Attributes.TypeName = _Type;
            }
        }

        /// <summary>
        /// Gets or sets the copy node.
        /// </summary>
        /// <value>The copy node.</value>
        [XmlAttribute("copyNode", DataType = "string")]
        public string CopyNode
        {
            get { return _CopyNode; }
            set { _CopyNode = value; }
        }

        /// <summary>
        /// Gets or sets the table.
        /// </summary>
        /// <value>The table.</value>
        [XmlAttribute("table", DataType = "string")]
        public string Table
        {
            get { return _Table; }
            set { _Table = value; }
        }

        /// <summary>
        /// Gets or sets the schema of the table.
        /// </summary>
        /// <value>The table.</value>
        [XmlAttribute("schema", DataType = "string")]
        public string Schema
        {
            get { return _Schema; }
            set { _Schema = value; }
        }


        /// <summary>
        /// Gets or sets the discriminator field.
        /// </summary>
        /// <value>The discriminator field.</value>
        [XmlAttribute("discriminator-field", DataType = "string")]
        public string DiscriminatorField
        {
            get { return _DiscriminatorField; }
            set { _DiscriminatorField = value; }
        }

        /// <summary>
        /// Gets or sets the discriminator_ value.
        /// </summary>
        /// <value>The discriminator value.</value>
        [XmlAttribute("discriminator-value", DataType = "string")]
        public string DiscriminatorValue
        {
            get { return _DiscriminatorValue; }
            set { _DiscriminatorValue = value; }
        }

        /// <summary>
        /// Gets or sets the ids.
        /// </summary>
        /// <value>The ids.</value>
        [XmlElementAttribute("Id")]
        public PrimaryKeyMappingCollection Ids
        {
            get { return _Ids; }
            set { _Ids = value; }
        }

        /// <summary>
        /// Gets the id fields.
        /// </summary>
        /// <value>The id fields.</value>
        [XmlIgnore()]
        public string IdFields
        {
            get
            {
                string[] result = new string[Ids.Count];
                for (int i = 0; i < Ids.Count; i++)
                    result[i] = Ids[i].Field;

                return string.Join(SqlMapperProvider.IDSEP.ToString(), result);

            }
        }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>The attributes.</value>
        [XmlElementAttribute("Attribute")]
        public AttributeMappingCollection Attributes
        {
            get { return _Attributes; }
            set { _Attributes = value; }
        }

        /// <summary>
        /// Gets or sets the references.
        /// </summary>
        /// <value>The references.</value>
        [XmlElementAttribute("Reference")]
        public ReferenceMappingCollection References
        {
            get { return _References; }
            set { _References = value; }
        }

        public string GetIdField(PrimaryKeyMapping pk)
        {
            return pk.Field;
        }

        public string GetIdFieldAs(PrimaryKeyMapping pk)
        {
            return string.Concat(PREFIX_ID, pk.Field);
        }

        public ITagMapping FindTagMappingByFieldName(string name)
        {
            foreach (PrimaryKeyMapping pkm in Ids)
            {
                if (pkm.Field == name)
                    return pkm;
            }
            AttributeMapping am = Attributes.FindByField(name);
            if (am != null)
                return am;
            foreach (ReferenceMapping rm in References)
            {
                foreach (RuleMapping rrm in rm.Rules)
                {
                    foreach (string parentField in rrm.ParentFields)
                    {
                        if (parentField == name)
                            return rrm;
                    }
                    foreach (string childField in rrm.ChildFields)
                    {
                        if (childField == name)
                            return rrm;
                    }
                }
            }
            throw new MappingNotFoundException(string.Format("No field with name {0} could be found",name));
        }
    }
}
