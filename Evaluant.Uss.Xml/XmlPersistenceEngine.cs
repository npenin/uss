using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.XPath;

using Evaluant.Uss;
using Evaluant.Uss.Commands;
using System.Collections.Specialized;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Domain;
using Evaluant.Uss.Model;
using Evaluant.NLinq;

namespace Evaluant.Uss.Xml
{
    /// <summary>
    /// Description résumée de XmlPersistenceEngine.
    /// </summary>
    public class XmlPersistenceEngine : PersistenceEngineImplementation
    {
        private string _Filename;
        private XmlCommandProcessor _CommandProcessor;

        public XmlPersistenceEngine(XmlProvider factory):base(factory)
        {
            LoadFile(factory.FileName);
        }

        private XmlDocument _Document;
        /// <summary>
        /// Gets or sets the document.
        /// </summary>
        /// <value></value>
        public XmlDocument Document
        {
            get { return _Document; }
            set { _Document = value; }
        }

        public override EntitySet LoadWithId(string type, string[] id, string[] attributes)
        {
            EntitySet result = new EntitySet();

            foreach (string itemId in id)
            {
                string strQuote = itemId.IndexOf("'") > 0 ? "\"" : "'"; // if id contains ' limiter is " else '

                ArrayList subQueries = new ArrayList();

                // Creates a query for each sub-type, and concatenates them using and OR expression
                foreach (string subtype in Factory.Model.GetTreeAsArray(type))
                    subQueries.Add(String.Concat("id(", strQuote, _CommandProcessor.GetKey(subtype, itemId), strQuote, ")"));

                if (subQueries.Count == 0)
                    throw new ModelElementNotFoundException("No such types in metadata: " + type);

                EntitySet es = LoadWithXPath(String.Join(" | ", (string[])subQueries.ToArray(typeof(string))));

                if (es.Count > 0)
                    result.Add(es[0]);
            }

            if (result.Count > 0)
                LoadAttribute(result, attributes);

            return result;
        }

        public override EntitySet Load(Evaluant.NLinq.NLinqQuery query, string[] attributes, string orderby, int first, int max)
        {
            if (first <= 0)
                throw new ArgumentException("first must be greater than 0");

            if (max < 0)
                throw new ArgumentException("max must be none negative");

            XPathTransformer transformer = new XPathTransformer(Factory.Model);
            string xpath = transformer.ConvertToXPath(query);
            EntitySet result = LoadWithXPath(xpath);

            LoadAttribute(result, attributes);

            // sort
            if (orderby != null)
                result.Sort(orderby.Split(','));

            // page
            EntitySet pageResult = new EntitySet();
            Utils.TruncateList(result, pageResult, first, max);

            return pageResult;
        }

        private EntitySet LoadWithXPath(string xPath)
        {
            EntitySet result = new EntitySet();

            foreach (XmlNode n in _Document.SelectNodes(xPath))
            {
                Entity e = CreateEntity(n);
                if (e != null)
                {
                    result.Add(e);
                }
            }

            return result;
        }


        private void LoadAttribute(IEnumerable entities, string[] attributes)
        {
            // if attributes is null, do nothing
            if (attributes == null)
                return;

            // if attributes is empty the filter is empty
            string attributesCriteria = attributes.Length == 0
                ? String.Empty
                : String.Concat("[@Name='", String.Join("' or @Name = '", attributes), "']"); // @Name = 'id1' or @Name = 'id2' ...

            foreach (Entity entity in entities)
            {
                State tempState = entity.State;

                string strQuote = entity.Id.IndexOf("'") > 0 ? "\"" : "'"; // if id contains ' limiter is " else '
                XmlNodeList nodes = _Document.SelectNodes(String.Concat("id(", strQuote, _CommandProcessor.GetKey(entity.Type, entity.Id), strQuote, ")/Attribute", attributesCriteria));

                foreach (XmlNode n in nodes)
                {
                    EntityEntry entry = CreateEntityEntry(entity.Type, n);

                    // Tests in case two attributes are declared with the same name in the XML
                    if (entity.FindEntry(entry.Name) == null)
                    {
                        entity.AddValue(entry.Name, entry.Value, entry.Type, State.UpToDate);
                    }
                }

                entity.State = tempState;
            }
        }


        public override void LoadReference(IEnumerable entities, string[] references)
        {
            // if references is empty the filter is empty
            string referencesCriteria = references.Length == 0
                ? String.Empty
                : String.Concat("[@Role='", String.Join("' or @Role = '", references), "']"); // @Role = 'role1' or @Role = 'role2' ...

            foreach (Entity entity in entities)
            {
                string strQuote = entity.Id.IndexOf("'") > 0 ? "\"" : "'"; // if id contains ' limiter is " else '
                XmlNodeList refIds = _Document.SelectNodes(String.Concat("id(", strQuote, _CommandProcessor.GetKey(entity.Type, entity.Id), strQuote, ")/Reference", referencesCriteria));
                State tempState = entity.State;

                entity.RemoveReference(references);

                foreach (XmlNode refNode in refIds)
                {
                    string id = refNode.Attributes["RefId"].Value;
                    string role = refNode.Attributes["Role"].Value;

                    XmlNode eNode = _Document.SelectSingleNode((String.Format("id('{0}')", id)));
                    Entity related = CreateEntity(eNode);
                    LoadAttribute(new Entity[] { related }, new string[0]);

                    entity.AddValue(role, related, State.UpToDate);
                }

                // mark references as loaded for each entity
                foreach (string refName in references)
                    if (!entity.InferredReferences.Contains(refName))
                        entity.InferredReferences.Add(refName);

                entity.State = tempState;
            }
        }

        public override object LoadScalar(string opath)
        {
            XPathTransformer transformer = new XPathTransformer(false, this.Model);
            string query = string.Format("eval({0})", opath);
            string xpath = transformer.ConvertToScalarXPath(query);

            object result = null;
            result = _Document.CreateNavigator().Evaluate(xpath);

            // Ensure the result in an Int32 in cas of a count()
            if (opath.Trim().StartsWith("count"))
                result = Convert.ToInt32(result);

            return result;
        }

        public override object LoadScalar(NLinqQuery query)
        {
            XPathTransformer transformer = new XPathTransformer(false, this.Model);
            string xpath = transformer.Visit(query);

            object result = null;
            result = _Document.CreateNavigator().Evaluate(xpath);

            // Ensure the result in an Int32 in cas of a count()
            if (query.Expression.Operands[0] is OPath.Expressions.Function)
                result = Convert.ToInt32(result);

            return result;
        }

        protected const string DTD = @"
<!DOCTYPE USS [
	<!ELEMENT USS (Entity*)> 
	<!ELEMENT Entity (Reference*, Attribute*)> 
	<!ELEMENT Reference EMPTY>
	<!ELEMENT Attribute (#PCDATA)>
	<!ATTLIST Entity 
		Type CDATA #REQUIRED
		Id ID #REQUIRED
		>
	<!ATTLIST Reference 
		Role CDATA #REQUIRED
		RefId IDREF #REQUIRED
	>
	<!ATTLIST Attribute 
		Name CDATA #REQUIRED
	>
]>
";

        public override void InitializeRepository()
        {
            base.InitializeRepository();
            if (File.Exists(_Filename))
            {
                File.Delete(_Filename);
            }

            LoadFile();
        }

        public override void Initialize()
        {
            base.Initialize();
            LoadFile();
        }

        protected void LoadFile()
        {
            if (!File.Exists(_Filename))
            {
                File.Delete(_Filename);
                using (StreamWriter sw = File.CreateText(_Filename))
                    sw.Write(DTD + "\n<USS/>");

            }

            _Document = new XmlDocument();
            _Document.Load(_Filename);
            _CommandProcessor = new XmlCommandProcessor(_Document);
        }

        public void LoadFile(string filename)
        {
            _Filename = filename;
            LoadFile();
        }

        public override void BeforeProcessCommands(Transaction tx)
        {
            LoadFile();
            base.BeforeProcessCommands(tx);
        }

        public override void ProcessCommands(Transaction tx)
        {
            foreach (Command c in tx.PendingCommands)
                c.Accept(_CommandProcessor);
        }

        static Hashtable synLocks = new Hashtable();

        public override void AfterProcessCommands(Transaction tx)
        {
            base.AfterProcessCommands(tx);

            object synLock = null;

            lock (synLocks)
            {
                if (!synLocks.Contains(_Filename))
                    synLocks.Add(_Filename, new object());

                synLock = synLocks[_Filename];
            }

            lock (synLock)
            {
                _Document.Save(_Filename);
            }
        }

        private Entity CreateEntity(XmlNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            Entity e = new Entity(node.Attributes.GetNamedItem("Type").Value);

            // Gets the last token as the type can contain ":"
            string[] tokens = node.Attributes.GetNamedItem("Id").Value.Split(':');
            e.Id = tokens[tokens.Length - 1]; // 
            e.State = State.UpToDate;

            return e;
        }

        private EntityEntry CreateEntityEntry(string entityType, XmlNode n)
        {
            if (n == null)
                return null;

            string name = n.SelectSingleNode("@Name").Value;
            string svalue = n.InnerText;

            Type type = Model.GetAttribute(entityType, name, true).Type;

            object value;
            if (type != null && (type.IsValueType || type == typeof(String))) // type can be null if it is not a ValueType
                value = Utils.StringToObject(svalue, type);
            else
            {
                value = Utils.UnSerialize(svalue);
            }

            return new EntityEntry(name, value, type, State.UpToDate, null);
        }

    }
}
