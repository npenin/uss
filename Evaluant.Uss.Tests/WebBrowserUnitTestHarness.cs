using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Silverlight.Testing.Harness;

namespace Evaluant.Uss.Tests
{
    public class AsyncUnitTestHarness : UnitTestHarness
    {
        public override void RestartRunDispatcher()
        {
            RunDispatcher = new EventRunDispatcher(RunNextStep, Dispatcher);
            RunDispatcher.Complete += new EventHandler(DispatchRunDispatcherComplete);
            RunDispatcher.Run();
        }

        private void DispatchRunDispatcherComplete(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new EventHandler(RunDispatcherComplete), sender, e);
        }

        protected override void OnTestMethodCompleted(TestMethodCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action<TestMethodCompletedEventArgs>(base.OnTestMethodCompleted), e);
        }

        public EventRunDispatcher EventRunDispatcher { get { return (EventRunDispatcher)RunDispatcher; } }
    }
}
