using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlMapper.Mapper
{
    public class IdMapperOption : IMapperOption
    {
        public string EntityType { get; set; }

        public string IdPropertyName { get; set; }
    }
}
