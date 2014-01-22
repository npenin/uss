using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;

namespace Evaluant.Uss.WCFService.MessageContracts
{
    [MessageContract]
    public class LoadScalarResponse
    {
        [MessageHeader]
        public string returnType;

        [MessageBodyMember]
        public object result;
    }
}
