using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Evaluant.Uss.Era
{
    public class Reference<TEntity, TAttribute, TReference>
        where TEntity : Entity<TEntity, TAttribute, TReference>
        where TAttribute : Attribute<TEntity, TAttribute, TReference>
        where TReference : Reference<TEntity, TAttribute, TReference>
    {
        [XmlAttribute("name", DataType = "string")]
        public string Name { get; set; }

        [XmlIgnore]
        public TEntity Parent { get; set; }

        private string parentType;

        [XmlAttribute("parent-type", DataType = "string")]
        public string ParentType
        {
            get
            {
                if (Parent == null)
                    return parentType;
                return Parent.Type;
            }
            set
            {
                if (Parent != null)
                    Parent = Parent.Model[value];
                parentType = value;
            }
        }

        [XmlIgnore]
        public TEntity Target
        {
            get;
            set;
        }


        private string childType;

        [XmlAttribute("type", DataType = "string")]
        public string ChildType
        {
            get
            {
                if (Target != null)
                    return Target.Type;
                return childType;
            }
            set
            {
                childType = value;
                if (Target != null)
                    Target = Target.Model[value];
                if (Target == null && Parent != null)
                    Target = this.Parent.Model[value];
            }
        }

        private bool fromMany, toMany;

        [XmlAttribute("fromMany")]
        public bool FromMany
        {
            get { return fromMany; }
            set { fromMany = value; Cardinality = null; }
        }

        [XmlAttribute("toMany")]
        public bool ToMany
        {
            get { return toMany; }
            set { toMany = value; Cardinality = null; }
        }

        private Cardinality cardinality;

        [XmlIgnore]
        public Cardinality Cardinality
        {
            get
            {
                if (cardinality == null)
                {
                    if (ToMany)
                    {
                        if (FromMany)
                            Cardinality = Cardinality.ToMany.Many;
                        else
                            Cardinality = Cardinality.ToMany.One;
                    }
                    else
                    {
                        if (FromMany)
                            Cardinality = Cardinality.ToOne.Many;
                        else
                            Cardinality = Cardinality.ToOne.One;
                    }
                }
                return cardinality;
            }
            set { cardinality = value; }
        }

        public override string ToString()
        {
            return String.Concat(ParentType, ", ", ChildType, ", ", Name);
        }
    }
}
