using System;
using System.Xml;
using Evaluant.Uss.Commands;
using Evaluant.Uss.PersistenceEngine.Contracts.CommonVisitors;

namespace Evaluant.Uss.Xml
{
    /// <summary>
    /// Visitor pattern to execute concrete commands
    /// </summary>
    class XmlCommandProcessor : BaseCommandProcessor
    {

        private string _NewId;
        public string NewId
        {
            get { return _NewId; }
            set { _NewId = value; }
        }

        private XmlDocument _Document;
        private XmlPersistenceEngine engine;

        public XmlCommandProcessor(XmlDocument document, XmlPersistenceEngine engine)
        {
            _Document = document;
            this.engine = engine;
        }

        public void Visit(CreateEntityCommand c)
        {
            XmlElement entityNode;
            _Document.DocumentElement.AppendChild(entityNode = _Document.CreateElement("Entity"));

            XmlAttribute entityType = _Document.CreateAttribute("Type");
            entityType.Value = c.Type;
            entityNode.Attributes.Append(entityType);

            XmlAttribute entityId = _Document.CreateAttribute("Id");
            entityId.Value = GetKey(c.Type, c.ParentId);
            entityNode.Attributes.Append(entityId);
        }

        public void Visit(DeleteEntityCommand c)
        {
            XmlNode entity = _Document.GetElementById(GetKey(c.Type, c.ParentId));

            if (entity == null)
                throw new UniversalStorageException("The entity to delete was not found");

            _Document.DocumentElement.RemoveChild(entity);

            XmlNodeList referencers = _Document.SelectNodes(String.Format("//Reference[@RefId='{0}']", GetKey(c.Type, c.ParentId)));
            foreach (XmlNode n in referencers)
                n.ParentNode.RemoveChild(n);
        }

        public CreateAttributeCommand Visit(CreateAttributeCommand c)
        {
            XmlNode entityAttribute;

            XmlNode parentEntity = _Document.GetElementById(GetKey(c.ParentType, c.ParentId));

            if (parentEntity == null)
            {
                throw new Exception("Unknown Entity");
            }

            parentEntity.AppendChild(entityAttribute = _Document.CreateElement("Attribute"));

            XmlAttribute entityName = entityAttribute.Attributes.Append(_Document.CreateAttribute("Name"));
            entityName.Value = c.Name;

            entityAttribute.InnerText = engine.Factory.Serializer.SerializeToString(c.Value);

            return c;
        }

        public UpdateAttributeCommand Visit(UpdateAttributeCommand c)
        {
            XmlNode entityAttribute = FindAttribute(GetKey(c.ParentType, c.ParentId), c.Name);

            if (entityAttribute == null)
            {
                Visit(new CreateAttributeCommand(c.ParentId, c.ParentType, c.Name, c.Type, c.Value));
                return c;
            }

            // Treat null values as a DeleteAttributeCommand in this engine
            if (c.Value == null)
            {
                DeleteAttributeCommand dac = new DeleteAttributeCommand(c.ParentId, c.ParentType, c.Name, c.Type, c.Value);
                Visit(dac);
                return c;
            }

            entityAttribute.InnerText = engine.Factory.Serializer.SerializeToString(c.Value);

            return c;
        }

        public DeleteAttributeCommand Visit(DeleteAttributeCommand c)
        {
            XmlNode entityAttribute = FindAttribute(GetKey(c.ParentType, c.ParentId), c.Name);

            if (entityAttribute == null)
                throw new Exception("Unknown Attribute");

            entityAttribute.ParentNode.RemoveChild(entityAttribute);

            return c;
        }

        public CreateReferenceCommand Visit(CreateReferenceCommand c)
        {
            XmlNode parentEntity = _Document.GetElementById(GetKey(c.ParentType, c.ParentId));

            if (parentEntity == null)
                throw new Exception("Not found parent Entity while creating Reference");

            XmlElement reference;
            parentEntity.AppendChild(reference = _Document.CreateElement("Reference"));

            XmlAttribute name = reference.Attributes.Append(_Document.CreateAttribute("Role"));
            name.Value = c.Role;

            XmlAttribute child = reference.Attributes.Append(_Document.CreateAttribute("RefId"));
            child.Value = GetKey(c.ChildType, c.ChildId);

            return c;
        }

        public DeleteReferenceCommand Visit(DeleteReferenceCommand c)
        {
            XmlElement reference = FindReference(GetKey(c.ParentType, c.ParentId), GetKey(c.ChildType, c.ChildId));

            if (reference == null)
                throw new Exception("Unknow Reference");

            reference.ParentNode.RemoveChild(reference);

            return c;
        }

        internal string GetKey(string type, string id)
        {
            return String.Concat(type, ".", id);
        }

        #region Lookup methods

        /// <summary>
        /// Finds the attribute.
        /// </summary>
        /// <param name="parentId">Parent id.</param>
        /// <param name="name">Name.</param>
        /// <returns></returns>
        private XmlElement FindAttribute(string parentId, string name)
        {
            XmlNode attribute = _Document.SelectSingleNode(String.Format("//Entity[@Id='{0}']/Attribute[@Name='{1}']", parentId, name));
            return attribute as XmlElement;
        }

        /// <summary>
        /// Finds the reference.
        /// </summary>
        /// <param name="parentId">Parent id.</param>
        /// <param name="childId">Child id.</param>
        /// <returns></returns>
        private XmlElement FindReference(string parentId, string childId)
        {
            XmlNode attribute = _Document.SelectSingleNode(String.Format("//Entity[@Id='{0}']/Reference[@RefId='{1}']", parentId, childId));
            return attribute as XmlElement;
        }

        #endregion

    }
}
