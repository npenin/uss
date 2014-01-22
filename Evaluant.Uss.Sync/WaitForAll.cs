using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Evaluant.Uss.Sync
{
    internal class WaitForAll : IAsyncResult
    {
        public WaitForAll(AsyncCallback callbackWhenOver, object asyncState)
        {
            callbackResults = new Dictionary<string, IAsyncResult>();
            this.callbackWhenOver = callbackWhenOver;
            this.asyncState = asyncState;
            handle = new AutoResetEvent(false);
        }

        Dictionary<string, IAsyncResult> callbackResults;
        private AsyncCallback callbackWhenOver;
        private object asyncState;
        private int callsLeft;
        private AutoResetEvent handle;

        public IAsyncResult this[string callbackName]
        {
            get { return callbackResults[callbackName]; }
        }

        public AsyncCallback Callback(string callbackName)
        {
            callbackResults.Add(callbackName, null);
            return (IAsyncResult result) =>
            {
                callbackResults[callbackName] = result;
                lock (this)
                {
                    callsLeft--;
                    if (IsCompleted)
                    {
                        handle.Set();
                        callbackWhenOver(this);
                    }
                }
            };
        }

        #region IAsyncResult Members

        public object AsyncState
        {
            get { return asyncState; }
        }

        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get { return handle; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public bool IsCompleted
        {
            get { return callsLeft == 0; }
        }

        #endregion
    }
}
