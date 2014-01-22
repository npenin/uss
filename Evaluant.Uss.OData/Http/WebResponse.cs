using System;
using System.IO;

namespace Evaluant.Uss.OData.Http
{
    internal abstract class WebResponse : IDisposable
    {
        // Methods
        protected WebResponse()
        {
        }

        public abstract void Close();
        protected abstract void Dispose(bool disposing);
        public abstract Stream GetResponseStream();
        void IDisposable.Dispose()
        {
            this.Dispose(false);
        }

        // Properties
        public abstract long ContentLength { get; }

        public abstract string ContentType { get; }
    }


}
