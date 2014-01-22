using System;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Commands
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public class DeleteEntityCommand : EntityCommand
    {
         [ObsoleteAttribute]
        public DeleteEntityCommand(string id, string type)
            : base(id, type)
        {
            _ProcessOrder = 2;
        }

        public DeleteEntityCommand(Entity e)
            :base(e)
         {
             _ProcessOrder = 2;
         }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.DeleteEntity; }
        }

    }
}
