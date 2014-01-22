namespace Evaluant.Uss.OData.Http
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Browser;
    using System.Threading;
    using System.Net;
    using Evaluant.Uss.PersistenceEngine.Contracts;

    internal sealed class ScriptXmlHttpRequest
    {
        private ScriptObject request = CreateNativeRequest();

        internal void Abort()
        {
            ScriptObject request = this.request;
            if (request != null)
            {
                request.Invoke("abort", new object[0]);
            }
        }

        private static bool CreateInstance(string typeName, object arg, out ScriptObject request)
        {
            request = null;
            try
            {
                object[] args = (arg == null) ? null : new object[] { arg };
                request = HtmlPage.Window.CreateInstance(typeName, args);
            }
            catch (Exception exception)
            {
                if (DoNotHandleException(exception))
                {
                    throw;
                }
            }
            return (null != request);
        }

        internal static bool DoNotHandleException(Exception ex)
        {
            if (ex == null)
            {
                return false;
            }
            return (((ex is StackOverflowException) || (ex is OutOfMemoryException)) || (ex is ThreadAbortException));
        }

        private static ScriptObject CreateNativeRequest()
        {
            ScriptObject obj2;
            if (((!CreateInstance("XMLHttpRequest", null, out obj2) && !CreateInstance("ActiveXObject", "MSXML2.XMLHTTP.6.0", out obj2)) && (!CreateInstance("ActiveXObject", "MSXML2.XMLHTTP.3.0", out obj2) && !CreateInstance("ActiveXObject", "MSXML2.XMLHTTP.2.0", out obj2))) && ((!CreateInstance("ActiveXObject", "MSXML2.XMLHTTP", out obj2) && !CreateInstance("ActiveXObject", "XMLHttpRequest", out obj2)) && !CreateInstance("ActiveXObject", "Microsoft.XMLHTTP", out obj2)))
            {
                throw WebException.CreateInternal("ScriptXmlHttpRequest.CreateNativeRequest");
            }
            return obj2;
        }

        public void Dispose()
        {
            ScriptObject request = this.request;
            if (request != null)
            {
                try
                {
                    ScriptObjectUtility.SetReadyStateChange(request, null);
                }
                finally
                {
                    this.request = null;
                }
            }
        }

        public string GetResponseHeaders()
        {
            string str = (string) this.request.Invoke("getAllResponseHeaders", new object[0]);
            if (string.IsNullOrEmpty(str))
            {
                return string.Empty;
            }
            int index = str.IndexOf(':');
            int num2 = str.IndexOf('\n');
            if (index > num2)
            {
                str = str.Substring(num2 + 1);
            }
            if (str.IndexOf("\r\n", StringComparison.Ordinal) == -1)
            {
                str = str.Replace("\n", "\r\n");
            }
            if (str.EndsWith("\r\n\r\n", StringComparison.Ordinal))
            {
                return str;
            }
            if (!str.EndsWith("\r\n", StringComparison.Ordinal))
            {
                return (str + "\r\n\r\n");
            }
            return (str + "\r\n");
        }

        public void GetResponseStatus(out int statusCode)
        {
            try
            {
                statusCode = Convert.ToInt32((double) this.request.GetProperty("status"));
            }
            catch (Exception exception)
            {
                throw new WebException("ScriptXmlHttpRequest.HttpWebRequest", exception);
            }
        }

        public void Open(string uri, string method, System.Action readyStateChangeCallback)
        {
            Util.CheckArgumentNull<string>(uri, "uri");
            Util.CheckArgumentNull<string>(method, "method");
            Util.CheckArgumentNull<System.Action>(readyStateChangeCallback, "readyStateChangeCallback");
            ScriptObject callback = ScriptObjectUtility.ToScriptFunction(readyStateChangeCallback);
            ScriptObjectUtility.CallOpen(this.request, method, uri);
            ScriptObjectUtility.SetReadyStateChange(this.request, callback);
        }

        public string ReadResponseAsString()
        {
            return (string) this.request.GetProperty("responseText");
        }

        public void Send(string content)
        {
            object[] args = new object[] { content ?? string.Empty };
            this.request.Invoke("send", args);
        }

        public void SetRequestHeader(string header, string value)
        {
            this.request.Invoke("setRequestHeader", new object[] { header, value });
        }

        internal bool IsCompleted
        {
            get
            {
                if (this.request != null)
                {
                    return (Convert.ToInt32((double) this.request.GetProperty("readyState")) == 4);
                }
                return true;
            }
        }
    }
}

