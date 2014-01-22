using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.Domain;
using System.Xml;

namespace Evaluant.Uss.OData.Atom
{
    internal class AtomFeed
    {
        public AtomFeed()
        {
            Entries = new List<AtomEntry>();
            InferredRelationships = new Dictionary<string, AtomFeed>();
        }

        public long? Count { get; set; }
        public IList<AtomEntry> Entries { get; set; }
        public Uri NextLink { get; set; }
        public IDictionary<string, AtomFeed> InferredRelationships { get; set; }
        string typeName = null;

        internal void Read(XmlReader reader)
        {
            AtomFeed inferredRelationship;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Attribute:
                        break;
                    case XmlNodeType.CDATA:
                        break;
                    case XmlNodeType.Comment:
                        break;
                    case XmlNodeType.Document:
                        break;
                    case XmlNodeType.DocumentFragment:
                        break;
                    case XmlNodeType.DocumentType:
                        break;
                    case XmlNodeType.Element:
                        switch (reader.LocalName)
                        {
                            case "title":
                                typeName = reader.ReadElementContentAsString();
                                foreach (var e in Entries)
                                {
                                    e.TypeName = typeName;
                                }
                                break;
                            case "count":
                                this.Count = reader.ReadElementContentAsLong();
                                break;
                            case "link":
                                if (reader.GetAttribute("rel") == "next")
                                    NextLink = new Uri(reader.GetAttribute("href"));
                                break;
                            case "inline":
                                inferredRelationship = new AtomFeed();
                                inferredRelationship.Read(reader);
                                InferredRelationships.Add(inferredRelationship.typeName, inferredRelationship);
                                break;
                            case "entry":
                                AtomEntry entry = new AtomEntry();
                                entry.TypeName = typeName;
                                entry.Read(reader);
                                Entries.Add(entry);
                                break;
                            default:
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement:
                        break;
                    case XmlNodeType.EndEntity:
                        break;
                    case XmlNodeType.Entity:
                        break;
                    case XmlNodeType.EntityReference:
                        break;
                    case XmlNodeType.None:
                        break;
                    case XmlNodeType.Notation:
                        break;
                    case XmlNodeType.ProcessingInstruction:
                        break;
                    case XmlNodeType.SignificantWhitespace:
                        break;
                    case XmlNodeType.Text:
                        break;
                    case XmlNodeType.Whitespace:
                        break;
                    case XmlNodeType.XmlDeclaration:
                        break;
                    default:
                        break;
                }
            }
        }
    }

}
