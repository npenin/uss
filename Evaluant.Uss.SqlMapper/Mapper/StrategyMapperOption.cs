using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlMapper.Mapper
{
    public class StrategyMapperOption : IMapperOption
    {
        #region IMapperOption Members

        public string EntityType
        {
            get;
            set;
        }

        #endregion
    }
}
