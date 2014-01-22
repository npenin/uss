using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Evaluant.Uss.SqlExpressions.Mapping
{
    public class Field : ColumnExpression
    {
        public Field(string columnName, bool isPrimaryKey) : base(null, columnName) { IsPrimaryKey = isPrimaryKey; }
        public Field(Table table, string columnName, bool isPrimaryKey) : this(columnName, isPrimaryKey) { Table = table; }

        [XmlAttribute("tableName")]
        public string TableName { get { return Table.TableName; } }

        [XmlAttribute("is-identity")]
        public bool IsIdentity { get; set; }

        /// <summary>
        /// Gets or sets the size of the field. Defined as <c>null</c> if not suitable for this db type.
        /// </summary>[XmlAttribute("size")]
        [DefaultValue(0)]
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
        [DefaultValue(false)]
        public bool IsNullable { get; set; }

        [XmlAttribute("primary-key")]
        [DefaultValue(false)]
        public bool IsPrimaryKey { get; set; }

        [XmlIgnore]
        public Table Table { get; internal set; }

        [XmlIgnore]
        public Field References { get; set; }

        private Field Clone()
        {
            return (Field)MemberwiseClone();
        }

        public Field Clone(Table table)
        {
            Field clone = Clone();
            clone.Table = table;
            return clone;
        }

        public Field Clone(Table table, string fieldName)
        {
            Field clone = Clone(table);
            clone.ColumnName = new Evaluant.NLinq.Expressions.Identifier(fieldName);
            return clone;
        }
    }
}
