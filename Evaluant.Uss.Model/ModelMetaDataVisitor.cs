using System;
using Evaluant.Uss.MetaData;

namespace Evaluant.Uss.Model
{
    public class ModelMetaDataVisitor : IMetaDataVisitor
    {
        private Model _Model;

        public ModelMetaDataVisitor(Model model)
        {
            _Model = model;
        }

        public void Process(ReferenceMetaData reference)
        {
            Entity entity = _Model[reference.ParentType];

            if (entity == null)
            {
                entity = new Entity(reference.ParentType);
                _Model.Entities.Add(entity.Type, entity);
            }

            Reference mref = _Model.GetReference(reference.ParentType, reference.Name);

            if (mref != null && reference.Ignore)
            {
                entity.References.Remove(mref.Name);
                _Model.ClearReferenceCache();
                return;
            }

            if (mref == null)
            {
                mref = new Reference(reference.Name, reference.ParentType, reference.ChildType, reference.IsComposition, reference.FromMany, reference.ToMany);
                entity.References.Add(mref.Name, mref);
            }

            // To override a previous value
            if (mref.IsComposition != reference.IsComposition)
                mref.IsComposition = reference.IsComposition;

            if (mref.FromMany != reference.FromMany)
                mref.FromMany = reference.FromMany;

            if (mref.ToMany != reference.ToMany)
                mref.ToMany = reference.ToMany;

            if (mref.ChildType != reference.ChildType)
                mref.ChildType = reference.ChildType;
        }

        void Evaluant.Uss.MetaData.IMetaDataVisitor.Process(InterfaceMetadata interf)
        {
            Entity entity = _Model[interf.Name];

            if (entity == null)
            {
                entity = new Entity(interf.Name);
                _Model.Entities.Add(entity.Type, entity);
            }

            entity.IsInterface = true;
        }

        void Evaluant.Uss.MetaData.IMetaDataVisitor.Process(TypeMetaData type)
        {
            Entity entity = _Model[type.Name, false];

            if (type.Ignore)
            {
                if (entity != null)
                {
                    _Model.Entities.Remove(entity.Type);
                }

                return;
            }

            if (entity == null)
            {
                entity = new Entity(type.Name);
                entity.Model = _Model;
                if (!type.Ignore)
                    _Model.Entities.Add(entity.Type, entity);
            }

            if (type.IsInterface)
            {
                if (entity.Implement != null && entity.Implement != String.Empty)
                    entity.Implement += ",";

                entity.Implement += type.ParentName;

                Entity parent = _Model[type.ParentName];

                if (parent == null)
                {
                    parent = new Entity(type.ParentName);
                    _Model.Entities.Add(type.ParentName, parent);
                }

                parent.IsInterface = true;
            }
            else
            {
                // To override a previous value
                if (type.ParentName != String.Empty && type.ParentName != null && entity.Inherit != type.ParentName)
                    entity.Inherit = type.ParentName;
            }


        }

        void Evaluant.Uss.MetaData.IMetaDataVisitor.Process(PropertyMetaData property)
        {
            Entity entity = _Model[property.Type];

            if (entity == null)
            {
                entity = new Entity(property.Type);
                _Model.Entities.Add(entity.Type, entity);
            }

            Attribute attribute = _Model.GetAttribute(property.Type, property.PropertyName);

            if (attribute != null && property.Ignore)
            {
                entity.Attributes.Remove(attribute.Name);
                _Model.ClearAttributeCache();
                return;
            }

            if (attribute == null)
            {
                attribute = new Attribute(property.PropertyName, property.PropertyType);
                attribute.Values = property.Values;
                if (!attribute.IsId)
                    attribute.IsId = string.Equals(attribute.Name, entity.Type.Substring(entity.Type.LastIndexOf('.')) + "Id", StringComparison.OrdinalIgnoreCase);
                entity.Attributes.Add(attribute.Name, attribute);
            }

            // To override a previous value
            if (attribute.Type != property.PropertyType)
                attribute.Type = property.PropertyType;

            if (attribute.Values != property.Values)
                attribute.Values = property.Values;
        }
    }
}
