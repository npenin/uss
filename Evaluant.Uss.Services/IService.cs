using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Services
{
    [Flags]
    public enum Operation
    {
        Loaded = 1,
        Inserted = 2,
        Updated = 4,
        Deleted = 8,
        CreatedRelationship = 16,
        RemovedRelationship = 32,
        Inserting = 64,
        Updating = 128,
        Deleting = 256,
        CreatingRelationship = 512,
        RemovingRelationship = 1024
    }

    public interface IService
    {
        void ServiceAdded();
        void ServiceRemoved();

#if !EUSS12
        
        void Visit(object item, ServiceContext context);
        void Visit(object parent, object child, ReferenceServiceContext context);

#else

        void Visit<T>(T item, ServiceContext context)
            where T : class;
        void Visit<Parent, Child>(Parent parent, Child child, ReferenceServiceContext<Child> context)
            where Child : class
            where Parent : class;

#endif
    }
}
