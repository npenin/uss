using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Evaluant.Uss.PersistenceEngine.Contracts
{
    public class EntityEventArgs : EventArgs
    {
        public EntityEventArgs() : base() { }

        public new static readonly EntityEventArgs Empty=new EntityEventArgs();
    }

    public class CancelEntityEventArgs : CancelEventArgs
    {
        public CancelEntityEventArgs() : base() { }
        public CancelEntityEventArgs(bool cancel) : base(cancel) { }

        protected bool entityChanged;
        public bool EntityChanged
        {
            get { return entityChanged; }
            set { entityChanged = value; }
        }
    }
}
