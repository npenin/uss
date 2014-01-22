using System;

namespace Evaluant.Uss.OData.Edm
{
    public class Property
    {
        public string Name { get; set; }

        public string EdmType { get; set; }

        public string Type { get; set; }

        public bool IsNavigationProperty { get; set; }

        public bool IsId { get; set; }
    }
}
