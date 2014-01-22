using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.SqlExpressions;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.Commands;
using System.Data;
using Evaluant.Uss.SqlExpressions.Mapping;
using Evaluant.Uss.Era;
using Evaluant.Uss.SqlMapper.Mapper;

namespace Evaluant.Uss.SqlMapper
{
    public class SqlCommandProcessor : PersistenceEngine.Contracts.CommonVisitors.BaseCommandProcessor
    {
        private SqlMapperEngine engine;
        private IDriver driver;
        private IDialect dialect;

        private IDbConnection Connection { get; set; }

        public SqlCommandProcessor(SqlMapperEngine engine)
        {
            this.engine = engine;
            if (engine.Driver.SupportDataManipulationLanguage)
            {
                driver = engine.Driver;
                dialect = engine.Dialect;
            }
            else
            {
                driver = engine.AlternateDriver;
                dialect = engine.AlternateDialect;
            }
            Connection = driver.CreateConnection();
        }

        IDictionary<Domain.Entity, IDbStatement> commands = new Dictionary<Domain.Entity, IDbStatement>();

        #region IVisitor<CreateCommand> Members

        public override CreateEntityCommand Visit(CreateEntityCommand item)
        {
            Mapping.Entity entity = engine.Provider.Mapping.Entities[item.Type];
            commands.Add(item.ParentEntity, engine.Provider.Mapping.Mapper.Create(entity));

            foreach (Mapping.Attribute attribute in entity.Attributes.Values)
            {
                if (attribute.IsId && attribute.Generator == Mapping.Generator.Guid)
                    Visit(new CreateAttributeCommand(item.ParentEntity[attribute.Name]));
            }
            //if (item.ParentEntity.Id == null)
            //{
            //    item.ParentEntity.TryCreateId();
            //    Domain.EntityEntry id = null;
            //    using (IEnumerator<Models.Attribute> enumerator = item.ParentEntity.Model.Ids.Values.GetEnumerator())
            //    {
            //        if (enumerator.MoveNext())
            //            id = item.ParentEntity[enumerator.Current];
            //    }
            //    if (id != null)
            //        new CreateAttributeCommand(item.ParentEntity, id).Accept(this);
            //    else
            //    {
            //        //If there is no Id in the business model
            //        AddParameter(command, engine.Mapping.Entities[item.ParentEntity.Type].Attributes["#Id"].Field, null);
            //    }
            //}
            return item;
        }

        #endregion

        #region IVisitor<DeleteCommand> Members

        public override DeleteEntityCommand Visit(Evaluant.Uss.Commands.DeleteEntityCommand item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVisitor<CompositeCreateCommand> Members

        public override CompoundCreateCommand Visit(CompoundCreateCommand item)
        {
            Visit((CreateEntityCommand)item);
            foreach (Command command in item.InnerCommands)
                Visit(command);
            //Assign a null value if the parameter was not defined in the inner commands
            foreach (Mapping.Attribute attribute in engine.Provider.Mapping.Entities[item.ParentEntity.Type].Attributes.Values)
                AddParameter(commands[item.ParentEntity], attribute, null);

            return item;
        }

        #endregion

        #region IVisitor<CompositeUpdateCommand> Members

        public override CompoundUpdateCommand Visit(CompoundUpdateCommand item)
        {
            Mapping.Entity entity = engine.Provider.Mapping.Entities[item.ParentEntity.Type];
            UpdateStatement update = engine.Provider.Mapping.Mapper.Update(entity);
            commands.Add(item.ParentEntity, update);
            foreach (Command command in item.InnerCommands)
                Visit(command);
            return item;
        }

        #endregion

        #region IVisitor<CreateAttributeCommand> Members

        public override Evaluant.Uss.Commands.CreateAttributeCommand Visit(Evaluant.Uss.Commands.CreateAttributeCommand item)
        {
            AddParameter(commands[item.ParentEntity], engine.Provider.Mapping.Entities[item.ParentEntity.Type].Attributes[item.RelatedEntry.Name], item.RelatedEntry.Value);
            return item;
        }

        private ValuedParameter AddParameter(IDbStatement command, Field field, object value)
        {
            ValuedParameter param = CreateParameter(command, field);
            if (param == null)
                return param;
            param.Value = value ?? DBNull.Value;
            command.Add(param);
            return param;
        }


        private ValuedParameter AddParameter(IDbStatement command, Mapping.Attribute entry, object value)
        {
            ValuedParameter parameter = AddParameter(command, entry.Field, value);
            if (parameter == null)
                return null;
            parameter.Name = entry.Name;
            return parameter;
        }


        /// <summary>
        /// Creates the parameter.
        /// </summary>
        /// <remarks>
        /// The parameter will be null if the command already contains a parameter for this field.
        /// </remarks>
        /// <param name="command">The command.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private ValuedParameter CreateParameter(IDbStatement command, Field field)
        {
            if (command.Parameters.ContainsKey(field.ColumnName.Text))
                return null;
            ValuedParameter param = new ValuedParameter(field.ColumnName.Text, null, field.DbType, ParameterDirection.Input);
            param.Precision = field.Precision;
            param.Scale = field.Scale;
            param.Size = field.Size;
            return param;
        }

        #endregion

        #region IVisitor<DeleteAttributeCommand> Members

        public override Evaluant.Uss.Commands.DeleteAttributeCommand Visit(Evaluant.Uss.Commands.DeleteAttributeCommand item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IVisitor<UpdateAttributeCommand> Members

        public override Evaluant.Uss.Commands.UpdateAttributeCommand Visit(Evaluant.Uss.Commands.UpdateAttributeCommand item)
        {
            UpdateStatement update = (UpdateStatement)commands[item.ParentEntity];
            Mapping.Attribute attribute = engine.Provider.Mapping.Entities[item.ParentEntity.Type].Attributes[item.Name];
            update.Set.Add(new ColumnExpression(null, attribute.ColumnName), new DbParameter(attribute.Name, attribute.DbType, ParameterDirection.Input));
            AddParameter(((UpdateStatement)commands[item.ParentEntity]), attribute, item.Value);
            return item;
        }

        #endregion

        #region IVisitor<CreateReferenceCommand> Members

        public override Evaluant.Uss.Commands.CreateReferenceCommand Visit(Evaluant.Uss.Commands.CreateReferenceCommand item)
        {
            Mapping.Reference reference = engine.Provider.Mapping.Entities[item.ParentEntity.Type].References[item.Role ?? item.Reference.Name];
            if (commands.ContainsKey(item.ParentEntity) && item.Reference.Cardinality is Cardinality.ToOne)
            {
                Domain.Entity key = new Domain.Entity(item.ParentEntity.Type);
                IDbStatement command;
                if (reference.IsComposite)
                {
                    command = commands[item.ParentEntity];
                    IDbStatement childStatement;
                    if (commands.TryGetValue(item.ChildEntity, out childStatement))
                    {
                        foreach (DbParameter parameter in childStatement.Parameters.Values)
                        {
                            if (!command.Parameters.ContainsKey(parameter.Name))
                                command.Add(parameter);
                        }
                        commands[item.ChildEntity] = command;
                    }
                }
                else
                    command = engine.Provider.Mapping.Mapper.Create(reference, ((InsertStatement)commands[item.ParentEntity]).Identity);
                commands.Add(key, command);
                for (int i = 0; i < reference.Rules.Count; i++)
                {
                    Mapping.Rule rule = reference.Rules[i];
                    if (rule.ParentTable == reference.Parent.Table && reference.Model.Cardinality.IsOne)
                    {
                        for (int fieldNumber = 0; fieldNumber < rule.ParentFields.Count; fieldNumber++)
                        {
                            Field parentField = rule.ParentFields[fieldNumber];
                            Field childField = rule.ChildFields[fieldNumber];
                            Mapping.Attribute attribute = null;
                            foreach (Mapping.Attribute a in reference.Target.Attributes.Values)
                            {
                                if (a.Field == childField)
                                {
                                    attribute = a;
                                    break;
                                }
                            }
                            AddParameter(command, parentField, item.ChildEntity[attribute.Name]);
                        }
                    }
                    if (rule.ChildTable == reference.Target.Table && reference.Model.Cardinality is Cardinality.ToOne)
                    {
                        for (int fieldNumber = 0; fieldNumber < rule.ParentFields.Count; fieldNumber++)
                        {
                            Field parentField = rule.ParentFields[fieldNumber];
                            Field childField = rule.ChildFields[fieldNumber];
                            Mapping.Attribute attribute = null;
                            foreach (Mapping.Attribute a in reference.Target.Attributes.Values)
                            {
                                if (a.Field == childField)
                                {
                                    attribute = a;
                                    break;
                                }
                            }
                            if (attribute != null)
                                AddParameter(command, parentField, item.ChildEntity[attribute.Name].Value);
                        }
                        foreach (Mapping.Attribute attribute in reference.Parent.Ids.Values)
                        {
                            AddParameter(command, attribute, item.ParentEntity[attribute.Name].Value);
                        }
                    }
                }
            }
            if (item.Reference.Cardinality.IsMany && item.Reference.Cardinality is Cardinality.ToMany)
            {
                Domain.Entity key = new Domain.Entity(item.ParentEntity.Type);
                IDbStatement command = engine.Provider.Mapping.Mapper.Create(reference);
                commands.Add(key, command);
                Table indexTable = reference.Rules[0].ChildTable;
                for (int i = 0; i < reference.Rules.Count; i++)
                {
                    Mapping.Rule rule = reference.Rules[i];
                    if (rule.ParentTable == indexTable)
                    {
                        for (int fieldNumber = 0; fieldNumber < rule.ParentFields.Count; fieldNumber++)
                        {
                            Field parentField = rule.ParentFields[fieldNumber];
                            Field childField = rule.ChildFields[fieldNumber];
                            Mapping.Attribute attribute = null;
                            foreach (Mapping.Attribute a in reference.Target.Attributes.Values)
                            {
                                if (a.Field == childField)
                                {
                                    attribute = a;
                                    break;
                                }
                            }
                            IDbStatement statement;
                            if (commands.TryGetValue(item.ChildEntity, out statement))
                            {
                                InsertStatement insert = statement as InsertStatement;
                                if (insert != null && insert.Identity != null)
                                    command.Add(new LazyParameter(parentField.ColumnName.Text, insert.Identity, ParameterDirection.Input));
                                else
                                    AddParameter(command, parentField, item.ChildEntity[attribute.Name].Value);
                            }
                            else
                                AddParameter(command, parentField, item.ChildEntity[attribute.Name].Value);
                        }
                    }
                    if (rule.ChildTable == indexTable)
                    {
                        for (int fieldNumber = 0; fieldNumber < rule.ParentFields.Count; fieldNumber++)
                        {
                            Field parentField = rule.ParentFields[fieldNumber];
                            Field childField = rule.ChildFields[fieldNumber];
                            Mapping.Attribute attribute = null;
                            foreach (Mapping.Attribute a in reference.Parent.Attributes.Values)
                            {
                                if (a.Field == parentField)
                                {
                                    attribute = a;
                                    break;
                                }
                            }
                            IDbStatement statement;
                            if (commands.TryGetValue(item.ParentEntity, out statement))
                            {
                                InsertStatement insert = statement as InsertStatement;
                                if (insert != null && insert.Identity != null)
                                {
                                    command.Add(new LazyParameter(childField.ColumnName.Text, insert.Identity, ParameterDirection.Input));
                                }
                                else
                                    AddParameter(command, childField, item.ParentEntity[attribute.Name].Value);
                            }
                            else
                                AddParameter(command, childField, item.ParentEntity[attribute.Name].Value);
                        }
                    }

                }

                //foreach (Mapping.Field field in engine.Mapping.Entities[item.ParentEntity.Type].References[item.Role.Name].Fields)
                //{
                //    if (field.ParentEntity.Type == item.ParentEntity.Type)
                //    {
                //        //insertEntities[item.ParentEntity].Values.Add(new ColumnExpression(null,field.ColumnName),item.ChildEntity.Entries[
                //    }
                //}
            }

            return item;
        }

        #endregion

        #region IVisitor<DeleteReferenceCommand> Members

        public override Evaluant.Uss.Commands.DeleteReferenceCommand Visit(Evaluant.Uss.Commands.DeleteReferenceCommand item)
        {
            throw new NotImplementedException();
        }

        #endregion

        internal void Commit()
        {
            IDbConnection connection = engine.Driver.CreateConnection();

            if (connection.State == ConnectionState.Closed)
                connection.Open();

            List<IDbStatement> processedStatements = new List<IDbStatement>();

            IDbTransaction transaction = driver.BeginTransaction(connection);
            try
            {


                foreach (IDbStatement statement in commands.Values)
                {
                    if (processedStatements.Contains(statement))
                        continue;
                    processedStatements.Add(statement);
                    var command = engine.Driver.GetCommand(connection, engine.Dialect.Render(statement));

                    foreach (ValuedParameter parameter in statement.Parameters.Values)
                    {
                        command.Parameters.Add(driver.GetParameter(command, parameter));
                    }

                    if (command.Transaction == null)
                        command.Transaction = transaction;
                    command.ExecuteNonQuery();

                    foreach (IDbDataParameter parameter in command.Parameters)
                    {
                        if (parameter.Direction != ParameterDirection.Input)
                            ((ValuedParameter)statement.Parameters[parameter.ParameterName]).Value = parameter.Value;
                    }
                }

                if (transaction != null)
                    transaction.Commit();
            }
            catch
            {
                if (transaction != null)
                    transaction.Rollback();
                throw;

            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }
    }
}
