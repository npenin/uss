using System;
using System.Collections.Generic;
using System.Text;

namespace SelfReferencingTableWithManyToManyAssociation
{
    public class MediaCategory
    {
        public string CategoryId { get; set; }
        public string Name { get; set; }

        public IList<MediaCategory> SubCategories { get; set; }
        public IList<Media> Medias { get; set; }
    }
}
