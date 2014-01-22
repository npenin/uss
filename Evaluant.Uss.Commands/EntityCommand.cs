using System;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Commands
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public abstract class EntityCommand : Command
    {
        protected string _Type;

        public string Type
        {
            get { return _Type; }
        }

        public EntityCommand(Entity e)
            : base(e)
        {
            this._Type = e.Type;
        }

        [Obsolete]
        public EntityCommand(string parentId, string type)
            : base(parentId)
        {
            this._Type = type;
        }
    }
}
