using System;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Commands
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public class CreateReferenceCommand : ReferenceCommand
    {
        [Obsolete]
        public CreateReferenceCommand(string role, string parentId, string parentType, string childId, string childType)
            : base(role, parentId, parentType, childId, childType)
        {
            _ProcessOrder = 5;
        }

        public CreateReferenceCommand(Model.Reference role, Entity parentEntity, Entity childEntity)
            : base(role, parentEntity, childEntity)
        {
            _ProcessOrder = 5;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CreateReference; }
        }
    }
}
