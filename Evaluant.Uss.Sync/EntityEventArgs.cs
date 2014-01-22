using System;
using System.Text;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Sync
{
    public class EntityEventArgs : EventArgs
    {
        public EntityEventArgs(Entity entity)
        {
            this.entity = entity;
        }

        private Entity entity;

        public Entity Entity
        {
            get { return entity; }
            set { entity = value; }
        }

        private bool cancel = false;

        public bool Cancel
        {
            get { return cancel; }
            set { cancel = value; }
        }

    }
}
