using System;
using System.Collections;
using System.Data;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using SQLObject;
using System.Data.SqlClient;
using Evaluant.Uss.Commands;
using Evaluant.Uss.Common;
using Evaluant.Uss.SqlMapper.SqlObjectModel.LDD;

namespace Evaluant.Uss.SqlMapper
{
    /// <summary>
    /// Visitor pattern to execute concrete commands
    /// </summary>
    class SqlMapperCommandProcessor : ICommandProcessor
    {
        private IDbTransaction _Transaction;

        public IDbTransaction Transaction
        {
            get { return _Transaction; }
            set { _Transaction = value; }
        }

        private string _NewId;
        public string NewId
        {
            get { return _NewId; }
            set { _NewId = value; }
        }

        private StringDictionary _NewIds = new StringDictionary();
        public StringDictionary NewIds
        {
            get { return _NewIds; }
            set { _NewIds = value; }
        }

        private IDbConnection _Connection
        {
            get { return _Engine.Connection; }
        }

        private IDriver _Driver
        {
            get { return _Engine.Driver; }
        }

        private DBDialect _Dialect
        {
            get { return _Engine.Dialect; }
        }

        private Mapping _Mapping
        {
            get { return _Engine.Mapping; }
        }

        private SqlMapperPersistenceEngine _Engine;

        public SqlMapperCommandProcessor(SqlMapperPersistenceEngine engine)
        {
            _Engine = engine;
        }

        /// <summary>
        /// Converts an entity's id to the corresponding database one
        /// </summary>
        /// <param name="generator">The <seealso cref="GeneratorMapping.GeneratorType"/> used in the mapping for the entity</param>
        /// <param name="id">The entity's id to convert</param>
        /// <returns>A database generated id or the entity's one</returns>
        private string ConvertId(GeneratorMapping generator, string id)
        {
            switch (generator.Name)
            {
                // Returns the database generated Id instead of the internal one
                case GeneratorMapping.GeneratorType.native:
                case GeneratorMapping.GeneratorType.business:
                case GeneratorMapping.GeneratorType.assigned:
                    if (_NewIds.ContainsKey(id))
                        return _NewIds[id];
                    return id;

                // Returns the internal entity's id
                case GeneratorMapping.GeneratorType.guid:
                    return id;

                case GeneratorMapping.GeneratorType.inherited:
                    return id;
                    throw new NotImplementedException("GeneratorMapping.GeneratorType.inherited"); /// TODO : Implement behaviour for GeneratorType.inherited

                default:
                    return id;
            }
        }

        private int GetIndexOfPrimaryKey(EntityMapping entity, string paramName)
        {
            int indexPrimaryKey = 0;
            for (; indexPrimaryKey < entity.Ids.Count; indexPrimaryKey++)
            {
                if (entity.Ids[indexPrimaryKey].Field == paramName)
                    return indexPrimaryKey;
            }
            return 0;
        }
        private string ConvertId(EntityMapping entity, string paramName, string id)
        {
            string[] ids = id.Split(SqlMapperProvider.IDSEP);
            int indexPrimaryKey = GetIndexOfPrimaryKey(entity, paramName);

            switch (entity.Ids[indexPrimaryKey].Generator.Name)
            {
                // Returns the database generated Id instead of the internal one
                case GeneratorMapping.GeneratorType.native:
                case GeneratorMapping.GeneratorType.business:
                case GeneratorMapping.GeneratorType.assigned:
                    if (_NewIds.ContainsKey(id))
                    {
                        string[] newIds = _NewIds[id].Split(SqlMapperProvider.IDSEP);

                        return newIds[indexPrimaryKey];
                    }
                    return ids[indexPrimaryKey];

                // Returns the internal entity's id
                case GeneratorMapping.GeneratorType.guid:
                    return ids[indexPrimaryKey];

                case GeneratorMapping.GeneratorType.inherited:
                    return ids[indexPrimaryKey];

                default:
                    return ids[indexPrimaryKey];

            }
            return ids[indexPrimaryKey];
        }

        private ParameterDirection GetParameterDirectionToId(GeneratorMapping.GeneratorType generator)
        {
            switch (generator)
            {
                case GeneratorMapping.GeneratorType.native:
                    return ParameterDirection.Output;
                case GeneratorMapping.GeneratorType.business:
                case GeneratorMapping.GeneratorType.guid:
                case GeneratorMapping.GeneratorType.assigned:
                default:
                    return ParameterDirection.Input;
            }
        }

        public void Process(Command c)
        {
        }

        public void Process(CreateEntityCommand c)
        {
            EntityMapping e = _Mapping.Entities[c.Type, true];

            IDbCommand command;

            if (e.Ids[0].Generator.Name == GeneratorMapping.GeneratorType.native)
            {
                if (e.DiscriminatorField != null)
                {
                    StringCollection column_list = new StringCollection();
                    column_list.Add(e.DiscriminatorField);

                    StringCollection value_list = new StringCollection();
                    value_list.Add(c.Type);

                    string query = String.Format(@"INSERT INTO {0}({1}) VALUES ({2})",
                                                 _Dialect.FormatAttribute(e.Table),
                                                 _Dialect.FormatAttribute(e.DiscriminatorField),
                                                 _Driver.FormatParameter("Type"));

                    command = _Driver.CreateCommand(query, _Connection, _Transaction);
                    command.Parameters.Add(_Driver.CreateParameter("Type", DbType.AnsiString, c.Type));

                }
                else
                {
                    string query = String.Format(@"INSERT INTO {0} DEFAULT VALUES", _Dialect.FormatAttribute(e.Table));

                    command = _Driver.CreateCommand(query, _Connection, _Transaction);
                }

                command.Transaction = _Transaction;


                if (_Engine.TraceSqlSwitch.Enabled)
                {
                    TraceHelpler.Trace(command, _Dialect);
                }

                command.ExecuteNonQuery();

                command = _Driver.CreateCommand(_Dialect.GetIdentitySelect(e.Table), _Connection, _Transaction);
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        NewId = Convert.ToString(reader[0]);
                        _NewIds.Add(c.ParentId, NewId);
                    }
                }

            }
            else
            {
                if (e.DiscriminatorField != null)
                {
                    StringCollection column_list = new StringCollection();
                    column_list.Add(e.IdFields);
                    column_list.Add(e.DiscriminatorField);

                    StringCollection value_list = new StringCollection();
                    value_list.Add(_Dialect.FormatValue(c.ParentId, _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator)));
                    value_list.Add(_Dialect.FormatValue(c.Type, DbType.AnsiString));

                    string query = String.Format(@"INSERT INTO {0}({1}, {2}) VALUES ({3}, {4})",
                                                 _Dialect.FormatAttribute(e.Table),
                                                 _Dialect.FormatAttribute(e.IdFields),
                                                 _Dialect.FormatAttribute(e.DiscriminatorField),
                                                 _Driver.FormatParameter("Id"),
                                                 _Driver.FormatParameter("Type"));

                    command = _Driver.CreateCommand(query, _Connection, _Transaction);
                    command.Parameters.Add(_Driver.CreateParameter("Id", c.ParentId, _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), GetParameterDirectionToId(e.Ids[0].Generator.Name)));
                    command.Parameters.Add(_Driver.CreateParameter("Type", DbType.AnsiString, c.Type));

                }
                else
                {
                    StringCollection column_list = new StringCollection();
                    column_list.Add(e.IdFields);

                    StringCollection value_list = new StringCollection();
                    value_list.Add(_Dialect.FormatValue(c.ParentId, _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator)));

                    string query = String.Format(@"INSERT INTO {0}({1}) VALUES ({2})",
                                                 _Dialect.FormatAttribute(e.Table),
                                                 _Dialect.FormatAttribute(e.IdFields),
                                                 _Driver.FormatParameter("Id"));

                    command = _Driver.CreateCommand(query, _Connection, _Transaction);
                    command.Parameters.Add(_Driver.CreateParameter("Id", c.ParentId, _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), GetParameterDirectionToId(e.Ids[0].Generator.Name)));
                }

                command.Transaction = _Transaction;

                if (_Engine.TraceSqlSwitch.Enabled)
                {
                    TraceHelpler.Trace(command, _Dialect);
                }

                command.ExecuteNonQuery();
            }

        }

        private DictionaryEntry FindDictionaryEntry(CacheQueryEntry entry, ArrayList queries)
        {
            foreach (DictionaryEntry d in queries)
            {
                if (d.Key == entry)
                    return d;
            }

            return new DictionaryEntry(entry, _Driver.CreateCommand(_Dialect.RenderQueries(entry.Query, _Driver)[0], _Connection));
        }

        private Hashtable ProcessCreateAttributeCommands(EntityMapping e, ArrayList innerCommands)
        {
            Hashtable table = new Hashtable();

            //
            if (e.Attributes != null)
            {
                foreach (AttributeMapping a in e.Attributes)
                {
                    table.Add(a.Name, new ArrayList());
                }
            }

            foreach (CreateAttributeCommand cc in innerCommands)
            {
                AttributeMapping a = e.Attributes[cc.Name];
                if (a != null)
                {
                    ArrayList list = (ArrayList)table[a.Name];
                    list.Add(cc);
                }
            }

            return table;
        }

        private Hashtable ProcessUpdateAttributeCommands(EntityMapping e, ArrayList innerCommands)
        {
            Hashtable table = new Hashtable();

            //
            if (e.Attributes != null)
            {
                foreach (AttributeMapping a in e.Attributes)
                {
                    table.Add(a, new ArrayList());
                }
            }

            // 
            foreach (UpdateAttributeCommand cc in innerCommands)
            {
                AttributeMapping a = e.Attributes[cc.Name];
                if (a != null)
                {
                    ArrayList list = (ArrayList)table[a];
                    list.Add(cc);
                }
            }

            return table;
        }

        public void Process(CompoundCreateCommand c)
        {
            ArrayList queries = new ArrayList();
            CacheEntityEntry entityEntry = _Engine.GetCacheEntityEntry(c.Type);

            if (entityEntry == null)
                throw new SqlMapperException("Cannot find the Entity " + c.Type + " in the mapping file");

            EntityMapping e = (EntityMapping)entityEntry.Mapping;

            //Get the inherited attributes if one table per sub class hierarchy

            Hashtable table = ProcessCreateAttributeCommands(e, c.InnerCommands);

            foreach (CacheQueryEntry entry in entityEntry.InsertCompoundQueryEntries)
            {
                ArrayList genericEntries = new ArrayList();
                if (entry.IsAttributeGenericQuery)
                {
                    AttributeMapping a = e.Attributes["*"];
                    for (int index = 0; index < ((ArrayList)table[a.Name]).Count; index++)
                    {
                        genericEntries.Add(entry.Clone());
                    }
                }

                foreach (Parameter p in entry.Parameters)
                {
                    switch (p.Name)
                    {
                        // Process parameter value to "Entity Id"
                        case "EntityId":
                            {
                                DictionaryEntry dEntry = FindDictionaryEntry(entry, queries);
                                if (!queries.Contains(dEntry))
                                    queries.Add(dEntry);

                                IDbDataParameter param = _Driver.CreateParameter("EntityId", _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), c.ParentId);
                                ((IDbCommand)dEntry.Value).Parameters.Add(param);
                            }
                            break;

                        // Process parameter value to "Entity Discriminator"
                        case "EntityDiscriminator":
                            {
                                DictionaryEntry dEntry = FindDictionaryEntry(entry, queries);
                                if (!queries.Contains(dEntry))
                                    queries.Add(dEntry);

                                IDbDataParameter param = _Driver.CreateParameter("EntityDiscriminator", DbType.String, c.Type);
                                ((IDbCommand)dEntry.Value).Parameters.Add(param);
                            }
                            break;

                        // Process parameter value to "FK_Entity"
                        case "FK_Entity":
                            {
                                AttributeMapping a = (AttributeMapping)p.TagMapping;
                                if (!entry.IsAttributeGenericQuery && ((ArrayList)table[a.Name]).Count == 0)
                                {
                                    DictionaryEntry dEntry = FindDictionaryEntry(entry, queries);
                                    if (!queries.Contains(dEntry))
                                        queries.Add(dEntry);

                                    IDbDataParameter param = _Driver.CreateParameter("FK_Entity", _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), c.ParentId);
                                    ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                }
                                foreach (CreateAttributeCommand cc in (ArrayList)table[a.Name])
                                {
                                    DictionaryEntry dEntry;
                                    if (entry.IsAttributeGenericQuery)
                                        dEntry = FindDictionaryEntry((CacheQueryEntry)genericEntries[((ArrayList)table[a.Name]).IndexOf(cc)], queries);
                                    else
                                        dEntry = FindDictionaryEntry(entry, queries);


                                    if (!queries.Contains(dEntry))
                                        queries.Add(dEntry);
                                    IDbDataParameter param = _Driver.CreateParameter("FK_Entity", _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), c.ParentId);
                                    ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                }
                            }
                            break;

                        // Process parameter value to "AttributeDiscriminator"
                        case "AttributeDiscriminator":
                            {
                                AttributeMapping a = (AttributeMapping)p.TagMapping;
                                if (!entry.IsAttributeGenericQuery && ((ArrayList)table[a.Name]).Count == 0)
                                {
                                    DictionaryEntry dEntry = FindDictionaryEntry(entry, queries);
                                    if (!queries.Contains(dEntry))
                                        queries.Add(dEntry);

                                    IDbDataParameter param = _Driver.CreateParameter("AttributeDiscriminator", DbType.String, a.Name);
                                    ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                }
                                foreach (CreateAttributeCommand cc in (ArrayList)table[a.Name])
                                {
                                    DictionaryEntry dEntry;
                                    if (entry.IsAttributeGenericQuery)
                                        dEntry = FindDictionaryEntry((CacheQueryEntry)genericEntries[((ArrayList)table[a.Name]).IndexOf(cc)], queries);
                                    else
                                        dEntry = FindDictionaryEntry(entry, queries);

                                    if (!queries.Contains(dEntry))
                                        queries.Add(dEntry);

                                    IDbDataParameter param = _Driver.CreateParameter("AttributeDiscriminator", DbType.String, cc.Name);
                                    ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                }
                            }
                            break;

                        // Process parameter value to "AttributeType"
                        case "AttributeType":
                            {
                                AttributeMapping a = (AttributeMapping)p.TagMapping;
                                foreach (CreateAttributeCommand cc in (ArrayList)table[a.Name])
                                {
                                    DictionaryEntry dEntry;
                                    if (entry.IsAttributeGenericQuery)
                                        dEntry = FindDictionaryEntry((CacheQueryEntry)genericEntries[((ArrayList)table[a.Name]).IndexOf(cc)], queries);
                                    else
                                        dEntry = FindDictionaryEntry(entry, queries);

                                    if (!queries.Contains(dEntry))
                                        queries.Add(dEntry);

                                    IDbDataParameter param = _Driver.CreateParameter("AttributeType", DbType.String, Utils.GetFullName(cc.Type));
                                    ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                }
                            }
                            break;

                        // Process value to attribute
                        default:
                            {
                                AttributeMapping a = (AttributeMapping)p.TagMapping;



                                if (!entry.IsAttributeGenericQuery && ((ArrayList)table[a.Name]).Count == 0)
                                {
                                    bool isPrimaryKey = false;
                                    foreach (PrimaryKeyMapping pkm in e.Ids)
                                    {
                                        if (pkm.Field == a.Field && pkm.Generator.Name == GeneratorMapping.GeneratorType.business)
                                        {
                                            isPrimaryKey = true;
                                            break;
                                        }
                                    }

                                    if (isPrimaryKey && ((ArrayList)table[a.Name]).Count == 0)
                                    {
                                        DictionaryEntry dEntry = FindDictionaryEntry(entry, queries);
                                        if (!queries.Contains(dEntry))
                                            queries.Add(dEntry);

                                        IDbDataParameter param = _Driver.CreateParameter(p.Name, a.DbType, c.ParentId);
                                        ((IDbCommand)dEntry.Value).Parameters.Add(param);

                                    }
                                    else
                                    {
                                        DictionaryEntry dEntry = FindDictionaryEntry(entry, queries);
                                        if (!queries.Contains(dEntry))
                                            queries.Add(dEntry);

                                        // if attributecommand not found : apply a default value
                                        // initialize value to parameter
                                        SerializableType serializableType = a.GetSerializableType(a.DbType, a.Type);
                                        object value = null;
                                        if (a.DefaultValue != null)
                                        {
                                            switch (serializableType)
                                            {
                                                case SerializableType.BinarySerialization:
                                                    value = Utils.SerializeToArray(a.DefaultValue);
                                                    break;
                                                case SerializableType.StringSerialization:
                                                    value = Utils.SerializeToString(a.DefaultValue);
                                                    break;
                                                case SerializableType.Standard:
                                                    if (a.DefaultValue != String.Empty)
                                                        value = a.DefaultValue;
                                                    break;
                                                case SerializableType.String:
                                                    value = Utils.ConvertToString(a.DefaultValue, a.Type);
                                                    break;
                                                case SerializableType.Int:
                                                    value = Convert.ToInt32(a.DefaultValue);
                                                    break;
                                            }
                                        }

                                        object formattedValue = _Dialect.PreProcessValue(value);

                                        IDbDataParameter param = _Driver.CreateParameter(p.Name, a.DbType, formattedValue);
                                        ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                    }
                                }



                                foreach (CreateAttributeCommand cc in (ArrayList)table[a.Name])
                                {
                                    DictionaryEntry dEntry;
                                    if (entry.IsAttributeGenericQuery)
                                        dEntry = FindDictionaryEntry((CacheQueryEntry)genericEntries[((ArrayList)table[a.Name]).IndexOf(cc)], queries);
                                    else
                                        dEntry = FindDictionaryEntry(entry, queries);


                                    if (!queries.Contains(dEntry))
                                        queries.Add(dEntry);

                                    // if attributecommand not find : apply a default value
                                    object commandValue = cc == null ? a.DefaultValue : cc.Value;
                                    // initialize value to parameter
                                    SerializableType serializableType = a.Type == null ? a.GetSerializableType(a.DbType, cc.Type) : a.GetSerializableType(a.DbType, a.Type);
                                    object value = null;
                                    if (commandValue != null)
                                    {
                                        switch (serializableType)
                                        {
                                            case SerializableType.BinarySerialization:
                                                value = Utils.SerializeToArray(commandValue);
                                                break;
                                            case SerializableType.StringSerialization:
                                                value = Utils.SerializeToString(commandValue);
                                                break;
                                            case SerializableType.Standard:
                                                value = commandValue;
                                                break;
                                            case SerializableType.String:
                                                value = Utils.ConvertToString(commandValue, cc.Type);
                                                break;
                                            case SerializableType.Int:
                                                value = Convert.ToInt32(commandValue);
                                                break;
                                        }
                                    }

                                    object formattedValue = _Dialect.PreProcessValue(value);


                                    IDbDataParameter param = a.CreateDbDataParameter(p.Name, formattedValue);
                                    ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                }

                            }
                            break;

                    }
                }
            }



            foreach (DictionaryEntry dEntry in queries)
            {
                IDbCommand cmd = (IDbCommand)dEntry.Value;
                cmd.Transaction = _Transaction;

                if (cmd.Parameters.Contains("@FK_Entity") && _NewIds.ContainsKey((string)((IDbDataParameter)cmd.Parameters["@FK_Entity"]).Value))
                    ((IDbDataParameter)cmd.Parameters["@FK_Entity"]).Value = ConvertId(e.Ids[0].Generator, (string)((IDbDataParameter)cmd.Parameters["@FK_Entity"]).Value);

                if (_Engine.TraceSqlSwitch.Enabled)
                {
                    TraceHelpler.Trace(cmd, _Dialect);
                }

                cmd.ExecuteNonQuery();

                StringCollection ids = new StringCollection();
                foreach (PrimaryKeyMapping pmk in e.Ids)
                {
                    if (pmk.Generator.Name == GeneratorMapping.GeneratorType.business || pmk.Generator.Name == GeneratorMapping.GeneratorType.assigned)
                    {
                        AttributeMapping am = e.Attributes.FindByField(pmk.Field);

                        if (am == null)
                        {
                            if (e.DiscriminatorField == pmk.Field)
                            {
                                ids.Add(e.DiscriminatorValue);
                                continue;
                            }
                            if (pmk.Generator.Name == GeneratorMapping.GeneratorType.business)
                                throw new MappingNotFoundException(string.Format("The field [{0}] is not declared as an attribute in the entity [{1}] where the Id Generator type({2}) need it.)", pmk.Field, e.Table, pmk.Generator.Name));
                        }

                        // identifier l'attribute mapping de la clé primaire
                        foreach (CreateAttributeCommand cc in c.InnerCommands)
                        {
                            bool attributeCommandFound = false;

                            if (pmk.Generator.Name == GeneratorMapping.GeneratorType.business)
                                attributeCommandFound = am.Name == cc.Name;

                            if (pmk.Generator.Name == GeneratorMapping.GeneratorType.assigned)
                                attributeCommandFound = cc.Name == cc.ParentType + "Id" || cc.Name == "Id";

                            if (attributeCommandFound && !_NewIds.ContainsKey(c.ParentId))
                            {
                                ids.Add(cc.Value.ToString());
                                break;
                            }


                        }

                    }

                    if (!((CacheQueryEntry)dEntry.Key).IsAttributeGenericQuery && pmk.Generator.Name == GeneratorMapping.GeneratorType.native)
                    {
                        IDbCommand command = _Driver.CreateCommand(_Dialect.GetIdentitySelect(e.Table), _Connection, _Transaction);
                        using (IDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ids.Add(Convert.ToString(reader[0]));
                            }
                        }
                    }
                }


                if (ids.Count != 0)
                {
                    string[] idsArray = new string[ids.Count];
                    for (int i = 0; i < ids.Count; i++)
                        idsArray[i] = ids[i];

                    _NewIds.Add(c.ParentId, string.Join(SqlMapperProvider.IDSEP.ToString(), idsArray));
                }
            }
        }

        public Hashtable InitializeCompoundUpdateCommand(EntityMapping entity, ArrayList updateCommands)
        {
            Hashtable updateCompoundQueryEntries = new Hashtable();

            if (entity.Attributes != null)
            {
                // Loops through attributes so as to create as many SQL Update Command as Tables
                foreach (UpdateAttributeCommand a in updateCommands)
                {
                    AttributeMapping attributeMapping = entity.Attributes[a.Name];
                    if (attributeMapping == null)
                        throw new SqlMapperException(string.Format("Cannot find the attribute '{0}' of the entity '{1}' in your mapping file", a.Name, entity.Type));

                    CacheQueryEntry entry = null;

                    if (attributeMapping.Discriminator == null)
                    {
                        // Creates an UpdateCommand for each Table and add the Where clause for the command
                        if (!updateCompoundQueryEntries.Contains(attributeMapping.Table))
                        {
                            CacheQueryEntry cacheEntry = new CacheQueryEntry(new UpdateCommand(attributeMapping, attributeMapping.Table));
                            updateCompoundQueryEntries.Add(attributeMapping.Table, cacheEntry);

                            if ((attributeMapping.ParentField == null || attributeMapping.ParentField == string.Empty) && attributeMapping.Table != entity.Table)
                            {
                                attributeMapping.ParentField = entity.IdFields;
                                Parameter param = new Parameter(attributeMapping, "FK_Entity");
                                cacheEntry.Parameters.Add(param);
                                ((UpdateCommand)cacheEntry.Query).WhereClause.SearchCondition.Add(new BinaryLogicExpression(new Column(attributeMapping, attributeMapping.ParentField), BinaryLogicOperator.Equals, param));
                            }
                        }

                        entry = (CacheQueryEntry)updateCompoundQueryEntries[attributeMapping.Table];
                    }
                    else
                    {
                        // Initialization 
                        entry = new CacheQueryEntry(new UpdateCommand(attributeMapping, attributeMapping.Table));

                        // A tag attribute mapping which name = "*", is an implicit generic query (a mapping for all attributes don't describe)
                        // it is differente to a explicit generic mapping (a mapping foreach attribute)
                        entry.IsAttributeGenericQuery = attributeMapping.Name == "*";
                        updateCompoundQueryEntries.Add(attributeMapping.Table, entry);
                    }

                    UpdateCommand query = (UpdateCommand)entry.Query;
                    ArrayList parameters = entry.Parameters;

                    if (!query.ColumnValueCollection.Contains(attributeMapping.Field))
                    {
                        Parameter param = new Parameter(attributeMapping, attributeMapping.Field);
                        parameters.Add(param);
                        query.ColumnValueCollection.Add(attributeMapping.Field, param);

                        if (attributeMapping.Discriminator != null)
                        {
                            if (attributeMapping.Name != "*")
                            {
                                query.WhereClause.SearchCondition.Add(new BinaryLogicExpression(
                                    new Column(attributeMapping, attributeMapping.Discriminator),
                                    BinaryLogicOperator.Equals,
                                    new Constant(attributeMapping.DiscriminatorValue == null ? attributeMapping.Name : attributeMapping.DiscriminatorValue, DbType.String)));


                                param = new Parameter(attributeMapping, "FK_Entity");
                                parameters.Add(param);
                                query.WhereClause.SearchCondition.Add(new BinaryLogicExpression(new Column(attributeMapping, attributeMapping.ParentField), BinaryLogicOperator.Equals, param));
                            }
                            else
                            {
                                param = new Parameter(attributeMapping, "AttributeDiscriminator");
                                parameters.Add(param);
                                query.WhereClause.SearchCondition.Add(new BinaryLogicExpression(new Column(attributeMapping, attributeMapping.Discriminator), BinaryLogicOperator.Equals, param));

                                param = new Parameter(attributeMapping, "FK_Entity");
                                parameters.Add(param);
                                query.WhereClause.SearchCondition.Add(new BinaryLogicExpression(new Column(attributeMapping, attributeMapping.ParentField), BinaryLogicOperator.Equals, param));
                            }
                        }

                        if (attributeMapping.ParentField == null && !entry.ContainsParameter("EntityId") && !entry.ContainsParameter("Id0"))
                        {
                            if (entity.Ids.Count == 1)
                            {
                                param = new Parameter(entity.Ids[0], "EntityId");
                                parameters.Add(param);

                                query.WhereClause.SearchCondition.Add(new BinaryLogicExpression(
                                    new Column(entity, entity.IdFields),
                                    BinaryLogicOperator.Equals,
                                    param));
                            }
                            else
                            {
                                for (int i = 0; i < entity.Ids.Count; i++)
                                {
                                    PrimaryKeyMapping pkId = entity.Ids[i];
                                    param = new Parameter(pkId, string.Concat("Id", i.ToString()));
                                    parameters.Add(param);

                                    query.WhereClause.SearchCondition.Add(new BinaryLogicExpression(
                                        new Column(entity, entity.GetIdField(pkId)),
                                        BinaryLogicOperator.Equals,
                                        param));
                                }
                            }
                        }
                    }
                }
            }

            return updateCompoundQueryEntries;
        }

        public void Process(CompoundUpdateCommand c)
        {
            // As a CompoundUpdateCommand can also contain CreateAttributeCommand we convert them if necessary
            for (int i = 0; i < c.InnerCommands.Count; i++)
                if (c.InnerCommands[i] is CreateAttributeCommand)
                {
                    CreateAttributeCommand uac = c.InnerCommands[i] as CreateAttributeCommand;
                    c.InnerCommands[i] = new UpdateAttributeCommand(uac.ParentId, uac.ParentType, uac.Name, uac.Type, uac.Value);
                }

            ArrayList queries = new ArrayList();
            CacheEntityEntry entityEntry = _Engine.GetCacheEntityEntry(c.ParentType);
            EntityMapping e = (EntityMapping)entityEntry.Mapping;
            Hashtable table = ProcessUpdateAttributeCommands(e, c.InnerCommands);

            foreach (CacheQueryEntry entry in InitializeCompoundUpdateCommand(e, c.InnerCommands).Values)
            {
                ArrayList genericEntries = new ArrayList();
                if (entry.IsAttributeGenericQuery)
                {
                    AttributeMapping a = e.Attributes["*"];
                    for (int index = 0; index < ((ArrayList)table[a]).Count; index++)
                    {
                        genericEntries.Add(entry.Clone());
                    }
                }

                foreach (Parameter p in entry.Parameters)
                {
                    switch (p.Name)
                    {
                        // Process parameter value to "Entity Id"
                        case "EntityId":
                            {
                                DictionaryEntry dEntry = FindDictionaryEntry(entry, queries);
                                if (!queries.Contains(dEntry))
                                    queries.Add(dEntry);

                                if (!((IDbCommand)dEntry.Value).Parameters.Contains("@EntityId"))
                                {
                                    IDbDataParameter param = _Driver.CreateParameter("EntityId", _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), c.ParentId);
                                    ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                }
                            }
                            break;
                        // Process parameter value to "Entity Discriminator"
                        case "EntityDiscriminator":
                            {
                                DictionaryEntry dEntry = FindDictionaryEntry(entry, queries);
                                if (!queries.Contains(dEntry))
                                    queries.Add(dEntry);

                                if (!((IDbCommand)dEntry.Value).Parameters.Contains("@EntityDiscriminator"))
                                {
                                    IDbDataParameter param = _Driver.CreateParameter("EntityDiscriminator", DbType.String, c.ParentType);
                                    ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                }
                            }
                            break;
                        // Process parameter value to "FK_Entity"
                        case "FK_Entity":
                            {
                                AttributeMapping a = (AttributeMapping)p.TagMapping;
                                if (!entry.IsAttributeGenericQuery && ((ArrayList)table[a]).Count == 0)
                                {
                                    DictionaryEntry dEntry = FindDictionaryEntry(entry, queries);
                                    if (!queries.Contains(dEntry))
                                        queries.Add(dEntry);

                                    IDbDataParameter param = _Driver.CreateParameter("FK_Entity", _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), c.ParentId);
                                    ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                }
                                foreach (UpdateAttributeCommand cc in (ArrayList)table[a])
                                {
                                    DictionaryEntry dEntry;
                                    if (entry.IsAttributeGenericQuery)
                                        dEntry = FindDictionaryEntry((CacheQueryEntry)genericEntries[((ArrayList)table[a]).IndexOf(cc)], queries);
                                    else
                                        dEntry = FindDictionaryEntry(entry, queries);


                                    if (!queries.Contains(dEntry))
                                        queries.Add(dEntry);
                                    IDbDataParameter param = _Driver.CreateParameter("FK_Entity", DbType.String, c.ParentId);
                                    ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                }
                            }
                            break;
                        // Process parameter value to "AttributeDiscriminator"
                        case "AttributeDiscriminator":
                            {
                                AttributeMapping a = (AttributeMapping)p.TagMapping;
                                if (!entry.IsAttributeGenericQuery && ((ArrayList)table[a]).Count == 0)
                                {
                                    DictionaryEntry dEntry = FindDictionaryEntry(entry, queries);
                                    if (!queries.Contains(dEntry))
                                        queries.Add(dEntry);

                                    IDbDataParameter param = _Driver.CreateParameter("AttributeDiscriminator", DbType.String, a.Name);
                                    ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                }
                                foreach (UpdateAttributeCommand cc in (ArrayList)table[a])
                                {
                                    DictionaryEntry dEntry;
                                    if (entry.IsAttributeGenericQuery)
                                        dEntry = FindDictionaryEntry((CacheQueryEntry)genericEntries[((ArrayList)table[a]).IndexOf(cc)], queries);
                                    else
                                        dEntry = FindDictionaryEntry(entry, queries);

                                    if (!queries.Contains(dEntry))
                                        queries.Add(dEntry);

                                    IDbDataParameter param = _Driver.CreateParameter("AttributeDiscriminator", DbType.String, cc.Name);
                                    ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                }
                            }
                            break;
                        // Process value to attribute
                        default:
                            {
                                if (p.TagMapping as AttributeMapping != null)
                                {
                                    AttributeMapping a = (AttributeMapping)p.TagMapping;
                                    if (!entry.IsAttributeGenericQuery && ((ArrayList)table[a]).Count == 0)
                                    {
                                        DictionaryEntry dEntry = FindDictionaryEntry(entry, queries);
                                        if (!queries.Contains(dEntry))
                                            queries.Add(dEntry);

                                        // if attributecommand not find : apply a default value
                                        // initialize value to parameter
                                        SerializableType serializableType = a.GetSerializableType(a.DbType, a.Type);
                                        object value = null;
                                        if (a.DefaultValue != null)
                                        {
                                            switch (serializableType)
                                            {
                                                case SerializableType.BinarySerialization:
                                                    value = Utils.SerializeToArray(a.DefaultValue);
                                                    break;
                                                case SerializableType.StringSerialization:
                                                    value = Utils.SerializeToString(a.DefaultValue);
                                                    break;
                                                case SerializableType.Standard:
                                                    if (a.DefaultValue != String.Empty)
                                                        value = a.DefaultValue;
                                                    break;
                                                case SerializableType.String:
                                                    value = Utils.ConvertToString(a.DefaultValue, a.Type);
                                                    break;
                                                case SerializableType.Int:
                                                    value = Convert.ToInt32(a.DefaultValue);
                                                    break;
                                            }
                                        }

                                        object formattedValue = _Dialect.PreProcessValue(value);

                                        IDbDataParameter param = _Driver.CreateParameter(p.Name, a.DbType, formattedValue);
                                        ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                    }
                                    foreach (UpdateAttributeCommand cc in (ArrayList)table[a])
                                    {
                                        DictionaryEntry dEntry;
                                        if (entry.IsAttributeGenericQuery)
                                            dEntry = FindDictionaryEntry((CacheQueryEntry)genericEntries[((ArrayList)table[a]).IndexOf(cc)], queries);
                                        else
                                            dEntry = FindDictionaryEntry(entry, queries);


                                        if (!queries.Contains(dEntry))
                                            queries.Add(dEntry);

                                        // if attributecommand not find : apply a default value
                                        object commandValue = cc == null ? a.DefaultValue : cc.Value;
                                        // initialize value to parameter
                                        SerializableType serializableType = a.Type == null ? a.GetSerializableType(a.DbType, cc.Type) : a.GetSerializableType(a.DbType, a.Type);
                                        object value = null;
                                        if (commandValue != null)
                                        {
                                            switch (serializableType)
                                            {
                                                case SerializableType.BinarySerialization:
                                                    value = Utils.SerializeToArray(commandValue);
                                                    break;
                                                case SerializableType.StringSerialization:
                                                    value = Utils.SerializeToString(commandValue);
                                                    break;
                                                case SerializableType.Standard:
                                                    value = commandValue;
                                                    break;
                                                case SerializableType.String:
                                                    //value = Utils.ConvertToString(commandValue, cc.Type, true);
                                                    value = Utils.ConvertToString(commandValue, cc.Type);
                                                    break;
                                                case SerializableType.Int:
                                                    value = Convert.ToInt32(commandValue);
                                                    break;
                                            }
                                        }

                                        object formattedValue = _Dialect.PreProcessValue(value);

                                        IDbDataParameter param = a.CreateDbDataParameter(p.Name, formattedValue);
                                        ((IDbCommand)dEntry.Value).Parameters.Add(param);
                                    }
                                }
                                else if (p.TagMapping as PrimaryKeyMapping != null)
                                {
                                    string[] parentIds = c.ParentId.Split(SqlMapperProvider.IDSEP);

                                    int idPosition = int.Parse(p.Name.Replace("Id", string.Empty));

                                    DictionaryEntry dEntry = FindDictionaryEntry(entry, queries);
                                    if (!queries.Contains(dEntry))
                                        queries.Add(dEntry);

                                    ((IDbCommand)dEntry.Value).Parameters.Add(_Driver.CreateParameter(p.Name, _Dialect.GetDbTypeToPrimaryKey(e.Ids[idPosition].Generator), ConvertId(e.Ids[idPosition].Generator, parentIds[idPosition])));
                                }
                            }
                            break;
                    }
                }
            }


            foreach (DictionaryEntry dEntry in queries)
            {
                IDbCommand command = (IDbCommand)dEntry.Value;

                // By using OleDb, the order of IDataParameter must match
                // the order of the values in the request
                _Driver.PreProcessQuery(command);

                command.Transaction = _Transaction;

                if (_Engine.TraceSqlSwitch.Enabled)
                {
                    TraceHelpler.Trace(command, _Dialect);
                }

                command.ExecuteNonQuery();
            }
        }

        public void Process(DeleteEntityCommand c)
        {
            EntityMapping deleteEntityMapping = _Mapping.Entities[c.Type, true];
            Hashtable cmds = new Hashtable();

            foreach (ReferenceMapping refm in deleteEntityMapping.References)
            {
                Evaluant.Uss.Models.Reference reference = _Engine.Model.GetReference(c.Type, refm.Name);
                EntityMapping entityChildMapping = _Mapping.Entities[refm.EntityChild];

                if (!reference.ToMany)
                    continue;
                foreach (RuleMapping rulem in refm.Rules)
                {
                    if (rulem.ParentTable == deleteEntityMapping.Table)
                    {
                        // update child table
                        SQLCommand cmd = (SQLCommand)cmds[rulem.ChildTable];
                        EntityMapping childEntity = _Mapping.Entities[refm.EntityChild];
                        if (!reference.IsComposition && rulem.ChildTable == childEntity.Table)
                        {
                            if (cmd == null)
                            {
                                cmd = new UpdateCommand(rulem, rulem.ChildTable);
                                cmds.Add(rulem.ChildTable, cmd);
                            }
                        }
                        else
                        {
                            if (cmd == null)
                            {
                                cmd = new DeleteCommand(rulem, rulem.ChildTable);
                                cmds.Add(rulem.ChildTable, cmd);
                            }
                        }

                        //if (cmd is DeleteCommand)
                        //    continue;

                        for (int i = 0; i < rulem.ParentFields.Length; i++)
                        {
                            DbType dbType;
                            if (deleteEntityMapping.IdFields.Contains(refm.Rules[0].ParentFields[i]))
                            {
                                dbType = _Dialect.GetDbTypeToPrimaryKey(deleteEntityMapping.Ids[refm.Rules[0].ParentFields[i]].Generator);
                            }
                            else
                            {
                                dbType = _Dialect.GetDbTypeToPrimaryKey(childEntity.Ids[refm.Rules[0].ChildFields[i]].Generator);
                            }


                            Constant newValue;
                            if (rulem.ParentDefaultValues.Length > i && !string.IsNullOrEmpty(rulem.ParentDefaultValues[i]))
                                newValue = new Constant(rulem.ParentDefaultValues[i], dbType);
                            else
                                newValue = new Constant(DBNull.Value, dbType);

                            // SSX 28/01/08: add contains test
                            if (entityChildMapping.Ids[rulem.ChildFields[i]] == null && cmd is UpdateCommand
                                && !((UpdateCommand)cmd).ColumnValueCollection.Contains(rulem.ChildFields[i]))
                                ((UpdateCommand)cmd).ColumnValueCollection.Add(rulem.ChildFields[i], newValue);

                            Column childField = new Column(rulem, rulem.ChildFields[i]);
                            Constant childParameter = new Constant(ConvertId(deleteEntityMapping, rulem.ParentFields[i], c.ParentId), dbType);
                            BinaryLogicExpression parentClause = new BinaryLogicExpression(childField, BinaryLogicOperator.Equals, childParameter);
                            if (cmd is UpdateCommand)
                                ((UpdateCommand)cmd).WhereClause.SearchCondition.Add(parentClause);
                            else
                                ((DeleteCommand)cmd).Condition.SearchCondition.Add(parentClause);
                        }

                        if (!string.IsNullOrEmpty(rulem.Constraint))
                            DisableForeignKeys(rulem.ChildTable);
                    }
                }
            }

            // Deletes all references to and from it first
            foreach (EntityMapping em in _Mapping.Entities)
            {
                foreach (ReferenceMapping refm in em.References)
                {
                    //Evaluant.Uss.Models.Model childModel = _Engine.Model.GetEntity(

                    if (refm.EntityChild == deleteEntityMapping.Type) // To
                    {
                        for (int indexRule = 0; indexRule < refm.Rules.Count; indexRule++)
                        {
                            RuleMapping rulem = refm.Rules[indexRule];
                            if ((rulem.ChildField == deleteEntityMapping.IdFields &&
                                deleteEntityMapping.DiscriminatorField == null) ||
                                (rulem.ChildField + ";" + deleteEntityMapping.DiscriminatorField == deleteEntityMapping.IdFields &&
                                deleteEntityMapping.DiscriminatorField != null))
                            {
                                // delete childtable

                                if (cmds.ContainsKey(rulem.ChildTable))
                                    continue;

                                DeleteCommand cmd = new DeleteCommand(rulem, rulem.ChildTable);
                                cmds.Add(rulem.ChildTable, cmd);

                                for (int index = 0; index < rulem.ChildFields.Length; index++)
                                {
                                    DbType dbType = _Dialect.GetDbTypeToPrimaryKey(deleteEntityMapping.Ids[index].Generator);

                                    cmd.Condition.SearchCondition.Add(new BinaryLogicExpression(
                                        new Column(rulem, rulem.ChildFields[index]),
                                        BinaryLogicOperator.Equals,
                                        new Constant(ConvertId(deleteEntityMapping, rulem.ChildFields[index], c.ParentId), dbType)
                                    ));
                                }


                                if (!string.IsNullOrEmpty(refm.DiscriminatorField) && indexRule == refm.Rules.Count - 1)
                                {
                                    cmd.Condition.SearchCondition.Add(new BinaryLogicExpression(
                                        new Column(rulem, refm.DiscriminatorField),
                                        BinaryLogicOperator.Equals,
                                        new Parameter(rulem, refm.DiscriminatorValue)
                                        ));
                                }

                                if (!string.IsNullOrEmpty(rulem.Constraint))
                                    DisableForeignKeys(rulem.ChildTable);
                            }

                            if (rulem.ParentField != em.IdFields && em.DiscriminatorField != null
                                || rulem.ParentTable != em.Table)
                            {
                                // update parent table
                                SQLCommand cmd = (SQLCommand)cmds[rulem.ParentTable];
                                if (cmd == null)
                                {
                                    cmd = new UpdateCommand(rulem, rulem.ParentTable);
                                    cmds.Add(rulem.ParentTable, cmd);
                                }

                                if (cmd is DeleteCommand)
                                    continue;

                                for (int i = 0; i < rulem.ParentFields.Length; i++)
                                {
                                    DbType dbType = _Dialect.GetDbTypeToPrimaryKey(deleteEntityMapping.Ids[i].Generator);

                                    Constant newValue = new Constant(DBNull.Value, dbType);
                                    if (rulem.ParentDefaultValues.Length > i && !string.IsNullOrEmpty(rulem.ParentDefaultValues[i]))
                                        newValue = new Constant(rulem.ParentDefaultValues[i], dbType);

                                    // SSX 28/01/08: add contains test
                                    if (em.Ids[rulem.ParentFields[i]] == null
                                        && !((UpdateCommand)cmd).ColumnValueCollection.Contains(rulem.ParentFields[i]))
                                        ((UpdateCommand)cmd).ColumnValueCollection.Add(rulem.ParentFields[i], newValue);

                                    Column parentField = new Column(rulem, rulem.ParentFields[i]);
                                    Constant parentParameter = new Constant(ConvertId(deleteEntityMapping, rulem.ParentFields[i], c.ParentId), dbType);
                                    BinaryLogicExpression parentClause = new BinaryLogicExpression(parentField, BinaryLogicOperator.Equals, parentParameter);
                                    ((UpdateCommand)cmd).WhereClause.SearchCondition.Add(parentClause);
                                }

                                // UPDATE parentTable SET parentField = NULL WHERE (parentField = @parentValue AND discriminator = 'name's relation')
                                if (!string.IsNullOrEmpty(refm.DiscriminatorField) && indexRule == refm.Rules.Count - 1)
                                {
                                    Column discriminatorField = new Column(rulem, rulem.ParentField);
                                    Constant discriminatorValue = new Constant(refm.DiscriminatorValue, DbType.AnsiString);
                                    BinaryLogicExpression discriminatorClause = new BinaryLogicExpression(discriminatorField, BinaryLogicOperator.Equals, discriminatorValue);
                                    ((UpdateCommand)cmd).WhereClause.SearchCondition.Add(discriminatorClause);
                                }

                                if (!string.IsNullOrEmpty(rulem.Constraint))
                                    DisableForeignKeys(rulem.ParentTable);

                            }
                        }

                    }

                }
            }


            // Deletes all attributes if they are in a separate table
            if (deleteEntityMapping.Attributes != null)
            {
                foreach (AttributeMapping am in deleteEntityMapping.Attributes)
                {
                    if (am.Table != deleteEntityMapping.Table)
                    {
                        DeleteCommand cmd = new DeleteCommand(am, am.Table);
                        for (int index = 0; index < deleteEntityMapping.Ids.Count; index++)
                        {
                            PrimaryKeyMapping pkm = deleteEntityMapping.Ids[index];
                            cmd.Condition.SearchCondition.Add(new BinaryLogicExpression(
                                new Column(am, am.ParentFields[index]),
                                BinaryLogicOperator.Equals,
                                new Constant(ConvertId(deleteEntityMapping, pkm.Field, c.ParentId), _Dialect.GetDbTypeToPrimaryKey(pkm.Generator))));
                        }
                        cmds.Add(am.Table, cmd);
                    }
                }
            }

            // SSX 28/01/08: remove table if already existing
            if (cmds.Contains(deleteEntityMapping.Table))
            {
                cmds.Remove(deleteEntityMapping.Table);
            }

            if (!cmds.ContainsKey(deleteEntityMapping.Table))
            {
                DeleteCommand cmd = new DeleteCommand(deleteEntityMapping, deleteEntityMapping.Table);
                for (int index = 0; index < deleteEntityMapping.Ids.Count; index++)
                {
                    PrimaryKeyMapping pkm = deleteEntityMapping.Ids[index];
                    cmd.Condition.SearchCondition.Add(new BinaryLogicExpression(
                        new Column(deleteEntityMapping, pkm.Field),
                        BinaryLogicOperator.Equals,
                        new Constant(ConvertId(deleteEntityMapping, pkm.Field, c.ParentId), _Dialect.GetDbTypeToPrimaryKey(pkm.Generator))));
                }
                cmds.Add(deleteEntityMapping.Table, cmd);
            }

            foreach (string tableName in cmds.Keys)
            {

                foreach (string query in _Dialect.RenderQueries((ISQLExpression)cmds[tableName]))
                {
                    if (!string.IsNullOrEmpty(query))
                    {
                        IDbCommand cmd = _Driver.CreateCommand(query, _Connection, _Transaction);
                        if (_Engine.TraceSqlSwitch.Enabled)
                            TraceHelpler.Trace(cmd, _Dialect);

                        cmd.ExecuteNonQuery();
                    }
                }


            }


        }

        private string GetWhereId(PrimaryKeyMappingCollection pkms)
        {
            string[] contraints = new string[pkms.Count];
            for (int i = 0; i < pkms.Count; i++)
            {
                contraints[i] = String.Concat(
                    _Dialect.FormatAttribute(pkms[i].Field),
                    " = ",
                    _Driver.FormatParameter("Id" + i.ToString()));
            }

            return String.Concat("( ", String.Join(" AND ", contraints), " )");
        }

        public void Process(CreateAttributeCommand c)
        {
            EntityMapping e = _Mapping.Entities[c.ParentType, true];
            AttributeMapping a = e.Attributes[c.Name, true];

            IDbCommand command;

            // teste si les attributs doivent être mis dans la même table
            if (e.Table == a.Table)
            {

                command = _Driver.CreateCommand(String.Format(@"UPDATE {0} SET {1} = {2} WHERE {3}",
                    _Dialect.FormatAttribute(e.Table),
                    _Dialect.FormatAttribute(a.Field),
                    _Driver.FormatParameter("Value"),
                    GetWhereId(e.Ids)), _Connection, _Transaction);

                SerializableType serializableType = a.Type == null ? a.GetSerializableType(a.DbType, c.Type) : a.GetSerializableType(a.DbType, a.Type);
                object value = null;
                if (c.Value != null)
                {
                    switch (serializableType)
                    {
                        case SerializableType.BinarySerialization:
                            value = Utils.SerializeToArray(c.Value);
                            break;
                        case SerializableType.StringSerialization:
                            value = Utils.SerializeToString(c.Value);
                            break;
                        case SerializableType.Standard:
                            value = c.Value;
                            break;
                        case SerializableType.String:
                            value = Utils.ConvertToString(c.Value, c.Type);
                            break;
                        case SerializableType.Int:
                            value = (int)c.Value;
                            break;
                    }
                }
                command.Parameters.Add(a.CreateDbDataParameter("Value", value));

                string[] parentIds = c.ParentId.Split(SqlMapperProvider.IDSEP);
                for (int i = 0; i < e.Ids.Count; i++)
                {
                    command.Parameters.Add(_Driver.CreateParameter("Id" + i.ToString(), _Dialect.GetDbTypeToPrimaryKey(e.Ids[i].Generator), ConvertId(e.Ids[i].Generator, parentIds[i])));
                }
            }
            else
            {
                if (a.Discriminator != null)
                {
                    string query = String.Format(@"INSERT INTO {0}( {1}, {2}, {3}) VALUES ( {4}, {5}, {6})",
                                                 _Dialect.FormatAttribute(a.Table),
                                                 _Dialect.FormatAttribute(a.Discriminator),
                                                 _Dialect.FormatAttribute(a.Field),
                                                 _Dialect.FormatAttribute(a.ParentField),
                                                 _Driver.FormatParameter("Name"),
                                                 _Driver.FormatParameter("Value"),
                                                 _Driver.FormatParameter("FK_Entity"));

                    command = _Driver.CreateCommand(query, _Connection, _Transaction);
                    command.Parameters.Add(_Driver.CreateParameter("Name", DbType.AnsiString, a.DiscriminatorValue == "*" ? c.Name : a.DiscriminatorValue));

                    SerializableType serializableType = a.Type == null ? a.GetSerializableType(a.DbType, c.Type) : a.GetSerializableType(a.DbType, a.Type);
                    object value = null;
                    if (c.Value != null)
                    {
                        switch (serializableType)
                        {
                            case SerializableType.BinarySerialization:
                                value = Common.Utils.SerializeToArray(c.Value);
                                break;
                            case SerializableType.StringSerialization:
                                value = Common.Utils.SerializeToString(c.Value);
                                break;
                            case SerializableType.Standard:
                                value = c.Value;
                                break;
                            case SerializableType.String:
                                value = Common.Utils.ConvertToString(c.Value, c.Type);
                                break;
                            case SerializableType.Int:
                                value = (int)c.Value;
                                break;
                        }
                    }
                    command.Parameters.Add(a.CreateDbDataParameter("Value", value));
                    command.Parameters.Add(_Driver.CreateParameter("FK_Entity", _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), ConvertId(e.Ids[0].Generator, c.ParentId)));

                }
                else
                {
                    string sValue;
                    if (c.Type.IsValueType || c.Type == typeof(String)) // string is not a ValueType
                        sValue = Common.Utils.ConvertToString(c.Value, c.Type);
                    else
                        sValue = Common.Utils.SerializeToString(c.Value);

                    string query = String.Format(@"INSERT INTO {0}({1}, {2}) VALUES ( {3}, {4})",
                                                 _Dialect.FormatAttribute(a.Table),
                                                 _Dialect.FormatAttribute(a.Field),
                                                 _Dialect.FormatAttribute(a.ParentField),
                                                 _Driver.FormatParameter("Value"),
                                                 _Driver.FormatParameter("FK_Entity"));

                    command = _Driver.CreateCommand(query, _Connection, _Transaction);
                    command.Parameters.Add(_Driver.CreateParameter("Value", DbType.AnsiString, sValue));
                    command.Parameters.Add(_Driver.CreateParameter("FK_Entity", _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), ConvertId(e.Ids[0].Generator, c.ParentId)));
                }
            }

            command.Transaction = _Transaction;

            if (_Engine.TraceSqlSwitch.Enabled)
            {
                TraceHelpler.Trace(command, _Dialect);
            }

            command.ExecuteNonQuery();

        }

        public void Process(UpdateAttributeCommand c)
        {
            EntityMapping e = _Mapping.Entities[c.ParentType, true];
            AttributeMapping a = e.Attributes[c.Name, true];

            if (a == null)
                throw new Exception(String.Format("The attribute '{0}' of the entity '{1}' is not defined in your mapping file", c.Name, c.ParentType));

            IDbCommand command;
            // teste si les attributs doivent être mis dans la même table
            if (a.Table == null | e.Table == a.Table)
            {

                command = _Driver.CreateCommand(String.Format(@"UPDATE {0} SET {1} = {2} WHERE {3}",
                    _Dialect.FormatAttribute(e.Table),
                    _Dialect.FormatAttribute(a.Field),
                    _Driver.FormatParameter("Value"),
                    GetWhereId(e.Ids)), _Connection, _Transaction);

                SerializableType serializableType = a.Type == null ? a.GetSerializableType(a.DbType, c.Type) : a.GetSerializableType(a.DbType, a.Type);

                switch (serializableType)
                {
                    case SerializableType.BinarySerialization:
                        command.Parameters.Add(a.CreateDbDataParameter("Value", Common.Utils.SerializeToArray(c.Value)));
                        break;
                    case SerializableType.StringSerialization:
                        command.Parameters.Add(a.CreateDbDataParameter("Value", Common.Utils.SerializeToString(c.Value)));
                        break;

                    case SerializableType.Standard:
                        command.Parameters.Add(a.CreateDbDataParameter("Value", c.Value));
                        break;
                    case SerializableType.String:
                        command.Parameters.Add(a.CreateDbDataParameter("Value", Common.Utils.ConvertToString(c.Value, c.Type)));
                        break;
                    case SerializableType.Int:
                        command.Parameters.Add(a.CreateDbDataParameter("Value", Convert.ToInt32(c.Value)));
                        break;
                }

                string[] parentIds = c.ParentId.Split(SqlMapperProvider.IDSEP);
                for (int i = 0; i < e.Ids.Count; i++)
                {
                    command.Parameters.Add(_Driver.CreateParameter("Id" + i.ToString(), _Dialect.GetDbTypeToPrimaryKey(e.Ids[i].Generator), parentIds[i]));
                }
            }
            else
            {
                if (a.Discriminator != null)
                {
                    string query = String.Format(@"UPDATE {0} SET {1} = {4}, TYPE = {5} WHERE {2} = {6} and {3} = {7}",
                                                 _Dialect.FormatAttribute(a.Table),
                                                 _Dialect.FormatAttribute(a.Field),
                                                 _Dialect.FormatAttribute(a.Discriminator),
                                                 _Dialect.FormatAttribute(a.ParentField),
                                                 _Driver.FormatParameter("Value"),
                                                 _Driver.FormatParameter("Type"),
                                                 _Driver.FormatParameter("Name"),
                                                 _Driver.FormatParameter("FK_Entity"));

                    command = _Driver.CreateCommand(query, _Connection, _Transaction);

                    SerializableType serializableType = a.Type == null ? a.GetSerializableType(a.DbType, c.Type) : a.GetSerializableType(a.DbType, a.Type);
                    switch (serializableType)
                    {
                        case SerializableType.BinarySerialization:
                            command.Parameters.Add(a.CreateDbDataParameter("Value", Common.Utils.SerializeToArray(c.Value)));
                            break;
                        case SerializableType.StringSerialization:
                            command.Parameters.Add(a.CreateDbDataParameter("Value", Common.Utils.SerializeToString(c.Value)));
                            break;
                        case SerializableType.Standard:
                            command.Parameters.Add(a.CreateDbDataParameter("Value", c.Value));
                            break;
                        case SerializableType.String:
                            command.Parameters.Add(a.CreateDbDataParameter("Value", Common.Utils.ConvertToString(c.Value, c.Type)));
                            break;
                        case SerializableType.Int:
                            command.Parameters.Add(a.CreateDbDataParameter("Value", (int)c.Value));
                            break;
                    }
                    command.Parameters.Add(_Driver.CreateParameter("Type", DbType.AnsiString, Utils.GetFullName(c.Type)));
                    command.Parameters.Add(_Driver.CreateParameter("Name", DbType.AnsiString, a.DiscriminatorValue == "*" ? c.Name : a.DiscriminatorValue));
                    command.Parameters.Add(_Driver.CreateParameter("FK_Entity", _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), c.ParentId));
                }
                else
                {
                    string query = String.Format(@"UPDATE {0} SET {1} = {3} WHERE {2} = {4}",
                                                 _Dialect.FormatAttribute(a.Table),
                                                 _Dialect.FormatAttribute(a.Field),
                                                 _Dialect.FormatAttribute(a.ParentField),
                                                 _Driver.FormatParameter("Value"),
                                                 _Driver.FormatParameter("FK_Entity"));

                    command = _Driver.CreateCommand(query, _Connection, _Transaction);

                    SerializableType serializableType = a.Type == null ? a.GetSerializableType(a.DbType, c.Type) : a.GetSerializableType(a.DbType, a.Type);

                    switch (serializableType)
                    {
                        case SerializableType.BinarySerialization:
                            command.Parameters.Add(a.CreateDbDataParameter("Value", Common.Utils.SerializeToArray(c.Value)));
                            break;
                        case SerializableType.StringSerialization:
                            command.Parameters.Add(a.CreateDbDataParameter("Value", Common.Utils.SerializeToString(c.Value)));
                            break;
                        case SerializableType.Standard:
                            command.Parameters.Add(a.CreateDbDataParameter("Value", c.Value));
                            break;
                        case SerializableType.String:
                            command.Parameters.Add(a.CreateDbDataParameter("Value", Common.Utils.ConvertToString(c.Value, c.Type)));
                            break;
                        case SerializableType.Int:
                            command.Parameters.Add(a.CreateDbDataParameter("Value", (int)c.Value));
                            break;
                    }

                    command.Parameters.Add(_Driver.CreateParameter("FK_Entity", _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), c.ParentId));
                }
            }

            command.Transaction = _Transaction;

            if (_Engine.TraceSqlSwitch.Enabled)
            {
                TraceHelpler.Trace(command, _Dialect);
            }

            command.ExecuteNonQuery();
        }

        public void Process(DeleteAttributeCommand c)
        {
            EntityMapping e = _Mapping.Entities[c.ParentType, true];
            AttributeMapping a = e.Attributes[c.Name, true];

            IDbCommand command;

            // Specific case
            if (a.Table == null | e.Table == a.Table)
            {
                UpdateCommand upd = new UpdateCommand(a, e.Table);
                upd.ColumnValueCollection.Add(a.Field, new Constant(DBNull.Value, a.DbType));
                upd.WhereClause.SearchCondition.Add(new BinaryLogicExpression(new Column(a, e.IdFields), BinaryLogicOperator.Equals, new Parameter(null, "Id")));
                upd.Accept(_Dialect);

                command = _Driver.CreateCommand(_Dialect.RenderQueries(upd)[0], _Connection, _Transaction);

                /*command = _Driver.CreateCommand(String.Format(@"UPDATE {0} SET {1} = NULL  WHERE ({2} = {3})",
                                                               _Dialect.FormatAttribute(e.Table),
                                                               _Dialect.FormatAttribute(a.Field),
                                                               _Dialect.FormatAttribute(e.IdFields),
                                                               _Driver.FormatParameter("Id")), _Connection, _Transaction);*/

                string parentid = ConvertId(e.Ids[0].Generator, c.ParentId).ToString();
                command.Parameters.Add(_Driver.CreateParameter("Id", _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), parentid));
            }
            // Generic case
            else
            {
                if (a.Discriminator != null)
                {
                    command = _Driver.CreateCommand(String.Format(@"DELETE FROM {0} WHERE ({1} = {3} and {2} = {4})",
                                                                  _Dialect.FormatAttribute(a.Table),
                                                                  _Dialect.FormatAttribute(a.Discriminator),
                                                                  _Dialect.FormatAttribute(a.ParentField),
                                                                  _Driver.FormatParameter("Name"),
                                                                  _Driver.FormatParameter("FK_Entity")), _Connection, _Transaction);

                    command.Parameters.Add(_Driver.CreateParameter("Name", DbType.AnsiString, c.Name));
                    string parentid = ConvertId(e.Ids[0].Generator, c.ParentId).ToString();
                    command.Parameters.Add(_Driver.CreateParameter("FK_Entity", _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), parentid));
                }
                else
                {
                    command = _Driver.CreateCommand(String.Format(@"DELETE FROM {0} WHERE ({1} = {2})",
                                                                   _Dialect.FormatAttribute(a.Table),
                                                                   _Dialect.FormatAttribute(a.ParentField),
                                                                   _Driver.FormatParameter("FK_Entity")), _Connection, _Transaction);

                    string parentid = ConvertId(e.Ids[0].Generator, c.ParentId).ToString();
                    command.Parameters.Add(_Driver.CreateParameter("FK_Entity", _Dialect.GetDbTypeToPrimaryKey(e.Ids[0].Generator), parentid));
                }
            }

            command.Transaction = _Transaction;

            if (_Engine.TraceSqlSwitch.Enabled)
            {
                TraceHelpler.Trace(command, _Dialect);
            }

            command.ExecuteNonQuery();

        }

        public void Process(CreateReferenceCommand c)
        {
            // Exception management
            ReferenceMapping referenceMapping = _Mapping.Entities[c.ParentType, true].References[c.Role, c.ParentType, c.ChildType];

            Evaluant.Uss.Models.Entity current = _Engine.Model.GetEntity(c.ParentType);
            if (referenceMapping == null)
            {

                while (referenceMapping == null && current != null)
                {
                    Evaluant.Uss.Models.Entity currentChild = _Engine.Model.GetEntity(c.ChildType);
                    while (referenceMapping == null && currentChild != null)
                    {
                        referenceMapping = _Mapping.Entities[current.Type, true].References[c.Role, current.Type, currentChild.Type];
                        currentChild = _Engine.Model.GetParent(currentChild);
                    }

                    if (referenceMapping == null)
                        current = _Engine.Model.GetParent(current);
                }

            }

            if (referenceMapping == null)
                throw new MappingNotFoundException(String.Format("Rule mapping [{0}] not found for [{1}]", c.Role, c.ParentType));

            string currentType = string.Empty;
            if (current != null)
                currentType = current.Type;
            else
                currentType = c.ParentType;

            CacheReferenceEntry referenceEntry = _Engine.GetCacheEntityEntry(currentType).GetCreateReferenceEntries(referenceMapping.Name, referenceMapping.EntityParent.Type, referenceMapping.EntityChild);
            foreach (CacheQueryEntry entry in referenceEntry.QueryEntries)
            {
                foreach (string table in referenceEntry.DisableTable)
                    DisableForeignKeys(table);

                string cmdText = _Dialect.RenderQueries(entry.Query, _Driver)[0];
                if (!string.IsNullOrEmpty(cmdText))
                {
                    IDbCommand command = _Driver.CreateCommand(cmdText, _Connection, _Transaction);


                    foreach (Parameter param in entry.Parameters)
                    {
                        if (param.TagMapping is RuleMapping)
                        {
                            if (((RuleMapping)param.TagMapping).IsParentId)
                            {
                                EntityMapping parentMapping = ((RuleMapping)param.TagMapping).ParentReference.EntityParent;
                                string parentid = ConvertId(parentMapping, param.Name, c.ParentId).ToString();
                                if (!command.Parameters.Contains(_Driver.FormatParameter(param.Name)))
                                    command.Parameters.Add(_Driver.CreateParameter(param.Name, _Dialect.GetDbTypeToPrimaryKey(parentMapping.Ids[GetIndexOfPrimaryKey(parentMapping, param.Name)].Generator), parentid));
                            }
                            else
                            {
                                EntityMapping childMapping = _Mapping.Entities[((RuleMapping)param.TagMapping).ParentReference.EntityChild, true];
                                string childid = ConvertId(childMapping, param.Name, c.ChildId).ToString();
                                if (!command.Parameters.Contains(_Driver.FormatParameter(param.Name)))
                                    command.Parameters.Add(_Driver.CreateParameter(param.Name, _Dialect.GetDbTypeToPrimaryKey(childMapping.Ids[GetIndexOfPrimaryKey(childMapping, param.Name)].Generator), childid));
                            }
                        }

                        if (param.TagMapping is ReferenceMapping)
                            command.Parameters.Add(_Driver.CreateParameter(param.Name, DbType.String, c.Role));

                        if (param.TagMapping is EntityMapping)
                        {
                            EntityMapping mapping = param.TagMapping as EntityMapping;

                            IList children = _Engine.Model.GetTree(mapping.Type);
                            StringCollection childrenTypes = new StringCollection();
                            foreach (Evaluant.Uss.Models.Entity child in children)
                                childrenTypes.Add(child.Type);

                            if (childrenTypes.Contains(c.ParentType) && childrenTypes.Contains(c.ChildType))
                            {
                                if (param.UseParentValue)
                                {
                                    string parentid = ConvertId(mapping, param.Name, c.ParentId).ToString();
                                    if (!command.Parameters.Contains(_Driver.FormatParameter(param.Name)))
                                        command.Parameters.Add(_Driver.CreateParameter(param.Name, _Dialect.GetDbTypeToPrimaryKey(mapping.Ids[GetIndexOfPrimaryKey(mapping, param.Name)].Generator), parentid));
                                }
                                else
                                {
                                    string childid = ConvertId(mapping, param.Name, c.ChildId).ToString();
                                    if (!command.Parameters.Contains(_Driver.FormatParameter(param.Name)))
                                        command.Parameters.Add(_Driver.CreateParameter(param.Name, _Dialect.GetDbTypeToPrimaryKey(mapping.Ids[GetIndexOfPrimaryKey(mapping, param.Name)].Generator), childid));
                                }
                            }
                            else
                            {
                                if (childrenTypes.Contains(c.ParentType))
                                {
                                    string parentid = ConvertId(mapping, param.Name, c.ParentId).ToString();
                                    if (!command.Parameters.Contains(_Driver.FormatParameter(param.Name)))
                                        command.Parameters.Add(_Driver.CreateParameter(param.Name, _Dialect.GetDbTypeToPrimaryKey(mapping.Ids[GetIndexOfPrimaryKey(mapping, param.Name)].Generator), parentid));
                                }

                                if (childrenTypes.Contains(c.ChildType))
                                {
                                    string childid = ConvertId(mapping, param.Name, c.ChildId).ToString();
                                    if (!command.Parameters.Contains(_Driver.FormatParameter(param.Name)))
                                        command.Parameters.Add(_Driver.CreateParameter(param.Name, _Dialect.GetDbTypeToPrimaryKey(mapping.Ids[GetIndexOfPrimaryKey(mapping, param.Name)].Generator), childid));
                                }
                            }
                        }
                    }

                    command.Transaction = _Transaction;

                    if (_Engine.TraceSqlSwitch.Enabled)
                    {
                        TraceHelpler.Trace(command, _Dialect);
                    }

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Process(DeleteReferenceCommand c)
        {
            ReferenceMapping referenceMapping = null;
            if (!string.IsNullOrEmpty(c.ChildType))
                referenceMapping = _Mapping.Entities[c.ParentType, true].References[c.Role, c.ChildType];
            else
                referenceMapping = _Mapping.Entities[c.ParentType, true].References[c.Role];

            if (referenceMapping == null)
            {
                Evaluant.Uss.Models.Entity current = _Engine.Model.GetEntity(c.ParentType);
                while (referenceMapping == null && current != null)
                {
                    Evaluant.Uss.Models.Entity currentChild = _Engine.Model.GetEntity(c.ChildType);
                    while (referenceMapping == null && currentChild != null)
                    {
                        referenceMapping = _Mapping.Entities[current.Type, true].References[c.Role, current.Type, currentChild.Type];
                        currentChild = _Engine.Model.GetParent(currentChild);
                    }

                    if (referenceMapping == null)
                        current = _Engine.Model.GetParent(current);
                }
            }

            if (referenceMapping == null)
                throw new Exception(String.Format("The reference '{0}' of the entity '{1}' is not defined in your mapping file", c.Role, c.ParentType));

            EntityMapping parentMapping = referenceMapping.EntityParent;
            EntityMapping childMapping = _Mapping.Entities[referenceMapping.EntityChild, true];

            RuleMappingCollection rules = referenceMapping.Rules;

            bool isParentRefered = true;

            // Delete all records to index tables
            for (int index = 0; index < rules.Count; index++)
            {
                ISQLExpression query = null;

                //if first rule : ParentField is a foreign key ==> update parent table
                if ((index == 0) && (rules[index].ParentField != parentMapping.IdFields))
                {
                    // UPDATE parenttable SET FK_column = NULL WHERE PK_Column = PK_Value
                    query = new UpdateCommand(rules[index], rules[index].ParentTable);

                    for (int i = 0; i < rules[index].ParentFields.Length; i++)
                    {
                        if (parentMapping.Ids[rules[index].ParentFields[i]] == null)
                        {
                            Constant constant = new Constant(DBNull.Value, _Dialect.GetDbTypeToPrimaryKey(childMapping.Ids[i].Generator));
                            if (rules[index].ParentDefaultValues.Length > i && !string.IsNullOrEmpty(rules[index].ParentDefaultValues[i]))
                                constant = new Constant(rules[index].ParentDefaultValues[i], _Dialect.GetDbTypeToPrimaryKey(childMapping.Ids[i].Generator));

                            ((UpdateCommand)query).ColumnValueCollection.Add(rules[index].ParentFields[i], constant);
                        }
                    }

                    for (int i = 0; i < parentMapping.Ids.Count; i++)
                    {
                        string PK_Column = parentMapping.IdFields.Split(SqlMapperProvider.IDSEP)[i];
                        string PK_Value = ConvertId(parentMapping, PK_Column, c.ParentId);
                        ((UpdateCommand)query).WhereClause.SearchCondition.Add(new BinaryLogicExpression(
                             new Column(parentMapping.Ids[i], PK_Column),
                             BinaryLogicOperator.Equals,
                             new Constant(PK_Value, _Dialect.GetDbTypeToPrimaryKey(parentMapping.Ids[i].Generator))));
                    }



                    ExecuteCommand(query);
                }

                //if last rule : ChildField is a foreign key ==> update child table
                if ((index == rules.Count - 1) && (rules[index].ChildField != childMapping.IdFields))
                {
                    // UPDATE childtable SET FK_column = NULL WHERE PK_Column = PK_Value
                    query = new UpdateCommand(rules[index], rules[index].ChildTable);


                    for (int i = 0; i < rules[index].ChildFields.Length; i++)
                    {
                        if (childMapping.Ids[rules[index].ChildFields[i]] == null)
                        {
                            Constant constant = new Constant(DBNull.Value, _Dialect.GetDbTypeToPrimaryKey(parentMapping.Ids[i].Generator));
                            if (rules[index].ChildDefaultValues.Length > i && !string.IsNullOrEmpty(rules[index].ChildDefaultValues[i]))
                                constant = new Constant(rules[index].ChildDefaultValues[i], _Dialect.GetDbTypeToPrimaryKey(parentMapping.Ids[i].Generator));

                            ((UpdateCommand)query).ColumnValueCollection.Add(rules[index].ChildFields[i], constant);
                        }
                    }

                    for (int i = 0; i < childMapping.Ids.Count; i++)
                        ((UpdateCommand)query).WhereClause.SearchCondition.Add(new BinaryLogicExpression(
                           new Column(childMapping.Ids[i], childMapping.IdFields.Split(SqlMapperProvider.IDSEP)[i]),
                           BinaryLogicOperator.Equals,
                           new Constant(ConvertId(childMapping, childMapping.IdFields.Split(SqlMapperProvider.IDSEP)[i], c.ChildId), _Dialect.GetDbTypeToPrimaryKey(childMapping.Ids[i].Generator))));

                    //PrimaryKeyMapping PK_mapping = index == 0 ? parentMapping.Ids[0] : childMapping.Ids[0];
                    //string PK_column = childMapping.IdFields;
                    //string PK_value = c.ChildId;



                    ExecuteCommand(query);
                }

                // Execute a delete command to index table
                if (index != rules.Count - 1)
                {
                    // Delete records to index table with one foreign key : rule(n).ChildField == rule(n+1).ParentField
                    if (rules[index].ChildField == rules[index + 1].ParentField)
                    {
                        // the refered table is parent table ==> WHERE rule(n).ChildField == ParentId
                        if (isParentRefered)
                        {
                            query = new DeleteCommand(rules[index], rules[index].ChildTable);
                            ((DeleteCommand)query).Condition.SearchCondition.Add(new BinaryLogicExpression(
                                new Column(rules[index], rules[index].ChildField),
                                BinaryLogicOperator.Equals,
                                new Constant(c.ParentId, _Dialect.GetDbTypeToPrimaryKey(parentMapping.Ids[0].Generator))));

                        }

                        // the refered table is child table ==>  WHERE rule(n).ChildField == ChildId
                        else
                        {
                            query = new DeleteCommand(rules[index], rules[index].ChildTable);
                            ((DeleteCommand)query).Condition.SearchCondition.Add(new BinaryLogicExpression(
                                new Column(rules[index], rules[index].ChildField),
                                BinaryLogicOperator.Equals,
                                new Constant(c.ChildId, _Dialect.GetDbTypeToPrimaryKey(childMapping.Ids[0].Generator))));
                        }

                    }

                    // Delete records to index table with two foreign key : rule(n).ChildField != rule(n+1).ParentField
                    else
                    {
                        query = new DeleteCommand(rules[index], rules[index].ChildTable);
                        ((DeleteCommand)query).Condition.SearchCondition.Add(new BinaryLogicExpression(
                            new Column(rules[index], rules[index].ChildField),
                            BinaryLogicOperator.Equals,
                            new Constant(c.ParentId, _Dialect.GetDbTypeToPrimaryKey(parentMapping.Ids[0].Generator))));
                        ((DeleteCommand)query).Condition.SearchCondition.Add(new BinaryLogicExpression(
                            new Column(rules[index + 1], rules[index + 1].ParentField),
                            BinaryLogicOperator.Equals,
                            new Constant(c.ChildId, _Dialect.GetDbTypeToPrimaryKey(childMapping.Ids[0].Generator))));
                        isParentRefered = false;
                    }

                    ExecuteCommand(query);
                }
            }
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="expression">The expression.</param>
        private void ExecuteCommand(ISQLExpression expression)
        {
            string[] queries = _Dialect.RenderQueries(expression);


            foreach (string q in queries)
            {
                if (!string.IsNullOrEmpty(q))
                {
                    IDbCommand command = _Driver.CreateCommand(q, _Connection, _Transaction);
                    command.Transaction = _Transaction;

                    if (_Engine.TraceSqlSwitch.Enabled)
                    {
                        TraceHelpler.Trace(command, _Dialect);
                    }

                    command.ExecuteNonQuery();
                }
            }

        }

        private StringCollection _DisabledTableNames = new StringCollection();
        private bool _DisableDataBase = false;

        public void DisableForeignKeys(string tableName)
        {

            if (_DisabledTableNames.Contains(tableName))
                return;

            _DisabledTableNames.Add(tableName);

            DisableForeignKey dfk = null;

            switch (_Dialect.GetDisableForeignKeyScope())
            {
                case DBDialect.ForeignKeyScope.None:
                    return;

                case DBDialect.ForeignKeyScope.DataBase:

                    if (_DisableDataBase)
                        return;

                    _DisableDataBase = true;

                    dfk = new DisableForeignKey(DBDialect.ForeignKeyScope.DataBase);
                    ExecuteCommand(dfk);
                    break;

                case DBDialect.ForeignKeyScope.Table:

                    dfk = new DisableForeignKey(DBDialect.ForeignKeyScope.Table);
                    dfk.Table = tableName;
                    ExecuteCommand(dfk);
                    break;

                case DBDialect.ForeignKeyScope.Constraint:

                    foreach (EntityMapping em in _Mapping.Entities)
                        foreach (ReferenceMapping rm in em.References)
                            for (int index = 0; index < rm.Rules.Count; index++)
                            {
                                RuleMapping rule = rm.Rules[index];
                                if (rule.Constraint != null && rule.Constraint != String.Empty && rule.ParentTable == tableName)
                                {
                                    dfk = new DisableForeignKey(DBDialect.ForeignKeyScope.Constraint);
                                    if ((index == 0 && rule.ParentField == rm.EntityParent.IdFields) || (index != 0 && rule.ParentField == rm.Rules[index - 1].ChildField))
                                    {
                                        dfk.Table = rule.ChildTable;
                                    }
                                    else
                                    {
                                        dfk.Table = rule.ParentTable;
                                    }

                                    dfk.Name = rule.Constraint;
                                    ExecuteCommand(dfk);
                                }
                            }
                    break;
            }
        }

        public void EnableForeignKeys()
        {
            EnableForeignKey dfk = null;

            switch (_Dialect.GetDisableForeignKeyScope())
            {
                case DBDialect.ForeignKeyScope.None:
                    return;

                case DBDialect.ForeignKeyScope.DataBase:

                    if (_DisableDataBase)
                    {
                        dfk = new EnableForeignKey(DBDialect.ForeignKeyScope.DataBase);
                        ExecuteCommand(dfk);
                    }

                    break;

                case DBDialect.ForeignKeyScope.Table:

                    foreach (string tableName in _DisabledTableNames)
                    {
                        dfk = new EnableForeignKey(DBDialect.ForeignKeyScope.Table);
                        dfk.Table = tableName;
                        ExecuteCommand(dfk);
                    }

                    break;

                case DBDialect.ForeignKeyScope.Constraint:

                    foreach (string tableName in _DisabledTableNames)
                    {
                        foreach (EntityMapping em in _Mapping.Entities)
                            foreach (ReferenceMapping refm in em.References)
                                foreach (RuleMapping rm in refm.Rules)
                                {
                                    if (rm.Constraint != null && rm.Constraint != String.Empty && rm.ParentTable == tableName)
                                    {
                                        dfk = new EnableForeignKey(DBDialect.ForeignKeyScope.Constraint);
                                        dfk.Table = tableName;
                                        dfk.Name = rm.Constraint;
                                        ExecuteCommand(dfk);
                                    }
                                }
                    }

                    break;
            }
        }
    }
}
