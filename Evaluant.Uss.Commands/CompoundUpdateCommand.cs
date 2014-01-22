using System;
using System.Collections;
using Evaluant.Uss.Domain;
using System.Collections.Generic;

namespace Evaluant.Uss.Commands
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public class CompoundUpdateCommand : Command
    {
        public CompoundUpdateCommand(Entity e, ICollection<AttributeCommand> commands)
            : base(e)
        {
            this._innerCommands.AddRange(commands);
            _ProcessOrder = 6;
        }

        [Obsolete]
        public CompoundUpdateCommand(string parentId, string parentType, ICollection<AttributeCommand> commands)
            : base(parentId)
        {
            this._ParentType = parentType;
            this._innerCommands.AddRange(commands);
            _ProcessOrder = 6;
        }

        protected string _ParentType;

        public string ParentType
        {
            get { return _ParentType; }
        }

        private List<AttributeCommand> _innerCommands = new List<AttributeCommand>();

        /// <summary>
        /// Gets the inner commands.
        /// </summary>
        /// <value>The inner commands.</value>
        /// <remarks>This can only be made of UpdateAttributeCommand items</remarks>
        public List<AttributeCommand> InnerCommands
        {
            get { return _innerCommands; }
        }

        public override void UpdateIds(IDictionary<string,string> newIds)
        {
            // Nothing to do as the entity is already exiting with its final id
        }

        public override CommandTypes CommandType
        {
            get { return CommandTypes.CompoundUpdate; }
        }
    }
}
