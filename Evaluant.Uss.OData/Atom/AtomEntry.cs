using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Evaluant.Uss.OData.Atom
{
    [DebuggerDisplay("AtomEntry {ResolvedObject} @ {Identity}")]
    internal class AtomEntry
    {
        public AtomEntry()
        {
            DataValues = new List<AtomContentProperty>();
        }

        private EntryFlags flags;

        // Methods
        private bool GetFlagValue(EntryFlags mask)
        {
            return ((this.flags & mask) != 0);
        }

        private void SetFlagValue(EntryFlags mask, bool value)
        {
            if (value)
            {
                this.flags |= mask;
            }
            else
            {
                this.flags &= ~mask;
            }
        }

        // Properties
        public Model.Entity ActualType
        {
            get;
            set;
        }

        public bool CreatedByMaterializer
        {
            get
            {
                return this.GetFlagValue(EntryFlags.CreatedByMaterializer);
            }
            set
            {
                this.SetFlagValue(EntryFlags.CreatedByMaterializer, value);
            }
        }

        public List<AtomContentProperty> DataValues
        {
            get;
            set;
        }

        public Uri EditLink
        {
            get;
            set;
        }

        public bool EntityHasBeenResolved
        {
            get
            {
                return this.GetFlagValue(EntryFlags.EntityHasBeenResolved);
            }
            set
            {
                this.SetFlagValue(EntryFlags.EntityHasBeenResolved, value);
            }
        }

        public bool EntityPropertyMappingsApplied
        {
            get
            {
                return this.GetFlagValue(EntryFlags.EntityPropertyMappingsApplied);
            }
            set
            {
                this.SetFlagValue(EntryFlags.EntityPropertyMappingsApplied, value);
            }
        }

        public string ETagText
        {
            get;
            set;
        }

        public string Identity
        {
            get;
            set;
        }

        public bool IsNull
        {
            get
            {
                return this.GetFlagValue(EntryFlags.IsNull);
            }
            set
            {
                this.SetFlagValue(EntryFlags.IsNull, value);
            }
        }

        public Uri MediaContentUri
        {
            get;
            set;
        }

        public Uri MediaEditUri
        {
            get;
            set;
        }

        public bool? MediaLinkEntry
        {
            get
            {
                if (!this.GetFlagValue(EntryFlags.MediaLinkEntryAssigned))
                {
                    return null;
                }
                return new bool?(this.GetFlagValue(EntryFlags.MediaLinkEntryValue));
            }
            set
            {
                this.SetFlagValue(EntryFlags.MediaLinkEntryAssigned, true);
                this.SetFlagValue(EntryFlags.MediaLinkEntryValue, value.Value);
            }
        }

        public Uri QueryLink
        {
            get;
            set;
        }

        public object ResolvedObject
        {
            get;
            set;
        }

        public bool ShouldUpdateFromPayload
        {
            get
            {
                return this.GetFlagValue(EntryFlags.ShouldUpdateFromPayload);
            }
            set
            {
                this.SetFlagValue(EntryFlags.ShouldUpdateFromPayload, value);
            }
        }

        public string StreamETagText
        {
            get;
            set;
        }

        public object Tag
        {
            get;
            set;
        }

        public string TypeName
        {
            get;
            set;
        }

        [Flags]
        private enum EntryFlags
        {
            CreatedByMaterializer = 2,
            EntityHasBeenResolved = 4,
            EntityPropertyMappingsApplied = 32,
            IsNull = 64,
            MediaLinkEntryAssigned = 16,
            MediaLinkEntryValue = 8,
            ShouldUpdateFromPayload = 1
        }

        internal void Read(System.Xml.XmlReader reader)
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case System.Xml.XmlNodeType.Attribute:
                        break;
                    case System.Xml.XmlNodeType.CDATA:
                        break;
                    case System.Xml.XmlNodeType.Comment:
                        break;
                    case System.Xml.XmlNodeType.Document:
                        break;
                    case System.Xml.XmlNodeType.DocumentFragment:
                        break;
                    case System.Xml.XmlNodeType.DocumentType:
                        break;
                    case System.Xml.XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "category":
                                TypeName = reader.GetAttribute("term");
                                break;
                            case "id":
                                this.Identity = reader.ReadElementContentAsString();
                                break;
                            case "link":
                                switch (reader.GetAttribute("rel"))
                                {
                                    case "edit":
                                        EditLink = new Uri(reader.GetAttribute("href"), UriKind.Relative);
                                        break;
                                }
                                break;
                            default:
                                break;
                            case "content":
                                while (reader.Read())
                                {
                                    switch (reader.NodeType)
                                    {
                                        case System.Xml.XmlNodeType.Element:
                                            if (reader.LocalName == "properties")
                                                break;
                                            var property = new AtomContentProperty();
                                            property.Read(reader);
                                            DataValues.Add(property);

                                            break;
                                        case System.Xml.XmlNodeType.EndElement:
                                            break;
                                    }
                                }
                                break;
                        }
                        break;
                    case System.Xml.XmlNodeType.EndElement:
                        if (reader.LocalName == "entry")
                            return;
                        break;
                    case System.Xml.XmlNodeType.EndEntity:
                        break;
                    case System.Xml.XmlNodeType.Entity:
                        break;
                    case System.Xml.XmlNodeType.EntityReference:
                        break;
                    case System.Xml.XmlNodeType.None:
                        break;
                    case System.Xml.XmlNodeType.Notation:
                        break;
                    case System.Xml.XmlNodeType.ProcessingInstruction:
                        break;
                    case System.Xml.XmlNodeType.SignificantWhitespace:
                        break;
                    case System.Xml.XmlNodeType.Text:
                        break;
                    case System.Xml.XmlNodeType.Whitespace:
                        break;
                    case System.Xml.XmlNodeType.XmlDeclaration:
                        break;
                    default:
                        break;
                }
            }
        }

        public string Category { get; set; }

        internal void Write(System.Xml.XmlWriter writer, ODataPersistenceEngineAsync engine)
        {
            writer.WriteStartElement("entry", "http://www.w3.org/2005/Atom");
            writer.WriteAttributeString("xmlns", "m", "http://www.w3.org/2000/xmlns/", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            writer.WriteAttributeString("xmlns", "http://www.w3.org/2005/Atom");
            writer.WriteAttributeString("xmlns", "d", "http://www.w3.org/2000/xmlns/", "http://schemas.microsoft.com/ado/2007/08/dataservices");

            //writer.WriteElementString("id", "http://www.w3.org/2005/Atom", Identity);

            writer.WriteStartElement("category", "http://www.w3.org/2005/Atom");
            writer.WriteAttributeString("scheme", "http://schemas.microsoft.com/ado/2007/08/dataservices/scheme");
            writer.WriteAttributeString("term", Category);
            writer.WriteEndElement();

            writer.WriteStartElement("title", "http://www.w3.org/2005/Atom");
            //writer.WriteAttributeString("type", "text");
            writer.WriteEndElement();

            writer.WriteStartElement("updated", "http://www.w3.org/2005/Atom");
            writer.WriteString(DateTime.UtcNow.ToString("o"));
            writer.WriteEndElement();

            writer.WriteStartElement("author", "http://www.w3.org/2005/Atom");
            writer.WriteStartElement("name", "http://www.w3.org/2005/Atom");
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteStartElement("id", "http://www.w3.org/2005/Atom");
            writer.WriteEndElement();


            writer.WriteStartElement("content", "http://www.w3.org/2005/Atom");
            writer.WriteAttributeString("type", "application/xml");
            writer.WriteStartElement("properties", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");

            foreach (var item in DataValues)
            {
                item.Write(writer);
            }

            //End of properties
            writer.WriteEndElement();
            //End of content
            writer.WriteEndElement();

            //End of entry
            writer.WriteEndElement();
        }

        internal void Complete(Model.Model model)
        {
            foreach (var attribute in model.GetInheritedAttributes(Category))
            {
                if (!Contains(attribute.Name))
                    DataValues.Add(new AtomContentProperty() { Name = attribute.Name, IsNull = true });
            }
        }

        private bool Contains(string p)
        {
            foreach (var item in DataValues)
            {
                if (item.Name == p)
                    return true;
            }
            return false;
        }
    }
}
