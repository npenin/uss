using System.Collections;
//using Evaluant.Uss.Common;
using Evaluant.Uss.MetaData;
using System.Threading;
using System.Diagnostics;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Collections.Generic;

namespace Evaluant.Uss.Cache
{
	/// <summary>
	/// Description résumée de CacheProvider.
	/// </summary>
	public class CacheProvider : PersistenceProviderImplementation
	{
        internal static string CATEGORY = "Euss";
        internal static string NAME = "Cached Queries";

        internal IdentityMap _Entities;
        internal Dictionary<string,object> _Scalars;
        internal Hashtable _Loads;
        internal ReaderWriterLock _RWL;

        public CacheProvider() : base()
		{
            //if (!PerformanceCounterCategory.Exists(CATEGORY))
            //{
            //    PerformanceCounterCategory.Create(CATEGORY, "Contains counters for Euss", NAME, "Number of Cached Queries");
            //}
		}

		public override void InitializeConfiguration()
		{
            _Entities = new IdentityMap();
            _Scalars = new Dictionary<string,object>();
            _Loads = new Hashtable();
            _RWL = new ReaderWriterLock();
		}

		public override IPersistenceEngine CreatePersistenceEngine()
		{
			return new CacheEngine(_Delegator.CreatePersistenceEngine(), this);
		}

		public override void RegisterMetaData(IMetaData[] metadata)
		{
			base.RegisterMetaData(metadata);
			_Delegator.RegisterMetaData(metadata);
		}

		private IPersistenceProvider _Delegator;
		public IPersistenceProvider Delegator
		{
			get { return _Delegator; }
			set { _Delegator = value;}
		}
	}
}
