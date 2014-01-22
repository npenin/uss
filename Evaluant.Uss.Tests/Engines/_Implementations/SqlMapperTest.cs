using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.PersistentDescriptors;
using Evaluant.Uss.PersistenceEngine.Contracts.Instrumentation;
using Evaluant.Uss.SqlMapper;
using Evaluant.Uss.SqlMapper.Drivers;
using System.Data;
using System.Transactions;

namespace Evaluant.Uss.Tests.Engines
{
    /// <summary>
    /// Summary description for SqlMapperTest
    /// </summary>
    [TestClass]
    public class SqlMapperTest : EngineTest
    {
        static ObjectService os;
        protected string connectionString;

        public SqlMapperTest()
        {
            connectionString = "Data Source=EUSS;Initial Catalog=uss2;Integrated Security=true";
        }

        protected override Evaluant.Uss.ObjectContext.Contracts.IObjectContext CreateContext()
        {
            if (os == null)
            {
                IPersistenceProvider provider = new Uss.SqlMapper.SqlMapperProvider();
                ((Uss.SqlMapper.SqlMapperProvider)provider).ConnectionString = connectionString;
                ((Uss.SqlMapper.SqlMapperProvider)provider).UseDefaultMapping = true;
                ((Uss.SqlMapper.SqlMapperProvider)provider).Driver = GetDriver();
                ((Uss.SqlMapper.SqlMapperProvider)provider).Dialect = GetDialect();

                provider.RegisterMetaData(MetaData.MetaDataFactory.FromAssembly(GetType().Assembly, "Evaluant.Uss.Tests.Model"));
                provider.InitializeConfiguration();
                os = new ObjectService(provider);
                os.ObjectContextType = typeof(EntityResolver.Proxy.Dynamic.ObjectContext).AssemblyQualifiedName;
            }
            return (IPersistenceEngineObjectContext)os.CreateObjectContext();
        }

        protected virtual IDialect GetDialect()
        {
            return new Evaluant.Uss.SqlMapper.Dialects.SqlServer();
        }

        protected virtual IDriver GetDriver()
        {
            return new SqlServer();
        }

        [TestMethod]
        public virtual void EnsureQueryExecutedWithNoLock()
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                IObjectContext c = GetContext();
                if (c == null)
                    return;
                c.InitializeRepository();
                c.Load<Model.Person>();
            }
        }

        [TestMethod]
        public void EnsureTransactionIsCreatedDependingOnParentTransactionScope()
        {
            IDriver driver = new SqlServer();
            driver.Initialize("Data Source=EUSS;Initial Catalog=uss2;Integrated Security=true");
            IDbConnection connection = driver.CreateConnection();
            connection.Open();
            IDbTransaction transaction = connection.BeginTransaction();
            Assert.AreEqual(System.Data.IsolationLevel.ReadCommitted, transaction.IsolationLevel);
            transaction.Rollback();

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                transaction = driver.BeginTransaction(connection);
                Assert.AreEqual(System.Data.IsolationLevel.ReadUncommitted, transaction.IsolationLevel);
                transaction.Rollback();
            }

            using (TransactionScope scope1 = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted }))
            {
                using (TransactionScope scope2 = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
                {
                    transaction = driver.BeginTransaction(connection);
                    Assert.AreEqual(System.Data.IsolationLevel.ReadUncommitted, transaction.IsolationLevel);
                    transaction.Rollback();
                }
            }

            connection.Close();
        }
    }
}
