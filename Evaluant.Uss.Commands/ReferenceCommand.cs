using System;
using System.Collections;
using Evaluant.Uss.Domain;
using System.Collections.Generic;

namespace Evaluant.Uss.Commands
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public abstract class ReferenceCommand : Command
    {

        protected string _Role;
        protected string _ParentType;
        protected string _ChildType;
        protected string _ChildId;

        public string ParentType
        {
            get { return _ParentType; }
            set { _ParentType = value; }
        }

        public string ChildId
        {
            get { return _ChildId; }
            set { _ChildId = value; }
        }

        public string ChildType
        {
            get { return _ChildType; }
            set { _ChildType = value; }
        }

        public Entity ChildEntity { get; set; }


        public Model.Reference Reference { get; set; }

        public string Role
        {
            get { return _Role; }
            set { _Role = value; }
        }

        public ReferenceCommand(Model.Reference reference, Entity parentEntity, Entity childEntity)
            : base(parentEntity)
        {
            Reference = reference;
            ChildEntity = childEntity;
            _ParentType = parentEntity.Type;
            _ChildId = childEntity.Id;
            _ChildType = childEntity.Type;
            Role = reference.Name;
        }

        [Obsolete]
        public ReferenceCommand(string role, string parentId, string parentType, string childId, string childType)
            : base(parentId)
        {
            this._Role = role;
            _ParentType = parentType;
            _ChildId = childId;
            _ChildType = childType;
        }

        public override void UpdateIds(IDictionary<string, string> newIds)
        {
            base.UpdateIds(newIds);

            if (newIds.ContainsKey(ChildId))
                ChildId = newIds[ChildId];
        }
    }
}
