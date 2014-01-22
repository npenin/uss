using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;

namespace Evaluant.Uss.Tests
{
    public class AsyncWorkItem : WorkItem
    {
        public AsyncWorkItem()
        {
            CanExecuteImmediately = false;
        }

        private AsyncCallback callback;
        public AsyncWorkItem(AsyncCallback callback, Microsoft.Silverlight.Testing.Action runDispatcher)
        {
            this.runDispatcher = runDispatcher;
            if (callback == null)
                throw new ArgumentNullException("callback");
            this.callback = callback;
            CanExecuteImmediately = false;
        }

        private IAsyncResult asyncResult;
        private Microsoft.Silverlight.Testing.Action runDispatcher;

        public override bool Invoke()
        {
            if (asyncResult == null && !CanExecuteImmediately)
                return true;
            callback(asyncResult);
            App.Current.RootVisual.Dispatcher.BeginInvoke(WorkItemComplete);
            return false;
        }

        public void SetAsyncResult(IAsyncResult res)
        {
            asyncResult = res;
            CanExecuteImmediately = true;
            runDispatcher();
        }
    }
}
