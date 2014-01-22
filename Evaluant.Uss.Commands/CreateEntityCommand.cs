using System;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Commands
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public class CreateEntityCommand : EntityCommand
    {
        [Obsolete]
        public CreateEntityCommand(string parentId, string type)
            : base(parentId, type)
        {
            _ProcessOrder = 3;
        }

        public CreateEntityCommand(Entity e)
            : base(e)
        {
            _ProcessOrder = 3;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CreateEntity; }
        }
    }
}
