using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Evaluant.Uss.SqlExpressions.Mapping;
using Evaluant.Uss.Collections;
using Evaluant.Uss.Era;
//using Evaluant.Uss.Mapping;

namespace Evaluant.Uss.SqlMapper.Mapping
{
    public class Entity : Entity<Entity, Attribute, Reference>
    {
        #region Common Begavior

        [XmlIgnore]
        public Dictionary<string, Attribute> Ids { get; internal set; }
        [XmlIgnore]
        public Entity Inherit { get; set; }
        [XmlIgnore]
        public List<Entity> Derived { get; private set; }


        #endregion

        public Entity()
        {
            Inheritance = new Inheritance();
            Ids = new Dictionary<string, Attribute>();
            AttributeMappings = new HashedList<Attribute>();
            ReferenceMappings = new HashedList<Reference>();
            Embedded = new List<Embed>();
            Derived = new List<Entity>();
            NotMappedFields = new List<Field>();
        }

        public void Add(SqlMapper.Mapping.Attribute attribute)
        {
            if (!AttributeMappings.Contains(attribute))
                AttributeMappings.Add(attribute);
            Attributes.Add(attribute.Name, attribute);
            if (attribute.IsId)
                Ids.Add(attribute.Name, attribute);
        }

        public void Add(SqlMapper.Mapping.Reference reference)
        {
            if (!ReferenceMappings.Contains(reference))
                ReferenceMappings.Add(reference);
            References.Add(reference.Name, reference);
        }

        private bool initialized;

        public void Add(Field field)
        {
            if (initialized)
                throw new NotSupportedException("A table definition cannot be changed at runtime");
            NotMappedFields.Add(field);
        }

        [XmlIgnore()]
        public List<Field> NotMappedFields { get; private set; }

        /// <summary>
        /// <remarks>
        /// Full implementation required to handle TryGetValue
        /// </remarks>
        /// </summary>
        private Model.Entity entityModel;

        [XmlIgnore()]
        public Model.Entity EntityModel
        {
            get { return entityModel; }
            set { entityModel = value; }
        }

        public void Initialize(bool needsModel)
        {
            if (initialized)
                return;
            initialized = true;

            if (needsModel && EntityModel == null)
            {
                if (!Mapping.Model.Entities.TryGetValue(Type, out entityModel))
                    throw new MappingException("Could not map an entity of type " + Type + " which is not imported as metadata");
            }



            if (!Mapping.Tables.TryGetValue(Schema + "." + TableName, out table))
            {
                Table = new Table(Schema, TableName);
                Mapping.Tables.Add(Schema + "." + TableName, Table);
            }

            foreach (Embed embed in Embedded)
            {
                embed.Mapping = Mapping;
                if (embed.Schema==Schema && embed.TableName == TableName)
                    throw new MappingException("An embed should not be used to map two entities of a same table. Please consider using a Composite reference.");

                embed.Initialize(needsModel);
            }

            //Flatten the mapping hierarchy
            if (Inherit != null)
            {
                foreach (Reference rm in Inherit.References.Values)
                {
                    if (!References.ContainsKey(rm.Name))
                        Add(rm.Clone(Inherit.TableName, this));
                }
                foreach (Attribute am in Inherit.Attributes.Values)
                {
                    if (!Attributes.ContainsKey(am.Name))
                        Add(am.Clone(Inherit.TableName, this));
                }
            }


            if (Attributes.Count == 0 && AttributeMappings.Count > 0)
            {
                Attributes = new Dictionary<string, Attribute>();
                foreach (Attribute a in AttributeMappings)
                    Add(a);
            }
            if (References.Count == 0 && ReferenceMappings.Count > 0)
            {
                References = new Dictionary<string, Reference>();
                foreach (Reference r in ReferenceMappings)
                    Add(r);
            }

            foreach (Attribute field in Attributes.Values)
            {
                field.Parent = this;
                field.Table = Table;
                if (field.Model == null && field.Name != "#Id" && EntityModel != null)
                    field.Model = EntityModel.Attributes[field.Name];
                field.Initialize(needsModel);
            }

            foreach (Field field in NotMappedFields)
            {
                Field existingField;
                if (!Table.Fields.TryGetValue(field.ColumnName.Text, out existingField))
                {
                    var f = field.Clone(Table);
                    f.IsNullable = false;
                    Table.Fields.Add(field.ColumnName.Text, f);
                }
                else
                {
                    existingField.Size = Math.Max(existingField.Size, field.Size);
                }
            }

            foreach (Reference reference in References.Values)
            {
                reference.Parent = this;
                if (reference.Model == null && EntityModel != null)
                    reference.Model = EntityModel.References[reference.Name];
                reference.Mapping = Mapping;
                reference.Initialize(needsModel);
                if (reference.IsComposite)
                    reference.Target.ParentComponent = this;
            }
            if (Inheritance != null)
            {
                switch (Inheritance.Type)
                {
                    case InheritanceMappings.TablePerHierarchy:
                        break;
                    case InheritanceMappings.TablePerClass:
                        if (Inherit != null)
                            Embedded.Add(Inherit.ToEmbed(needsModel));
                        break;
                    case InheritanceMappings.TablePerConcreteClass:

                        break;
                    default:
                        break;
                }
            }
        }

        protected virtual Embed ToEmbed(bool needsModel)
        {
            Embed embed = new Embed();
            foreach (Attribute attribute in Attributes.Values)
                embed.Add(attribute);
            foreach (Reference reference in References.Values)
                embed.Add(reference);
            embed.Mapping = Mapping;
            embed.EntityModel = EntityModel;
            embed.Table = Table;
            embed.Type = Type;
            embed.TableName = TableName;
            embed.Initialize(needsModel);
            return embed;
        }

        [XmlIgnore]
        public Mapping Mapping { get; internal set; }

        [XmlElement("Inheritance")]
        public Inheritance Inheritance { get; set; }

        /// <summary>
        /// Gets or sets the table name for this mapping.
        /// </summary>
        [XmlAttribute("table")]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the table name for this mapping.
        /// </summary>
        [XmlAttribute("schema")]
        public string Schema { get; set; }

        /// <summary>
        /// <remarks>
        /// A full property implementation was needed to handle the TryGetValue 
        /// operation on the Mapping.Tables dictionary
        /// </remarks>
        /// </summary>
        private Table table;

        [XmlIgnore]
        public Table Table
        {
            get { return table; }
            set { table = value; }
        }


        [XmlElement("Embed")]
        public List<Embed> Embedded { get; set; }

        [XmlElement("Attribute")]
        public HashedList<Attribute> AttributeMappings { get; set; }

        [XmlElement("Reference")]
        [XmlElement("Composite", typeof(CompositeReference))]
        public HashedList<Reference> ReferenceMappings { get; set; }

        [XmlAttribute("constraint")]
        public string Constraint { get; set; }

        [XmlIgnore]
        public Entity ParentComponent { get; set; }
    }
}
