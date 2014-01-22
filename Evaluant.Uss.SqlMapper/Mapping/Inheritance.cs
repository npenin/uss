using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Evaluant.NLinq.Expressions;
using Evaluant.NLinq;

namespace Evaluant.Uss.SqlMapper.Mapping
{
    public enum InheritanceMappings
    {
        [XmlEnum("table-per-hierarchy")]
        TablePerHierarchy,
        [XmlEnum("table-per-concrete-class")]
        TablePerConcreteClass,
        [XmlEnum("table-per-class")]
        TablePerClass,
    }

    public class Inheritance
    {
        /// <summary>
        /// Gets or sets the type of inheritance. 
        /// </summary>
        [XmlAttribute("type")]
        public InheritanceMappings Type { get; set; }

        /// <summary>
        /// Gets or sets the discriminator condition in case of table-per-hierarchy mapping
        /// </summary>
        [XmlAttribute("discriminator")]
        public string Discriminator { get; set; }

        private Expression discriminator;
        /// <summary>
        /// Gets or sets the discriminator condition in case of table-per-hierarchy mapping
        /// </summary>
        [XmlIgnore]
        public Expression DiscriminatorExpression
        {
            get
            {
                if (string.IsNullOrEmpty(Discriminator))
                    return null;
                if (discriminator == null)
                    discriminator = new NLinqQuery(Discriminator).Expression;
                discriminator = new DbExpressionVisitors.Mutators.ValueExpressionMutator().Visit(discriminator);
                return discriminator;
            }
        }
    }
}
