using System;
using System.Net;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Evaluant.Uss.OData.Edm
{
    public class Metadata
    {
        public Metadata()
        {
            EntityTypes = new Dictionary<string, EntityType>();
            Associations = new Dictionary<string, Association>();
        }

        public Dictionary<string, EntityType> EntityTypes { get; set; }

        public Dictionary<string, Association> Associations { get; set; }

        public string Name { get; set; }

        public EntityType GetEntityByEntitySetName(string entitySet)
        {
            foreach (EntityType entity in EntityTypes.Values)
            {
                if (entity.EntitySetName == entitySet)
                    return entity;
            }
            return null;
        }

        public string GetEntitySetName(string type)
        {
            EntityType entity = EntityTypes[type];
            while (entity.EntitySetName == null && entity.BaseType != null)
            {
                entity = EntityTypes[entity.BaseType];
            }
            return entity.EntitySetName;
        }
    }
}
