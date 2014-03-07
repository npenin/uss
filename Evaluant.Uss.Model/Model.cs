using System;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using Evaluant.Uss.MetaData;
//using Evaluant.Uss.Common;
using System.Collections.Generic;
using Evaluant.Uss.Collections;
#if !SILVERLIGHT
using System.Collections.Specialized;
#endif

namespace Evaluant.Uss.Model
{
    /// <summary>
    /// Description résumée de Model.
    /// </summary>
    /// <remarks>This class is Thread Safe. An instance can be used by several Persistence Engines concurently</remarks>
#if !SILVERLIGHT
    [Serializable]
    [XmlRoot("Model")]
#endif
    public class Model : Era.Model<Entity, Attribute, Reference>, IEnumerable<Entity>
    {
        protected override void OnEntityNotFound(string key)
        {
            throw new ModelElementNotFoundException(String.Format("Type [{0}] not found", key));
        }
#if !SILVERLIGHT
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces xmlns;
#endif
        #region Members

        private Dictionary<string, Attribute> _AttributesIndex;
        private Dictionary<string, Reference> _ReferencesIndex;

        private IDictionary<string, string> _FullNameAttributeIndex;
        private Dictionary<string, IEnumerable<string>> _FullNameAttributeBranchIndex;

        #endregion

        #region Ctor

        public Model()
        {
            _AttributesIndex = new Dictionary<string, Attribute>();
            _ReferencesIndex = new Dictionary<string, Reference>();

            _FullNameAttributeIndex = new Dictionary<string, string>();
            _FullNameAttributeBranchIndex = new Dictionary<string, IEnumerable<string>>();

#if !SILVERLIGHT
            xmlns = new XmlSerializerNamespaces(new XmlQualifiedName[] { new XmlQualifiedName(String.Empty, "http://euss.evaluant.com/schemas/MetaDataModel.xsd") });
#endif
        }

        #endregion

        //#region Entities
        
        //public Entity GetEntity(string name)
        //{
        //    lock (_EntitiesIndex)
        //    {
        //        Entity result;
        //        if (_EntitiesIndex.TryGetValue(name, out result))
        //            return result;

        //        //foreach (Entity e in Entities.Values)
        //        //    if (e.Type == name)
        //        //    {
        //        //        _EntitiesIndex.Add(name, e);
        //        //        return e;
        //        //    }
        //    }

        //    return null;
        //}

        //public Entity GetEntity(string name, bool strict)
        //{
        //    Entity entity = GetEntity(name);

        //    if (entity == null && strict)
        //        throw new ModelElementNotFoundException(String.Format("Type [{0}] not found", name));

        //    return entity;
        //}

        //#endregion

        #region Inheritance

        public Entity GetParent(Entity entity)
        {
            if (Entities.ContainsKey(entity.Inherit))
                return Entities[entity.Inherit];
            //foreach (Entity e in Entities)
            //    if (e.Type == entity.Inherit)
            //        return e;
            return null;
        }

        public IDictionary<string, Entity> GetTree(string type)
        {
            IDictionary<string, Entity> list = new Dictionary<string, Entity>();
            GetTree(this[type], list);

            return list;
        }

        public string[] GetTreeAsArray(string type)
        {
            List<string> types = new List<string>();
            foreach (Entity e in GetTree(type).Values)
                types.Add(e.Type);

            return types.ToArray();
        }

        private void GetTree(Entity entity, IDictionary<string, Entity> list)
        {
            if (entity == null)
                return;

            if (list.ContainsKey(entity.Type))
            {
                return;
            }

            if (!entity.IsInterface)
            {
                list.Add(entity.Type, entity);
            }

            foreach (Entity sub in Entities.Values)
            {
                if (sub.Inherit == entity.Type)
                    GetTree(sub, list);
                else
                {
                    if (Array.IndexOf(sub.Interfaces, entity.Type) != -1)
                    {
                        GetTree(sub, list);
                    }
                }
            }
        }

        #endregion

        #region References

        public HashedList<Reference> GetInheritedReferences(string type)
        {
            Entity parent = this[type];

            HashedList<Reference> references = new HashedList<Reference>();

            while (parent != null)
            {
                foreach (Reference a in parent.References.Values)
                    references.Add(a);

                parent = GetParent(parent);
            }

            return references;
        }

        public Reference GetReference(string type, string name)
        {
            string index = String.Concat(type, ".", name);

            lock (_ReferencesIndex)
            {
                if (_ReferencesIndex.ContainsKey(index))
                    return _ReferencesIndex[index] as Reference;

                foreach (Reference e in GetInheritedReferences(type))
                    if (e.Name == name)
                    {
                        _ReferencesIndex.Add(index, e);
                        return e;
                    }
            }

            return null;
        }

        public Reference GetReference(string type, string name, bool strict)
        {
            Reference reference = GetReference(type, name);

            if (reference == null && strict)
                throw new ModelElementNotFoundException(String.Format("Reference [{0}] not found in Type [{1}]", name, type));

            return reference;
        }

        #endregion

        #region Attributes

        public HashedList<Attribute> GetInheritedAttributes(string type)
        {
            HashedList<Attribute> attributes = new HashedList<Attribute>();
            Entity parent = this[type];

            while (parent != null)
            {
                foreach (Attribute a in parent.Attributes.Values)
                    attributes.Add(a);

                parent = GetParent(parent);
            }

            return attributes;
        }

        public Attribute GetAttribute(string type, string name)
        {
            string index = String.Concat(type, ".", name);

            lock (_AttributesIndex)
            {
                if (_AttributesIndex.ContainsKey(index))
                    return _AttributesIndex[index] as Attribute;

                foreach (Attribute e in GetInheritedAttributes(type))
                    if (e.Name == name)
                    {
                        _AttributesIndex.Add(index, e);
                        return e;
                    }
            }

            return null;
        }

        public Attribute GetAttribute(string type, string name, bool strict)
        {
            Attribute attribute = GetAttribute(type, name);

            if (attribute == null && strict)
                throw new ModelElementNotFoundException(String.Format("Attribute [{0}] not found in Type [{1}]", name, type));

            return attribute;
        }

        #endregion

        #region Full Attribute Name (Inheritance branch)

        /// <summary>
        /// Return the full attribute name ([Type].[AttName]) with the top most classe type containing this attribute
        /// </summary>
        /// <param name="shortAttributeName"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public string GetFullAttributeName(string shortAttributeName, string typeName)
        {
            string strIndex = String.Concat(typeName, ".", shortAttributeName);

            lock (_FullNameAttributeIndex)
            {
                if (_FullNameAttributeIndex.ContainsKey(strIndex))
                    return _FullNameAttributeIndex[strIndex].ToString();

                string fullName = String.Empty;

                Evaluant.Uss.Model.Entity parent = this[typeName];
                List<Entity> parents = new List<Entity>();

                do
                {
                    parents.Add(parent);
                    parent = this.GetParent(parent);
                }
                while (parent != null);

                // get attributes of each parent classes, specified class
                foreach (Evaluant.Uss.Model.Entity entity in parents)
                    foreach (Evaluant.Uss.Model.Attribute attribute in entity.Attributes.Values)
                    {
                        if (shortAttributeName == attribute.Name)
                        {
                            fullName = String.Concat(entity.Type, ".", attribute.Name);
                            _FullNameAttributeIndex.Add(strIndex, fullName);
                            return fullName;
                        }
                    }

                // get attributes of each child classes of the specified one
                foreach (Evaluant.Uss.Model.Entity entity in this.GetTree(parents[0].Type).Values)
                {
                    // specified class already parsed in the foreach loop above
                    if (entity.Type == parents[0].Type) continue;

                    if (entity.Attributes.ContainsKey(shortAttributeName))
                    {
                        fullName = String.Concat(entity.Type, ".", shortAttributeName);
                        _FullNameAttributeIndex.Add(strIndex, fullName);
                        return fullName;
                    }
                }
            }

            return shortAttributeName;
        }

        /// <summary>
        /// Return an array of full attributes names ([Type].[AttName]) with the top most classe type, 
        /// for all class in the inheritance branch of the specified class type
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public string[] GetFullAttributesNames(string typeName)
        {
            lock (_FullNameAttributeBranchIndex)
            {
                if (_FullNameAttributeBranchIndex.ContainsKey(typeName))
                    return (string[])_FullNameAttributeBranchIndex[typeName];

                List<string> fullNames = new List<string>();

                Evaluant.Uss.Model.Entity parent = this[typeName];
                List<Entity> parents = new List<Entity>();

                do
                {
                    parents.Add(parent);
                    parent = this.GetParent(parent);
                }
                while (parent != null);

                // get attributes of each parent classes, specified class
                foreach (Evaluant.Uss.Model.Entity entity in parents)
                    foreach (Evaluant.Uss.Model.Attribute attribute in entity.Attributes.Values)
                        fullNames.Add(String.Concat(entity.Type, ".", attribute.Name));

                // get attributes of each child classes of the specified one
                foreach (Evaluant.Uss.Model.Entity entity in this.GetTree(parents[0].Type).Values)
                {
                    // specified class already parsed in the foreach loop above
                    if (entity.Type == parents[0].Type) continue;

                    foreach (Evaluant.Uss.Model.Attribute attribute in entity.Attributes.Values)
                        fullNames.Add(String.Concat(entity.Type, ".", attribute.Name));
                }

                string[] arrayFullNames = new string[fullNames.Count];
                fullNames.CopyTo(arrayFullNames, 0);

                _FullNameAttributeBranchIndex.Add(typeName, arrayFullNames);

                return arrayFullNames;
            }
        }

        #endregion

        /// <summary>
        /// Returns all the base entities in the model
        /// </summary>
        /// <returns>All the base entities in the model</returns>
        public ICollection<Entity> GetBaseEntities()
        {
            HashedList<Entity> baseEntities = new HashedList<Entity>();

            // Searches for all base types in the model
            foreach (Entity entity in this.Entities.Values)
            {
                if (entity.IsInterface)
                {
                    continue;
                }

                // Steps ahead until the base class is reached
                Entity parent = entity;
                while (this.GetParent(parent) != null && !this.GetParent(parent).IsInterface)
                    parent = this.GetParent(parent);

                if (!baseEntities.Contains(parent))
                    baseEntities.Add(parent);
            }

            return baseEntities;
        }

        /// <summary>
        /// Gets all the references in the model
        /// </summary>
        /// <returns>A collection of all the references in the model</returns>
        public ICollection<Reference> GetAllReferences()
        {
            ICollection<Reference> references = new List<Reference>();

            foreach (Entity entity in this.Entities.Values)
                foreach (Reference reference in entity.References.Values)
                    references.Add(reference);

            return references;
        }

        public void RegisterMetadata(IMetaData[] metadata)
        {
            ModelMetaDataVisitor visitor = new ModelMetaDataVisitor(this);
            foreach (IMetaData md in metadata)
                md.Accept(visitor);
        }

        /// <summary>
        /// Clears the attribute cache.
        /// </summary>
        public void ClearAttributeCache()
        {
            _AttributesIndex = new Dictionary<string, Attribute>();
        }

        /// <summary>
        /// Clears the reference cache.
        /// </summary>
        public void ClearReferenceCache()
        {
            _ReferencesIndex = new Dictionary<string, Reference>();
        }

        #region IEnumerable<Entity> Members

        public new IEnumerator<Entity> GetEnumerator()
        {
            return Entities.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Entities.Values.GetEnumerator();
        }

        #endregion

        public bool Inherits(string childType, string parentType)
        {
            if (childType == parentType)
                return true;
            if (string.IsNullOrEmpty(childType))
                return false;
            return Inherits(Entities[childType].Inherit, parentType);
        }
    }
}
