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
    public class ActionEventWorkItem : WorkItem
    {
        public ActionEventWorkItem()
        {
            CanExecuteImmediately = false;
        }

        public override bool Invoke()
        {
            return base.Invoke();
        }

        public void Callback()
        {
            CanExecuteImmediately = true;
            IsComplete = true;

        }
    }
}
