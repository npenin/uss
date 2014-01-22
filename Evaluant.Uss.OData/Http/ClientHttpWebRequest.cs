namespace Evaluant.Uss.OData.Http
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Browser;

    internal sealed class ClientHttpWebRequest : Evaluant.Uss.OData.Http.HttpWebRequest
    {
        private ClientWebHeaderCollection headerCollection;
        private readonly System.Net.HttpWebRequest innerRequest;

        public ClientHttpWebRequest(Uri requestUri)
        {
            this.innerRequest = (System.Net.HttpWebRequest) WebRequestCreator.ClientHttp.Create(requestUri);
        }

        public override void Abort()
        {
            this.innerRequest.Abort();
        }

        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            return this.innerRequest.BeginGetRequestStream(callback, state);
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            return this.innerRequest.BeginGetResponse(callback, state);
        }

        public override System.Net.WebHeaderCollection CreateEmptyWebHeaderCollection()
        {
            return new System.Net.WebHeaderCollection();
        }

        protected override void Dispose(bool disposing)
        {
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return this.innerRequest.EndGetRequestStream(asyncResult);
        }

        public override Http.WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            Http.WebResponse response3;
            try
            {
                System.Net.HttpWebResponse innerResponse = (System.Net.HttpWebResponse) this.innerRequest.EndGetResponse(asyncResult);
                response3 = new Http.ClientHttpWebResponse(innerResponse, this);
            }
            catch (System.Net.WebException exception)
            {
                Http.ClientHttpWebResponse response = new Http.ClientHttpWebResponse((System.Net.HttpWebResponse) exception.Response, this);
                throw new Http.WebException(exception.Message, exception, response);
            }
            return response3;
        }

        public override string Accept
        {
            get
            {
                return this.innerRequest.Accept;
            }
            set
            {
                this.innerRequest.Accept = value;
            }
        }

        public override bool AllowReadStreamBuffering
        {
            get
            {
                return this.innerRequest.AllowReadStreamBuffering;
            }
            set
            {
                this.innerRequest.AllowReadStreamBuffering = value;
            }
        }

        public override long ContentLength
        {
            set
            {
            }
        }

        public override string ContentType
        {
            get
            {
                return this.innerRequest.ContentType;
            }
            set
            {
                this.innerRequest.ContentType = value;
            }
        }

        public override System.Net.ICredentials Credentials
        {
            get
            {
                return this.innerRequest.Credentials;
            }
            set
            {
                this.innerRequest.Credentials = value;
            }
        }

        public override Http.WebHeaderCollection Headers
        {
            get
            {
                if (this.headerCollection == null)
                {
                    this.headerCollection = new ClientWebHeaderCollection(this.innerRequest.Headers, this.innerRequest);
                }
                return this.headerCollection;
            }
        }

        public override string Method
        {
            get
            {
                return this.innerRequest.Method;
            }
            set
            {
                this.innerRequest.Method = value;
            }
        }

        public override Uri RequestUri
        {
            get
            {
                return this.innerRequest.RequestUri;
            }
        }

        public override bool UseDefaultCredentials
        {
            get
            {
                return false;
            }
            set
            {
            }
        }
    }
}

