using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace Evaluant.Uss.Utility
{
    public class DelegateCaller : IAsyncResult
    {
        private AsyncCallback callback;
        private object result;
        private Exception error;
        public DelegateCaller(AsyncCallback callback)
        {
            this.callback = callback;
        }

        public static T Invoke<T>(Delegate @delegate, object[] parameters)
        {
            return (T)@delegate.DynamicInvoke(parameters);
        }

        public static void BeginInvoke(Delegate @delegate, object[] parameters, AsyncCallback callback, object asyncState)
        {
            new DelegateCaller(callback).BeginInvoke(@delegate, parameters, asyncState);
        }

        public static void BeginInvoke<T>(Delegate @delegate, object[] parameters, AsyncCallback callback, object asyncState)
        {
            new DelegateCaller(callback).BeginInvoke(@delegate, parameters, asyncState);
        }

        private void BeginInvoke(Delegate @delegate, object[] parameters, object asyncState)
        {
            AsyncState = asyncState;
            Task t = new Task(new Action<object>(delegate
            {
                try
                {
                    result = @delegate.DynamicInvoke(parameters);
                    IsCompleted = true;
                }
                catch (TargetInvocationException ex)
                {
                    error = ex.InnerException;
                }

            }), asyncState);
            t.ContinueWith((task) => callback(this));
            t.Start();
        }

        public static void EndInvoke(IAsyncResult result)
        {
        }

        public static T EndInvoke<T>(IAsyncResult result)
        {
            if (((DelegateCaller)result).error != null)
                throw new Exception("An error has occurred", ((DelegateCaller)result).error);
            return (T)((DelegateCaller)result).result;
        }

        public static Exception GetLastError(IAsyncResult result)
        {
            return ((DelegateCaller)result).error;
        }

        public static void ClearLastError(IAsyncResult result)
        {
            ((DelegateCaller)result).error = null;
        }

        #region IAsyncResult Members

        public object AsyncState
        {
            get;
            set;
        }

        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get;
            set;
        }

        public bool CompletedSynchronously
        {
            get;
            set;
        }

        public bool IsCompleted
        {
            get;
            set;
        }

        #endregion
    }
}
