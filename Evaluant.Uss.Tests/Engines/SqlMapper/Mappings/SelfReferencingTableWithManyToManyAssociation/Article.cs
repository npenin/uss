using System;
using System.Collections.Generic;
using System.Text;

namespace SelfReferencingTableWithManyToManyAssociation
{
    public class Article : Media
    {
        public string ArticleContent { get; set; }
    }
}
