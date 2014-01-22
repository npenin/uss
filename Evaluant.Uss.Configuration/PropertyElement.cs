using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Evaluant.Uss.Configuration
{
    public class PropertyElement : ConfigurationElement
    {
        [ConfigurationProperty("name",IsKey=true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("value")]
        public string Value
        {
            get { return (string)base["value"]; }
            set { base["value"] = value; }
        }
    }

    public class PropertyElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new PropertyElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PropertyElement)element).Name;
        }
    }
}
