using System;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Commands
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public class CreateAttributeCommand : AttributeCommand
    {
        public CreateAttributeCommand(Entry entry)
            : base(entry)
        {
            _ProcessOrder = 4;
        }

        [Obsolete]
        public CreateAttributeCommand(string parentId, string parentType, string name, Type type, object value)
            : base(parentId, parentType, name, type, value)
        {
            _ProcessOrder = 4;
        }

        //public override void Accept(IVisitor<> processor)
        //{
        //    processor.Process(this);
        //}

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CreateAttribute; }
        }
    }
}
