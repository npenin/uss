
using System;
using System.Collections;
using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper
{
    /// <summary>
    /// Description résumée de ReferenceMapping.
    /// </summary>
    public class ReferenceMapping : ITagMapping
    {
        string _Name;
        string _DiscriminatorField;
        string _DiscriminatorValue;

        EntityMapping _EntityParent;
        string _EntityChild;
        string orderby;

        RuleMappingCollection _Rules;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceMapping"/> class.
        /// </summary>
        public ReferenceMapping()
        {
            _Rules = new RuleMappingCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceMapping"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="entityChild">Entity child.</param>
        /// <param name="entityParent">Entity parent.</param>
        public ReferenceMapping(string name, string entityChild, EntityMapping entityParent)
        {
            _Name = name;
            _EntityChild = entityChild;
            _EntityParent = entityParent;
            _Rules = new RuleMappingCollection();
        }

        public void Initialize()
        {
            RuleMappingCollection tmp = _Rules;

            if (_Rules[0].ParentTable == null || _Rules[0].ParentTable == string.Empty)
            {
                _Rules[0].ParentTable = _EntityParent.Table;
            }

            for (int i = 0; i < _Rules.Count; i++)
            {
                _Rules[i].ParentReference = this;

                if (i > 0 && (_Rules[i].ParentTable == null || _Rules[i].ParentTable == string.Empty))
                {
                    _Rules[i].ParentTable = _Rules[i - 1].ChildTable;
                }
            }

            // If no value is defined for the discriminator, use the name of the relationship
            if (_DiscriminatorField != null && _DiscriminatorValue == null)
            {
                _DiscriminatorValue = _Name;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value></value>
        [XmlAttribute("name", DataType = "string")]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        /// <summary>
        /// Gets or sets the entity parent.
        /// </summary>
        /// <value></value>
        [XmlIgnoreAttribute()]
        public EntityMapping EntityParent
        {
            get { return _EntityParent; }
            set { _EntityParent = value; }
        }

        /// <summary>
        /// Gets or sets the entity child.
        /// </summary>
        /// <value></value>
        [XmlAttribute("entityChild", DataType = "string")]
        public string EntityChild
        {
            get { return _EntityChild; }
            set { _EntityChild = value; }
        }

        /// <summary>
        /// Gets or sets the discriminator.
        /// </summary>
        /// <value></value>
        [XmlAttribute("discriminator-field", DataType = "string")]
        public string DiscriminatorField
        {
            get { return _DiscriminatorField; }
            set { _DiscriminatorField = value; }
        }

        /// <summary>
        /// Gets or sets the discriminator.
        /// </summary>
        /// <value></value>
        [XmlAttribute("discriminator-value", DataType = "string")]
        public string DiscriminatorValue
        {
            get { return _DiscriminatorValue; }
            set { _DiscriminatorValue = value; }
        }

        /// <summary>
        /// Gets or sets the rules.
        /// </summary>
        /// <value></value>
        [XmlElementAttribute("Rule")]
        public RuleMappingCollection Rules
        {
            get { return _Rules; }
            set { _Rules = value; }
        }

        [XmlAttribute("orderBy")]
        public string OrderBy
        {
            get { return orderby; }
            set { orderby = value; }
        }
    }
}
