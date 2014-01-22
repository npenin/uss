using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Evaluant.Uss.Collections;

namespace Evaluant.Uss.Era
{
    public class Entity<TEntity, TAttribute, TReference>
        where TEntity : Entity<TEntity, TAttribute, TReference>
        where TAttribute : Attribute<TEntity, TAttribute, TReference>
        where TReference : Reference<TEntity, TAttribute, TReference>
    {
        public Entity()
        {
            Attributes = new Dictionary<string, TAttribute>();
            References = new Dictionary<string, TReference>();
        }

        [XmlAttribute("type", DataType = "string")]
        public string Type { get; set; }

        [XmlIgnore]
        public Dictionary<string, TAttribute> Attributes { get; set; }

        [XmlIgnore]
        public Dictionary<string, TReference> References { get; set; }

        [XmlIgnore]
        public Model<TEntity, TAttribute, TReference> Model { get; set; }

        public override string ToString()
        {
            return Type;
        }
    }
}
