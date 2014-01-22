using System;
using Evaluant.Uss.ObjectContext.Contracts;

namespace Evaluant.Uss.EntityResolver.Proxy
{
    /// <summary>
    /// Used to mark a dynamic proxy
    /// </summary>
    public interface IPersistableProxy : IPersistable
    {
        /// <summary>
        /// Force the values contained in the Entity property to be pushed into the protected fields
        /// </summary>
        void Set();
        void SetReferences();

        /// <summary>
        /// Updates the inner Entity entries.
        /// </summary>
        void Update();
    }
}
