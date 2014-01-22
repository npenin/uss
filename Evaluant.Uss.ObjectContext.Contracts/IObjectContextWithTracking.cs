using System;
using System.Collections;
using System.Collections.Generic;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    public interface IObjectContextWithTracking : IObjectContextSyncBase, IDisposable, IServiceProvider
    {
        /// <summary>
        /// Removes an object from the first-level cache.
        /// </summary>
        /// <param name="persistable">Persistable object.</param>
        void RemoveFromCache(object persistable);
    }
}
