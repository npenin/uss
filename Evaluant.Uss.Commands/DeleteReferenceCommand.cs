using System;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Commands
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public class DeleteReferenceCommand : ReferenceCommand
    {
        [Obsolete]
        public DeleteReferenceCommand(string role, string parentId, string parentType, string childId, string childType)
            : base(role, parentId, parentType, childId, childType)
        {
            _ProcessOrder = 0;
        }



        public DeleteReferenceCommand(Model.Reference role, Entity parentEntity, Entity childEntity)
            : base(role, parentEntity, childEntity)
        {
            _ProcessOrder = 0;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.DeleteReference; }
        }
    }
}
