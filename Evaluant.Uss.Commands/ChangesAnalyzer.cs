using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.Domain;
using Evaluant.Uss.Collections;

namespace Evaluant.Uss.Commands
{
    public class ChangesAnalyzer : IChangesAnalyzer
    {
        public ChangesAnalyzer(Model.Model model)
        {
            Commands = new CommandCollection();
            Model = model;
        }

        #region IChangesAnalyzer Members

        public Model.Model Model { get; set; }

        public CommandCollection Commands { get; set; }

        public void ComputeChanges(Domain.Entity entity)
        {
            ComputeChanges(new Entity[] { entity });
        }

        /// <summary>
        /// Generated a set of commands to be processed to take into account any modification applied to an entity graph
        /// </summary>
        /// <param name="entity">The Entity to compute</param>
        /// <param name="processing">A collection of all currently processed entities</param>
        /// <param name="commands">A collection of all currently created commands</param>
        public void ComputeChanges(Entity entity, ICollection<Entity> processing, ICollection<Command> commands)
        {
            // Nothing to do if the entity is currently processed
            if (processing.Contains(entity))
                return;

            processing.Add(entity);

            // To know if we can optimize the command with a compound
            bool canCreateCompound = true;

            // Will contain the commands created for the attributes of the current entity
            List<AttributeCommand> attributeCommands = new List<AttributeCommand>(entity.Count);

            // First process all attributes not to increase the stack size because of deep hierarchies in the object graph and the recursive algorithm
            if (entity.State == State.Modified || entity.State == State.New)
                foreach (Entry e in entity)
                {
                    if (!e.IsEntity)
                    {
                        // The current entry is an Attribute
                        switch (e.State)
                        {
                            case State.Deleted:
                                attributeCommands.Add(new DeleteAttributeCommand(e));
                                break;

                            case State.Modified:
                                attributeCommands.Add(new UpdateAttributeCommand(e));
                                break;

                            case State.New:
                                attributeCommands.Add(new CreateAttributeCommand(e));
                                break;
                            default:
                                break;
                        }
                    }
                }

            // Selects the optimized command
            /// TODO: Add a property to PE so that the user could specify whether he wants all attributes to be sent with a compound command
            switch (entity.State)
            {
                case State.New:
                    if (canCreateCompound)
                        commands.Add(new CompoundCreateCommand(entity, attributeCommands));
                    else
                    {
                        commands.Add(new CreateEntityCommand(entity));
                        foreach (Command attributeCommand in attributeCommands)
                            commands.Add(attributeCommand);
                    }

                    break;

                case State.Deleted:
                    commands.Add(new DeleteEntityCommand(entity.Id, entity.Type));
                    break;

                case State.Modified:

                    if (attributeCommands.Count > 1)
                    {
                        CompoundUpdateCommand cuc = new CompoundUpdateCommand(entity, attributeCommands);
                        commands.Add(cuc);
                    }
                    else
                        foreach (Command attributeCommand in attributeCommands)
                            commands.Add(attributeCommand);
                    break;

                default:
                    foreach (Command attributeCommand in attributeCommands)
                        commands.Add(attributeCommand);
                    break;
            }

            foreach (Entry e in entity)
            {
                if (e.IsEntity)
                {
                    Model.Reference referenceModel = Model.GetReference(entity.Type, e.Name);
                    if (e.IsMultiple)
                    {
                        foreach (Entity child in ((IEnumerable<Entity>)e))
                        {
                            ComputeChanges(child, processing, commands);


                            switch (e.State)
                            {
                                case State.New:
                                    CreateReferenceCommand crc1 = new CreateReferenceCommand(referenceModel, entity, child);
                                    commands.Add(crc1);
                                    break;

                                case State.Deleted:
                                    DeleteReferenceCommand drc = new DeleteReferenceCommand(referenceModel, entity, child);
                                    commands.Add(drc);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // The current entry is an Entity
                        Entity child = (Entity)e.Value;
                        ComputeChanges(child, processing, commands);

                        switch (e.State)
                        {
                            case State.New:
                                CreateReferenceCommand crc1 = new CreateReferenceCommand(referenceModel, entity, child);
                                commands.Add(crc1);
                                break;

                            case State.Deleted:
                                DeleteReferenceCommand drc = new DeleteReferenceCommand(referenceModel, entity, child);
                                commands.Add(drc);
                                break;
                        }
                    }
                }
            }
        }

        public void ComputeChanges(Evaluant.Uss.Domain.Entity entity, bool ignoreReferences)
        {
            ComputeChanges(new Entity[] { entity }, ignoreReferences);
        }

        #endregion

        #region IChangesAnalyzer Members


        public void ComputeChanges(IEnumerable<Entity> entities)
        {
            HashedList<Entity> processing = new HashedList<Entity>();
            foreach (Entity e in entities)
            {
                ComputeChanges(e, processing, Commands);
            }
        }

        public void ComputeChanges(IEnumerable<Entity> entities, bool ignoreReferences)
        {
            HashedList<Entity> processing = new HashedList<Entity>();
            foreach (Entity e in entities)
            {
                if (ignoreReferences)
                {
                    foreach (Entry entry in e)
                    {
                        if (!(!entry.IsEntity || processing.Contains((Entity)entry.Value)))
                        {
                            processing.Add((Entity)entry.Value);
                        }
                    }
                }
                ComputeChanges(e, processing, Commands);
            }
        }

        public void ComputeChanges(Entity entity, ICollection<Entity> alreadyProcessedEntities)
        {
            ComputeChanges(entity, alreadyProcessedEntities, Commands);
        }

        #endregion
    }
}
