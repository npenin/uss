using System;
using System.Collections.Generic;
using System.IO;

namespace Evaluant.Uss.OData.Operations
{
    internal class Changeset : IOperation
    {
        private bool isRoot;
        private IDictionary<string, string> headers;
        public Changeset(bool isRoot)
        {
            this.isRoot = isRoot;
            Id = Guid.NewGuid();
            headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "multipart/mixed; boundary=changeset_" + Id);
            Changes = new List<Change>();
        }

        public Guid Id { get; set; }

        public List<Change> Changes { get; set; }

        #region IOperation Members

        public UriExpressions.RootContainerExpression Expression { get; set; }

        public int WriteTo(System.IO.StreamWriter sw, ODataPersistenceEngineAsync engine)
        {
            long position = sw.BaseStream.Position;
            if (!isRoot)
            {
                foreach (KeyValuePair<string, string> header in headers)
                    sw.WriteLine(header.Key + ": " + header.Value);
            }
            //MemoryStream ms;
            //StreamWriter msw;
            //ms = new MemoryStream();
            //msw = new StreamWriter(ms);
            //msw.NewLine = sw.NewLine;
            //int length = 0;
            //foreach (Change change in Changes)
            //{
            //    length += ("--changeset_" + Id + "\n").Length;
            //    length += ("Content-Type: application/http" + "\n").Length;
            //    if (change.Expression.Name != null && change.Expression.Name[0] == '$')
            //        length += (change.Method + " " + engine.GetUri(change.Expression).AbsolutePath.Substring(((ODataPersistenceProvider)engine.FactoryAsync).ConnectionString.AbsolutePath.Length) + " HTTP/1.1").Length + sw.NewLine.Length;
            //    else
            //        length += (change.Method + " " + engine.GetUri(change.Expression) + " HTTP/1.1").Length + sw.NewLine.Length;
            //    length += ("Host: " + ((ODataPersistenceProvider)engine.FactoryAsync).ConnectionString.Host + (((ODataPersistenceProvider)engine.FactoryAsync).ConnectionString.Port == 80 ? "" : ":" + ((ODataPersistenceProvider)engine.FactoryAsync).ConnectionString.Port) + "\n").Length;
            //    length += change.WriteTo(msw, engine);
            //}
            //length += ("--changeset_" + Id + "--\n").Length;
            //length += 2;
            //sw.WriteLine("Content-Length: " + length);
            sw.WriteLine();
            sw.Flush();
            //msw.Close();
            foreach (Change change in Changes)
            {
                sw.WriteLine("--changeset_" + Id);
                sw.WriteLine("Content-Type: application/http");
                sw.WriteLine("Content-Transfer-Encoding: binary");
                sw.WriteLine();
                if (change.Expression.Name != null && change.Expression.Name[0] == '$')
                    sw.WriteLine(change.Method + " " + engine.GetUri(change.Expression).AbsolutePath.Substring(((ODataPersistenceProvider)engine.FactoryAsync).ConnectionString.AbsolutePath.Length ) + " HTTP/1.1");
                else
                    sw.WriteLine(change.Method + " " + engine.GetUri(change.Expression) + " HTTP/1.1");
                sw.WriteLine("Host: " + ((ODataPersistenceProvider)engine.FactoryAsync).ConnectionString.Host + (((ODataPersistenceProvider)engine.FactoryAsync).ConnectionString.Port == 80 ? "" : ":" + ((ODataPersistenceProvider)engine.FactoryAsync).ConnectionString.Port));
                sw.Flush();
                change.WriteTo(sw, engine);
                //sw.WriteLine();
                sw.WriteLine();
            }
            sw.WriteLine("--changeset_" + Id + "--");
            sw.WriteLine();
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

        #region IOperation Members


        public void Complete(Model.Model model)
        {
            foreach (var item in Changes)
            {
                item.Complete(model);
            }
        }

        #endregion
    }
}
