using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.WCFService.Surrogates
{
    [DataContract]
    public class MultipleEntry
    {
        public string Name { get; set; }
        public List<Entry<Entity>> Entities { get; set; }
    }
}
