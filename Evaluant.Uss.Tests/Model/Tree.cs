using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evaluant.Uss.Tests.Model
{
    public class Tree
    {
        public int Id { get; set; }
        public IList<Tree> Children { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        private string parentPath;

        public string ParentPath
        {
            get { return parentPath; }
            set
            {
                parentPath = value;
                Path = value + "/" + Id;

            }
        }

    }
}
