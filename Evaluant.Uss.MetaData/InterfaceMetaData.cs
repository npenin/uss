using System;
using System.Runtime.Serialization;

namespace Evaluant.Uss.MetaData
{
    //[DataContract]
    public class InterfaceMetadata : IMetaData
    {
        private string _Name;

        /// <summary>
        /// Initializes a new instance of the <see cref="InterfaceMetadata"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public InterfaceMetadata(string name)
        {
            _Name = name;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        //[DataMember]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
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
