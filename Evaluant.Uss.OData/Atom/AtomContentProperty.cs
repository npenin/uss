using System;
using System.Net;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.OData.Atom
{
    [DebuggerDisplay("AtomContentProperty {TypeName} {Name}")]
    internal class AtomContentProperty
    {
        // Methods
        public AtomContentProperty()
        {

        }

        public void EnsureProperties()
        {
            if (this.Properties == null)
            {
                this.Properties = new List<AtomContentProperty>();
            }

        }

        // Properties
        public AtomEntry Entry { get; set; }
        public AtomFeed Feed { get; set; }
        public bool IsNull { get; set; }
        public object MaterializedValue { get; set; }
        public string Name { get; set; }
        public List<AtomContentProperty> Properties { get; set; }
        public string Text { get; set; }
        public string TypeName { get; set; }

        internal void Read(System.Xml.XmlReader reader)
        {
            this.Name = reader.LocalName;
            bool isNull;
            if (bool.TryParse(reader.GetAttribute("null"), out isNull))
                IsNull = isNull;
            if (!IsNull)
            {
                if (reader.HasAttributes)
                {
                    reader.MoveToAttribute(0);
                    if (reader.LocalName == "type")
                        TypeName = reader.Value;
                    reader.MoveToElement();
                }
                while (reader.Read())
                {
                    if (reader.HasValue && !string.IsNullOrEmpty(reader.Value.Trim()))
                    {
                        this.Text = reader.Value;
                        break;
                    }
                }
                switch (TypeName)
                {
                    case "Edm.Binary":
                        MaterializedValue = Convert.FromBase64String(Text);
                        break;
                    case "Edm.Boolean":
                        MaterializedValue = Convert.ToBoolean(Text);
                        break;
                    case "Edm.Byte":
                        MaterializedValue = Convert.ToByte(Text);
                        break;
                    case "Edm.DateTime":
                        MaterializedValue = Convert.ToDateTime(Text);
                        break;
                    case "Edm.Decimal":
                        MaterializedValue = Convert.ToDecimal(Text);
                        break;
                    case "Edm.Double":
                        MaterializedValue = Convert.ToDouble(Text);
                        break;
                    case "Edm.Guid":
                        MaterializedValue = new Guid(Text);
                        break;
                    case "Edm.Int16":
                        MaterializedValue = Convert.ToInt16(Text);
                        break;
                    case "Edm.Int32":
                        MaterializedValue = Convert.ToInt32(Text);
                        break;
                    case "Edm.Int64":
                        MaterializedValue = Convert.ToInt64(Text);
                        break;
                    case "Edm.SByte":
                        MaterializedValue = Convert.ToSByte(Text);
                        break;
                    case "Edm.Single":
                        MaterializedValue = Convert.ToSingle(Text);
                        break;
                    case "Edm.Time":
                        MaterializedValue = TimeSpan.Parse(Text);
                        break;
                    case "Edm.DateTimeOffset":
                        MaterializedValue = DateTimeOffset.Parse(Text);
                        break;
                    case "Edm.String":
                    default:
                        MaterializedValue = Text;
                        break;
                }
            }
        }

        internal void Write(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("d", Name, "http://schemas.microsoft.com/ado/2007/08/dataservices");
            if (!string.IsNullOrEmpty(TypeName))
                writer.WriteAttributeString("m", "type", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", TypeName);
            if (MaterializedValue == null)
                writer.WriteAttributeString("m", "null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata", "true");
            else
            {
                switch (TypeName)
                {
                    case "Edm.Binary":
                        writer.WriteString(Convert.ToBase64String((byte[])MaterializedValue));
                        break;
                    default:
                        writer.WriteString(Convert.ToString(MaterializedValue));
                        break;
                }
            }

            writer.WriteEndElement();
        }

        internal static string GetEdmType(Type type)
        {
            switch (type.Name)
            {
                case "byte[]":
                    return "Edm.Binary";
                case "bool":
                    return "Edm.Boolean";
                case "byte":
                    return "Edm.Byte";
                case "DateTime":
                    return "Edm.DateTime";
                case "decimal":
                    return "Edm.Decimal";
                case "double":
                    return "Edm.Double";
                case "Guid":
                    return "Edm.Guid";
                case "short":
                    return "Edm.Int16";
                case "int":
                    return "Edm.Int32";
                case "long":
                    return "Edm.Int64";
                case "sbyte":
                    return "Edm.SByte";
                case "float":
                    return "Edm.Single";
                case "TimeSpan":
                    return "Edm.Time";
                case "DateTimeOffset":
                    return "Edm.DateTimeOffset";
                case "string":
                default:
                    return null;
            }
        }
    }
}
