using System;
using System.Windows.Browser;

namespace Evaluant.Uss.OData.Http
{
    internal static class ScriptObjectUtility
    {
        private const string HelperScript = "({\r\n    cd: function(f) { return function() { f(); }; },\r\n    callOpen: function(requestObj, method, uri) {\r\n        requestObj.open(method,uri,true);\r\n    },\r\n    setReadyStateChange: function(requestObj, o1) {\r\n        requestObj.onreadystatechange = o1;\r\n    },\r\n    setReadyStateChangeToNull: function(requestObj) {\r\n        try { requestObj.onreadystatechange = null; }\r\n        catch (e) { requestObj.onreadystatechange = new Function(); }\r\n    }\r\n})";
        private static readonly ScriptObject HelperScriptObject = ((ScriptObject) HtmlPage.Window.Eval("({\r\n    cd: function(f) { return function() { f(); }; },\r\n    callOpen: function(requestObj, method, uri) {\r\n        requestObj.open(method,uri,true);\r\n    },\r\n    setReadyStateChange: function(requestObj, o1) {\r\n        requestObj.onreadystatechange = o1;\r\n    },\r\n    setReadyStateChangeToNull: function(requestObj) {\r\n        try { requestObj.onreadystatechange = null; }\r\n        catch (e) { requestObj.onreadystatechange = new Function(); }\r\n    }\r\n})"));

        internal static void CallOpen(ScriptObject request, string method, string uri)
        {
            HelperScriptObject.Invoke("callOpen", new object[] { request, method, uri });
        }

        internal static void SetReadyStateChange(ScriptObject request, ScriptObject callback)
        {
            if (callback == null)
            {
                HelperScriptObject.Invoke("setReadyStateChangeToNull", new object[] { request });
            }
            else
            {
                HelperScriptObject.Invoke("setReadyStateChange", new object[] { request, callback });
            }
        }

        internal static ScriptObject ToScriptFunction(Delegate d)
        {
            return (ScriptObject) HelperScriptObject.Invoke("cd", new object[] { d });
        }
    }
}

