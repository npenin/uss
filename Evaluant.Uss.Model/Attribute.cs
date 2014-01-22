using System;

using System.Xml;
using System.Xml.Serialization;
using Evaluant.Uss.Era;

//using Evaluant.Uss.Common;

namespace Evaluant.Uss.Model
{
    /// <summary>
    /// Description résumée de Attribute.
    /// </summary>
    /// 
#if !SILVERLIGHT
    [Serializable]
#endif
    public class Attribute : Attribute<Entity, Attribute, Reference>
    {
        private string[] _Values = new string[0];

        public Attribute(string name, Type type)
            : base(name)
        {
            Type = type;
            TypeCode = Type.GetTypeCode(type);
        }

        public Attribute()
            : this(String.Empty, typeof(object))
        {
        }

        [XmlIgnore]
        public Type Type { get; set; }

        [XmlIgnore]
        public TypeCode TypeCode { get; set; }

        [XmlAttribute("type", DataType = "string")]
        public string TypeName
        {
            get { return Type.AssemblyQualifiedName; }
            set { Type = GetType(value); }
        }

        [XmlAttribute("values")]
        public string[] Values
        {
            get { return _Values; }
            set { _Values = value; }
        }
    }
}
