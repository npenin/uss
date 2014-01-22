using System;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Commands
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public class DeleteAttributeCommand : AttributeCommand
	{
        public DeleteAttributeCommand(Entry entry)
            : base(entry)
        {
            _ProcessOrder = 1;
        }

        [Obsolete]
		public DeleteAttributeCommand(string parentId, string parentType, string name, Type type, object value) : base(parentId, parentType, name, type, value)
		{
            _ProcessOrder = 1;
		}

        public override CommandTypes CommandType
        {
            get { return CommandTypes.DeleteAttribute; }
        }
    }
}
