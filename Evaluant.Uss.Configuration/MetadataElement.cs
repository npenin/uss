using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Xml.Serialization;

namespace Evaluant.Uss.Configuration
{
    public class MetadataElement : PropertyElement
    {
        [ConfigurationProperty("type")]
        public MetadataType Type
        {
            get { return (MetadataType)this["type"]; }
            set { this["type"] = value; }
        }
    }

    public enum MetadataType
    {
        assembly,
        model,
        metadata
    }

    public class MetadataElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new MetadataElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((MetadataElement)element).Value;
        }
    }
}
