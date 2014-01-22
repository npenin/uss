using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.MongoDB
{
    public class DynamicEntity : Domain.Entity
    {
        public DynamicEntity()
            : base("")
        {

        }

        public new string Type
        {
            get { return base.Type; }
            set { base.Type = value; }
        }
    }
}
