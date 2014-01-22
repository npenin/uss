using System;
using System.IO;
using System.Threading;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Globalization;
using System.Text;

namespace Evaluant.Uss.OData.Http
{
    internal sealed class XHRHttpWebRequest : HttpWebRequest
    {
        // Fields
        private bool aborted;
        private HttpWebRequestAsyncResult asyncRequestResult;
        private HttpWebRequestAsyncResult asyncResponseResult;
        private NonClosingMemoryStream contentStream;
        private XHRWebHeaderCollection headers;
        private bool invoked;
        private string method;
        private HttpWebResponse response;
        private ScriptXmlHttpRequest underlyingRequest;
        private Uri uri;

        // Methods
        internal XHRHttpWebRequest(Uri uri)
        {
            this.uri = uri;
        }

        public override void Abort()
        {
            this.aborted = true;
            if (this.underlyingRequest != null)
            {
                this.underlyingRequest.Abort();
                this.underlyingRequest.Dispose();
                this.underlyingRequest = null;
            }
            if (this.response != null)
            {
                ((XHRHttpWebResponse)this.response).InternalRequest = null;
                this.response = null;
            }
            this.Close();
        }

        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            if (this.aborted)
            {
                throw CreateAbortException();
            }
            if (this.contentStream == null)
            {
                this.contentStream = new NonClosingMemoryStream();
            }
            else
            {
                this.contentStream.Seek(0L, SeekOrigin.Begin);
            }
            HttpWebRequestAsyncResult ar = new HttpWebRequestAsyncResult(callback, state);
            this.asyncRequestResult = ar;
            this.asyncRequestResult.CompletedSynchronously = true;
            if (ar != null)
            {
                ar.SetCompleted();
                ar.Callback(ar);
            }
            return ar;
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            HttpWebRequestAsyncResult result = new HttpWebRequestAsyncResult(callback, state);
            try
            {
                result.InsideBegin = true;
                this.asyncResponseResult = result;
                this.InvokeRequest();
            }
            finally
            {
                result.InsideBegin = false;
            }
            return result;
        }

        internal void Close()
        {
            this.Dispose(true);
        }

        private static WebException CreateAbortException()
        {
            return new WebException("Aborted");
        }

        public override System.Net.WebHeaderCollection CreateEmptyWebHeaderCollection()
        {
            return System.Net.WebRequest.Create(this.RequestUri).Headers;
        }

        private void CreateResponse()
        {
            int num;
            this.underlyingRequest.GetResponseStatus(out num);
            if (num != -1)
            {
                string responseHeaders = this.underlyingRequest.GetResponseHeaders();
                this.response = new XHRHttpWebResponse(this, num, responseHeaders);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.contentStream != null))
            {
                this.contentStream.InternalDispose();
                this.contentStream = null;
            }
        }

        public override Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (this.asyncRequestResult != asyncResult)
            {
                throw new InvalidOperationException("HttpWebRequest.EndGetRequestStream");
            }
            if (this.asyncRequestResult.EndCalled)
            {
                throw new InvalidOperationException("HttpWebRequest.EndGetRequestStream.2");
            }
            if (this.aborted)
            {
                throw CreateAbortException();
            }
            this.asyncRequestResult.EndCalled = true;
            this.asyncRequestResult.Dispose();
            this.asyncRequestResult = null;
            return this.contentStream;
        }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (this.asyncResponseResult != asyncResult)
            {
                throw new InvalidOperationException("HttpWebRequest.EndGetResponse");
            }
            if (this.asyncResponseResult.EndCalled)
            {
                throw new InvalidOperationException("HttpWebRequest.EndGetResponse.2");
            }
            if (this.aborted)
            {
                throw CreateAbortException();
            }
            this.asyncResponseResult.EndCalled = true;
            this.CreateResponse();
            this.asyncResponseResult.Dispose();
            this.asyncResponseResult = null;
            return this.response;
        }

        private void InvokeRequest()
        {
            if (this.aborted)
            {
                throw CreateAbortException();
            }
            if (this.invoked)
            {
                throw new InvalidOperationException("HttpWebRequest.InvokeRequest");
            }
            this.invoked = true;
            this.underlyingRequest = new ScriptXmlHttpRequest();
            this.underlyingRequest.Open(this.uri.AbsoluteUri, this.Method, new System.Action(ReadyStateChanged));
            if ((this.headers != null) && (this.headers.Count != 0))
            {
                foreach (string str in this.headers.AllKeys)
                {
                    string str2 = this.headers[str];
                    this.underlyingRequest.SetRequestHeader(str, str2);
                }
            }
            string content = null;
            if (this.contentStream != null)
            {
                byte[] bytes = this.contentStream.GetBuffer();
                if (bytes != null)
                {
                    int position = (int)this.contentStream.Position;
                    content = Encoding.UTF8.GetString(bytes, 0, position);
                    this.underlyingRequest.SetRequestHeader("content-length", position.ToString(CultureInfo.InvariantCulture));
                }
            }
            this.underlyingRequest.Send(content);
        }

        public static bool IsAvailable()
        {
            try
            {
                ScriptXmlHttpRequest request = new ScriptXmlHttpRequest();
                return (null != request);
            }
            catch (WebException)
            {
                return false;
            }
        }

        internal Stream ReadResponse(IDisposable connection)
        {
            if (((this.response.ContentType != null) && !this.response.ContentType.Contains("json")) && ((!this.response.ContentType.Contains("xml") && !this.response.ContentType.Contains("text")) && !this.response.ContentType.Contains("multipart")))
            {
                throw WebException.CreateInternal("HttpWebRequest.ReadResponse");
            }
            string str = this.underlyingRequest.ReadResponseAsString();
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            return new DisposingMemoryStream(connection, Encoding.UTF8.GetBytes(str));
        }

        private void ReadyStateChanged()
        {
            if (this.underlyingRequest.IsCompleted && (this.asyncResponseResult != null))
            {
                try
                {
                    if (this.asyncResponseResult.InsideBegin)
                    {
                        this.asyncResponseResult.CompletedSynchronously = true;
                    }
                    this.asyncResponseResult.SetCompleted();
                    this.asyncResponseResult.Callback(this.asyncResponseResult);
                }
                finally
                {
                    this.underlyingRequest.Dispose();
                }
            }
        }

        // Properties
        public override string Accept
        {
            get
            {
                return this.headers[HttpRequestHeader.Accept];
            }
            set
            {
                this.headers.SetSpecialHeader("accept", value);
            }
        }

        public override bool AllowReadStreamBuffering
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        public override long ContentLength
        {
            set
            {
                this.headers[HttpRequestHeader.ContentLength] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public override string ContentType
        {
            get
            {
                return this.headers[HttpRequestHeader.ContentType];
            }
            set
            {
                this.headers.SetSpecialHeader("content-type", value);
            }
        }

        public override System.Net.ICredentials Credentials
        {
            get
            {
                return null;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                if (this.headers == null)
                {
                    this.headers = new XHRWebHeaderCollection(WebHeaderCollectionType.HttpWebRequest);
                }
                return this.headers;
            }
        }

        public override string Method
        {
            get
            {
                return this.method;
            }
            set
            {
                this.method = value;
            }
        }

        public override Uri RequestUri
        {
            get
            {
                return this.uri;
            }
        }

        public override bool UseDefaultCredentials
        {
            get
            {
                return true;
            }
            set
            {
                if (!value)
                {
                    throw new NotSupportedException();
                }
            }
        }

        // Nested Types
        private sealed class DisposingMemoryStream : MemoryStream
        {
            // Fields
            private readonly IDisposable disposable;

            // Methods
            internal DisposingMemoryStream(IDisposable disposable, byte[] buffer)
                : base(buffer)
            {
                this.disposable = disposable;
            }

            protected override void Dispose(bool disposing)
            {
                this.disposable.Dispose();
                base.Dispose(disposing);
            }
        }

        private sealed class HttpWebRequestAsyncResult : IAsyncResult, IDisposable
        {
            // Fields
            private AsyncCallback callback;
            private bool completed;
            private bool completedSynchronously;
            private bool endCalled;
            private object state;
            private ManualResetEvent waitHandle;

            // Methods
            public HttpWebRequestAsyncResult(AsyncCallback callback, object state)
            {
                this.callback = callback;
                this.state = state;
            }

            public void Dispose()
            {
                if (this.waitHandle != null)
                {
                    ((IDisposable)this.waitHandle).Dispose();
                }
            }

            public void SetCompleted()
            {
                this.completed = true;
                if (this.waitHandle != null)
                {
                    this.waitHandle.Set();
                }
            }

            // Properties
            public object AsyncState { get { return this.state; } }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    if (this.waitHandle == null)
                    {
                        this.waitHandle = new ManualResetEvent(false);
                    }
                    return this.waitHandle;
                }
            }

            public AsyncCallback Callback { get { return this.callback; } }

            public bool CompletedSynchronously
            {
                get { return this.completedSynchronously; }
                internal set { this.completedSynchronously = value; }
            }

            public bool EndCalled
            {
                get { return this.endCalled; }
                set { this.endCalled = value; }
            }

            public bool InsideBegin { get; set; }

            public bool IsCompleted { get { return this.completed; } }
        }

        private sealed class NonClosingMemoryStream : MemoryStream
        {
            // Methods
            public override void Close()
            {
            }

            protected override void Dispose(bool disposing)
            {
            }

            internal void InternalDispose()
            {
                base.Dispose();
            }
        }
    }
}

