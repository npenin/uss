using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Evaluant.Uss.WCFService.Surrogates
{
    [DataContract]
    public class Command
    {
        [DataMember]
        public Commands.CommandTypes Type { get; protected set; }
    }
}
