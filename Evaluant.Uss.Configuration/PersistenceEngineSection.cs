using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Xml.Serialization;
using System.Collections;

namespace Evaluant.Uss.Configuration
{
    public class PersistenceEngineSection : PropertyElementCollection
    {

        //<PersistenceEngine Name="Time" Factory="Evaluant.Uss.SqlMapper.SqlMapperProvider">
        //    <ConnectionString>Server=.;Database=Helpdesk;UID=sa;PWD=</ConnectionString>
        //    <Dialect>Evaluant.Uss.SqlMapper.MsSqlDialect</Dialect>
        //    <Driver>Evaluant.Uss.SqlMapper.MsSqlDriver</Driver>
        //    <MappingFileName>~/domain.eum.xml</MappingFileName>
        //    <Metadata Type="assembly">Core</Metadata>
        //    <Metadata Type="metadata">~/domain.meta.xml</Metadata>
        //</PersistenceEngine>
        /// <summary>
        /// <code>
        ///<engine name="gnagna" factory="Evaluant.Uss.SqlMapper.SqlMapperProvider">
        ///    <add name="connectionString" value="MyConnectionString" />
        ///    <add name="dialect" value="Evaluant.Uss.SqlMapper.MsSqlDialect" />
        ///    <add name="driver" value="Evaluant.Uss.SqlMapper.MsSqlDialect" />
        ///    <add name="mappingFileName" value="~/domain.eum.xml;~/domain2.eum.xml" />
        ///    <metadata>
        ///        <add type="assembly" value="MyAssemblyName" />
        ///        <add type="metadata" value="~/domain.meta.xml" />
        ///    </metadata>
        ///</engine>
        /// </code>
        /// </summary>
        /// 

        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("factory", IsRequired = true)]
        public string Factory
        {
            get { return (string)base["factory"]; }
            set { base["factory"] = value; }
        }

        [ConfigurationProperty("culture")]
        public string Culture
        {
            get { return (string)base["culture"]; }
            set { base["culture"] = value; }
        }

        [ConfigurationProperty("connectionStringName")]
        public string connectionStringName
        {
            get { return (string)base["connectionStringName"]; }
            set { base["connectionStringName"] = value; }
        }

        public string ConnectionString
        {
            get
            {
                if (!string.IsNullOrEmpty(connectionStringName))
                    return ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
                try
                {
                    return ((PropertyElement)this.BaseGet("ConnectionString")).Value;
                }
                catch (NullReferenceException)
                {
                    foreach (ConnectionStringSettings cs in ConfigurationManager.ConnectionStrings)
                    {
                        if (cs.ProviderName == this.Name)
                            return cs.ConnectionString;
                    }
                    return null;
                }
            }
        }

        public new string this[string propertyName]
        {
            get
            {
		PropertyElement propElt = this.BaseGet(propertyName) as PropertyElement; 
		return propElt == null ? null : propElt.Value; 
            }
            set { ((PropertyElement)this.BaseGet(propertyName)).Value = value; }
        }

        [ConfigurationProperty("metadata")]
        [ConfigurationCollection(typeof(MetadataElementCollection))]
        public MetadataElementCollection Metadata
        {
            get { return (MetadataElementCollection)base["metadata"]; }
            set { base["metadata"] = value; }
        }
    }

    public class PersistenceEngineSectionCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new PersistenceEngineSection();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PersistenceEngineSection)element).Name;
        }

        public new PersistenceEngineSection this[string name]
        {
            get { return (PersistenceEngineSection)base.BaseGet(name); }
        }

        [ConfigurationProperty("defaultEngine")]
        public string DefaultEngine
        {
            get { return (string)base["defaultEngine"]; }
            set { base["defaultEngine"] = value; }
        }

        public PersistenceEngineSection DefaultEngineSection
        {
            get { return this[DefaultEngine]; }
        }
    }
}
