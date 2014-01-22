using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Configuration;
using System.Collections;
using System.Resources;
using System.IO;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.MetaData;

namespace Evaluant.Uss.Configuration
{
    public class ConfigLoader : XmlConfigLoader
    {
        static IDictionary<string, IPersistenceProvider> engines = new Dictionary<string, IPersistenceProvider>();

        public static IPersistenceProvider LoadConfig(EussConfiguration section, string engineName)
        {
            int lastIndexOfSlash = section.ElementInformation.Source.LastIndexOf('\\');

            if (engines.ContainsKey(engineName))
                return engines[engineName];

            return CreateEngine(section, section.PersistenceEngines[engineName], section.ElementInformation.Source.Substring(0, lastIndexOfSlash));
        }

        public static IPersistenceProvider LoadConfig(EussConfiguration eussConfiguration)
        {
            return LoadConfig(eussConfiguration, eussConfiguration.PersistenceEngines.DefaultEngine);
        }

        public static IPersistenceProvider LoadConfig(string engineName)
        {
            return LoadConfig((EussConfiguration)ConfigurationManager.GetSection("evaluant.uss"), engineName);
        }

        public static IPersistenceProvider LoadConfig()
        {
            return LoadConfig((EussConfiguration)ConfigurationManager.GetSection("evaluant.uss"));
        }

        protected static IPersistenceProvider CreateEngine(EussConfiguration config, PersistenceEngineSection persistenceEngineSection, string basePath)
        {
            string factory = persistenceEngineSection.Factory;

            Assembly assembly = typeof(ConfigLoader).Assembly;  // Evaluant.Uss
            string className;

            switch (factory)
            {
                case "Evaluant.Uss.SqlMapper.SqlMapperProvider":
                    factory = "Evaluant.Uss.SqlMapper.SqlMapperProvider, Evaluant.Uss.SqlMapper";
                    className = "Evaluant.Uss.SqlMapper.SqlMapperProvider";
                    break;
                default:
                    className = factory.Split(',')[0].Trim();
                    break;
            }


            if (factory.IndexOf(",") != -1)
            {
                string assemblyName = factory.Split(',')[1].Trim();

                // Loads the specified assembly
                assembly = Assembly.Load(assemblyName);
            }

            Type type = null;
            IPersistenceProvider engine = null;

            type = assembly.GetType(className);

            // Search the corresponding partial name as it's not the FullName which is given
            if (type == null)
                foreach (Type t in assembly.GetTypes())
                    if (t.Name == className)
                    {
                        type = t;
                        break;
                    }

            if (type == null)
                throw new UniversalStorageException(String.Format("Factory type not found: {0}", className));

            engine = Activator.CreateInstance(type) as IPersistenceProvider;

            if (engine == null)
                throw new UniversalStorageException("Unknown Persistence Factory : " + className);

            engines.Add(persistenceEngineSection.Name, engine);

            engine.Culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();

            // Initializes its properties
            if (!string.IsNullOrEmpty(persistenceEngineSection.Culture))
                engine.Culture = new CultureInfo(persistenceEngineSection.Culture);

            PropertyInfo prop = engine.GetType().GetProperty("ConnectionString");
            if (prop != null)
                prop.SetValue(engine, ResolveFilename(persistenceEngineSection.ConnectionString, basePath), null);

            foreach (PropertyElement pe in persistenceEngineSection)
            {
                prop = type.GetProperty(pe.Name, BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.GetProperty);
                if (pe.Name == "ConnectionString")
                {
                    continue;
                }
                if (prop != null && typeof(IPersistenceEngine).IsAssignableFrom(prop.PropertyType))
                {
                    if (!engines.ContainsKey(pe.Value))
                        CreateEngine(config, config.PersistenceEngines[pe.Value], basePath);
                    prop.SetValue(engines[pe.Value], engine, null);
                }
                if (pe.Name.ToLower() == "delegator")
                {
                    foreach (string engineName in pe.Value.Split(new char[] { ',' }))
                    {
                        if (!engines.ContainsKey(engineName))
                            CreateEngine(config, config.PersistenceEngines[engineName], basePath);
                        AddDelegator(engines[engineName], engine);
                    }
                    continue;
                }
                if (pe.Name == "MappingFileNames")
                {
                    continue;
                }
                if (pe.Name == "mapping")
                {
                    AddMapping(pe, engine, type, basePath);
                    continue;
                }
                if (prop == null)
                    throw new UniversalStorageException(String.Format("The property named {0} could not be found", pe.Name));
                if (Type.GetTypeCode(prop.PropertyType) != TypeCode.Object)
                    prop.SetValue(engine, Convert.ChangeType(ResolveFilename(pe.Value, basePath), prop.PropertyType), null);
                else
                {
                    Type typeToInstantiate = Type.GetType(pe.Value);
                    if (typeToInstantiate == null)
                        typeToInstantiate = assembly.GetType(pe.Value);
                    if (typeToInstantiate == null)
                        throw new UniversalStorageException(String.Format("The property value {0} could not be assigned to the persistence provider", pe.Value));
                    prop.SetValue(engine, Activator.CreateInstance(typeToInstantiate), null);
                }
            }

            foreach (MetadataElement me in persistenceEngineSection.Metadata)
            {
                AddMetadata(me, engine, basePath);
            }

            engine.InitializeConfiguration();

            return engine;
        }

        private static string ResolveContent(string content)
        {
            if (content.StartsWith("assembly:"))
            {
                content = content.Substring(9);
                string resourceName = content.Substring(0, content.IndexOf(','));
                string assemblyName = content.Substring(content.IndexOf(',') + 1);
                Assembly targetAssembly = Assembly.Load(assemblyName);
                Stream s = targetAssembly.GetManifestResourceStream(resourceName);
                if (s == null)
                {
                    ResourceManager rm = new ResourceManager(resourceName.Substring(0, resourceName.LastIndexOf('.')), targetAssembly);
                    return rm.GetString(resourceName.Substring(resourceName.LastIndexOf('.') + 1));
                    throw new TypeLoadException(string.Format("The resource type named {{{0}}} could not be found. Please make sure the resource is not internal", resourceName.Substring(0, resourceName.LastIndexOf('.'))));
                }
                StreamReader sr = new StreamReader(s);
                string result = sr.ReadToEnd();
                sr.Close();
                s.Close();
                return result;
            }
            return null;
        }

        private static void AddDelegator(IPersistenceProvider iPersistenceProvider, IPersistenceProvider engine)
        {
            PropertyInfo delegatorsProperty = engine.GetType().GetProperty("Delegators");
            List<IPersistenceProvider> delegators;
            if (delegatorsProperty.GetValue(engine, null) == null)
                delegators = new List<IPersistenceProvider>();
            else
                delegators = new List<IPersistenceProvider>((IEnumerable<IPersistenceProvider>)delegatorsProperty.GetValue(engine, null));
            delegators.Add(iPersistenceProvider);
            delegatorsProperty.SetValue(engine, delegators.ToArray(), null);
        }

        private static void AddMapping(PropertyElement pe, IPersistenceProvider engine, Type type, string basePath)
        {
            string[] paths = pe.Value.Split(';');
            foreach (string mappingPath in paths)
            {
                string path = mappingPath.Trim();
                string content = ResolveContent(path);
                if (!string.IsNullOrEmpty(content))
                    type.GetMethod("AddMappingContent").Invoke(engine, new object[] { content });
                else
                    type.GetMethod("AddMappingFilename").Invoke(engine, new object[] { ResolveFilename(path, basePath) });
            }
        }


        private static void AddMetadata(MetadataElement me, IPersistenceProvider engine, string basePath)
        {
            string content = ResolveContent(me.Value);
            switch (me.Type)
            {
                case MetadataType.assembly:
                    engine.RegisterMetaData(MetaDataFactory.FromAssembly(ResolveFilename(me.Value, basePath)));
                    break;
                case MetadataType.model:
                    if (!string.IsNullOrEmpty(content))
                    {
                        using (System.Xml.XmlReader reader = new System.Xml.XmlTextReader(new System.IO.StringReader(content)))
                            engine.RegisterMetaData(MetaDataFactory.FromModelFile(reader));
                    }
                    else
                    {
                        engine.RegisterMetaData(MetaDataFactory.FromModelFile(ResolveFilename(me.Value, basePath)));
                    }
                    break;

                case MetadataType.metadata:
                    if (!string.IsNullOrEmpty(content))
                    {
                        using (System.Xml.XmlReader reader = new System.Xml.XmlTextReader(new System.IO.StringReader(content)))
                            engine.RegisterMetaData(MetaDataFactory.FromMetaDataFile(reader));
                    }
                    else
                    {
                        engine.RegisterMetaData(MetaDataFactory.FromMetaDataFile(ResolveFilename(me.Value, basePath)));
                    }
                    break;
            }
        }
    }
}
