using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Evaluant.Uss.Tests
{
    public class UnitTestListener : TraceListener
    {
        public UnitTestListener()
        {

        }

        public UnitTestListener(TestContext testContext)
        {
            if (!testContext.Properties.Contains("listener"))
                testContext.Properties.Add("listener", this);
        }

        public void NextMessage(string source, string message)
        {
            Queue<string> messages;
            if (nextMessages.ContainsKey(source))
                messages = nextMessages[source];
            else
            {
                messages = new Queue<string>();
                nextMessages.Add(source, messages);
            }
            messages.Enqueue(message);
        }



        private Dictionary<string, Queue<string>> nextMessages = new Dictionary<string, Queue<string>>();

        private string NextMessage(string source)
        {
            if (nextMessages.ContainsKey(source))
            {
                Queue<string> messages = nextMessages[source];
                if (messages != null && messages.Count > 0)
                    return nextMessages[source].Dequeue();
            }
            return null;
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            if (Filter != null && Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
                EnsureCorrectMessage(source, data);
            base.TraceData(eventCache, source, eventType, id, data);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            if (Filter != null && Filter.ShouldTrace(eventCache, source, eventType, id, format, args, null, null))
                EnsureCorrectMessage(source, string.Format(format, args));
            base.TraceEvent(eventCache, source, eventType, id, format, args);
        }

        private void EnsureCorrectMessage(string source, object message)
        {
            string nextMessage = NextMessage(source);
            if (!string.IsNullOrEmpty(nextMessage))
                Assert.AreEqual(nextMessage, message.ToString());

        }



        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
        }
    }

    public delegate TResult Func<TResult>();
    public delegate TResult Func<T1, TResult>(T1 param1);
    public delegate TResult Func<T1, T2, TResult>(T1 param1, T2 param2);
    public delegate TResult Func<T1, T2, T3, TResult>(T1 param1, T2 param2, T3 param3);

    public class EventIdFilter : TraceFilter
    {
        public EventIdFilter(Func<int, bool> operation)
        {
            test = operation;
        }

        Func<int, bool> test;

        public override bool ShouldTrace(TraceEventCache cache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
        {
            return test(id);
        }
    }
}
