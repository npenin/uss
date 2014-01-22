using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.PersistenceEngine.Contracts.CommonVisitors;
using Evaluant.Uss.MongoDB.Protocol;
using Evaluant.Uss.Commands;

namespace Evaluant.Uss.MongoDB
{
    public class MongoCommandProcessor : BaseCommandProcessor
    {
        public MongoCommandProcessor(string database)
        {
            this.database = database;
        }

        string database;
        IDictionary<Domain.Entity, IRequestMessage> commands = new Dictionary<Domain.Entity, IRequestMessage>();
        IList<IRequestMessage> messages = new List<IRequestMessage>();
        InsertMessage insert;

        public IEnumerable<IRequestMessage> Commands
        {
            get { return messages; }
        }

        public override Evaluant.Uss.Commands.CreateEntityCommand Visit(Evaluant.Uss.Commands.CreateEntityCommand item)
        {
            InsertMessage insert;
            IRequestMessage message;
            if (this.insert != null)
            {
                Domain.Entity e = new Evaluant.Uss.Domain.Entity(item.ParentEntity.Type);
                e.Id = item.ParentId;
                //e.AddValue("_id", item.ParentId);
                this.insert.Documents.Add(e);
                commands.Add(item.ParentEntity, this.insert);
            }
            else if (!commands.TryGetValue(item.ParentEntity, out message))
            {

                this.insert = new InsertMessage() { FullCollectionName = database + "." + item.Type };
                messages.Add(this.insert);
                message = this.insert;
                commands.Add(item.ParentEntity, message);
                insert = (InsertMessage)message;
                Domain.Entity e = new Evaluant.Uss.Domain.Entity(item.ParentEntity.Type);
                e.Id = item.ParentId;
                //e.AddValue("_id", item.ParentId);
                insert.Documents.Add(e);
            }
            else
                throw new NotSupportedException();

            return item;
        }

        public override Evaluant.Uss.Commands.CompoundCreateCommand Visit(Evaluant.Uss.Commands.CompoundCreateCommand item)
        {
            Visit((CreateEntityCommand)item);
            foreach (Command c in item.InnerCommands)
                Visit(c);
            return item;
        }

        public override Evaluant.Uss.Commands.CompoundUpdateCommand Visit(Evaluant.Uss.Commands.CompoundUpdateCommand item)
        {
            throw new NotImplementedException();
        }

        public override Evaluant.Uss.Commands.CreateAttributeCommand Visit(Evaluant.Uss.Commands.CreateAttributeCommand item)
        {
            IRequestMessage message;
            if (item.Type == typeof(Guid))
                return item;
            if (!commands.TryGetValue(item.ParentEntity, out message))
                throw new NotSupportedException();
            InsertMessage im = message as InsertMessage;
            if (im != null)
            {
                foreach (Domain.Entity e in im.Documents)
                {
                    if (e.Id == item.ParentId)
                    {
                        e.Add(item.Name, item.Value, item.Type, State.New);
                        break;
                    }
                }
            }
            return item;
        }

        public override Evaluant.Uss.Commands.DeleteAttributeCommand Visit(Evaluant.Uss.Commands.DeleteAttributeCommand item)
        {
            throw new NotImplementedException();
        }

        public override Evaluant.Uss.Commands.UpdateAttributeCommand Visit(Evaluant.Uss.Commands.UpdateAttributeCommand item)
        {
            throw new NotImplementedException();
        }

        public override Evaluant.Uss.Commands.CreateReferenceCommand Visit(Evaluant.Uss.Commands.CreateReferenceCommand item)
        {
            IRequestMessage message;
            if (!commands.TryGetValue(item.ParentEntity, out message))
                throw new NotSupportedException();

            InsertMessage im = message as InsertMessage;
            if (im != null)
            {
                foreach (Domain.Entity e in im.Documents)
                {
                    ICollection<Domain.Entry> entities;
                    if (e.Id == item.ParentId)
                    {
                        Domain.Entry entry;
                        if (item.Reference.ToMany)
                        {
                            if (!e.TryGet(item.Reference.Name, out entry))
                            {
                                entry = new Domain.MultipleEntry(item.Reference.Name);
                                e.Add(entry);
                            }
                            entities = (Domain.MultipleEntry)entry;
                        }
                        else
                            entities = e;
                        Domain.Entity dbref = new Evaluant.Uss.Domain.Entity(item.ChildType);
                        dbref.Add("$ref", item.ChildType);
                        dbref.Add("$id", item.ChildId);
                        entities.Add(Domain.Entry.Create(item.Reference.Name, State.New, dbref));
                        break;
                    }
                }
            }
            return item;

        }

        public override Evaluant.Uss.Commands.DeleteReferenceCommand Visit(Evaluant.Uss.Commands.DeleteReferenceCommand item)
        {
            throw new NotImplementedException();
        }

        public override DeleteEntityCommand Visit(DeleteEntityCommand item)
        {
            throw new NotImplementedException();
        }
    }
}
