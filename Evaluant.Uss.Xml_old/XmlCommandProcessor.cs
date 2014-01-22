using System;
using System.Xml;
using Evaluant.Uss.Commands;
using Evaluant.Uss.Common;

namespace Evaluant.Uss.Xml
{
	/// <summary>
	/// Visitor pattern to execute concrete commands
	/// </summary>
	class XmlCommandProcessor : ICommandProcessor
	{

		private string _NewId;
		public string NewId
		{
			get { return _NewId; }
			set { _NewId = value; }
		}

		private XmlDocument _Document;

		public XmlCommandProcessor(XmlDocument document)
		{
			_Document = document;
		}

		public void Process(Command c)
		{
		}

		public void Process(CreateEntityCommand c)
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

		public void Process(CompoundCreateCommand c)
		{
			Process((CreateEntityCommand)c);
			foreach(CreateAttributeCommand cc in c.InnerCommands)
				Process(cc);
		}

		public void Process(CompoundUpdateCommand c)
		{
			foreach(Command cc in c.InnerCommands)
				cc.Accept(this);
		}

		public void Process(DeleteEntityCommand c)
		{
			XmlNode entity = _Document.GetElementById(GetKey(c.Type, c.ParentId));

            if (entity == null)
                throw new UniversalStorageException("The entity to delete was not found");

			_Document.DocumentElement.RemoveChild(entity);

			XmlNodeList referencers = _Document.SelectNodes(String.Format("//Reference[@RefId='{0}']", GetKey(c.Type, c.ParentId)));
			foreach(XmlNode n in referencers)
				n.ParentNode.RemoveChild(n);
		}

		public void Process(CreateAttributeCommand c)
		{
			XmlNode entityAttribute;

			XmlNode parentEntity = _Document.GetElementById( GetKey(c.ParentType, c.ParentId));
			
			if(parentEntity == null)
			{
				throw new Exception("Unknown Entity");
			}

			parentEntity.AppendChild(entityAttribute = _Document.CreateElement("Attribute"));
			
			XmlAttribute entityName = entityAttribute.Attributes.Append(_Document.CreateAttribute("Name"));
			entityName.Value = c.Name;

			if(Utils.IsStandardType(c.Type))
                entityAttribute.InnerText = Utils.ConvertToString(c.Value, c.Type);
			else
				entityAttribute.InnerText = Utils.SerializeToString(c.Value);
		}

		public void Process(UpdateAttributeCommand c)
		{
			XmlNode entityAttribute = FindAttribute(GetKey(c.ParentType, c.ParentId), c.Name);

			if(entityAttribute == null)
			{
				Process(new CreateAttributeCommand(c.ParentId, c.ParentType, c.Name, c.Type, c.Value));
				return;
			}

			// Treat null values as a DeleteAttributeCommand in this engine
			if(c.Value == null)
			{
				DeleteAttributeCommand dac = new DeleteAttributeCommand(c.ParentId, c.ParentType, c.Name, c.Type, c.Value);
				Process(dac);
				return;
			}

            if (Utils.IsStandardType(c.Type)) // string is not a ValueType
                entityAttribute.InnerText = Utils.ConvertToString(c.Value, c.Type);
			else
				entityAttribute.InnerText = Utils.SerializeToString(c.Value);
		}

		public void Process(DeleteAttributeCommand c)
		{
			XmlNode entityAttribute = FindAttribute(GetKey(c.ParentType, c.ParentId), c.Name);

			if(entityAttribute == null)
				throw new Exception("Unknown Attribute");

			entityAttribute.ParentNode.RemoveChild(entityAttribute);
		}

		public void Process(CreateReferenceCommand c)
		{
			XmlNode parentEntity = _Document.GetElementById(GetKey(c.ParentType, c.ParentId));
			
			if(parentEntity == null)
				throw new Exception("Not found parent Entity while creating Reference");

			XmlElement reference;
			parentEntity.AppendChild(reference = _Document.CreateElement("Reference"));

			XmlAttribute name = reference.Attributes.Append(_Document.CreateAttribute("Role"));
			name.Value = c.Role;

			XmlAttribute child = reference.Attributes.Append(_Document.CreateAttribute("RefId"));
			child.Value = GetKey(c.ChildType, c.ChildId);

		}

		public void Process(DeleteReferenceCommand c)
		{
			XmlElement reference = FindReference(GetKey(c.ParentType, c.ParentId), GetKey(c.ChildType, c.ChildId));

			if(reference == null)
				throw new Exception("Unknow Reference");

			reference.ParentNode.RemoveChild(reference);
		}

		internal string GetKey(string type, string id)
		{
			return String.Concat(type, ":", id);
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
