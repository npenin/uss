using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.SqlExpressions;
using Evaluant.Uss.SqlExpressions.Mapping;
using Evaluant.Uss.MetaData;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators;
using Evaluant.Uss.SqlExpressions.Statements;

namespace Evaluant.Uss.SqlMapper
{
    public class SqlMapperEngine : PersistenceEngineImplementation
    {
        IDriver driver;
        IDriver alternateDriver;
        IDialect dialect;
        IDialect alternateDialect;
        //Models.Model model;
        Mapping.Mapping mapping;
        SqlMapperProvider provider;

        public IDriver Driver { get { return driver; } }
        public IDriver AlternateDriver { get { return alternateDriver; } }
        public IDialect Dialect { get { return dialect; } }
        public IDialect AlternateDialect { get { return alternateDialect; } }
        public SqlMapperProvider Provider { get { return provider; } }

        //public override IPersistenceProvider Factory { get { return provider; } }


        public SqlMapperEngine(SqlMapperProvider provider, IDriver driver, IDriver alternateDriver, IDialect dialect, IDialect alternateDialect, Mapping.Mapping mapping)
            : base(provider)
        {
            this.provider = provider;
            this.driver = driver;
            this.alternateDriver = alternateDriver;
            this.dialect = dialect;
            this.alternateDialect = alternateDialect;
            this.mapping = mapping;
        }

        private List<Domain.Entity> ReadEntities(System.Data.IDbCommand command, SqlExpressions.ValuedParameter[] parameters)
        {
            command.Connection.Open();
            command.Transaction = driver.BeginTransaction(command.Connection);
            List<Domain.Entity> entities = new List<Domain.Entity>();
            try
            {
                CreateParams(command, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    bool hasEntityType=false;
                    foreach (DataRow row in reader.GetSchemaTable().Rows)
                    {
                        hasEntityType = (string)row["ColumnName"] == "EntityType";
                        if (hasEntityType)
                            break;
                    }
                    while (reader.Read())
                    {
                        Domain.Entity entity;
                        if (hasEntityType)
                        {
                            entity = new Domain.Entity(reader.GetString(reader.GetOrdinal("EntityType")));
                            Model.Entity entityModel = Provider.Model.Entities[entity.Type];
                            foreach (KeyValuePair<string, Model.Attribute> attribute in entityModel.Attributes)
                            {
                                object value = reader[attribute.Key];
                                if (value == DBNull.Value)
                                    value = null;
                                entity.Add(Domain.Entry.Create(attribute.Key, State.UpToDate, Convert(value, attribute.Value.Type), attribute.Value.Type));
                            }

                            while (!string.IsNullOrEmpty(entityModel.Inherit))
                            {
                                entityModel = Provider.Model.Entities[entityModel.Inherit];
                                foreach (KeyValuePair<string, Model.Attribute> attribute in entityModel.Attributes)
                                {
                                    object value = reader[attribute.Key];
                                    if (value == DBNull.Value)
                                        value = null;
                                    entity.Add(Domain.Entry.Create(attribute.Key, State.UpToDate, value, attribute.Value.Type));
                                }
                            }
                        }
                        else
                        {
                            entity = new Domain.Entity();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                entity.Add(reader.GetName(i), reader.GetValue(i));
                            }
                        }



                        entities.Add(entity);
                        entity.State = State.UpToDate;
                        CreateId(entity);
                    }
                }
            }
            finally
            {
                command.Transaction.Commit();
                command.Connection.Close();
            }
            return entities;
        }

        private object Convert(object value, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return System.Convert.ToBoolean(value);
                case TypeCode.Byte:
                    return System.Convert.ToByte(value);
                case TypeCode.Char:
                    return System.Convert.ToChar(value);
                case TypeCode.DBNull:
                case TypeCode.Empty:
                    return null;
                case TypeCode.DateTime:
                    return System.Convert.ToDateTime(value);
                case TypeCode.Decimal:
                    return System.Convert.ToDecimal(value);
                case TypeCode.Double:
                    return System.Convert.ToDouble(value);
                case TypeCode.Int16:
                    return System.Convert.ToInt16(value);
                case TypeCode.Int32:
                    return System.Convert.ToInt32(value);
                case TypeCode.Int64:
                    return System.Convert.ToInt64(value);
                case TypeCode.Object:
                    return value;
                case TypeCode.SByte:
                    return System.Convert.ToSByte(value);
                case TypeCode.Single:
                    return System.Convert.ToSingle(value);
                case TypeCode.String:
                    try
                    {
                        return System.Convert.ToString(value);
                    }
                    catch
                    {
                        return value == null ? null : value.ToString();
                    }
                case TypeCode.UInt16:
                    return System.Convert.ToUInt16(value);
                case TypeCode.UInt32:
                    return System.Convert.ToUInt32(value);
                case TypeCode.UInt64:
                    return System.Convert.ToUInt64(value);
                default:
                    break;
            }
            return value;
        }

        private T Read<T, U>(IDbCommand command, Evaluant.Uss.SqlExpressions.ValuedParameter[] parameters)
            where T : class, IEnumerable<U>
        {
            command.Connection.Open();
            command.Transaction = driver.BeginTransaction(command.Connection);
            List<U> result = new List<U>();
            try
            {
                CreateParams(command, parameters);
                using (IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (TypeResolver.IsPrimitive(typeof(U)))
                        {
                            object value = reader.GetValue(0);
                            U item;
                            if (DBNull.Value == value)
                                item = default(U);
                            else
                                item = (U)Convert(reader.GetValue(0), typeof(U));
                            result.Add(item);
                        }
                        else
                        {
                            var ctor = typeof(U).GetConstructor(Type.EmptyTypes);
                            if (ctor == null) // In case of anonymous type
                            {
                                ctor = typeof(U).GetConstructors()[0];
                                ParameterInfo[] ctorParameters = ctor.GetParameters();
                                if (reader.FieldCount != ctorParameters.Length)
                                    throw new NotSupportedException("There are more fields retrieved in the SQL query than the parameters of the constructor");
                                object[] paramValues = new object[reader.FieldCount];
                                for (int i = 0; i < reader.FieldCount; i++)
                                    paramValues[i] = reader.GetValue(i);
                                result.Add((U)ctor.Invoke(paramValues));
                            }
                            else
                            {
                                U item = (U)typeof(U).GetConstructor(Type.EmptyTypes).Invoke(null);
                                result.Add(item);
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    PropertyInfo prop = typeof(U).GetProperty(reader.GetName(i), BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.Instance);
                                    if (prop != null)
                                        prop.SetValue(item, reader.GetValue(i), null);
                                }
                            }
                        }
                    }
                }
                command.Transaction.Commit();
            }
            catch
            {
                if (command != null && command.Transaction != null)
                    command.Transaction.Rollback();
                throw;
            }
            finally
            {
                command.Connection.Close();
            }
            return result as T;
        }

        private void CreateParams(IDbCommand command, Evaluant.Uss.SqlExpressions.ValuedParameter[] parameters)
        {
            foreach (SqlExpressions.ValuedParameter param in parameters)
                command.Parameters.Add(Driver.GetParameter(command, param));
        }

        private T Read<T>(IDbCommand command, Evaluant.Uss.SqlExpressions.ValuedParameter[] parameters)
        {
            IList<T> list;
            if (typeof(T) == typeof(Domain.Entity))
                list = (IList<T>)ReadEntities(command, parameters);
            else
                list = (IList<T>)Read<List<T>, T>(command, parameters);
            if (list.Count == 0)
                return default(T);
            return list[0];
        }

        #region IStorageEngine Members

        //public IPersistenceProvider Factory { get; set; }

        //public List<Evaluant.Uss.Domain.Entity> Load(Evaluant.NLinq.Expressions.Expression query, params Evaluant.NLinq.Expressions.Parameter[] parameters)
        //{
        //    ExpressionTransformer transformer = new ExpressionTransformer(mapping);
        //    return ReadEntities(driver.GetCommand(driver.CreateConnection(), dialect.Render(transformer.Visit(query))), ConvertParameters(parameters));
        //}

        private Evaluant.Uss.SqlExpressions.ValuedParameter[] ConvertParameters(Dictionary<string, object> parameters)
        {
            List<ValuedParameter> sqlParameters = new List<ValuedParameter>();
            foreach (var parameter in parameters)
            {
                var param = new ValuedParameter(parameter.Key, parameter.Value);
                if (param.Value != null)
                    param.Type = Driver.GetDbType(param.Value.GetType());
                sqlParameters.Add(param);
            }
            return sqlParameters.ToArray();
        }

        //public double LoadScalar(Evaluant.NLinq.Expressions.Expression query, params Evaluant.NLinq.Expressions.Parameter[] parameters)
        //{
        //    return LoadScalar<double>(query, parameters);
        //}

        //public T LoadScalar<T>(Evaluant.NLinq.Expressions.Expression query, Evaluant.NLinq.Expressions.Parameter[] parameters)
        //{
        //    ExpressionTransformer transformer = new ExpressionTransformer(mapping);
        //    if (typeof(IEnumerable<>).IsAssignableFrom(typeof(T)))
        //    {
        //        return Read<T>(driver.GetCommand(driver.CreateConnection(), dialect.Render(transformer.Visit(query))), ConvertParameters(parameters));
        //    }
        //    return Read<T>(driver.GetCommand(driver.CreateConnection(), dialect.Render(transformer.Visit(query))), ConvertParameters(parameters));
        //}

        //public IEnumerable<U> LoadScalar<T, U>(Evaluant.NLinq.Expressions.Expression query, Evaluant.NLinq.Expressions.Parameter[] parameters)
        //    where T : IEnumerable<U>
        //{
        //    ExpressionTransformer transformer = new ExpressionTransformer(mapping);
        //    return Read<T, U>(driver.GetCommand(driver.CreateConnection(), dialect.Render(transformer.Visit(query))), ConvertParameters(parameters));
        //}

        public override void ProcessCommands(Transaction transaction)
        {
            SqlCommandProcessor processor = new SqlCommandProcessor(this);
            processor.Visit(transaction);
            processor.Commit();
        }

        public override void AfterProcessCommands(Transaction transaction)
        {
            base.AfterProcessCommands(transaction);
        }

        public override void InitializeRepository()
        {
            if (Driver.SupportDataManipulationLanguage)
            {
                InitializeRepository(Driver, Dialect);
            }
            else if (AlternateDriver != null && AlternateDriver.SupportDataManipulationLanguage)
            {
                InitializeRepository(AlternateDriver, AlternateDialect);
            }
        }

        private void InitializeRepository(IDriver driver, IDialect dialect)
        {
            IDbConnection connection = driver.CreateConnection();
            connection.Open();
            IDbTransaction transaction = null;
            try
            {
                transaction = driver.BeginTransaction(connection);

                List<string> list = new List<string>();

                foreach (var entity in mapping.Entities)
                {
                    foreach (var reference in entity.Value.References.Values)
                    {
                        bool isFirst = reference.Rules.Count > 1;

                        foreach (var rule in reference.Rules)
                        {
                            if (list.Contains(reference.Name + "_" + rule.ParentTableName))
                                continue;

                            list.Add(reference.Name + "_" + rule.ParentTableName);

                            if (isFirst)
                            {
                                isFirst = false;
                                IDbCommand command = driver.GetCommand(connection, dialect.Render((IDbExpression)new IfStatement(new Exists((Expression)dialect.FindFK(rule.ParentTable.Schema, "FK_" + rule.ParentTable.TableName + "_" + reference.Name, rule.ChildTable)), new AlterTableDropStatement()
                                {
                                    ConstraintName = "FK_" + rule.ParentTableName + "_" + reference.Name,
                                    Table = rule.ChildTable,
                                }, null)));
                                command.Transaction = transaction;
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                IDbCommand command = driver.GetCommand(connection, dialect.Render((IDbExpression)new IfStatement(new Exists((Expression)dialect.FindFK(rule.ParentTable.Schema, "FK_" + rule.ParentTableName + "_" + reference.Name, rule.ParentTable)), new AlterTableDropStatement()
                                {
                                    ConstraintName = "FK_" + rule.ParentTableName + "_" + reference.Name,
                                    Table = rule.ParentTable,
                                }, null)));
                                command.Transaction = transaction;
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }

                foreach (Table table in mapping.Tables.Values)
                {
                    IDbCommand command = driver.GetCommand(connection, dialect.Render((IDbExpression)MakeDropTableCommand(table)));
                    command.Transaction = transaction;
                    command.ExecuteNonQuery();
                }

                foreach (Table table in mapping.Tables.Values)
                {
                    IDbCommand command = driver.GetCommand(connection, dialect.Render((IDbExpression)new IfStatement(dialect.SchemaExists(table.Schema), new SqlExpressions.Functions.Exec(new SchemaStatement(StatementType.Insert, table.Schema)), null)));
                    command.Transaction = transaction;
                    command.ExecuteNonQuery();

                    command = driver.GetCommand(connection, dialect.Render((IDbExpression)MakeCreateTableCommand(table)));
                    command.Transaction = transaction;
                    command.ExecuteNonQuery();
                }

                list = new List<string>();

                foreach (var entity in mapping.Entities)
                {
                    foreach (var reference in entity.Value.References.Values)
                    {
                        bool isFirst = reference.Rules.Count > 1;

                        foreach (var rule in reference.Rules)
                        {
                            if (list.Contains(reference.Name + "_" + rule.ParentTableName))
                                continue;

                            list.Add(reference.Name + "_" + rule.ParentTableName);

                            if (isFirst)
                            {
                                isFirst = false;
                                IDbCommand command = driver.GetCommand(connection, dialect.Render((IDbExpression)new AlterTableAddStatement()
                                {
                                    ConstraintName = "FK_" + rule.ParentTableName + "_" + reference.Name,
                                    Table = rule.ChildTable,
                                    Constraint = new SqlExpressions.Statements.ForeignKeyConstraint()
                                    {
                                        Fields = rule.ChildFields,
                                        ReferencesTable = rule.ParentTable,
                                        References = rule.ParentFields
                                    }
                                }));
                                command.Transaction = transaction;
                                command.ExecuteNonQuery();
                            }
                            else
                            {
                                IDbCommand command = driver.GetCommand(connection, dialect.Render((IDbExpression)new AlterTableAddStatement()
                                {
                                    ConstraintName = "FK_" + rule.ParentTableName + "_" + reference.Name,
                                    Table = rule.ParentTable,
                                    Constraint = new SqlExpressions.Statements.ForeignKeyConstraint()
                                    {
                                        Fields = rule.ParentFields,
                                        ReferencesTable = rule.ChildTable,
                                        References = rule.ChildFields
                                    }
                                }));
                                command.Transaction = transaction;
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                connection.Close();

            }
        }

        private DbStatement MakeCreateTableCommand(Table table)
        {
            return new CreateTableStatement(table);
        }

        #endregion
        private DbStatement MakeDropTableCommand(Table table)
        {
            return new IfStatement(new Exists(dialect.FindTable(table)), new DropTableStatement(table), null);
        }


        public override IList<Evaluant.Uss.Domain.Entity> LoadWithId(string type, string[] ids)
        {
            Mapping.Entity entity = mapping[type];
            if (entity == null)
                throw new ArgumentException(type + "could not be found in the mapping", "type");
            Evaluant.NLinq.NLinqQuery query = new NLinq.NLinqQuery("from " + type + " e in context select e");
            query.Parameters.Add("ids", ids);
            BinaryExpression constraint = null;
            foreach (string id in ids)
            {
                BinaryExpression entityConstraint = null;
                if (entity.Ids.Count > 1)
                    throw new InvalidOperationException("LoadWithId should not be used when the entity key is represented by more than one attribute");
                foreach (Mapping.Attribute attribute in entity.Ids.Values)
                    entityConstraint = new BinaryExpression(BinaryExpressionType.Equal, new MemberExpression(new Identifier(attribute.Name), new Identifier("e")), new ValueExpression(id, attribute.Model.TypeCode));
                if (constraint == null)
                    constraint = entityConstraint;
                else
                    constraint = new BinaryExpression(BinaryExpressionType.Or, constraint, entityConstraint);
            }
            ((QueryExpression)query.Expression).QueryBody.Clauses.Add(new WhereClause(constraint));
            return LoadMany<IList<Domain.Entity>, Domain.Entity>(query);
        }

        public override void LoadReference(string reference, IEnumerable<Domain.Entity> entities, NLinq.NLinqQuery queryUsedToLoadEntities)
        {
            var firstEntity = entities.GetEnumerator().Current;
            var references = Load(GetQuery(reference, firstEntity, queryUsedToLoadEntities));
            foreach (Domain.Entity parent in entities)
            {
                parent.InferredReferences.Add(reference);
                Domain.Entry @ref = parent[reference];
                if (@ref == null)
                {
                    Model.Reference referenceModel = Factory.Model.GetReference(parent.Type, reference);
                    if (referenceModel.Cardinality.IsToMany)
                        @ref = new Domain.MultipleEntry(reference);
                    else
                        @ref = Domain.Entry.Create<Domain.Entity>(reference, State.UpToDate, null);
                }
                foreach (Domain.Entity target in references)
                {
                    Model.Entity model = Factory.Model[parent.Type];
                    bool match = true;
                    foreach (Model.Attribute attribute in model.Attributes.Values)
                    {
                        if (attribute.IsId && parent[attribute.Name].Value != target["parent_" + attribute.Name])
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        if (@ref.IsMultiple)
                            ((Domain.MultipleEntry)@ref).Add(target);
                        else
                            @ref.Value = target;
                        foreach (Model.Attribute attribute in model.Attributes.Values)
                        {
                            if (attribute.IsId)
                            {
                                target.Remove("parent_" + attribute.Name);
                            }
                        }
                    }
                }
            }
        }

        private NLinq.NLinqQuery GetQuery(string reference, Domain.Entity firstEntity, NLinq.NLinqQuery queryUsedToLoadEntities)
        {
            var referenceMapping = mapping[firstEntity.Type].References[reference];
            var query = GetQuery(reference, queryUsedToLoadEntities);
            ((QueryExpression)query.Expression).QueryBody.Clauses.Add(new WhereClause(referenceMapping.WhereExpression));
            ((QueryExpression)query.Expression).QueryBody.Clauses.Add(new WhereClause(referenceMapping.OrderByExpression));
            return query;
        }

        protected override T LoadSingle<T>(Evaluant.NLinq.NLinqQuery query)
        {
            ExpressionTransformer transformer = new ExpressionTransformer(this);
            return Read<T>(driver.GetCommand(driver.CreateConnection(), dialect.Render(transformer.Visit(query.Expression))), ConvertParameters(query.Parameters));
        }

        protected override T LoadMany<T, U>(Evaluant.NLinq.NLinqQuery query)
        {
            ExpressionTransformer transformer = new ExpressionTransformer(this);
            if (typeof(U) == typeof(Domain.Entity))
                return ReadEntities(driver.GetCommand(driver.CreateConnection(), dialect.Render(transformer.Visit(query.Expression))), ConvertParameters(query.Parameters)) as T;
            return Read<T, U>(driver.GetCommand(driver.CreateConnection(), dialect.Render(transformer.Visit(query.Expression))), ConvertParameters(query.Parameters));
        }

        public override void CreateId(Evaluant.Uss.Domain.Entity e)
        {
            StringBuilder sb = new StringBuilder();
            bool isFrist = true;
            if (e.Type != null)
                foreach (KeyValuePair<string, Mapping.Attribute> attributeMappings in mapping.Entities[e.Type].Ids)
                {
                    if (isFrist)
                        isFrist = false;
                    else
                        sb.Append(",");

                    string value = e.GetString(attributeMappings.Key);
                    if (attributeMappings.Value.Generator == Mapping.Generator.Guid && (value == null || value == Guid.Empty.ToString()))
                    {
                        Guid guid = Guid.NewGuid();
                        e.SetValue(attributeMappings.Key, guid);
                        sb.Append(guid);
                        continue;
                    }
                    if (attributeMappings.Value.Generator == Mapping.Generator.Native && e.State == State.New)
                    {
                        sb = new StringBuilder();
                        break;
                    }

                    sb.Append(value);
                }
            if (sb.Length == 0)
                e.Id = Guid.NewGuid().ToString();
            e.Id = sb.ToString();
        }
    }
}
