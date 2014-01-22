using System;
using Evaluant.Uss.Era;
using System.Collections.Generic;

namespace Evaluant.Uss.OData.Edm
{
    public class Association
    {
        public AssociationEnd FirstEnd { get; set; }
        public AssociationEnd LastEnd { get; set; }
        public string Name { get; set; }


    }

    public class AssociationEnd
    {
        public string Role { get; set; }
        public string EntitySet { get; set; }

        public string Type { get; set; }
    }
}
