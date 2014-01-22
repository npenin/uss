using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using System.Xml.Serialization;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.Model;
using Evaluant.Uss.Era;
using Evaluant.NLinq;

namespace Evaluant.Uss.SqlMapper.Mapping
{
    public class Reference : Reference<Entity, Attribute, Reference>
    {
        private Expression orderByExpression;
        private Expression whereExpression;
        public Reference()
        {
            Rules = new List<Rule>();
        }

        [XmlAttribute("order-by")]
        public string OrderBy { get; set; }

        [XmlIgnore]
        public Expression OrderByExpression
        {
            get
            {
                if (!string.IsNullOrEmpty(OrderBy) && orderByExpression == null)
                    orderByExpression = new NLinqQuery(OrderBy).Expression;
                return orderByExpression;
            }
        }

        [XmlAttribute("where")]
        public string Where { get; set; }

        [XmlIgnore]
        public Expression WhereExpression
        {
            get
            {
                if (!string.IsNullOrEmpty(Where) && whereExpression == null)
                    whereExpression = new NLinqQuery(Where).Expression;
                return whereExpression;
            }
        }

        [XmlAttribute("on")]
        public string Constraint { get; set; }

        [XmlIgnore]
        public Model.Reference Model { get; set; }

        [XmlElement("Rule")]
        public virtual List<Rule> Rules { get; set; }

        [XmlIgnore]
        public Mapping Mapping { get; internal set; }

        public virtual void Initialize(bool needsModel)
        {
            if (string.IsNullOrEmpty(Rules[0].ParentTableName))
            {
                Rules[0].ParentSchema = Parent.Schema;
                Rules[0].ParentTableName = Parent.TableName;
                Rules[0].ParentTable = Parent.Table;

            }
            if (Target == null)
            {
                Target = Mapping[ChildType];
                Target.Mapping = Mapping;
                Target.Initialize(needsModel);
            }
            if (string.IsNullOrEmpty(Rules[Rules.Count - 1].ChildTableName))
            {
                Rules[Rules.Count - 1].ChildSchema = Target.Schema;
                Rules[Rules.Count - 1].ChildTableName = Target.TableName;
                Rules[Rules.Count - 1].ChildTable = Target.Table;

            }
            foreach (Rule rule in Rules)
            {
                rule.Mapping = Mapping;
                rule.Initialize();
            }

            if (Rules.Count == 2)
            {
                List<string> indexTableIds = new List<string>();

                foreach (var field in Rules[0].ChildFields)
                    indexTableIds.Add(field.ColumnName.Text);
                foreach (SqlExpressions.Mapping.Field field in Rules[1].ParentFields)
                    indexTableIds.Add(field.ColumnName.Text);

                foreach (var field in Rules[0].ChildTable.Fields)
                {
                    if (indexTableIds.Contains(field.Key))
                    {
                        field.Value.IsPrimaryKey = true;
                        field.Value.IsIdentity = false;
                        field.Value.IsNullable = false;
                    }
                }
            }
        }

        public Reference Clone(string oldTableName, Entity newEntity)
        {
            Reference clone = new Reference();
            clone.Name = Name;
            clone.Model = Model;
            clone.Mapping = Mapping;
            clone.ChildType = ChildType;
            clone.Target = Target;
            clone.Parent = newEntity;
            if (newEntity.TableName != oldTableName)
            {
                foreach (Rule rule in Rules)
                    clone.Rules.Add(rule.Clone(oldTableName, newEntity.TableName));
            }
            else
            {
                clone.Rules.AddRange(Rules);
            }
            return clone;
        }

        public virtual bool IsComposite { get { return false; } }
    }
}
