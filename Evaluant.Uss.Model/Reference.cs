using System;

using System.Xml;
using System.Xml.Serialization;
using Evaluant.Uss.Era;

namespace Evaluant.Uss.Model
{
    /// <summary>
    /// Description résumée de Reference.
    /// </summary>
    /// 
#if !SILVERLIGHT
    [Serializable]
#endif
    public class Reference : Reference<Entity, Attribute, Reference>
    {
        private bool _IsComposition;

        private Reference()
        {
        }

        public Reference(string name, string parenttype, string childtype, bool isComposition, bool fromMany, bool toMany)
        {
            Name = name;
            ParentType = parenttype;
            ChildType = childtype;
            IsComposition = isComposition;
            FromMany = fromMany;
            ToMany = toMany;
        }

        [XmlAttribute("composition")]
        public bool IsComposition
        {
            get { return _IsComposition; }
            set { _IsComposition = value; }
        }

    }
}
