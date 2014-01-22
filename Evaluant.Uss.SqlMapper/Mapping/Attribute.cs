using System;
using System.Collections.Generic;
using System.Text;
//using Evaluant.Uss.Mapping;
using Evaluant.Uss.SqlExpressions.Mapping;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Data;
using Evaluant.Uss.Era;

namespace Evaluant.Uss.SqlMapper.Mapping
{
    public enum Generator
    {
        [XmlEnum("business")]
        Business,
        [XmlEnum("guid")]
        Guid,
        [XmlEnum("native")]
        Native,
    }

    public class Attribute : Attribute<Entity, Attribute, Reference>
    {
        protected Attribute()
            : base()
        {

        }

        public Attribute(string name)
            : base(name)
        {

        }

        [XmlIgnore]
        public Field Field { get; set; }

        [XmlAttribute("tableName")]
        public string TableName { get; set; }

        [XmlAttribute("generator")]
        [DefaultValue(Generator.Business)]
        public Generator Generator { get; set; }

        /// <summary>
        /// Gets or sets the field name to map. If not set, will be defined with the same value of <see cref="Name"/>
        /// </summary>
        [XmlAttribute("field")]
        public string ColumnName { get; set; }

        /// <summary>
        /// Gets or sets the size of the field. Defined as <c>null</c> if not suitable for this db type.
        /// </summary>[XmlAttribute("size")]
        [DefaultValue(0)]
        [XmlAttribute("size")]
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the precision of the field. Defined as <c>null</c> if not suitable for this db type.
        /// </summary>
        [XmlAttribute("precision")]
        [DefaultValue(0)]
        public byte Precision { get; set; }

        /// <summary>
        /// Gets or sets the scale of the field. Defined as <c>null</c> if not suitable for this db type.
        /// </summary>
        [XmlAttribute("scale")]
        [DefaultValue(0)]
        public byte Scale { get; set; }

        /// <summary>
        /// Gets or sets the default value of the field. Do not define for <c>null</c> value.
        /// </summary>
        [XmlAttribute("default-value")]
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the standard db type of the field.
        /// </summary>
        [XmlAttribute("db-type")]
        public DbType DbType { get; set; }

        /// <summary>
        /// Gets or sets whether the field allow <c>null</c>. <c>False</c> if not set.
        /// </summary>
        [XmlAttribute("nullable")]
        [DefaultValue(true)]
        public bool IsNullable { get; set; }

        [XmlAttribute("isId", DataType = "boolean")]
        [DefaultValue(false)]
        public override bool IsId { get { return base.IsId; } set { base.IsId = value; } }

        [XmlAttribute("primary-key", DataType = "boolean")]
        [DefaultValue(false)]
        public bool IsPrimaryKey { get { return IsId; } set { IsId = value; } }

        [XmlIgnore]
        public Model.Attribute Model { get; set; }

        [XmlIgnore]
        public Table Table { get; internal set; }

        public void Initialize(bool needsModel)
        {
            if (!Table.Fields.ContainsKey(this.ColumnName))
            {
                Field = new Field(Table, ColumnName, IsId) { IsIdentity = Generator == SqlMapper.Mapping.Generator.Native, DbType = DbType, DefaultValue = DefaultValue, IsNullable = IsNullable && !IsId, Precision = Precision, Scale = Scale, Size = Size };
                Table.Fields.Add(ColumnName, Field);
            }
            else
                Field = Table.Fields[ColumnName];
        }


        public Attribute Clone(string oldTableName, Entity entity)
        {
            return new Attribute(Name) { ColumnName = ColumnName, DbType = DbType, DefaultValue = DefaultValue, Generator = Generator, IsId = IsId, IsNullable = IsNullable, Model = Model, Parent = entity, Precision = Precision, Scale = Scale, Size = Size, TableName = entity.TableName };
        }
    }
}
