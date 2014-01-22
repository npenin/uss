using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Evaluant.Uss.SqlMapper.Drivers;
using System.Data.Common;
using System.Data.EntityClient;

namespace Evaluant.Uss.SqlMapper.EF
{
    public class EFDriver : Driver
    {
        public EFDriver()
            : this(EntityProviderFactory.Instance)
        {
        }

        public EFDriver(DbProviderFactory factory)
            : base(factory)
        {
        }

        public override bool IsOrm
        {
            get
            {
                return true;
            }
        }

        public override bool SupportDataManipulationLanguage
        {
            get
            {
                return false;
            }
        }

        public override void Initialize(string connectionString)
        {
            base.Initialize(connectionString);
            var csb = (EntityConnectionStringBuilder)factory.CreateConnectionStringBuilder();
            csb.ConnectionString = connectionString;
            alternateProviderName = csb.Provider;
            alternateFactory = SqlMapperProvider.GetFactory(csb.Provider);
            alternateConnectionString = csb.ProviderConnectionString;
        }

        public override IDriver CreateAlternateDriver()
        {
            var csb = (EntityConnectionStringBuilder)factory.CreateConnectionStringBuilder();
            csb.ConnectionString = connectionString;
            IDriver alternateDriver = SqlMapperProvider.CreateDriver(SqlMapperProvider.GetFactory(csb.Provider), csb.Provider);
            alternateDriver.Initialize(csb.ProviderConnectionString);
            return alternateDriver;
        }

        private DbProviderFactory alternateFactory;
        private string alternateProviderName;
        private string alternateConnectionString;

        public override DbProviderFactory AlternateFactory
        {
            get
            {
                return alternateFactory;
            }
        }

        public override string AlternateProviderName
        {
            get
            {
                return alternateProviderName;
            }
        }

        public override string AlternateConnectionString
        {
            get
            {
                return alternateConnectionString;
            }
        }
    }
}
