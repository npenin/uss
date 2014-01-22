using System;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using Evaluant.Uss.Collections;
using System.Collections.Generic;
using Evaluant.Uss.Era;

namespace Evaluant.Uss.Model
{
    /// <summary>
    /// Description résumée de Entity.
    /// </summary>
    /// 
#if !SILVERLIGHT
    [Serializable]
#endif
    public class Entity : Entity<Entity, Attribute, Reference>
    {
        private string _Inherit;
        private string _Implement;
        private bool _IsInterface;
        private string[] _Interfaces;
        private Dictionary<string, Attribute> _Attributes;
        private Dictionary<string, Reference> _References;

        public Entity(string type, string inherit)
        {
            Type = type;
            _Inherit = inherit;
            _Interfaces = new string[0];
            _Attributes = new Dictionary<string, Attribute>();
            _References = new Dictionary<string, Reference>();
        }

        public Entity(string type)
            : this(type, String.Empty)
        {
        }

        public Entity()
            : this(String.Empty, String.Empty)
        {
        }

        public string[] Interfaces
        {
            get { return _Interfaces; }
        }

        [XmlAttribute("inherit", DataType = "string")]
        public string Inherit
        {
            get { return _Inherit; }
            set { _Inherit = value; }
        }

        /// <summary>
        /// Gets or sets the implemented interfaces in the whole hierarchy, even of higher levels.
        /// </summary>
        /// <value>The interfaces.</value>
        [XmlAttribute("implement", DataType = "string")]
        public string Implement
        {
            get { return _Implement; }
            set
            {
                _Implement = value;
                _Interfaces = _Implement.Split(',');

                for (int i = 0; i < _Interfaces.Length; i++)
                {
                    _Interfaces[i] = _Interfaces[i].Trim();
                }
            }
        }


        [XmlAttribute("isInterface")]
        public bool IsInterface
        {
            get { return _IsInterface; }
            set { _IsInterface = value; }
        }
    }
}
