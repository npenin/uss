using System;
using System.Collections.Generic;
using System.Text;
//using Evaluant.Uss.Mapping;
using System.Xml.Serialization;
using Evaluant.Uss.SqlExpressions.Mapping;

namespace Evaluant.Uss.SqlMapper.Mapping
{
    public class Embed : Entity
    {
        protected override Embed ToEmbed(bool needsModel)
        {
            return this;
        }
    }
}
