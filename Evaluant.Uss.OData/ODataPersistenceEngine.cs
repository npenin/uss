using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Net;
using Evaluant.Uss.Domain;
using System.Xml;
using Evaluant.Uss.OData.Visitors;
using Evaluant.Uss.OData.UriExpressions;
using Evaluant.Uss.MetaData;
using System.Reflection;
using System.IO;

namespace Evaluant.Uss.OData
{
    public class ODataPersistenceEngine : PersistenceEngineImplementation
    {
        private Uri serviceUri;
        public ODataPersistenceEngine(ODataPersistenceProvider factory, Uri serviceUri)
            : base(factory)
        {
            this.serviceUri = serviceUri;
        }

        public override IList<Domain.Entity> LoadWithId(string type, string[] id)
        {
            if (id.Length == 1)
            {
                RootContainerExpression expression = new RootContainerExpression();
                expression.Type = type;
                expression.Id = id[0];
                return GetEntities(expression);
            }
            return null;
        }

        private EntitySet ReadEntities(System.IO.Stream stream)
        {
            EntitySet result = new EntitySet();
#if !SILVERLIGHT
            XmlDocument doc = new XmlDocument();
            doc.Load(stream);
            XmlNamespaceManager xmlnsm = new XmlNamespaceManager(doc.NameTable);
            string atom = xmlnsm.LookupPrefix("http://www.w3.org/2005/Atom");
            xmlnsm.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            xmlnsm.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            xmlnsm.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
            foreach (XmlNode entry in doc.SelectNodes("//atom:entry", xmlnsm))
            {
                Entity e = new Entity(entry.SelectSingleNode("atom:link[@rel='edit']/@title", xmlnsm).Value);
                foreach (XmlNode properties in entry.SelectNodes("atom:content/m:properties/d:*", xmlnsm))
                    e.Add(properties.LocalName, properties.InnerText);
                result.Add(e);
                e.State = State.UpToDate;
            }
            stream.Close();
#endif
            return result;
        }



        public override void LoadReference(IEnumerable<Domain.Entity> entities, string[] references)
        {
            RootContainerExpression expression = new RootContainerExpression();
            foreach (Entity e in entities)
            {
                expression.Id = e.Id;
                expression.Type = e.Type;
                foreach (string reference in references)
                {
                    expression.Target = new PropertyExpression();
                    expression.Target.Name = reference;
                    foreach (Entity ee in GetEntities(expression))
                        e.Add(reference, ee);
                }
            }
        }

        public override void ProcessCommands(Transaction tx)
        {
            throw new NotImplementedException();
        }

        protected IDictionary<string, string> Metadata
        {
            get { return ((ODataPersistenceProvider)Factory).Metadata; }
        }

        protected override T LoadSingle<T>(Evaluant.NLinq.NLinqQuery query)
        {
            var visitor = new NLinqVisitor(Metadata);
            return Read<T>((RootContainerExpression)visitor.Visit(query.Expression));

        }

        private T Read<T, U>(Stream stream)
    where T : class, IEnumerable<U>
        {
            List<U> result = new List<U>();

            EntitySet set = ReadEntities(stream);
            foreach (Entity e in set)
            {
                if (TypeResolver.IsPrimitive(typeof(U)))
                {
                    object value = e.EntityEntries[0].Value;
                    U item;
                    if (null == value)
                        item = default(U);
                    else
                        item = (U)Convert.ChangeType(value, typeof(U), Culture);
                    result.Add(item);
                }
                //else
                //{
                //    U item = (U)typeof(U).GetConstructor(Type.EmptyTypes).Invoke(null);
                //    result.Add(item);
                //    for (int i = 0; i < reader.FieldCount; i++)
                //    {
                //        PropertyInfo prop = typeof(U).GetProperty(reader.GetName(i), BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);
                //        if (prop != null)
                //            prop.SetValue(item, reader.GetValue(i), null);
                //    }
                //}
            }

            stream.Close();
            return result as T;
        }

        private T Read<T>(RootContainerExpression expression)
        {
            IList<T> list = Read<List<T>, T>(GetStream(expression));
            if (list.Count == 0)
                return default(T);
            return list[0];
        }

        private Uri GetUri(PropertyExpression expression)
        {
            RootContainerExpression container = (RootContainerExpression)expression;
            if (string.IsNullOrEmpty(container.Name))
                expression.Name = Metadata[container.Type];

            StringBuilder sb = new StringBuilder();
            while (expression != null)
            {
                sb.Append(expression.Name);
                if (!string.IsNullOrEmpty(expression.Id))
                    sb.AppendFormat("({0})", expression.Id);
                if (expression.Target != null)
                    sb.Append('/');
                expression = expression.Target;
            }
            if (!string.IsNullOrEmpty(container.Expand) ||
                !string.IsNullOrEmpty(container.Filter) ||
                !string.IsNullOrEmpty(container.Format) ||
                container.InLineCount != InLineCount.None ||
                !string.IsNullOrEmpty(container.Select) ||
                !string.IsNullOrEmpty(container.Skip) ||
                !string.IsNullOrEmpty(container.Top))
                sb.Append("?");

            if (!string.IsNullOrEmpty(container.Expand))
                sb.AppendFormat("$expand={0}", container.Expand);
            if (!string.IsNullOrEmpty(container.Filter))
                sb.AppendFormat("$filter={0}", container.Filter);
            if (!string.IsNullOrEmpty(container.Format))
                sb.AppendFormat("$filter={0}", container.Format);
            if (container.InLineCount != InLineCount.None)
                sb.Append("$inlinecount=allpages");
            if (!string.IsNullOrEmpty(container.Select))
                sb.AppendFormat("$select={0}", container.Select);
            if (!string.IsNullOrEmpty(container.Skip))
                sb.AppendFormat("$skip={0}", container.Skip);
            if (!string.IsNullOrEmpty(container.Top))
                sb.AppendFormat("$top={0}", container.Top);

            return new Uri(serviceUri, new Uri(sb.ToString(), UriKind.Relative));
        }

        private Stream GetStream(RootContainerExpression expression)
        {
            return GetStream(GetUri(expression));
        }

        private Stream GetStream(Uri uri)
        {
            return WebRequest.Create(uri).GetResponse().GetResponseStream();
        }

        private void BeginGetStream(AsyncCallback callback, Uri uri)
        {

            var request = WebRequest.Create(uri);
            request.BeginGetResponse(callback, request);
        }

        private Stream EndGetStream(IAsyncResult result)
        {
            return ((WebRequest)result.AsyncState).EndGetResponse(result).GetResponseStream();
        }


        private EntitySet GetEntities(RootContainerExpression expression)
        {
            return ReadEntities(GetStream(expression));
        }

        protected override T LoadMany<T, U>(Evaluant.NLinq.NLinqQuery query, int first, int max)
        {
            NLinqVisitor visitor = new NLinqVisitor(Metadata);
            if (typeof(U) == typeof(Entity))
                return GetEntities((RootContainerExpression)visitor.Visit(query.Expression)) as T;
            return Read<T, U>(GetStream((RootContainerExpression)visitor.Visit(query.Expression)));
        }

        public override void CreateId(Domain.Entity e)
        {
            e.Id = Guid.NewGuid().ToString();
        }
    }
}
