using System;
using System.Collections.Generic;
using System.Text;

namespace SelfReferencingTableWithManyToManyAssociation
{
    public class Video : Media
    {
        public int Rating { get; set; }
    }
}
