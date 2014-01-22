using System;

namespace Evaluant.Uss.OData.Http
{
    internal abstract class HttpWebResponse : WebResponse, IDisposable
    {
        // Methods
        protected HttpWebResponse()
        {
        }

        public abstract string GetResponseHeader(string headerName);

        // Properties
        public abstract WebHeaderCollection Headers { get; }

        public abstract HttpWebRequest Request { get; }

        public abstract HttpStatusCode StatusCode { get; }
    }

 

}
