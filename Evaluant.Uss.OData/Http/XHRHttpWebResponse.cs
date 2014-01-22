using System;
using System.Globalization;
using System.Text;
using System.IO;
using System.Windows.Browser;

namespace Evaluant.Uss.OData.Http
{
    internal sealed class XHRHttpWebResponse : HttpWebResponse
    {
        // Fields
        private XHRWebHeaderCollection headers;
        private XHRHttpWebRequest request;
        private int statusCode;

        // Methods
        internal XHRHttpWebResponse(XHRHttpWebRequest request, int statusCode, string responseHeaders)
        {
            this.request = request;
            NormalizeResponseStatus(ref statusCode);
            this.statusCode = statusCode;
            this.headers = new XHRWebHeaderCollection();
            this.ParseHeaders(responseHeaders);
        }

        public override void Close()
        {
            this.request.Close();
        }

        protected override void Dispose(bool disposing)
        {
            this.Close();
        }

        public override string GetResponseHeader(string headerName)
        {
            return this.headers[headerName];
        }

        public override Stream GetResponseStream()
        {
            return this.request.ReadResponse(this);
        }

        private static void NormalizeResponseStatus(ref int statusCodeParam)
        {
            string str = HtmlPage.BrowserInformation.UserAgent;
            if ((str != null) && str.ToUpper(CultureInfo.InvariantCulture).Contains("MSIE"))
            {
                if (statusCodeParam == 1223)
                {
                    statusCodeParam = 204;
                }
                else if (statusCodeParam == 12150)
                {
                    statusCodeParam = 399;
                }
            }
            if ((statusCodeParam > 599) || (statusCodeParam < 100))
            {
                throw WebException.CreateInternal("HttpWebResponse.NormalizeResponseStatus");
            }
        }

        private void ParseHeaders(string responseHeaders)
        {
            if (!string.IsNullOrEmpty(responseHeaders))
            {
                byte[] bytes = Encoding.UTF8.GetBytes(responseHeaders);
                WebParseError parseError = new WebParseError();
                int totalResponseHeadersLength = 0;
                int unparsed = 0;
                int maximumResponseHeadersLength = 64000;
                try
                {
                    if (this.headers.ParseHeaders(bytes, bytes.Length, ref unparsed, ref totalResponseHeadersLength, maximumResponseHeadersLength, ref parseError) != DataParseStatus.Done)
                    {
                        throw WebException.CreateInternal("HttpWebResponse.ParseHeaders");
                    }
                }
                catch (WebException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    throw new WebException("HttpWebResponse.ParseHeaders.2", exception);
                }
            }
        }

        // Properties
        public override long ContentLength
        {
            get
            {
                return Convert.ToInt64(this.Headers["Content-Length"], CultureInfo.InvariantCulture);
            }
        }

        public override string ContentType
        {
            get
            {
                return this.Headers["Content-Type"];
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                return this.headers;
            }
        }

        internal XHRHttpWebRequest InternalRequest
        {
            set
            {
                this.request = value;
            }
        }

        public override HttpWebRequest Request
        {
            get
            {
                return this.request;
            }
        }

        public override HttpStatusCode StatusCode
        {
            get
            {
                return (HttpStatusCode)this.statusCode;
            }
        }
    }
}

