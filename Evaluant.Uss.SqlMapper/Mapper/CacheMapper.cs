using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.SqlMapper.Mapper
{
    public class CacheMapper : ProxyMapper
    {
        Dictionary<Mapping.Entity, InsertStatement> insertEntities;
        Dictionary<Mapping.Entity, UpdateStatement> updateEntities;

        public CacheMapper(IMapper mapper)
            : base(mapper)
        {
            insertEntities = new Dictionary<Mapping.Entity, InsertStatement>();
            updateEntities = new Dictionary<Mapping.Entity, UpdateStatement>();
        }

        public override InsertStatement Create(Mapping.Entity entity)
        {
            if (insertEntities.ContainsKey(entity))
                return insertEntities[entity];

            InsertStatement insert=base.Create(entity);
            insertEntities.Add(entity, insert);
            return insert;
        }
    }
}
