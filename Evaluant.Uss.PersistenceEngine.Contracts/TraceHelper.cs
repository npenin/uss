using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Evaluant.Uss.PersistenceEngine.Contracts.Instrumentation
{
    [DebuggerStepThrough]
    public static class TraceHelper
    {
        static Dictionary<string, TraceSource> traceSources = new Dictionary<string, TraceSource>();
        static List<TraceListener> listeners = new List<TraceListener>();

        static TraceHelper()
        {
            AddSource("Evaluant.Uss.NLinq");
        }

        [Conditional("TRACE")]
        public static void AddSource(string sourceName)
        {
            TraceSource source = new TraceSource(sourceName);
            source.Switch = new SourceSwitch(sourceName, SourceLevels.Information.ToString());
            traceSources.Add(sourceName, source);
            foreach (TraceListener item in listeners)
                source.Listeners.Add(item);
        }

        public static TraceSource GetSource(string sourceName)
        {
            TraceSource traceSource;
            if (!traceSources.TryGetValue(sourceName, out traceSource))
            {
                AddSource(sourceName);
                return traceSources[sourceName];
            }
            else
                return traceSource;
        }

        [Conditional("TRACE")]
        public static void AddListener(TraceListener listener)
        {
            if (listeners.Contains(listener))
                return;
            listeners.Add(listener);
            foreach (KeyValuePair<string, TraceSource> traceSource in traceSources)
                traceSource.Value.Listeners.Add(listener);
        }

        [Conditional("TRACE")]
        public static void TraceInformation(string traceSource, string message, params object[] args)
        {
            GetSource(traceSource).TraceInformation(message, args);
        }

        [Conditional("TRACE")]
        public static void TraceEvent(string traceSource, TraceEventType eventType, int id, string message, params object[] args)
        {
            GetSource(traceSource).TraceEvent(eventType, id, message, args);
        }

        [Conditional("TRACE")]
        public static void TraceData(string source, TraceEventType eventType, int id, string data)
        {
            GetSource(source).TraceData(eventType, id, data);
        }
    }
}
