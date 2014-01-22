using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;
//using Evaluant.Uss.Metadata;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.PersistenceEngine.Contracts.Instrumentation;
using Evaluant.Uss.SqlMapper.Mapper;
using System.Data.Common;
using System.Data;

namespace Evaluant.Uss.SqlMapper
{
    public class SqlMapperProvider : PersistenceProviderImplementation
    {
        public const string TraceSource = "Evaluant.Uss.Mapper.Sql";
        private string providerName;
        private DbProviderFactory factory;

        static SqlMapperProvider()
        {
            TraceHelper.AddSource(TraceSource);
        }

        public SqlMapperProvider(string providerName)
        {

            this.factory = GetFactory(providerName);
            this.providerName = providerName;
        }

        public static DbProviderFactory GetFactory(string providerName)
        {
            DbProviderFactory factory = null;
            foreach (DataRow row in DbProviderFactories.GetFactoryClasses().Select("InvariantName = '" + providerName + '\''))
            {
                factory = DbProviderFactories.GetFactory(row);
                break;
            }
            return factory;
        }

        public static IDriver CreateDriver(DbProviderFactory factory, string providerName)
        {
            switch (providerName)
            {
                case "System.Data.SqlClient":
                    return new Drivers.SqlServer(factory);
                case "System.Data.EntityClient":
                    return (IDriver)Activator.CreateInstance(Type.GetType("Evaluant.Uss.SqlMapper.EF.EFDriver, Evaluant.Uss.SqlMapper.EF"));
                default:
                    Console.WriteLine("Warning : The default driver will be used. It has the same behavior as SqlServer. If it does not work properly, please try using an existing driver.");
                    return new Drivers.Driver(factory);
            }
        }

        public static IDialect CreateDialect(string providerName)
        {
            switch (providerName)
            {
                case "System.Data.SqlClient":
                    return new Dialects.SqlServer();
                case "System.Data.EntityClient":
                    return (IDialect)Activator.CreateInstance(Type.GetType("Evaluant.Uss.SqlMapper.EF.EFDialect, Evaluant.Uss.SqlMapper.EF"));
                default:
                    Console.WriteLine("Warning : The default dialect will be used. It has the same behavior as SqlServer. If it does not work properly, please try using an existing dialect.");
                    return new Dialects.SqlServer();
            }
        }

        public SqlMapperProvider()
        {
            Driver = new Drivers.SqlServer();
            Dialect = new Dialects.SqlServer();
            //this.TypeResolver = new TypeResolver.TypeResolverImpl();
            this.Serializer = new Serializer.BinarySerializer();
        }

        public IDriver Driver { get; set; }
        public IDriver AlternateDriver { get; set; }
        public IDialect Dialect { get; set; }
        public IDialect AlternateDialect { get; set; }

        public string ConnectionString { get; set; }

        #region IStorageProvider Members

        public override IPersistenceEngine CreatePersistenceEngine()
        {
            EnsureInitializeConfiguration();
            SqlMapperEngine engine = new SqlMapperEngine(this, Driver, AlternateDriver, Dialect, AlternateDialect, Mapping);
            return engine;
        }

        private void EnsureInitializeConfiguration()
        {
            if (!initialized)
                InitializeConfiguration();
        }

        private void InitializeMapping()
        {
            if (Mapping.Mapper == null)
                Mapping.Mapper = new DefaultMapper(Driver, Mapping);

            foreach (KeyValuePair<string, Mapping.Entity> entity in Mapping.Entities)
            {
                Model.Entity entityModel;
                if (!Model.Entities.TryGetValue(entity.Key, out entityModel))
                    throw new Mapping.MappingException(string.Format("The entity '{0}' does not exist in the model", entity.Key));
                entity.Value.EntityModel = Model.Entities[entity.Key];
            }
            Mapping.Model = Model;
            Mapping.Initialize(true);
        }

        #endregion

        public Mapping.Mapping Mapping { get; set; }

        private string mappingFileName;

        public string MappingFileName
        {
            get { return mappingFileName; }
            set
            {
                mappingFileName = value;
                Mapping = SqlMapper.Mapping.Mapping.Load(value);
            }
        }


        public bool UseDefaultMapping { get; set; }

        public void DefaultMap()
        {
            TraceHelper.TraceEvent(TraceSource, System.Diagnostics.TraceEventType.Start, 1, "Default mapping creation.");
            if (Model == null)
                throw new NotSupportedException("The model must be set");
            DefaultMapper mapper = new DefaultMapper(Driver);
            Mapping = mapper.Mapping = new Mapping.Mapping();
            Mapping.Mapper = mapper;
            mapper.Map(Model);
            TraceHelper.TraceEvent(TraceSource, System.Diagnostics.TraceEventType.Stop, 1, "Default mapping creation completed.");
        }

        #region IPersistenceProvider Members

        public void MarkAsInitialized()
        {
            initialized = true;
        }

        public override void InitializeConfiguration()
        {
            initialized = true;
            if (factory == null && (Driver == null || Dialect == null))
                throw new NotSupportedException(string.Format("The provider named {0} is not supported by SQLMapper engine", providerName));

            if (Driver == null)
                Driver = CreateDriver(factory, providerName);

            if (Dialect == null)
                Dialect = CreateDialect(providerName);

            if (UseDefaultMapping)
                DefaultMap();
            InitializeMapping();
            Driver.Initialize(ConnectionString);
            if (!Driver.SupportDataManipulationLanguage && Driver.AlternateProviderName != null)
            {
                AlternateDriver = CreateDriver(Driver.AlternateFactory, Driver.AlternateProviderName);
                AlternateDriver.Initialize(Driver.AlternateConnectionString);
                AlternateDialect = CreateDialect(Driver.AlternateProviderName);
            }
        }

        #endregion
    }
}
