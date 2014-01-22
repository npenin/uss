using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.OData
{
    class RequestState
    {
        public Http.HttpWebRequest Request { get; set; }
        public object AsyncState { get; set; }

        public RequestState(Http.HttpWebRequest httpWebRequest, object asyncState)
        {
            // TODO: Complete member initialization
            this.Request = httpWebRequest;
            this.AsyncState = asyncState;
        }
    }
}
