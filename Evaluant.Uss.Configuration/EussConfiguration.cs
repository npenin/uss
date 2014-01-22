using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Evaluant.Uss.Configuration
{
    public class EussConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("engines")]
        [ConfigurationCollection(typeof(PersistenceEngineSectionCollection), AddItemName = "engine")]
        public PersistenceEngineSectionCollection PersistenceEngines
        {
            get { return (PersistenceEngineSectionCollection)this["engines"]; }
            set { this["engines"] = value; }
        }


        [ConfigurationProperty("objectContextType")]
        public string ObjectContextType
        {
            get { return (string)this["objectContextType"]; }
            set { this["objectContextType"] = value; }
        }
    }
}