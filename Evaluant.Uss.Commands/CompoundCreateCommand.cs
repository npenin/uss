using System;
using System.Collections;
using Evaluant.Uss.Domain;
using System.Collections.Generic;

namespace Evaluant.Uss.Commands
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public class CompoundCreateCommand : CreateEntityCommand
    {
        public CompoundCreateCommand(Entity e, ICollection<AttributeCommand> commands)
            : base(e)
        {
            InnerCommands.AddRange(commands);
        }


        [Obsolete]
        public CompoundCreateCommand(string id, string type, ICollection<AttributeCommand> commands)
            : base(id, type)
        {
            InnerCommands.AddRange(commands);
        }

        //public override void Accept(ICommandProcessor processor)
        //{
        //    processor.Process(this);
        //}

        private List<AttributeCommand> _innerCommands = new List<AttributeCommand>();

        /// <summary>
        /// Gets the inner commands.
        /// </summary>
        /// <value>The inner commands.</value>
        /// <remarks>This can only be made of CreateAttributeCommand items</remarks>
        public List<AttributeCommand> InnerCommands
        {
            get { return _innerCommands; }
        }

        public override void UpdateIds(IDictionary<string,string> newIds)
        {
            // Nothing to do as CreateEntity commands are the one which create new Idsbase
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CompoundCreate; }
        }

    }
}
