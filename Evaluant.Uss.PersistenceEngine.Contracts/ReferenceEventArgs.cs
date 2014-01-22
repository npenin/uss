using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public class ReferenceEventArgs : EntityEventArgs
    {
        public ReferenceEventArgs() { }

        public ReferenceEventArgs(Model.Reference role, object child)
        {
            this.role = role;
            this.child = child;
        }

        protected Model.Reference role;
        public Model.Reference Role
        {
            get { return role; }
            set { role = value; }
        }

        protected object child;
        public object Child
        {
            get { return child; }
            set { child = value; }
        }
    }

    public class ReferenceCancelEventArgs : CancelEntityEventArgs
    {
        public ReferenceCancelEventArgs() { }


        public ReferenceCancelEventArgs(Model.Reference role, object child)
        {
            this.role = role;
            this.child = child;
        }

        public ReferenceCancelEventArgs(ReferenceEventArgs e)
        {
            Role = e.Role;
            Child = e.Child;
        }

        protected Model.Reference role;
        public Model.Reference Role
        {
            get { return role; }
            set { role = value; }
        }

        protected object child;
        public object Child
        {
            get { return child; }
            set { child = value; }
        }

        protected bool childEntityChanged;
        public bool ChildEntityChanged
        {
            get { return childEntityChanged; }
            set
            {
                childEntityChanged = value;
                if (parentEntityChanged || value)
                    entityChanged = value;
            }
        }

        protected bool parentEntityChanged;
        public bool ParentEntityChanged
        {
            get { return parentEntityChanged; }
            set
            {
                parentEntityChanged = value;
                if (childEntityChanged || value)
                    entityChanged = value;
            }
        }
    }
}
