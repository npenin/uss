using System;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.Commands
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public abstract class AttributeCommand : Command
    {

        protected object _Value;
        protected string _Name;
        protected string _ParentType;
        protected Type _Type;
        public Entry RelatedEntry { get; set; }

        public object Value
        {
            get { return _Value; }
        }

        public string Name
        {
            get { return _Name; }
        }

        public Type Type
        {
            get { return _Type; }
        }

        public string ParentType
        {
            get { return _ParentType; }
        }

        public AttributeCommand(Entry entry)
            : base(entry.Parent)
        {
            this._ParentType = entry.Parent.Type;
            this._Name = entry.Name;
            this._Value = entry.Value;
            this._Type = entry.Type;
            RelatedEntry = entry;
        }

        [Obsolete]
        public AttributeCommand(string parentId, string parentType, string name, Type type, object value) : base(parentId)
        {
            this._ParentType = parentType;
            this._Name = name;
            this._Value = value;
            this._Type = type;
        }
    }
}
