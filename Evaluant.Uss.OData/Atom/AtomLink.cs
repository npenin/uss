using System;
using System.Net;
using Evaluant.Uss.OData.UriExpressions;

namespace Evaluant.Uss.OData.Atom
{
    internal class AtomLink
    {
        public AtomLink(string type, string id)
        {
            Value = new RootContainerExpression() { Name = type, Id = id };
        }

        public AtomLink(string contentId)
        {
            Value = new RootContainerExpression() { Name = contentId };
        }
        public PropertyExpression Value { get; set; }

        internal void Write(System.Xml.XmlWriter writer, ODataPersistenceEngineAsync engine)
        {
            writer.WriteStartElement("url", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            if (Value.Name[0] == '$')
                writer.WriteString(Value.Name);
            else
                writer.WriteString(engine.GetUri(Value).ToString());
            writer.WriteEndElement();
        }
    }
}
