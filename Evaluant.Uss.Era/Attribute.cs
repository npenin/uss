using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Evaluant.Uss.Era
{
    public class Attribute<TEntity, TAttribute, TReference>

        where TEntity : Entity<TEntity, TAttribute, TReference>
        where TAttribute : Attribute<TEntity, TAttribute, TReference>
        where TReference : Reference<TEntity, TAttribute, TReference>
    {
        public Attribute(string name)
        {
            Name = name;
            if (string.Equals(name, "Id", StringComparison.OrdinalIgnoreCase))
                IsId = true;
        }

        protected Attribute()
        {

        }

        [XmlAttribute("name", DataType = "string")]
        public string Name { get; set; }

        [XmlAttribute("isId", DataType = "boolean")]
        [DefaultValue(false)]
        public virtual bool IsId { get; set; }

        [XmlIgnore]
        public TEntity Parent { get; set; }

        public static Type GetType(string typeName)
        {
            Type type = Type.GetType(typeName, true);

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return type.GetGenericArguments()[0];

            return type;
        }
    }
}
