using System;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Commands
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public class UpdateAttributeCommand : AttributeCommand
    {
        public UpdateAttributeCommand(Entry entry)
            : base(entry)
        {
            _ProcessOrder = 6;
        }

        [Obsolete]
        public UpdateAttributeCommand(string parentId, string parentType, string name, Type type, object value)
            : base(parentId, parentType, name, type, value)
        {
            _ProcessOrder = 6;
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.UpdateAttribute; }
        }
    }
}
