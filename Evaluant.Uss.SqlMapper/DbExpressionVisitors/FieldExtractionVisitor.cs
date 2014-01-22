using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.SqlExpressions.Mapping;

namespace Evaluant.Uss.SqlMapper.DbExpressionVisitors
{
    class FieldExtractionVisitor : DbExpressionVisitor
    {
        private Field lastField;
        private IDriver driver;
        public FieldExtractionVisitor(IDriver driver)
        {
            Fields = new List<Field>();
            this.driver = driver;
        }

        protected override NLinq.Expressions.Identifier VisitIdentifier(NLinq.Expressions.Identifier identifier)
        {
            lastField = new Field(identifier.Text, false);
            Fields.Add(lastField);
            return identifier;
        }

        public override SqlExpressions.IDbExpression Visit(SqlExpressions.Constant item)
        {
            if (item.Value == null)
                throw new NotSupportedException(string.Format("The value for the field {0} has to be not null to determine the value type", lastField.ColumnName));


            driver.GetTypeInformation(new Model.Attribute(lastField.ColumnName.Text, item.Value.GetType()), new Mapping.Attribute(lastField.ColumnName.Text) { Field = lastField });
            lastField.DefaultValue = item.Value.ToString();
            lastField.Size = lastField.DefaultValue.Length;

            return item;
        }

        public override NLinq.Expressions.Expression Visit(NLinq.Expressions.ValueExpression c)
        {
            if (c.Value == null)
                throw new NotSupportedException(string.Format("The value for the field {0} has to be not null to determine the value type", lastField.ColumnName));


            driver.GetTypeInformation(new Model.Attribute(lastField.ColumnName.Text, c.Value.GetType()), new Mapping.Attribute(lastField.ColumnName.Text) { Field = lastField });
            lastField.DefaultValue = c.Value.ToString();
            lastField.Size = (lastField.DefaultValue.Length / 50 + 1) * 50;

            return c;
        }

        public List<Field> Fields { get; set; }
    }
}
