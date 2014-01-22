using System;
using System.IO;
using System.Windows.Browser;

namespace Evaluant.Uss.OData.Http
{
    internal abstract class WebRequest
    {
        // Methods
        protected WebRequest()
        {
        }

        public abstract void Abort();
        public abstract IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state);
        public abstract IAsyncResult BeginGetResponse(AsyncCallback callback, object state);
        public static WebRequest Create(Uri requestUri, HttpStack httpStack)
        {
            if ((requestUri.Scheme != Uri.UriSchemeHttp) && (requestUri.Scheme != Uri.UriSchemeHttps))
            {
                throw new NotSupportedException();
            }
            if (httpStack == HttpStack.Auto)
            {
                if (UriRequiresClientHttpWebRequest(requestUri))
                {
                    httpStack = HttpStack.ClientHttp;
                }
                else
                {
                    httpStack = HttpStack.XmlHttp;
                }
            }
            if (httpStack == HttpStack.ClientHttp)
            {
                return new ClientHttpWebRequest(requestUri);
            }
            return new XHRHttpWebRequest(requestUri);
        }

        public abstract Stream EndGetRequestStream(IAsyncResult asyncResult);
        public abstract WebResponse EndGetResponse(IAsyncResult asyncResult);
        private static bool UriRequiresClientHttpWebRequest(Uri uri)
        {
            if (XHRHttpWebRequest.IsAvailable())
            {
                Uri uri2 = HtmlPage.Document.DocumentUri;
                if ((!(uri2.Scheme != uri.Scheme) && (uri2.Port == uri.Port)) && string.Equals(uri2.DnsSafeHost, uri.DnsSafeHost, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }

        // Properties
        public abstract string ContentType { get; set; }

        public abstract WebHeaderCollection Headers { get; }

        public abstract string Method { get; set; }

        public abstract Uri RequestUri { get; }
    }
}
