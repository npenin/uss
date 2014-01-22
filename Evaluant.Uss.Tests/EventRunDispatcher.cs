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
using Microsoft.Silverlight.Testing.Harness;
using System.Windows.Threading;

namespace Evaluant.Uss.Tests
{
    public class EventRunDispatcher : RunDispatcher
    {
        private Dispatcher dispatcher;
        public EventRunDispatcher(Func<bool> runNextStep, Dispatcher dispatcher)
            : base(runNextStep)
        {
            this.dispatcher = dispatcher;
        }

        public void DispatchRun()
        {
            dispatcher.BeginInvoke(Run);
        }

        public override void Run()
        {
            if (!(IsRunning || RunNextStep()))
            {
                OnComplete();
            }
        }
    }
}
