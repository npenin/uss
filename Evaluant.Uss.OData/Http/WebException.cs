namespace Evaluant.Uss.OData.Http
{
    using System;

    internal sealed class WebException : InvalidOperationException
    {
        private HttpWebResponse response;

        public WebException()
        {
        }

        public WebException(string message) : this(message, null)
        {
        }

        public WebException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WebException(string message, Exception innerException, HttpWebResponse response) : base(message, innerException)
        {
            this.response = response;
        }

        internal static WebException CreateInternal(string location)
        {
            return new WebException(location);
        }

        public HttpWebResponse Response
        {
            get
            {
                return this.response;
            }
        }
    }
}

