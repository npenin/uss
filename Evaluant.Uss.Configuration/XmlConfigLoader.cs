using System;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Xml;
//using Evaluant.Uss.Common;
using System.Text.RegularExpressions;
using System.Globalization;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.MetaData;

namespace Evaluant.Uss
{
	/// <summary>
	/// Description résumée de XmlConfigLoader.
	/// </summary>
	public class XmlConfigLoader
	{
		public XmlConfigLoader()
		{
		}

		/// <summary>
		/// Loads the XML config with the default engine.
		/// </summary>
		/// <param name="filename">Filename.</param>
		/// <returns>An IPersistenceProvider representing the default configuration</returns>
		public static IPersistenceProvider LoadXmlConfig(string filename)
		{
			return LoadXmlConfig(filename, null);
		}

		/// <summary>
		/// Loads the XML config with the named engine.
		/// </summary>
		/// <param name="filename">Filename.</param>
		/// <param name="engineName">Name of the engine.</param>
		/// <returns>An IPersistenceProvider representing the specified configuration</returns>
		/// <remarks>If the specified engine was not found an exception is thown.</remarks>
		public static IPersistenceProvider LoadXmlConfig(string filename, string engineName)
		{
			Stream stream = null;

			// Searches for a resource in the executing Assembly if the file doesn't exists
			if (File.Exists(filename))
				stream = new FileStream(filename, FileMode.Open, FileAccess.Read);

			if (stream == null)
				throw new PersistenceEngineException("The configuration file was not found", new FileNotFoundException(filename));

			using (stream)
			{
				return LoadXmlConfig(stream, engineName, new FileInfo(filename).DirectoryName);
			}
		}

		public static IPersistenceProvider LoadXmlConfig(Assembly assembly, string resource, string engineName)
		{
			Stream stream = assembly.GetManifestResourceStream(resource);

			if (stream == null)
				throw new PersistenceEngineException("The resource was not found.");

			using (stream)
			{
				return LoadXmlConfig(stream, engineName, new FileInfo(assembly.Location).DirectoryName);
			}
		}

		/// <summary>
		/// Loads the XML config with the named engine.
		/// </summary>
		/// <param name="filename">A stream containing the document to load.</param>
		/// <param name="engineName">Name of the engine.</param>
		/// <returns>An IPersistenceProvider representing the specified configuration</returns>
		/// <remarks>If the specified engine was not found an exception is thown.</remarks>
		public static IPersistenceProvider LoadXmlConfig(Stream stream, string engineName)
		{
			return LoadXmlConfig(stream, engineName, String.Empty);
		}

		/// <summary>
		/// Loads the XML config with the named engine.
		/// </summary>
		/// <param name="filename">A stream containing the document to load.</param>
		/// <param name="engineName">Name of the engine.</param>
		/// <param name="basePath">The base path for virtual filenames contained in the file.</param>
		/// <returns>An IPersistenceProvider representing the specified configuration</returns>
		/// <remarks>If the specified engine was not found an exception is thown.</remarks>
		public static IPersistenceProvider LoadXmlConfig(Stream stream, string engineName, string basePath)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			XmlDocument config = new XmlDocument();

			XmlTextReader xmlReader = null;

			try
			{
				xmlReader = new XmlTextReader(stream);
				config.Load(xmlReader);
			}
			catch
			{
				throw new UniversalStorageException("The configuration file is not a valid XML file");
			}
			finally
			{
				if (xmlReader != null)
					xmlReader.Close();
			}

			// The reason for needing a prefix for the default namespace in our XPath query is due to the fact that in XPath, there is no concept of a default namespace.
			XmlNamespaceManager nsm = new XmlNamespaceManager(config.NameTable);
			nsm.AddNamespace("ns", "http://euss.evaluant.com/schemas/EngineConfiguration.xsd");

			// Searches the named engine, or return the default one
			XmlNode nodeEngine = null;
			if (engineName != null)
				nodeEngine = config.SelectSingleNode(String.Format("/ns:PersistenceEngines/ns:PersistenceEngine[@Name='{0}']", engineName), nsm);
			else
				nodeEngine = config.SelectSingleNode("/ns:PersistenceEngines/ns:PersistenceEngine[@Name = /ns:PersistenceEngines/@DefaultEngine]", nsm);

			// No engine was found
			if (nodeEngine == null)
				throw new UniversalStorageException(String.Format("Persistence Engine not found: {0}", engineName == null ? "Default Engine" : engineName));

			return CreateEngine(nodeEngine, basePath);
		}

		/// <summary>
		/// Creates the engine.
		/// </summary>
		/// <param name="nodeEngine">XmlNode representing the engine to create.</param>
		/// <returns></returns>
		private static IPersistenceProvider CreateEngine(XmlNode nodeEngine, string basePath)
		{
			Assembly assembly = Assembly.GetCallingAssembly();
			string className = nodeEngine.Attributes.GetNamedItem("Factory").Value.Split(',')[0].Trim();

			if (nodeEngine.Attributes.GetNamedItem("Factory").Value.IndexOf(",") != -1)
			{
				string assemblyName = nodeEngine.Attributes.GetNamedItem("Factory").Value.Split(',')[1].Trim();

				// Loads the specified assembly
				assembly = Assembly.Load(assemblyName);
			}

			// The reason for needing a prefix for the default namespace in our XPath query is due to the fact that in XPath, there is no concept of a default namespace.
			XmlNamespaceManager nsm = new XmlNamespaceManager(nodeEngine.OwnerDocument.NameTable);
			nsm.AddNamespace("ns", "http://euss.evaluant.com/schemas/EngineConfiguration.xsd");

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

			engine.Culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();

			// Initializes its properties
			foreach (XmlNode propertyNode in nodeEngine.ChildNodes)
			{
				// skip comments nodes
				if (propertyNode.NodeType == XmlNodeType.Comment)
					continue;

				// Specific node: Culture
				if (propertyNode.Name.ToLower() == "culture")
				{
					if (propertyNode.InnerText.Trim() != String.Empty)
						engine.Culture = new CultureInfo(propertyNode.InnerText);

					XmlNode node = null;

					if ((node = propertyNode.Attributes.GetNamedItem("DecimalSeparator")) != null)
						engine.Culture.NumberFormat.NumberDecimalSeparator = node.Value;

					if ((node = propertyNode.Attributes.GetNamedItem("DateFormat")) != null)
						engine.Culture.DateTimeFormat.ShortDatePattern = node.Value;

					if ((node = propertyNode.Attributes.GetNamedItem("TimeFormat")) != null)
						engine.Culture.DateTimeFormat.LongTimePattern = node.Value;

					continue;
				}

				// Specific node: <Assembly> 
				if (propertyNode.Name.ToLower() == "assembly")
				{
					// Loads the assembly in the Application Domain
					Assembly.LoadFrom(ResolveFilename(propertyNode.InnerText, basePath));
				}
				else if (propertyNode.Name.ToLower() == "metadata") // Specific node: <Metadata> 
				{
					if (propertyNode.Attributes.GetNamedItem("Type") == null)
						throw new UniversalStorageException(String.Format("Attribute \"Type\" missing for <Metadata>"));

					switch (propertyNode.Attributes.GetNamedItem("Type").Value)
					{
						case "assembly":
							engine.RegisterMetaData(MetaDataFactory.FromAssembly(ResolveFilename(propertyNode.InnerText, basePath)));
							break;

						case "model":
							engine.RegisterMetaData(MetaDataFactory.FromModelFile(ResolveFilename(propertyNode.InnerText, basePath)));
							break;

						case "metadata":
							engine.RegisterMetaData(MetaDataFactory.FromMetaDataFile(ResolveFilename(propertyNode.InnerText, basePath)));
							break;

						default:
							throw new UniversalStorageException(String.Format("Metadata Type unknown: {0}", propertyNode.Attributes.GetNamedItem("Type").Value));

					}
					continue;
				}

				// property nodes
				PropertyInfo propertyInfo = type.GetProperty(propertyNode.Name);
				if (propertyInfo == null)
					throw new UniversalStorageException(String.Format("Property not found {0} in type {1}", propertyNode.Name, type.FullName));

				// Is it another PersistenceEngine or an ordinal value ?
				if (propertyNode.ChildNodes[0].NodeType == XmlNodeType.Text)
				{
					// Ordinal value
					string strval = ResolveFilename(propertyNode.InnerText, basePath);
					object changedTypeValue = Convert.ChangeType(strval, propertyInfo.PropertyType);
					propertyInfo.SetValue(engine, changedTypeValue, new object[0]);
				}
				else
				{
					// Persistence Engine(s)

					ArrayList engines = new ArrayList();

					foreach (XmlNode childNode in propertyNode.ChildNodes)
					{
						IPersistenceProvider childEngine = null;
						XmlNode subEngineNode = null;

						if (childNode.NodeType == XmlNodeType.Comment)
							continue;

						// Is it a reference to an existing definition ?
						if (childNode.Attributes.GetNamedItem("Name") != null)
						{
							subEngineNode = nodeEngine.OwnerDocument.SelectSingleNode(
								String.Format("/ns:PersistenceEngines/ns:PersistenceEngine[@Name='{0}']",
								childNode.Attributes.GetNamedItem("Name").Value
								), nsm);
						}
						else
						{
							subEngineNode = childNode;
						}

						childEngine = CreateEngine(subEngineNode, basePath);
						engines.Add(childEngine);
					}

					if (propertyInfo.PropertyType.IsArray)
					{
						propertyInfo.SetValue(engine, engines.ToArray(propertyInfo.PropertyType.GetElementType()), new object[0]);
					}
					else
						propertyInfo.SetValue(engine, engines[0], new object[0]);
				}
			}

			engine.InitializeConfiguration();

			return engine;
		}

		/// <summary>
		/// Process any virtual filename to return the real path. For instance "~" means the same folder as the configuration file
		/// </summary>
		/// <param name="filename">The filename to resolve</param>
		/// <param name="basePath">The encoded virtual path</param>
		/// <returns>The real filename</returns>
		public static string ResolveFilename(string filename, string basePath)
		{
			if (string.IsNullOrEmpty(filename))
				return filename;
			if (filename.Contains("~/"))
			{
				return filename.Replace("~/", basePath + Path.DirectorySeparatorChar);

				//return Path.Combine(basePath, filename.Substring(2, filename.Length - 2));
			}

			return filename;
		}


        //public static void ImportRepository(IPersistenceEngine pes, IPersistenceEngine pet, bool bulk)
        //{
        //    // Contains id translation table
        //    Hashtable translationTable = new Hashtable();
        //    Hashtable reverseTable = new Hashtable();

        //    Transaction t = new Transaction();

        //    // Import all entities
        //    foreach (Models.Entity entity in pes.Model.Entities.Values)
        //    {
        //        // Process only root types as it will load all children
        //        if (entity.Inherit != String.Empty && entity.Inherit != null && pes.Model.Entities.ContainsKey(entity.Inherit))
        //            continue;

        //        int count = Convert.ToInt32(pes.LoadScalar(String.Concat("count(", entity.Type, ")")));
        //        System.Diagnostics.Trace.WriteLine("Processing " + entity.Type + "(" + count.ToString() + ")");

        //        // Loads a set of entities (span)
        //        if (count > 0)
        //        {
        //            t = new Transaction();

        //            Domain.EntitySet entities = pes.Load(entity.Type);

        //            foreach (Domain.Entity e in entities)
        //            {
        //                e.State = State.New;
        //                t.Serialize(e);

        //            }

        //            t.Commit(pet, false);

        //            // Retrieves the translated ids
        //            foreach (DictionaryEntry de in t.NewIds)
        //            {
        //                translationTable.Add(String.Concat(entity.Type, ":", de.Key), de.Value);
        //                reverseTable.Add(String.Concat(entity.Type, ":", de.Value), de.Key);
        //            }
        //        }
        //    }

        //    if (bulk)
        //    {
        //        t = new Transaction();
        //    }

        //    // Import all relationships
        //    foreach (Models.Entity entity in pes.Model.Entities)
        //    {
        //        // Process only root types as it will load all children
        //        if (entity.Inherit != String.Empty && entity.Inherit != null)
        //            continue;

        //        System.Diagnostics.Trace.WriteLine(String.Empty);
        //        System.Diagnostics.Trace.WriteLine("Processing relationships from " + entity.Type);

        //        // Loads entities in the client repository as it can have been filtered
        //        foreach (Entity parent in pet.Load(entity.Type))
        //        {
        //            string uniqueParentKey = String.Concat(entity.Type, ":", parent.Id);
        //            string reversedParentId = reverseTable.Contains(uniqueParentKey) ? reverseTable[uniqueParentKey].ToString() : parent.Id;

        //            foreach (Models.Reference reference in pes.Model.GetInheritedReferences(parent.Type))
        //            {
        //                System.Diagnostics.Trace.Write(".");

        //                string query = String.Concat(parent.Type, "[id('", reversedParentId, "')].", reference.Name);
        //                int count = Convert.ToInt32(pes.LoadScalar(String.Concat("count(", query, ")")));

        //                // Loads a set of entities (span)
        //                if (count > 0)
        //                {
        //                    if (!bulk)
        //                    {
        //                        t = new Transaction();
        //                    }

        //                    EntitySet entities = pes.Load(query);

        //                    foreach (Entity e in entities)
        //                    {
        //                        string uniqueChildKey = String.Concat(e.Type, ":", e.Id);
        //                        string childId = translationTable.Contains(uniqueChildKey) ? translationTable[uniqueChildKey].ToString() : e.Id;

        //                        t.PushCommand(new Commands.CreateReferenceCommand(reference.Name, parent.Id, parent.Type, childId, e.Type));
        //                    }

        //                    if (!bulk)
        //                    {
        //                        t.Commit(pet, false);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if (bulk)
        //    {
        //        t.Commit(pet, false);
        //    }
        //}
	}
}
