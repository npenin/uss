using System;
using System.Net;
using Evaluant.Uss.OData.UriExpressions;
using Evaluant.Uss.Domain;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Evaluant.Uss.MetaData;
using Evaluant.Uss.OData.Visitors;
using System.Threading;
using Evaluant.NLinq;

namespace Evaluant.Uss.OData
{
    public class ODataPersistenceEngineAsync : PersistenceEngine.Contracts.PersistenceEngineAsyncImplementation
    {
        private Uri serviceUri;
        public ODataPersistenceEngineAsync(ODataPersistenceProvider factory, Uri serviceUri)
            : base(factory)
        {
            this.serviceUri = serviceUri;
        }

        protected internal Edm.Metadata Metadata
        {
            get { return ((ODataPersistenceProvider)FactoryAsync).Metadata; }
        }

        public override void BeginLoadWithIds(AsyncCallback callback, string type, string[] id, object asyncState)
        {
            if (id.Length == 1)
            {
                RootContainerExpression expression = new RootContainerExpression();
                expression.Type = type;
                expression.Id = id[0];
                BeginGetEntities(callback, expression, asyncState);
            }
        }

        internal Uri GetUri(PropertyExpression expression)
        {
            return new Uri(serviceUri, new Uri(GetRelativeUri(expression), UriKind.Relative));
        }

        private void BeginGetEntities(AsyncCallback callback, RootContainerExpression expression, object asyncState)
        {
            BeginGetStream(callback, expression, asyncState);
        }

        private void BeginGetStream(AsyncCallback callback, RootContainerExpression expression, object asyncState)
        {
            BeginGetStream(callback, GetUri(expression), asyncState);
        }

        private void BeginGetStream(AsyncCallback callback, Uri uri, object asyncState)
        {
            var request = WebRequest.Create(uri);
            request.BeginGetResponse(callback, new ODataAsyncState(request, asyncState));
        }

        private Stream EndGetStream(IAsyncResult result)
        {
            return ((ODataAsyncState)result.AsyncState).Request.EndGetResponse(result).GetResponseStream();
        }


        private EntitySet ReadEntities(System.IO.Stream stream)
        {
            EntitySet result = new EntitySet();
            XmlReader reader = XmlReader.Create(stream);
            XmlNamespaceManager xmlnsm = new XmlNamespaceManager(new NameTable());
            xmlnsm.AddNamespace("atom", "http://www.w3.org/2005/Atom");
            xmlnsm.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            xmlnsm.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
            Atom.AtomFeed feed = new Atom.AtomFeed();
            feed.Read(reader);
            foreach (Atom.AtomEntry entry in feed.Entries)
            {
                Entity e = new Entity(entry.TypeName);
                foreach (Atom.AtomContentProperty property in entry.DataValues)
                    e.Add(property.Name, property.MaterializedValue);
                result.Add(e);
                e.State = State.UpToDate;
            }
            stream.Close();
            return result;
        }

        public override IList<Entity> EndLoadWithIds(IAsyncResult result)
        {
            return ReadEntities(EndGetStream(result));
        }

        protected override Commands.CommandCollection ComputeCascadeDeleteCommands(string id, string type, IDictionary<string, Commands.Command> processed)
        {
            throw new NotImplementedException();
        }

        public override void CreateId(Entity e)
        {
            //e.Id = Guid.NewGuid().ToString();
        }

        protected override void BeginLoadSingle<T>(AsyncCallback callback, NLinq.NLinqQuery query, object asyncState)
        {
            NLinqVisitor visitor = new NLinqVisitor(Metadata);
            BeginGetStream(callback, (RootContainerExpression)visitor.Visit(query.Expression), asyncState);
        }

        protected override void BeginLoadMany<T, U>(AsyncCallback callback, NLinq.NLinqQuery query, object asyncState)
        {
            NLinqVisitor visitor = new NLinqVisitor(Metadata);
            BeginGetStream(callback, (RootContainerExpression)visitor.Visit(query.Expression), asyncState);
        }

        protected override T EndLoadSingle<T>(IAsyncResult result)
        {
            if (!string.IsNullOrEmpty(((ODataAsyncState)result.AsyncState).Request.RequestUri.Query))
                return Read<T>(EndGetStream(result));
            return ReadScalar<T>(EndGetStream(result));
        }

        private T ReadScalar<T>(Stream stream)
        {
            Atom.AtomContentProperty property = new Atom.AtomContentProperty();
            XmlReader reader = XmlReader.Create(stream);
            while (reader.NodeType != XmlNodeType.Element)
            {
                if (!reader.Read())
                    return default(T);
            }
            property.Read(reader);
            return (T)property.MaterializedValue;
        }

        protected override T EndLoadMany<T, U>(IAsyncResult result)
        {
            NLinqVisitor visitor = new NLinqVisitor(Metadata);
            if (typeof(U) == typeof(Entity))
                return ReadEntities(EndGetStream(result)) as T;
            return Read<T, U>(EndGetStream(result));
        }

        private T Read<T, U>(Stream stream)
where T : class, IEnumerable<U>
        {
            List<U> result = new List<U>();

            EntitySet set = ReadEntities(stream);
            foreach (Entity e in set)
            {
                if (Evaluant.Uss.MetaData.TypeResolver.IsPrimitive(typeof(U)))
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

        private T Read<T>(Stream s)
        {
            IList<T> list = Read<List<T>, T>(s);
            if (list.Count == 0)
                return default(T);
            return list[0];
        }



        public override void BeginLoadReference(AsyncCallback callback, string reference, IEnumerable<Entity> entities, NLinqQuery query, object asyncState)
        {
            throw new NotImplementedException();
            RootContainerExpression expression = new RootContainerExpression();
            foreach (Entity e in entities)
            {
                expression.Id = e.Id;
                expression.Type = e.Type;
                expression.Target = new PropertyExpression();
                //expression.Target.Name = ((Evaluant.NLinq.Expressions.Identifier)reference.Statement).Text;
                BeginGetEntities(callback, expression, asyncState);
            }
        }

        public override void EndLoadReference(IAsyncResult result)
        {
        }

        public override void BeginInitializeRepository(AsyncCallback callback, object asyncState)
        {
            callback(null);
        }

        public override void EndInitializeRepository(IAsyncResult result)
        {
        }

        public override void BeginProcessCommands(PersistenceEngine.Contracts.Transaction tx, AsyncCallback callback, object asyncState)
        {
            var commandProcessor = new ODataCommandProcessor(this);
            commandProcessor.RequestReady += delegate
            {
                commandProcessor.Request.BeginGetResponse(callback, new RequestState(commandProcessor.Request, asyncState));
            };
            commandProcessor.Visit(tx);
        }

        public override void EndProcessCommands(IAsyncResult result)
        {
            ((RequestState)result.AsyncState).Request.EndGetResponse(result);
        }

        internal string GetRelativeUri(PropertyExpression expression)
        {
            RootContainerExpression container = (RootContainerExpression)expression;
            if (string.IsNullOrEmpty(container.Name))
            {
                if (Metadata == null)
                    throw new NotSupportedException("The engine is not ready yet");
                expression.Name = Metadata.GetEntitySetName(container.Type);
            }

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

            return sb.ToString();
        }
    }
}
