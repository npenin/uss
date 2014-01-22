using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlExpressions;
using System.Xml.Serialization;
using Evaluant.Uss.SqlExpressions.Mapping;

namespace Evaluant.Uss.SqlMapper.Mapping
{
    public class Rule
    {
        public Rule()
        {
            ParentFields = new List<Field>();
            ChildFields = new List<Field>();
        }

        public Mapping Mapping { get; set; }

        [XmlAttribute("parent-schema")]
        public string ParentSchema { get; set; }

        [XmlAttribute("parent-table")]
        public string ParentTableName { get; set; }

        [XmlAttribute("parent-fields")]
        public string ParentFieldNames { get; set; }

        [XmlAttribute("child-table")]
        public string ChildTableName { get; set; }

        [XmlAttribute("child-schema")]
        public string ChildSchema { get; set; }

        [XmlAttribute("child-fields")]
        public string ChildFieldNames { get; set; }

        [XmlIgnore]
        public Table ParentTable { get; set; }
        [XmlIgnore]
        public Table ChildTable { get; set; }
        [XmlIgnore]
        public IList<Field> ParentFields { get; set; }
        [XmlIgnore]
        public IList<Field> ChildFields { get; set; }

        [XmlIgnore]
        public BinaryExpression Constraint { get; private set; }

        bool initialized = false;

        public void Initialize()
        {
            if (initialized)
                return;
            initialized = true;
            if (ParentTable == null)
                ParentTable = Mapping.Tables[ParentSchema + "." + ParentTableName];

            if (ChildTable == null)
            {
                if (Mapping.Tables.ContainsKey(ChildSchema + "." + ChildTableName))
                    ChildTable = Mapping.Tables[ChildSchema + "." + ChildTableName];
                else
                    ChildTable = Mapping.Tables[ChildSchema + "." + ChildTableName] = new Table(ChildSchema, ChildTableName);
            }
            string[] parentFieldNames = ParentFieldNames.Split(',');
            string[] childFieldNames = ChildFieldNames.Split(',');

            for (int i = 0; i < parentFieldNames.Length; i++)
            {
                string parentField = parentFieldNames[i].Trim();
                string childField = childFieldNames[i].Trim();
                if (ParentTable.Fields.ContainsKey(parentField))
                    ParentFields.Add(ParentTable.Fields[parentField]);
                else
                {
                    Field field;
                    if (ChildTable.Fields.ContainsKey(childField))
                    {
                        field = ChildTable.Fields[childField].Clone(ParentTable, parentField);
                        field.IsPrimaryKey = false;
                        field.IsNullable = true;
                    }
                    else
                        field = new Field(ParentTable, parentField, false);
                    ParentTable.Fields.Add(field.ColumnName.Text, field);
                    ParentFields.Add(field);
                }
                if (ChildTable.Fields.ContainsKey(childField))
                    ChildFields.Add(ChildTable.Fields[childField]);
                else
                {
                    Field field = ParentFields[i].Clone(ChildTable, childField);
                    field.IsPrimaryKey = false;
                    field.IsIdentity = false;
                    field.IsNullable = true;
                    ChildTable.Fields.Add(field.ColumnName.Text, field);
                    ChildFields.Add(field);
                }
            }
        }

        internal Rule Clone(string oldTableName, string newTableName)
        {
            Rule clone;
            if (oldTableName != newTableName)
            {
                clone = new Rule();
                clone.ChildFieldNames = ChildFieldNames;
                if (oldTableName == ChildTableName)
                    clone.ChildTableName = newTableName;
                else
                    clone.ChildTableName = ChildTableName;
                clone.Mapping = Mapping;
                clone.ParentFieldNames = ParentFieldNames;
                if (oldTableName == ParentTableName)
                    clone.ParentTableName = newTableName;
                else
                    clone.ParentTableName = ParentTableName;
                clone.Initialize();
            }
            else
                clone = this;
            return clone;
        }
    }
}
