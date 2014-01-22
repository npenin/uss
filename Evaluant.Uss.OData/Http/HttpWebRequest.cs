using System;

namespace Evaluant.Uss.OData.Http
{
    internal abstract class HttpWebRequest : WebRequest, IDisposable
    {
        // Methods
        protected HttpWebRequest()
        {
        }

        public abstract System.Net.WebHeaderCollection CreateEmptyWebHeaderCollection();
        protected abstract void Dispose(bool disposing);
        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        // Properties
        public abstract string Accept { get; set; }

        public abstract bool AllowReadStreamBuffering { get; set; }

        public abstract long ContentLength { set; }

        public abstract System.Net.ICredentials Credentials { get; set; }

        public abstract bool UseDefaultCredentials { get; set; }
    }
}
