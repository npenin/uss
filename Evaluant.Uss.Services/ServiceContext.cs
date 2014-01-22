using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Evaluant.Uss.PersistenceEngine.Contracts;
//using Evaluant.Uss.Common;

namespace Evaluant.Uss.Services
{
    public class ServiceContext
    {
        protected EventArgs eventArgs;

        public ServiceContext(Operation operation, EventArgs e)
        {
            this.operation = operation;
            this.eventArgs = e;
        }

        protected Operation operation;
        public Operation Operation
        {
            get { return operation; }
            set { operation = value; }
        }

        public bool Cancel
        {
            get
            {
                if (eventArgs != null && eventArgs is CancelEntityEventArgs)
                    return ((CancelEntityEventArgs)eventArgs).Cancel;
                return false;
            }
            set
            {
                if (eventArgs != null && eventArgs is CancelEntityEventArgs)
                    ((CancelEntityEventArgs)eventArgs).Cancel = value;
            }
        }

        public bool EntityChanged
        {
            get
            {
                if (eventArgs != null && eventArgs is CancelEntityEventArgs)
                    return ((CancelEntityEventArgs)eventArgs).EntityChanged;
                return false;
            }
            set
            {
                if (eventArgs != null && eventArgs is CancelEntityEventArgs)
                    ((CancelEntityEventArgs)eventArgs).EntityChanged = value;
            }
        }
    }

#if EUSS12

    public static class ReferenceServiceContext
    {
        public static ReferenceServiceContext<T> Create<T>(Operation operation, T child, ReferenceCancelEventArgs e)
            where T : class
        {
            return new ReferenceServiceContext<T>(operation, child, e);
        }
        public static ReferenceServiceContext<T> Create<T>(Operation operation, T child, ReferenceEventArgs e)
            where T : class
        {
            return new ReferenceServiceContext<T>(operation, child, e);
        }
    }

    public class ReferenceServiceContext<T> : ServiceContext where T : class
    {
        public ReferenceServiceContext(Operation operation, T child, EventArgs e) : base(operation, e) { }

        public string Role
        {
            get
            {
                if (eventArgs is ReferenceCancelEventArgs)
                    return ((ReferenceCancelEventArgs)eventArgs).Role;
                if (eventArgs is ReferenceEventArgs)
                    return ((ReferenceEventArgs)eventArgs).Role;
                return string.Empty;
            }
        }

        public T Child
        {
            get
            {
                if (eventArgs is ReferenceCancelEventArgs)
                    return ((ReferenceCancelEventArgs)eventArgs).Child as T;
                if (eventArgs is ReferenceCancelEventArgs)
                    return ((ReferenceCancelEventArgs)eventArgs).Child as T;
                return null;
            }
        }

        public bool ParentEntityChanged
        {
            get
            {
                if (eventArgs != null && eventArgs is ReferenceCancelEventArgs)
                    return ((ReferenceCancelEventArgs)eventArgs).ParentEntityChanged;
                return false;
            }
            set
            {
                if (eventArgs != null && eventArgs is ReferenceCancelEventArgs)
                    ((ReferenceCancelEventArgs)eventArgs).ParentEntityChanged = value; ;
            }
        }

        public bool ChildEntityChanged
        {
            get
            {
                if (eventArgs != null && eventArgs is ReferenceCancelEventArgs)
                    return ((ReferenceCancelEventArgs)eventArgs).ChildEntityChanged;
                return false;
            }
            set
            {
                if (eventArgs != null && eventArgs is ReferenceCancelEventArgs)
                    ((ReferenceCancelEventArgs)eventArgs).ChildEntityChanged = value;
            }
        }
    }
#else

    public class ReferenceServiceContext : ServiceContext
    {
        public ReferenceServiceContext(Operation operation, object child, EventArgs e) : base(operation, e) { }

        public Model.Reference Role
        {
            get
            {
                if (eventArgs is ReferenceCancelEventArgs)
                    return ((ReferenceCancelEventArgs)eventArgs).Role;
                if (eventArgs is ReferenceEventArgs)
                    return ((ReferenceEventArgs)eventArgs).Role;
                return null;
            }
        }

        public object Child
        {
            get
            {
                if (eventArgs is ReferenceCancelEventArgs)
                    return ((ReferenceCancelEventArgs)eventArgs).Child;
                if (eventArgs is ReferenceCancelEventArgs)
                    return ((ReferenceCancelEventArgs)eventArgs).Child;
                return null;
            }
        }

        public bool ParentEntityChanged
        {
            get
            {
                if (eventArgs != null && eventArgs is ReferenceCancelEventArgs)
                    return ((ReferenceCancelEventArgs)eventArgs).ParentEntityChanged;
                return false;
            }
            set
            {
                if (eventArgs != null && eventArgs is ReferenceCancelEventArgs)
                    ((ReferenceCancelEventArgs)eventArgs).ParentEntityChanged = value; ;
            }
        }

        public bool ChildEntityChanged
        {
            get
            {
                if (eventArgs != null && eventArgs is ReferenceCancelEventArgs)
                    return ((ReferenceCancelEventArgs)eventArgs).ChildEntityChanged;
                return false;
            }
            set
            {
                if (eventArgs != null && eventArgs is ReferenceCancelEventArgs)
                    ((ReferenceCancelEventArgs)eventArgs).ChildEntityChanged = value;
            }
        }

        public static ReferenceServiceContext Create(Operation operation, object child, EventArgs e)
        {
            return new ReferenceServiceContext(operation, child, e);
        }
    }

#endif

}
