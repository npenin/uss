using System;
using System.Collections;
using Evaluant.Uss;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.ObjectContext.Contracts
{
	public interface IPersistable
	{
		Entity Entity
		{
			get;
			set;
		}
		IPersistenceEngineObjectContext ObjectContext
		{
			get;
			set;
		}
        IPersistenceEngineObjectContextAsync ObjectContextAsync
        {
            get;
            set;
        }

        //void TrackChildren();
	}
}