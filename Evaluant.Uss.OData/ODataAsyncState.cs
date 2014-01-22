using System;
using System.Collections.Generic;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.OData
{
    public class ODataAsyncState
    {
        public ODataAsyncState(WebRequest request, object asyncState)
        {
            this.Request = request;
            this.CustomAsyncState = asyncState;
        }   
        public WebRequest Request { get; set; }
        public IEnumerable<Entity> Entities { get; set; }
        public object CustomAsyncState { get; set; }
    }
}
