using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;

namespace Evaluant.Uss.WCFService.MessageContracts
{
    [MessageContract]
    public class LoadScalarRequest
    {
        [MessageHeader]
        public string ExpectedType;

        [MessageHeader]
        public int First;

        [MessageHeader]
        public int Max;

        [MessageBodyMember]
        public string Query;
    }
}
