using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.Domain;
using Evaluant.Uss.PersistenceEngine.Contracts;

namespace Evaluant.Uss.EntityResolver.Proxy.Dynamic
{
    public class ObjectContext : PersistenceEngineObjectContextImpl
    {
        public ObjectContext(ObjectService factory, IPersistenceEngine engine)
            : base(factory, engine)
        {
            Resolver = new AnonymousTypeResolver(new EntityResolver(this));
        }
    }

    
    public class ObjectContextAsync : PersistenceEngineObjectContextAsyncImpl
    {
        public ObjectContextAsync(ObjectService factory, IPersistenceEngineAsync engine)
            : base(factory, engine)
        {
            Resolver = new AnonymousTypeResolver(new EntityResolver(this));
        }
    }
}
