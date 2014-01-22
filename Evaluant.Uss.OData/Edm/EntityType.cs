using System;
using System.Collections.Generic;

namespace Evaluant.Uss.OData.Edm
{
    public class EntityType
    {
        public EntityType()
        {
            Properties = new Dictionary<string, Property>();
        }

        public string EntitySetName { get; set; }

        public string Type { get; set; }

        public Dictionary<string, Property> Properties { get; set; }

        public string BaseType { get; set; }
    }
}
