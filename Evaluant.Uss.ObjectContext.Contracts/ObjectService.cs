using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
//using System.Runtime.Remoting.Messaging;

//using Evaluant.Uss.ObjectContext.DynamicProxy;
//using Evaluant.Uss.ObjectContext.Descriptors;
#if !SILVERLIGHT
using System.Configuration;
using System.Runtime.Remoting.Messaging;
using Evaluant.Uss.Configuration;
#endif
using System.Text;
using Evaluant.Uss.MetaData;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Services;
using Evaluant.Uss.Utility;
//using System.Data.Common;
//using Evaluant.Uss.Common;
//using Evaluant.Uss.ObjectContext.Services;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    /// <summary>
    /// Description résumée de ObjectService.
    /// </summary>
    public class ObjectService : IServiceProvider
    {
        private static string _Slot;
        private List<Assembly> _CachedAssemblies = new List<Assembly>();
        private Dictionary<string, Type> _TypeChache = new Dictionary<string, Type>(20);
        private string _DefaultNamespace = String.Empty;
        private IPersistenceProviderAsync _AsyncPersistenceEngineFactory;
        private IPersistenceProvider _PersistenceEngineFactory;
        //private IPersistableProxyFactory _ProxyFactory;
        //private IPersistentDescriptor _Descriptor;

        private List<IMetaData> _MetaData;

        private object _SynLock = new object();

        static ObjectService()
        {
            _Slot = typeof(ObjectService).AssemblyQualifiedName;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Creates a new <see cref="ObjectService"/> instance using the application settings.
        /// </summary>
        public ObjectService()
            : this((EussConfiguration)ConfigurationManager.GetSection("evaluant.uss"))
        {
        }

        public ObjectService(EussConfiguration config)
        {
            if (config == null)
                Initialize();
            else
                Initialize(ConfigLoader.LoadConfig(config, config.PersistenceEngines.DefaultEngine));

            if (!string.IsNullOrEmpty(config.ObjectContextType))
                ObjectContextType = config.ObjectContextType;

        }

        private void Initialize()
        {
            if (ConfigurationManager.ConnectionStrings.Count == 0)
                throw new ConfigurationException("No matchable connection string could be found");

            ConnectionStringSettings cs = ConfigurationManager.ConnectionStrings["euss"];
            foreach (ConnectionStringSettings c in ConfigurationManager.ConnectionStrings)
            {
                if (!string.IsNullOrEmpty(c.ProviderName))
                {
                    cs = c;
                    break;
                }
            }
            if (cs == null)
                cs = ConfigurationManager.ConnectionStrings[0];

            if (!string.IsNullOrEmpty(cs.ProviderName))
            {
                if (cs.ProviderName.StartsWith("Evaluant.Uss."))
                    Initialize(GetPersistenceProvider(cs.ProviderName.Split(';')));
                else if (cs.ProviderName == "System.Data.Entity")
                    ObjectContextType = "Evaluant.Uss.EntityFramework.ObjectContext";

            }


        }
#endif

        public static IPersistenceProvider GetPersistenceProvider(params string[] providers)
        {
            switch (providers[0])
            {
                case "Evaluant.Uss.Sync":
                case "Evaluant.Uss.Hub":
                    throw new NotSupportedException(string.Format("The provider {0} cannot be initialized with only a connection string", providers[0]));
                case "Evaluant.Uss.Xml":
                case "Evaluant.Uss.Document":
                case "Evaluant.Uss.Memory":
                default:
                    throw new NotImplementedException();
            }
        }

        private void Initialize(IPersistenceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            _PersistenceEngineFactory = provider;
            ObjectContextType = "Evaluant.Uss.EntityResolver.Proxy.Dynamic.ObjectContext";
            if (Type.GetType(ObjectContextType) == null)
                if (Type.GetType(ObjectContextType + ", " + GetType().Assembly.FullName) != null)
                    ObjectContextType += ", " + GetType().Assembly.FullName;
                else
                    ObjectContextType += ", Evaluant.Uss.EntityResolver.Proxy.Dynamic";
            _MetaData = new List<IMetaData>();

            // Makes it unique as many ObjectService could be used in the same application
            _Slot = Guid.NewGuid().ToString();
        }

        private void Initialize(IPersistenceProviderAsync provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            _AsyncPersistenceEngineFactory = provider;
            ObjectContextType = "Evaluant.Uss.EntityResolver.Proxy.Dynamic.ObjectContext";
            if (Type.GetType(ObjectContextType) == null)
                if (Type.GetType(ObjectContextType + ", " + GetType().Assembly.FullName) != null)
                    ObjectContextType += ", " + GetType().Assembly.FullName;
                else
                    ObjectContextType += ", Evaluant.Uss.EntityResolver.Proxy.Dynamic";

            _MetaData = new List<IMetaData>();

            // Makes it unique as many ObjectService could be used in the same application
            _Slot = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates a new <see cref="ObjectService"/> instance.
        /// </summary>
        /// <param name="engine">Engine.</param>
        public ObjectService(IPersistenceProvider engineFactory)
        {
            Initialize(engineFactory);

            //_Descriptor = new ReflectionDescriptor();
            //_ProxyFactory = new PersistableProxyFactory(_Descriptor, engineFactory.Model);

            // Makes it unique as many ObjectService could be used in the same application
        }

        public ObjectService(IPersistenceProviderAsync engineFactory)
        {
            Initialize(engineFactory);

            //_Descriptor = new ReflectionDescriptor();
            //_ProxyFactory = new PersistableProxyFactory(_Descriptor, engineFactory.Model);

            // Makes it unique as many ObjectService could be used in the same application
        }


#if !SILVERLIGHT
        /// <summary>
        /// Creates a new <see cref="ObjectService"/> instance.
        /// </summary>
        /// <param name="filename">Filename.</param>
        public ObjectService(string filename)
            : this(XmlConfigLoader.LoadXmlConfig(filename))
        {
        }

        /// <summary>
        /// Creates a new <see cref="ObjectService"/> instance.
        /// </summary>
        /// <param name="stream">A stream containing the document to load.</param>
        public ObjectService(Stream stream)
            : this(XmlConfigLoader.LoadXmlConfig(stream, null))
        {
        }

        /// <summary>
        /// Creates a new <see cref="ObjectService"/> instance.
        /// </summary>
        /// <param name="stream">A stream containing the document to load.</param>
        /// <param name="engineName">Name of the engine.</param>
        public ObjectService(Stream stream, string engineName)
            : this(XmlConfigLoader.LoadXmlConfig(stream, engineName))
        {
        }

        /// <summary>
        /// Creates a new <see cref="ObjectService"/> instance.
        /// </summary>
        /// <param name="filename">Filename or <value>null</value> if in application configuration.</param>
        /// <param name="engineName">Name of the engine.</param>
        public ObjectService(string filename, string engineName)
        {
            IPersistenceProvider engineFactory = null;

            // Take the configuration from the appSetings ?
            if (String.IsNullOrEmpty(filename))
            {
                engineFactory = ConfigLoader.LoadConfig((EussConfiguration)ConfigurationManager.GetSection("evaluant.uss"), engineName);
            }
            else
            {
                engineFactory = XmlConfigLoader.LoadXmlConfig(filename, engineName);
            }

            Initialize(engineFactory);
        }

        /// <summary>
        /// Creates a new <see cref="ObjectService"/> instance.
        /// </summary>
        /// <param name="assembly">The assembly conaining the resource.</param>
        /// <param name="resource">Resource representing the configuration file.</param>
        public ObjectService(Assembly assembly, string resource)
            : this(XmlConfigLoader.LoadXmlConfig(assembly, resource, null))
        {
        }

        /// <summary>
        /// Creates a new <see cref="ObjectService"/> instance.
        /// </summary>
        /// <param name="assembly">The assembly conaining the resource.</param>
        /// <param name="resource">Resource representing the configuration file.</param>
        /// <param name="engineName">Name of the engine.</param>
        public ObjectService(Assembly assembly, string resource, string engineName)
            : this(XmlConfigLoader.LoadXmlConfig(assembly, resource, engineName))
        {
        }
#endif

        /// <summary>
        /// Gets or sets the persistence engine factory.
        /// </summary>
        /// <value></value>
        public IPersistenceProvider PersistenceEngineFactory
        {
            get { return _PersistenceEngineFactory; }
            set { _PersistenceEngineFactory = value; }
        }

        /// <summary>
        /// Gets or sets the persistence engine factory.
        /// </summary>
        /// <value></value>
        public IPersistenceProviderAsync AsyncPersistenceEngineFactory
        {
            get { return _AsyncPersistenceEngineFactory; }
            set { _AsyncPersistenceEngineFactory = value; }
        }

        ///// <summary>
        ///// Gets the persistable proxy factory.
        ///// </summary>
        ///// <value>The persistable proxy factory.</value>
        //public IPersistableProxyFactory PersistableProxyFactory
        //{
        //    get { return _ProxyFactory; }
        //}

        ///// <summary>
        ///// Gets or sets the persistent descriptor.
        ///// </summary>
        ///// <value>The persistent descriptor.</value>
        //public IPersistentDescriptor PersistentDescriptor
        //{
        //    get { return _Descriptor; }
        //    set
        //    {
        //        _ProxyFactory = new PersistableProxyFactory(value, _PersistenceEngineFactory.CreatePersistenceEngine().Model);
        //        _Descriptor = value;
        //    }
        //}

        /// <summary>
        /// Gets or sets the default namespace in OPath queries.
        /// </summary>
        /// <value></value>
        public string DefaultNamespace
        {
            get { return _DefaultNamespace; }
            set { _DefaultNamespace = value; }
        }

        public static Type ResolveDomainType(string domain, List<Assembly> assemblies)
        {
            // Generic type ?
            if (domain.Contains("{"))
            {
                int start = domain.IndexOf("{");
                List<Type> parameterTypes = new List<Type>();
                StringBuilder currentType = null;
                int level = 0;
                currentType = new StringBuilder();
                for (int i = start; i < domain.Length; i++)
                {
                    char c = domain[i];

                    if (c == '}')
                    {
                        level--;
                    }

                    if (level != 0)
                    {
                        currentType.Append(c);
                    }

                    if (c == '{')
                    {
                        level++;
                    }

                    if (level == 0)
                    {
                        parameterTypes.Add(ResolveDomainType(currentType.ToString(), assemblies));
                        currentType = new StringBuilder();
                    }

                }

                string fullName = domain.Substring(0, start);
                Type main = ResolveDomainType(fullName + '`' + parameterTypes.Count, assemblies);

                return main.MakeGenericType(parameterTypes.ToArray());
            }
            else
            {
                string typeName = TypeResolver.ConvertNamespaceEussToDomain(domain);
                Type resolvedType = Type.GetType(typeName);

                if (resolvedType != null)
                {
                    return resolvedType;
                }

                foreach (Assembly assembly in assemblies)
                {
                    resolvedType = assembly.GetType(typeName);
                    if (resolvedType != null)
                        return resolvedType;
                }

                return null;
            }
        }

        public Assembly[] RegisteredAssemblies { get { return _CachedAssemblies.ToArray(); } }

        /// <summary>
        /// Get a domain Type given an entity's type
        /// </summary>
        /// <param name="entityType">A full type of an entity</param>
        /// <returns>The corresponding CLR Type</returns>
        public Type GetDomainType(string entityType)
        {
            // Locks the access to the method in case concurrent threads access this method (ASP.Net)
            lock (_SynLock)
            {
                // Search in the Type cache
                if (_TypeChache.ContainsKey(entityType))
                    return (Type)_TypeChache[entityType];

                Type type = null;

                // Search in the registered assemblies
                lock (_CachedAssemblies)
                {
                    type = ResolveDomainType(entityType, _CachedAssemblies);
                }

                if (type != null)
                {
                    lock (_TypeChache)
                    {
                        if (!_TypeChache.ContainsKey(entityType))
                            _TypeChache.Add(entityType, type);
                    }
                }

                return type;
            }
        }

        /// <summary>
        /// Registers an Assembly which contains type definitions
        /// </summary>
        /// <param name="assemblyname">The name of the assembly to add</param>
        public void AddAssembly(string assemblyname)
        {
            if (assemblyname == null)
                throw new ArgumentNullException("assemblyname");

            AddAssembly(Assembly.Load(assemblyname));
        }

        /// <summary>
        /// Registers an Assembly which contains type definitions
        /// </summary>
        /// <param name="assemblyname">The assembly to add</param>
        public void AddAssembly(Assembly assembly)
        {
            if (assembly == typeof(int).Assembly)
                return;
            lock (_SynLock)
            {
                if (assembly == null || _CachedAssemblies.Contains(assembly))
                    return;

                _CachedAssemblies.Insert(0, assembly);
            }
        }

        // Contains IdentityMap instances indexed by the Type of the objects they contain
        /// <summary>
        /// Creates a new persistence manager.
        /// </summary>
        /// <returns></returns>
        public IObjectContext CreateObjectContext()
        {
            lock (_SynLock)
            {
                IObjectContext context = (IObjectContext)System.Activator.CreateInstance(Type.GetType(ObjectContextType), this, PersistenceEngineFactory.CreatePersistenceEngine());
                //context.Resolver = new IdentityMap(Resolver);
                return context;
            }
        }

        public IObjectContextAsync CreateAsyncObjectContext()
        {
            lock (_SynLock)
            {
                IObjectContextAsync context = (IObjectContextAsync)System.Activator.CreateInstance(Type.GetType(ObjectContextType), this, AsyncPersistenceEngineFactory.CreatePersistenceEngineAsync());
                //context.Resolver = new IdentityMap(Resolver);
                return context;
            }
        }


        public string ObjectContextType { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the current persistence manager.
        /// </summary>
        /// <value></value>
        public IObjectContext CurrentObjectContext
        {
            get
            {
                lock (_SynLock)
                {
                    IObjectContext _Instance = CallContext.GetData(_Slot) as IObjectContext;

                    if (_Instance == null)
                    {
                        CallContext.SetData(_Slot, _Instance = CreateObjectContext());
                    }

                    return _Instance;
                }
            }
        }
#endif

        /// <summary>
        /// Registers the meta data.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        public void RegisterMetaData(IMetaData metadata)
        {
            RegisterMetaData(new IMetaData[] { metadata });
        }

        public IEntityResolver Resolver { get; set; }
        public ActivatorFactory Activator { get; set; }

        /// <summary>
        /// Registers the meta data.
        /// </summary>
        /// <param name="metadata">The metadata.</param>
        public void RegisterMetaData(IMetaData[] metadata)
        {
            _PersistenceEngineFactory.RegisterMetaData(metadata);
        }

        //public static void ImportRepository(IObjectContext source, IObjectContext target, bool bulk)
        //{
        //    //XmlConfigLoader.ImportRepository(source.PersistenceEngine, target.PersistenceEngine, bulk);
        //}

        #region Services

        private IDictionary<Operation, ServicesCollection> services = new Dictionary<Operation, ServicesCollection>();

        public void AddService(IService service, params Operation[] operations)
        {
            if (operations.Length == 0)
                operations = EnumHelper.GetValues<Operation>();
            foreach (Operation operation in operations)
            {
                if (services.ContainsKey(operation))
                    services[operation].Add(service);
                else
                {
                    ServicesCollection servicesCollection = new ServicesCollection();
                    servicesCollection.Add(service);
                    services.Add(operation, servicesCollection);
                }
            }
        }

        public void RemoveService(IService service, params Operation[] operations)
        {
            if (operations.Length > 0)
            {
                foreach (Operation operation in operations)
                {
                    if (services.ContainsKey(operation))
                        services[operation].Add(service);
                    else
                    {
                        ServicesCollection servicesCollection = new ServicesCollection();
                        servicesCollection.Add(service);
                        services.Add(operation, servicesCollection);
                    }
                }
            }
            else
            {
                foreach (ServicesCollection collection in services.Values)
                {
                    if (collection.Contains(service))
                        collection.Remove(service);
                }
            }
        }

        #endregion

        #region IServiceProvider Membres

        public object GetService(Type serviceType)
        {
            foreach (ServicesCollection collection in services.Values)
            {
                if (collection.Contains(serviceType))
                    return collection[serviceType];
            }
            return null;
        }

        public ServicesCollection GetServices(Operation operation)
        {
            if (services.ContainsKey(operation))
                return services[operation];
            return null;
        }

        public T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        #endregion
    }
}
