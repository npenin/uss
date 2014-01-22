using System;
using System.Collections.Generic;
using System.IO;

namespace Evaluant.Uss.OData.Operations
{
    internal class Batch : IOperation
    {
        private bool isRoot;
        private IDictionary<string, string> headers;
        public Batch(bool isRoot)
        {
            this.isRoot = isRoot;
            Id = Guid.NewGuid();
            headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "multipart/mixed; boundary=batch_" + Id);

            Operations = new List<IOperation>();
        }

        public Guid Id { get; set; }

        public List<IOperation> Operations { get; set; }

        #region IOperation Members

        public UriExpressions.RootContainerExpression Expression
        {
            get { return new UriExpressions.RootContainerExpression() { Type = "$batch" }; }
            set { throw new NotSupportedException(); }
        }

        public int WriteTo(System.IO.StreamWriter sw, ODataPersistenceEngineAsync engine)
        {
            long position = sw.BaseStream.Position;
            if (!isRoot)
                foreach (KeyValuePair<string, string> header in headers)
                    sw.WriteLine(header.Key + ": " + header.Value);
            MemoryStream ms;
            StreamWriter msw;
            ms = new MemoryStream();
            msw = new StreamWriter(ms);
            msw.NewLine = sw.NewLine;
            int length = 0;
            foreach (IOperation change in Operations)
            {
                length += ("--batch_" + Id + "\n").Length;
                length += change.WriteTo(msw, engine);
            }
            length += ("--batch_" + Id + "--\n").Length;
            msw.Flush();
            if (!isRoot)
            {
                sw.WriteLine("Content-Length: " + length);
                sw.WriteLine();
            }
            sw.Flush();
            foreach (IOperation change in Operations)
            {
                sw.WriteLine("--batch_" + Id);
                sw.Flush();
                change.WriteTo(sw, engine);
            }
            sw.Write("--batch_" + Id + "--");
            //sw.WriteLine();
            sw.Flush();
            return (int)(sw.BaseStream.Length - position);

        }

        public string Method
        {
            get { return "POST"; }
        }

        public IDictionary<string, string> Headers
        {
            get { return headers; }
        }

        

        #endregion

        public void Complete(Model.Model model)
        {
            foreach (var item in Operations)
            {
                item.Complete(model);
            }
        }
    }
}
