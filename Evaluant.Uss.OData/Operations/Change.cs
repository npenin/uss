using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace Evaluant.Uss.OData.Operations
{
    public enum ChangeMode
    {
        Insert,
        Update,
        Delete
    }

    internal class Change : IOperation
    {
        private IDictionary<string, string> headers;
        public Change(ChangeMode mode)
        {
            Mode = mode;

            Entry = new Atom.AtomEntry();
            headers = new Dictionary<string, string>();
            if (Method != "POST")
                headers.Add("X-HTTP-Method", Method);
            //headers.Add("Content-Transfer-Encoding", "binary");
            method = "POST";
        }

        public ChangeMode Mode { get; private set; }
        public Atom.AtomEntry Entry { get; set; }

        #region IOperation Members

        public UriExpressions.RootContainerExpression Expression
        {
            get;
            set;
        }

        #endregion

        #region IOperation Members


        public int WriteTo(StreamWriter s, ODataPersistenceEngineAsync engine)
        {
            MemoryStream ms = new MemoryStream();
            XmlWriter writer = XmlWriter.Create(ms, new XmlWriterSettings() { Encoding = s.Encoding, Indent = true, NewLineChars = s.NewLine });
            if (Link != null)
                headers["Content-Type"] = "application/xml";
            else
                headers["Content-Type"] = "application/atom+xml;type=entry";
            foreach (KeyValuePair<string, string> item in Headers)
            {
                s.WriteLine(item.Key + ": " + item.Value);
            }

            switch (Mode)
            {
                case ChangeMode.Insert:
                    if (Link == null)
                        Entry.Write(writer, engine);
                    else
                        Link.Write(writer, engine);
                    writer.Flush();
                    s.WriteLine("Content-Length: " + (ms.Length + 1));
                    s.WriteLine();
                    s.Flush();
                    ms.Position = 0;
                    Util.Write(ms, s.BaseStream);
                    return (int)ms.Length;
                case ChangeMode.Update:
                    Entry.Write(writer, engine);
                    writer.Flush();
                    s.WriteLine("Content-Length: " + (ms.Length + 1));
                    ms.Position = 0;
                    Util.Write(ms, s.BaseStream);
                    return (int)ms.Length;
                case ChangeMode.Delete:
                    if (Entry != null)
                        Entry.Write(writer, engine);
                    else
                        Link.Write(writer, engine);
                    writer.Flush();
                    break;
                default:
                    break;
            }
            return 0;
        }

        #endregion

        #region IOperation Members

        string method;

        public string Method
        {
            get
            {
                if (method == null)
                    switch (Mode)
                    {
                        case ChangeMode.Insert:
                            return method = "POST";
                        case ChangeMode.Update:
                            return method = "MERGE";
                        case ChangeMode.Delete:
                            return method = "DELETE";
                    }
                return method;
            }
        }

        #endregion

        public Atom.AtomLink Link { get; set; }

        #region IOperation Members


        public System.Collections.Generic.IDictionary<string, string> Headers
        {
            get { return headers; }
        }

        string contentId;

        public string ContentId
        {
            get { return contentId; }
            set { contentId = headers["Content-ID"] = value; }
        }

        #endregion

        public void Complete(Model.Model model)
        {
            if (Link == null)
                Entry.Complete(model);
        }
    }
}
