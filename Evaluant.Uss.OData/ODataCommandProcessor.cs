using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Net;
using Evaluant.Uss.OData.Operations;
using System.IO;

namespace Evaluant.Uss.OData
{
    class ODataCommandProcessor : PersistenceEngine.Contracts.CommonVisitors.BaseCommandProcessor
    {
        private ODataPersistenceEngineAsync engine;
        private string boundary;
        private bool isMultiple;

        public ODataCommandProcessor(ODataPersistenceEngineAsync engine)
        {
            this.engine = engine;
            boundary = "batch_" + Guid.NewGuid();
        }

        Dictionary<string, IOperation> operations = new Dictionary<string, IOperation>();

        public Http.HttpWebRequest Request { get; set; }

        public event EventHandler RequestReady;
        private int lastId = 1;

        public override Transaction Visit(Transaction item)
        {
            isMultiple = item.PendingCommands.Count > 1;
            base.Visit(item);
            //if (isMultiple)
            //{
            Request = (Http.HttpWebRequest)Http.WebRequest.Create(new Uri(((ODataPersistenceProvider)engine.FactoryAsync).ConnectionString, new Uri("$batch", UriKind.Relative)), Http.HttpStack.XmlHttp);
            Request.Method = "POST";
            Request.Headers[Http.HttpRequestHeader.AcceptCharset] = "UTF-8";
            Request.Accept = "application/atom+xml,application/xml,*/*";
            Request.ContentType = "multipart/mixed; boundary=" + boundary;

            Request.BeginGetRequestStream(GotRequestStream, Request);
            //}
            //else
            //{
            //    var firstOperation = operations.GetEnumerator();
            //    //if (firstOperation.Current == default(KeyValuePair<string, IOperation>))
            //    if (firstOperation.MoveNext())
            //        Request = (HttpWebRequest)WebRequest.Create(engine.GetUri(firstOperation.Current.Value.Expression));
            //    else
            //        return item;
            //    Request.Method = firstOperation.Current.Value.Method;
            //    Request.BeginGetRequestStream(GotRequestStream, null);

            //}
            return item;
        }

        private void GotRequestStream(IAsyncResult result)
        {
            var request = (Http.HttpWebRequest)result.AsyncState;
            Stream s = request.EndGetRequestStream(result);
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.NewLine = "\n";
            if (isMultiple)
            {
                Batch b = new Batch(true);
                var changeSet = new Changeset(false);
                b.Operations.Add(changeSet);
                foreach (IOperation operation in operations.Values)
                    changeSet.Changes.Add((Change)operation);
                foreach (KeyValuePair<string, string> item in b.Headers)
                {
                    if (item.Key == "Content-Type")
                        request.ContentType = item.Value;
                    else
                        request.Headers[item.Key] = item.Value;
                }
                request.Headers["DataServiceVersion"] = "1.0";
                request.Headers["MaxDataServiceVersion"] = "2.0";
                b.Complete(engine.FactoryAsync.Model);
                b.WriteTo(sw, engine);
                sw.Flush();
                ms.Position = 0;
                Util.Write(ms, s);
            }
            else
            {
                foreach (IOperation operation in operations.Values)
                {
                    operation.WriteTo(sw, engine);
                }
            }
            sw.Flush();
            if (RequestReady != null)
                RequestReady(this, EventArgs.Empty);
        }

        public override Commands.CreateEntityCommand Visit(Commands.CreateEntityCommand item)
        {
            Atom.AtomEntry entry = GetOperation(item).Entry;
            entry.Category = item.Type;
            entry.Identity = item.ParentId;
            return item;
        }

        public override Commands.DeleteEntityCommand Visit(Commands.DeleteEntityCommand item)
        {
            throw new NotImplementedException();
        }

        public override Commands.CompoundCreateCommand Visit(Commands.CompoundCreateCommand item)
        {
            Visit((Commands.CreateEntityCommand)item);
            foreach (var subCommand in item.InnerCommands)
                Visit(subCommand);
            return item;
        }

        public override Commands.CompoundUpdateCommand Visit(Commands.CompoundUpdateCommand item)
        {
            foreach (var subCommand in item.InnerCommands)
                Visit(subCommand);
            return item;
        }

        public override Commands.CreateAttributeCommand Visit(Commands.CreateAttributeCommand item)
        {
            Atom.AtomEntry entry = GetOperation(item).Entry;
            entry.DataValues.Add(new Atom.AtomContentProperty() { Name = item.Name, MaterializedValue = item.Value, TypeName = Atom.AtomContentProperty.GetEdmType(item.Type) });
            return item;
        }

        public override Commands.DeleteAttributeCommand Visit(Commands.DeleteAttributeCommand item)
        {
            throw new NotImplementedException();
        }

        public override Commands.UpdateAttributeCommand Visit(Commands.UpdateAttributeCommand item)
        {
            Atom.AtomEntry entry = GetOperation(item).Entry;
            entry.DataValues.Add(new Atom.AtomContentProperty() { Name = item.Name, Text = item.Value.ToString() });
            return item;
        }

        private Change GetOperation(Commands.Command item)
        {
            IOperation operation = null;
            if (item.CommandType == Commands.CommandTypes.CreateReference || item.CommandType == Commands.CommandTypes.DeleteReference)
            {
                if (!operations.TryGetValue(item.ParentEntity.Type + item.ParentId + ((Commands.ReferenceCommand)item).Role, out operation))
                {
                    IOperation createOperation;

                    if (operations.TryGetValue(item.ParentEntity.Type + item.ParentId, out createOperation))
                    {
                        if (item.CommandType == Commands.CommandTypes.CreateReference)
                        {
                            if (string.IsNullOrEmpty(((Change)createOperation).ContentId))
                                ((Change)createOperation).ContentId = (lastId++).ToString();
                            operations.Add(item.ParentEntity.Type + item.ParentId + ((Commands.ReferenceCommand)item).Role, operation = new Change(ChangeMode.Insert) { Expression = new UriExpressions.RootContainerExpression() { Name = "$" + ((Change)createOperation).ContentId, Target = new UriExpressions.PropertyExpression() { Name = "$links", Target = new UriExpressions.PropertyExpression() { Name = ((Commands.ReferenceCommand)item).Role } } } });
                        }
                        else
                            operations.Add(item.ParentEntity.Type + item.ParentId + ((Commands.ReferenceCommand)item).Role, operation = new Change(ChangeMode.Delete) { Expression = new UriExpressions.RootContainerExpression() { Type = item.ParentEntity.Type, Id = item.ParentId, Target = new UriExpressions.PropertyExpression() { Name = "$links", Target = new UriExpressions.PropertyExpression() { Name = ((Commands.ReferenceCommand)item).Role } } } });
                    }
                    else
                    {
                        if (item.CommandType == Commands.CommandTypes.CreateReference)
                            operations.Add(item.ParentEntity.Type + item.ParentId + ((Commands.ReferenceCommand)item).Role, operation = new Change(ChangeMode.Insert) { Expression = new UriExpressions.RootContainerExpression() { Type = item.ParentEntity.Type, Id = item.ParentId, Target = new UriExpressions.PropertyExpression() { Name = "$links", Target = new UriExpressions.PropertyExpression() { Name = ((Commands.ReferenceCommand)item).Role } } } });
                        else
                            operations.Add(item.ParentEntity.Type + item.ParentId + ((Commands.ReferenceCommand)item).Role, operation = new Change(ChangeMode.Delete) { Expression = new UriExpressions.RootContainerExpression() { Type = item.ParentEntity.Type, Id = item.ParentId, Target = new UriExpressions.PropertyExpression() { Name = "$links", Target = new UriExpressions.PropertyExpression() { Name = ((Commands.ReferenceCommand)item).Role } } } });
                    }
                }
            }
            else if (!operations.TryGetValue(item.ParentEntity.Type + item.ParentId, out operation))
            {
                switch (item.CommandType)
                {
                    case Evaluant.Uss.Commands.CommandTypes.CompoundCreate:
                        operations.Add(item.ParentEntity.Type + item.ParentId, operation = new Change(ChangeMode.Insert) { Expression = new UriExpressions.RootContainerExpression() { Type = item.ParentEntity.Type } });
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.CompoundUpdate:
                        operations.Add(item.ParentEntity.Type + item.ParentId, operation = new Change(ChangeMode.Update) { Expression = new UriExpressions.RootContainerExpression() { Type = item.ParentEntity.Type, Id = item.ParentId } });
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.CreateAttribute:
                        operations.Add(item.ParentEntity.Type + item.ParentId, operation = new Change(ChangeMode.Insert) { Expression = new UriExpressions.RootContainerExpression() { Type = item.ParentEntity.Type, Id = item.ParentId } });
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.CreateEntity:
                        operations.Add(item.ParentEntity.Type + item.ParentId, operation = new Change(ChangeMode.Insert) { Expression = new UriExpressions.RootContainerExpression() { Type = item.ParentEntity.Type } });
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.DeleteAttribute:
                        operations.Add(item.ParentEntity.Type + item.ParentId, operation = new Change(ChangeMode.Update) { Expression = new UriExpressions.RootContainerExpression() { Type = item.ParentEntity.Type, Id = item.ParentId } });
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.DeleteEntity:
                        operations.Add(item.ParentEntity.Type + item.ParentId, operation = new Change(ChangeMode.Delete) { Expression = new UriExpressions.RootContainerExpression() { Type = item.ParentEntity.Type, Id = item.ParentId } });
                        break;
                    case Evaluant.Uss.Commands.CommandTypes.UpdateAttribute:
                        operations.Add(item.ParentEntity.Type + item.ParentId, operation = new Change(ChangeMode.Update) { Expression = new UriExpressions.RootContainerExpression() { Type = item.ParentEntity.Type, Id = item.ParentId } });
                        break;
                    default:
                        break;
                }
            }
            return ((Change)operation);
        }

        public override Commands.CreateReferenceCommand Visit(Commands.CreateReferenceCommand item)
        {
            IOperation operation = GetOperation(item);
            IOperation childOperation;
            if (operations.TryGetValue(item.ChildEntity.Type + item.ChildId, out childOperation))
            {
                if (string.IsNullOrEmpty(((Change)childOperation).ContentId))
                    ((Change)childOperation).ContentId = (lastId++).ToString();
                ((Change)operation).Link = new Atom.AtomLink("$" + ((Change)childOperation).ContentId);
            }
            else
                ((Change)operation).Link = new Atom.AtomLink(engine.Metadata.GetEntitySetName(item.ChildEntity.Type), item.ChildId);
            return item;
        }

        public override Commands.DeleteReferenceCommand Visit(Commands.DeleteReferenceCommand item)
        {
            throw new NotImplementedException();
        }
    }
}
