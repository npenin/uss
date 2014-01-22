using System;
//using Evaluant.Uss.Common;

namespace Evaluant.Uss.MetaData
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ReferenceMetaData : IMetaData
    {
        private string _ParentType;
        private string _ChildType;
        private string _Name;
        private bool _IsComposition;
        private bool _FromMany;
        private bool _ToMany;
        private bool _Ignore;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceMetaData"/> class.
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="name">The name.</param>
        public ReferenceMetaData(string parenttype, string name, string childtype) : this(parenttype, name, childtype, false, false, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceMetaData"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="composition">if set to <c>true</c> [composition].</param>
        public ReferenceMetaData(Type parenttype, string name, Type childtype, bool isComposition, bool fromMany, bool toMany):
            this(TypeResolver.ConvertNamespaceDomainToEuss(parenttype), name, TypeResolver.ConvertNamespaceDomainToEuss(childtype), isComposition, fromMany, toMany)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceMetaData"/> class.
        /// </summary>
        /// <param name="type">The type name.</param>
        /// <param name="name">The name.</param>
        /// <param name="composition"><c>true</c> if it is a composition; otherwise, <c>false</c>.</param>
        public ReferenceMetaData(string parenttype, string name, string childtype, bool isComposition, bool fromMany, bool toMany)
        {
            _ParentType = parenttype;
            _ChildType = childtype;
            _Name = name;
            _IsComposition = isComposition;
            _FromMany = fromMany;
            _ToMany = toMany;
        }

        /// <summary>
        /// Gets or sets the parent type.
        /// </summary>
        /// <value>The type.</value>
        public string ParentType
        {
            get { return _ParentType; }
            set { _ParentType = value; }
        }

        /// <summary>
        /// Gets or sets the child type.
        /// </summary>
        /// <value>The type.</value>
        public string ChildType
        {
            get { return _ChildType; }
            set { _ChildType = value; }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Reference"/> is composition.
        /// </summary>
        /// <value><c>true</c> if it is a composition; otherwise, <c>false</c>.</value>
        public bool IsComposition
        {
            get { return _IsComposition; }
            set { _IsComposition = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the relationship can be referenced by many entities.
        /// </summary>
        /// <value><c>true</c> if the relationship can be referenced by many entities; otherwise, <c>false</c>.</value>
        public bool FromMany
        {
            get { return _FromMany; }
            set { _FromMany = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the relationship can reference many entities.
        /// </summary>
        /// <value><c>true</c> if the relationship can reference many entities; otherwise, <c>false</c>.</value>
        public bool ToMany
        {
            get { return _ToMany; }
            set { _ToMany = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ReferenceMetadata"/> is ignored.
        /// </summary>
        /// <value><c>true</c> if ignored; otherwise, <c>false</c>.</value>
        public bool Ignore
        {
            get { return _Ignore; }
            set { _Ignore = value; }
        }

        /// <summary>
        /// Accepts the specified visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        void IMetaData.Accept(IMetaDataVisitor visitor)
        {
            visitor.Process(this);
        }

    }
}
