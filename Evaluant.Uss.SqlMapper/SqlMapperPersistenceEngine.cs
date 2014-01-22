using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Data.SqlClient;
using Evaluant.OPath;
using SQLObject;
using Evaluant.Uss.Commands;
using Evaluant.Uss.Common;
using Evaluant.Uss.SqlMapper.SqlObjectModel.LDD;
using System.Diagnostics;

namespace Evaluant.Uss.SqlMapper
{

    /// <summary>
    /// Description résumée de XmlPersistenceEngine.
    /// </summary>
    public class SqlMapperPersistenceEngine : PersistenceEngineImplementation
    {
        #region Members

        protected IDbTransaction _Transaction;
        protected IDbConnection _Connection;

        protected IDriver _Driver;
        protected DBDialect _Dialect;

        private ArrayList _CacheEntries;
        protected Mapping _Mapping;

        SqlMapperCommandProcessor _CommandProcessor;

        private static readonly string SWITCH_SQL = "Evaluant.Uss.SqlMapper.Sql";
        private static readonly string SWITCH_SQL_DESC = "Evaluant.Uss.SqlMapper.Sql";

        protected BooleanSwitch traceSqlSwitch = new BooleanSwitch(SWITCH_SQL, SWITCH_SQL_DESC);

        #endregion

        #region Ctor

        public SqlMapperPersistenceEngine()
        {
        }

        public SqlMapperPersistenceEngine(string connectionString, IDriver driver, DBDialect dialect, Mapping mapping, ArrayList cacheEntries, Models.Model model)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }
            if (driver == null)
            {
                throw new ArgumentNullException("driver");
            }
            if (dialect == null)
            {
                throw new ArgumentNullException("dialect");
            }
            if (mapping == null)
            {
                throw new ArgumentNullException("mapping");
            }
            this.Driver = driver;
            this.Dialect = dialect;
            this._Mapping = mapping;
            this._Connection = this._Driver.CreateConnection(connectionString);

            if (_Connection is Evaluant.Uss.Data.FileClient.FileConnection)
            {
                ((Evaluant.Uss.Data.FileClient.FileConnection)_Connection).Dialect = dialect;
            }

            base._Model = model;
            this._CacheEntries = cacheEntries;
            this.Initialize();
        }

        #endregion

        #region Properties

        public IDriver Driver
        {
            get { return _Driver; }
            set
            {
                _Driver = value;
                if (_Driver != null)
                    _Driver.Dialect = _Dialect;
            }
        }

        public DBDialect Dialect
        {
            get { return _Dialect; }
            set
            {
                _Dialect = value;
                if (_Driver != null)
                    _Driver.Dialect = value;
            }
        }

        public IDbConnection Connection
        {
            get { return _Connection; }
            set { _Connection = value; }
        }

        public ArrayList CacheEntries
        {
            get { return _CacheEntries; }
            set { _CacheEntries = value; }
        }

        public Mapping Mapping
        {
            get { return _Mapping; }
            set { _Mapping = value; }
        }

        public BooleanSwitch TraceSqlSwitch
        {
            get { return traceSqlSwitch; }
        }

        #endregion

        #region Initialize

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void InitializeRepository()
        {
            base.InitializeRepository();

            if (_InitCommands == null)
                CreateInitCommands();

            if (_Connection.State != ConnectionState.Open)
            {
                _Connection.Open();
            }

            try
            {
                foreach (ISQLExpression expression in _InitCommands)
                {
                    IDbCommand cmd = null;

                    CreateTableSQLCommand ctCommand = expression as CreateTableSQLCommand;

                    if (ctCommand != null)
                    {

                        DropTableSQLCommand delete_query = new DropTableSQLCommand(ctCommand.TagMapping, ctCommand.TableName);
                        foreach (string sql in _Dialect.RenderQueries(delete_query, _Driver))
                        {
                            try
                            {
                                _Transaction = _Connection.BeginTransaction();

                                cmd = _Driver.CreateCommand(sql, _Connection, _Transaction);

                                if (TraceSqlSwitch.Enabled)
                                {
                                    TraceHelpler.Trace(cmd, _Dialect);
                                }

                                cmd.ExecuteNonQuery();

                                _Transaction.Commit();

                            }
                            catch (Exception e)
                            {
                                _Transaction.Rollback();
                                System.Diagnostics.Trace.WriteLine(e.Message + " on:\n" + sql);
                            }
                        }

                        foreach (string sql in _Dialect.RenderQueries(ctCommand, _Driver))
                        {
                            try
                            {
                                _Transaction = _Connection.BeginTransaction();

                                cmd = _Driver.CreateCommand(sql, _Connection, _Transaction);

                                if (TraceSqlSwitch.Enabled)
                                {
                                    TraceHelpler.Trace(cmd, _Dialect);
                                }

                                cmd.ExecuteNonQuery();

                                _Transaction.Commit();
                            }
                            catch (Exception e)
                            {
                                _Transaction.Rollback();
                                System.Diagnostics.Trace.WriteLine(e.Message + " on:\n" + sql);
                            }
                        }

                    }

                    AlterTableSQLCommand atCommand = expression as AlterTableSQLCommand;

                    if (atCommand != null)
                    {

                        foreach (string sql in _Dialect.RenderQueries(atCommand, _Driver))
                        {
                            try
                            {
                                _Transaction = _Connection.BeginTransaction();

                                cmd = _Driver.CreateCommand(sql, _Connection, _Transaction);

                                if (TraceSqlSwitch.Enabled)
                                {
                                    TraceHelpler.Trace(cmd, _Dialect);
                                }

                                cmd.ExecuteNonQuery();
                                _Transaction.Commit();
                            }
                            catch (Exception e)
                            {
                                _Transaction.Rollback();
                                System.Diagnostics.Trace.WriteLine(e.Message + " on:\n" + sql);
                                // Dropping a constraint might fail during InitializeRepository if it doesn't exist
                            }
                        }
                    }
                }

            }
            finally
            {
                _Connection.Close();
            }

        }

        #endregion

        #region Load

        public override EntitySet Load(OPathQuery query, string[] attributes, string orderby, int first, int max)
        {
            if (first <= 0)
                throw new ArgumentException("first must be greater than 0");

            if (max < 0)
                throw new ArgumentException("max must be none negative");

            if (max == 0 && first != 1)
                throw new ArgumentException("max must be greater than zero if a first index is set");

            SqlMapperTransformer transformer = new SqlMapperTransformer(_Mapping, _Model);

            transformer.Dialect = _Dialect;

            ISQLExpression exp = transformer.TransformToSql((Evaluant.OPath.Expressions.Path)query.Path.Clone(),
                attributes,
                orderby == null || orderby == String.Empty ? new string[0] : orderby.Split(','),
                false);

            if (first != 1 || max != 0)
            {
                OPath.Expressions.Function countFunction = new OPath.Expressions.Function(query.Path, Evaluant.OPath.Expressions.FunctionEnum.Count);
                OPath.Expressions.Call callCount = new OPath.Expressions.Call("eval", new Evaluant.OPath.Expressions.Collections.ConstraintCollection(new OPath.Expressions.Constraint[] { countFunction }));
                query.Expression = callCount;
                int maxEntity = Convert.ToInt32(LoadScalar(query));
                if (maxEntity < first)
                    return new EntitySet();		// paging out of range

                if (max == 0)
                    max = maxEntity;

                if (first + max > maxEntity)
                    max = maxEntity - first + 1;

                exp = _Dialect.Page(exp, ((SelectStatement)exp).OrderByClause, first, max);
            }


            return LoadWithSql(exp, transformer.FullNameAttributes, transformer.ColumnAliasMapping, query.Path.Identifiers.Count > 1);
        }

        public EntitySet LoadWithSql(ISQLExpression query, string[] attributes, Hashtable columnAliasMapping, bool loadReference)
        {
            EntitySet es = new EntitySet();

            _Connection.Open();
            _Transaction = _Driver.BeginTransaction(_Connection);

#if !DEBUG
            try
            {
#endif
                foreach (string sql in _Dialect.RenderQueries(query, Driver))
                {
                    es = LoadSql(sql, attributes, columnAliasMapping, loadReference);
                }

                _Transaction.Commit();
#if !DEBUG
            }
            catch (Exception e)
            {
                _Transaction.Rollback();
                throw e;
            }
            finally
            {
#endif
                _Connection.Close();
#if !DEBUG
            }
#endif
            return es;
        }

        #endregion

        #region LoadWithId

        public override EntitySet LoadWithId(string type, string[] id, string[] attributes)
        {
            for (int i = 0; i < id.Length; i++)
            {
                if (id[i].IndexOf("'") != -1)
                {
                    id[i] = id[i].Replace("'", @"\'");
                }
            }

            string opath = string.Concat(type, "[ id( '", string.Join("', '", id), "' ) ]");
            return Load(opath, attributes, string.Empty, 1, 0);
        }

        #endregion

        #region LoadScalar

        public override object LoadScalar(OPath.OPathQuery query)
        {
            if (query.QueryType == OPathQueryTypeEnum.Path)
            {
                query.QueryType = OPathQueryTypeEnum.Expression;
                query.Expression = new OPath.Expressions.Call("eval", new Evaluant.OPath.Expressions.Collections.ConstraintCollection(new OPath.Expressions.Constraint[] { query.Path }));
            }

            SqlMapperTransformer transformer = new SqlMapperTransformer(_Mapping, _Model);
            transformer.Dialect = Dialect;

            ISQLExpression exp = transformer.TransformScalar(query);

            _Connection.Open();
            _Transaction = _Connection.BeginTransaction();

            object result = null;
            try
            {
                string[] queries = Dialect.RenderQueries(exp);

                IDbCommand command = Driver.CreateCommand(queries[queries.Length - 1], _Connection, _Transaction);

                if (TraceSqlSwitch.Enabled)
                {
                    TraceHelpler.Trace(command, _Dialect);
                }

                object res = command.ExecuteScalar();
                if (res != DBNull.Value)
                    result = res;

                _Transaction.Commit();
            }
            catch
            {
                _Transaction.Rollback();
                throw;
            }
            finally
            {
                _Connection.Close();
            }

            // Ensure the result in an Int32 in case of a count()
            if (query.Expression.Operands[0] is OPath.Expressions.Function
                && ((OPath.Expressions.Function)query.Expression.Operands[0]).Type == Evaluant.OPath.Expressions.FunctionEnum.Count)
                result = Convert.ToInt32(result);

            return result;
        }

        public IDbCommand CreateCommand(string query)
        {
            return Driver.CreateCommand(query, _Connection);
        }

        #endregion

        #region LoadReference

        public override void LoadReference(IEnumerable entities, string[] references)
        {
            //	references = null => don't load references
            if (references == null)
                return;

            //	Group entities by type
            Hashtable orderedEntities = new Hashtable();
            foreach (Entity e in entities)
            {
                if (!orderedEntities.ContainsKey(e.Type))
                    orderedEntities.Add(e.Type, new EntitySet());

                ((EntitySet)orderedEntities[e.Type]).Add(e);

                e.RemoveReference(references);
            }

            //	Process each entity by type
            foreach (string entityType in orderedEntities.Keys)
            {
                Hashtable entityIndex = new Hashtable();
                foreach (Entity entity in ((EntitySet)orderedEntities[entityType]))
                    entityIndex.Add(entity.Id, entity);

                string[] ids = new string[((EntitySet)orderedEntities[entityType]).Count];
                int i = 0;
                foreach (Entity e in (EntitySet)orderedEntities[entityType])
                    ids[i++] = e.Id;

                string id = String.Join("', '", ids);

                Hashtable refs = GetReferenceMappings(entityType, references);

                //	Process each reference of the current entity type
                foreach (string refName in refs.Keys)
                {
                    ReferenceMapping rm = (ReferenceMapping)refs[refName];
                    Evaluant.Uss.Models.Reference referenceModel =
                        _Model.GetReference(rm.EntityParent.Type, refName.Substring(refName.LastIndexOf('.') + 1), true);

                    if (_Mapping.Entities[referenceModel.ParentType] == null)
                        referenceModel = new Evaluant.Uss.Models.Reference(referenceModel.Name, entityType, referenceModel.ChildType, referenceModel.IsComposition, referenceModel.FromMany, referenceModel.ToMany);

                    string parentType = entityType;

                    string opath = String.Concat(parentType, "[ id( '", id, "' ) ].", refName);
                    OPathQuery opathQuery = new OPathQuery(opath);
                    opathQuery.Compile();

                    if (opathQuery.HasErrors)
                        throw new OPathException(opathQuery);

                    SqlMapperTransformer transformer = new SqlMapperTransformer(_Mapping, _Model);

                    transformer.Dialect = _Dialect;

                    string childType = referenceModel.ChildType;

                    //StringCollection attributes = new StringCollection();
                    //if(_Model.GetEntity(rm.EntityChild) != null)
                    //    attributes = GetInheritedAttributes(rm.EntityChild);

                    //string[] atts = new string[attributes.Count];

                    //for(i = 0; i < attributes.Count; i++)
                    //    atts[i] = attributes[i];
                    string[] atts = new string[0];

                    UnionStatement exp = (UnionStatement)transformer.TransformToSql(opathQuery.Path, atts, new string[] { }, false);

                    //	ToDo : Add expression for children types

                    IList childrenSubTypes = _Model.GetTree(childType);

                    foreach (SelectStatement select in exp.SelectExpressions)
                    {
                        EntityMapping entityChild = _Mapping.Entities[referenceModel.ChildType];

                        //	Insert the ParentId field in case of generic reference mapping
                        //	Then, we can add the OrderBy clause
                        for (int pmkIndex = 0; pmkIndex < rm.EntityParent.Ids.Count; pmkIndex++)
                        {
                            PrimaryKeyMapping pkm = rm.EntityParent.Ids[pmkIndex];
                            string parentIdAlias = SqlMapperTransformer.GetParentIdAlias(rm.EntityParent, pkm.Field);
                            if (!FieldExists(select.SelectList, parentIdAlias))
                                select.SelectList.Insert(2 + pmkIndex, new Column(pkm, select.TableAlias, parentIdAlias, parentIdAlias));

                        }
                    }

                    if (rm.OrderBy != null && rm.OrderBy != string.Empty)
                    {
                        exp.OrderByClause = new OrderByClause(exp);
                        string[] orderbies = rm.OrderBy.Split(',');
                        foreach (string orderby in orderbies)
                        {
                            string[] orderbyDirection = orderby.Split(' ');
                            if (orderbyDirection.Length == 1)
                                exp.OrderByClause.Add(new OrderByClauseColumn(orderbyDirection[0]));
                            else
                                exp.OrderByClause.Add(new OrderByClauseColumn(orderbyDirection[0], orderbyDirection[1].ToLower() == "desc"));
                        }
                    }

                    exp.SelectedAllColumns = true;
                    exp.TableAlias = "q";

                    _Connection.Open();
                    _Transaction = _Connection.BeginTransaction();

                    try
                    {
                        foreach (string q in _Dialect.RenderQueries(exp, Driver))
                        {

                            IDbCommand command = _Driver.CreateCommand(q, _Connection, _Transaction);

                            if (TraceSqlSwitch.Enabled)
                            {
                                TraceHelpler.Trace(command, _Dialect);
                            }

                            using (IDataReader reader = command.ExecuteReader())
                            {

                                EntityMapping entityChild = _Mapping.Entities[referenceModel.ChildType];
                                EntityMapping baseEntityChild = entityChild;
                                string parentId = String.Empty;
                                string refId = null;	//String.Empty;	Empty string can be the value 
                                Entity parentEntity = null, reference = null;

                                while (reader.Read())
                                {
                                    string trueType = reader[SqlMapperTransformer.TYPE_ALIAS].ToString();
                                    if (baseEntityChild == null || baseEntityChild.Type != trueType)
                                        entityChild = _Mapping.Entities[trueType];
                                    if (entityChild == null)
                                        throw new MappingNotFoundException(string.Format("The entity [{0}] could not be found the mapping", trueType));
                                    int nbParentIds = _Mapping.Entities[referenceModel.ParentType].Ids.Count;
                                    string[] newParentIds = new string[nbParentIds];

                                    for (int indexId = 0; indexId < nbParentIds; indexId++)
                                        newParentIds[indexId] = reader.GetValue(reader.GetOrdinal("ParentId" + (nbParentIds == 1 ? "" : indexId.ToString()))).ToString().Trim();

                                    string newParentId = string.Join(SqlMapperProvider.IDSEP.ToString(), newParentIds);

                                    int nbChildIds = entityChild.Ids.Count;
                                    string[] newChildIds = new string[nbChildIds];

                                    for (int indexId = 0; indexId < nbChildIds; indexId++)
                                    {
                                        if (entityChild.Ids[indexId].Generator.Name == Evaluant.Uss.SqlMapper.GeneratorMapping.GeneratorType.business)
                                            newChildIds[indexId] = reader[entityChild.GetIdFieldAs(entityChild.Ids[indexId])].ToString().Trim();
                                        else
                                            newChildIds[indexId] = reader.GetValue(1 + nbParentIds + indexId).ToString().Trim();
                                    }
                                    string newReferenceId = string.Join(SqlMapperProvider.IDSEP.ToString(), newChildIds);

                                    // Go the next parent
                                    if (parentId != newParentId)
                                    {
                                        if (parentEntity != null)
                                            parentEntity.State = State.UpToDate;

                                        parentId = newParentId;
                                        parentEntity = entityIndex[parentId] as Entity;
                                        refId = null;	//String.Empty;	Empty string can be the value 
                                    }

                                    // Create a new reference
                                    if (refId != newReferenceId)
                                    {
                                        if (reference != null)
                                            reference.State = State.UpToDate;

                                        reference = new Entity(entityChild.Type);
                                        refId = newReferenceId;
                                        reference.Id = refId;
                                        parentEntity.AddValue(refName, reference, State.UpToDate);

                                    }
                                    ImportAttributes(reader, new string[] { }, _Mapping.Entities[reference.Type], reference, transformer.ColumnAliasMapping);
                                    entityChild = baseEntityChild;
                                }
                                if (parentEntity != null)
                                    parentEntity.State = State.UpToDate;

                                if (reference != null)
                                    reference.State = State.UpToDate;

                                // mark the reference as loaded for each parentEntity
                                foreach (Entity e in entities)
                                    if (!e.InferredReferences.Contains(refName))
                                        e.InferredReferences.Add(refName);
                            }
                        }

                        _Transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        _Transaction.Rollback();
                        throw e;
                    }
                    finally
                    {
                        _Connection.Close();
                    }
                }
            }
        }

        #endregion

        public DataTable ExecuteSql(string query)
        {
            _Connection.Open();

            _Transaction = _Driver.BeginTransaction(_Connection);

            IDbCommand cmd = _Driver.CreateCommand(query, _Connection, _Transaction);

            if (TraceSqlSwitch.Enabled)
            {
                TraceHelpler.Trace(cmd, _Dialect);
            }

            DataTable result = new DataTable();

            using (IDataReader reader = cmd.ExecuteReader())
            {
                result.Load(reader);
            }

            _Transaction.Commit();
            _Connection.Close();

            return result;
        }

        public void ExecuteStoredProcedure(string name, ValuedParameterCollection parameters)
        {
            IDbCommand cmd = _Driver.CreateCommand(name, _Connection, CommandType.StoredProcedure);
            foreach (ValuedParameter parameter in parameters)
            {
                IDbDataParameter p = _Driver.CreateParameter(parameter.Name, parameter.Value, parameter.DbType, parameter.Direction);
                cmd.Parameters.Add(p);
            }

            cmd.Connection.Open();
            try
            {
                cmd.ExecuteNonQuery();
                foreach (ValuedParameter parameter in parameters)
                {
                    if (parameter.Direction == ParameterDirection.InputOutput)
                        parameter.Value = ((IDbDataParameter)cmd.Parameters[_Driver.FormatParameter(parameter.Name)]).Value;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                cmd.Connection.Close();
            }
        }

        #region LoadSql

        /// <summary>
        /// Loads an EntitySet by specifying a raw SQL query
        /// </summary>
        /// <param name="query">The SQL query to execute on the database</param>
        /// <returns></returns>
        public EntitySet LoadSql(string query)
        {
            return LoadSql(query, new string[0], null, false);
        }

        protected EntitySet LoadSql(string query, string[] attributes, Hashtable columnAliasMapping, bool referenceloading)
        {
            EntitySet es = new EntitySet();
            Hashtable entities = new Hashtable();

            IDbCommand cmd = _Driver.CreateCommand(query, _Connection, _Transaction);

            if (TraceSqlSwitch.Enabled)
            {
                TraceHelpler.Trace(cmd, _Dialect);
            }

            using (IDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    // the real type of the entity must be returned as the first column
                    string type = reader[SqlMapperTransformer.TYPE_ALIAS].ToString();

                    EntityMapping em = _Mapping.Entities[type];

                    // Creates the id using all the primary key columns
                    string id;
                    int idPosition = 1;
                    if (referenceloading)
                    {
                        idPosition++;
                        while (idPosition < reader.FieldCount && !reader.GetName(idPosition).ToLower().StartsWith(EntityMapping.PREFIX_ID.ToLower()))
                            idPosition++;
                        if (idPosition == reader.FieldCount)
                            idPosition = 2;
                        id = reader[idPosition].ToString().Trim();
                    }
                    else
                        id = reader[1].ToString().Trim();
                    for (int i = 1; i < em.Ids.Count; i++)
                    {
                        //if (em.Ids[i].Generator.Name == GeneratorMapping.GeneratorType.business)
                        //    id += SqlMapperProvider.IDSEP + reader[em.Attributes.FindByField(em.Ids[i].Field).Name].ToString().Trim();
                        //else
                        id += SqlMapperProvider.IDSEP + reader[i + idPosition].ToString().Trim();
                    }

                    Entity e = entities[String.Concat(type, ":", id)] as Entity;

                    if (e == null)
                    {
                        e = new Entity(type);
                        e.Id = id;
                        e.State = State.UpToDate;
                        es.Add(e);
                        entities.Add(String.Concat(type, ":", id), e);
                    }

                    ImportAttributes(reader, attributes, _Mapping.Entities[e.Type], e, columnAliasMapping);
                }
            }

            return es;
        }

        #endregion

        #region ImportAttributes

        private void ImportAttributes(IDataReader reader, string[] attributes, EntityMapping entityMapping, Entity entity, Hashtable columnAliasMapping)
        {
            if (attributes != null && _Model.GetInheritedAttributes(entity.Type).Count > 0)
            {
                ArrayList attrs = new ArrayList(attributes);
                StringCollection names = new StringCollection();

                AttributeMappingCollection attributeMappings = new AttributeMappingCollection();
                attributeMappings.TypeName = entityMapping.Type;

                // Retrieve attributes of the current entity type and all its sub classes attributes
                foreach (AttributeMapping a in entityMapping.Attributes)
                    attributeMappings.Add(a);

                IList children = _Model.GetTree(entityMapping.Type);

                if (children != null && children.Count > 0)
                {
                    foreach (Evaluant.Uss.Models.Entity e in children)
                    {
                        EntityMapping currentEM = _Mapping.Entities[e.Type];
                        if (currentEM == null)
                            throw new SqlMapperException(string.Concat("Cannot find the entity ", e.Type, " in your mapping file"));
                        foreach (Evaluant.Uss.Models.Attribute a in e.Attributes)
                        {
                            if (attributeMappings[a.Name] == null)
                                if (currentEM.Attributes[a.Name] != null)
                                    attributeMappings.Add(currentEM.Attributes[a.Name]);
                        }
                    }
                }

                foreach (AttributeMapping am in attributeMappings)
                {
                    string fullAttributeName = _Model.GetFullAttributeName(am.Name, am.ParentEntity.Type);

                    // load only specified attributes ?
                    if (((attrs.Count > 0) && !attrs.Contains(fullAttributeName)))
                        continue;

                    //Process generic Attributes
                    string name = string.Empty;
                    if (am.Discriminator != null && am.Discriminator != string.Empty)
                        if (reader[am.Discriminator] != DBNull.Value)
                            name = (string)reader[am.Discriminator];

                    if (columnAliasMapping != null && columnAliasMapping[fullAttributeName] == null)
                        continue;

                    object val = columnAliasMapping == null ? reader[fullAttributeName] : reader[columnAliasMapping[fullAttributeName].ToString()];

                    string type = (am.Discriminator != null) ? reader[am.Discriminator].ToString() : am.Name;
                    if (((type != string.Empty) && !names.Contains(type)) && (type == am.Name))
                    {
                        Type t = am.Type;

                        Evaluant.Uss.Models.Entity current = _Model.GetEntity(entity.Type);


                        if (t == null && name != null && name != string.Empty)
                            t = _Model.GetAttribute(entity.Type, name).Type;

                        string attributeName = am.Name;


                        if (t == null && entity != null && _Model.GetAttribute(entity.Type, attributeName) != null)
                            t = _Model.GetAttribute(entity.Type, attributeName).Type;

                        if (current != null && t == null)
                        {
                            while (_Model.GetParent(current) != null)
                            {
                                current = _Model.GetParent(current);
                                if (_Model.GetAttribute(current.Type, attributeName) != null)
                                    t = _Model.GetAttribute(current.Type, attributeName).Type;
                            }
                        }

                        if (current != null && t == null)
                        {
                            foreach (Evaluant.Uss.Models.Entity child in _Model.GetTree(current.Type))
                                if (_Model.GetAttribute(child.Type, attributeName) != null)
                                    t = _Model.GetAttribute(child.Type, attributeName).Type;
                        }

                        if (((t == null) || (!t.IsValueType && (t != typeof(string)))) && (am.Name == "*"))
                        {
                            object obj2 = Utils.UnSerialize((string)val);
                            t = obj2.GetType();
                        }
                        SerializableType serializable = am.GetSerializableType(am.DbType, t);

                        if (val != DBNull.Value)
                        {
                            object sValue = null;
                            switch (serializable)
                            {

                                case SerializableType.BinarySerialization:
                                    sValue = Utils.UnSerialize((byte[])val);
                                    break;
                                case SerializableType.StringSerialization:
                                    sValue = Utils.UnSerialize((string)val);
                                    t = val.GetType();

                                    break;
                                case SerializableType.Standard:
#if !EUSS11
                                    if ((t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>))))
                                    {
                                        sValue = new System.ComponentModel.NullableConverter(t).ConvertTo(val, t.GetGenericArguments()[0]);
                                    }
                                    else
#endif
                                    {
                                        if (am.DbType == DbType.String)
                                            val = ((string)val).Trim();

                                        sValue = Convert.ChangeType(val, t, CultureInfo.InvariantCulture);
                                    }
                                    break;

                                case SerializableType.String:
                                    if (val.GetType() == t)
                                        sValue = val;
                                    else
                                        sValue = Utils.StringToObject((string)val, t);
                                    break;
                                case SerializableType.Int:
                                    val = Convert.ToInt32(val);
                                    if (t.IsEnum)
                                        sValue = Enum.ToObject(t, val);
                                    else
                                        sValue = Convert.ChangeType(val, t, CultureInfo.InvariantCulture);
                                    break;

                            }

                            if (t == null)
                                t = typeof(object);

                            if (entity.FindEntry(type) == null)
                                entity.AddValue(type, sValue, t, State.UpToDate);
                        }
                        names.Add(type);
                        entity.State = State.UpToDate;
                    }
                }
            }
        }

        #endregion

        #region GetCommands

        private ArrayList _InitCommands;

        public ArrayList CreateInitCommands()
        {
            _InitCommands = new ArrayList();

            ArrayList dropFkCommands = new ArrayList();
            ArrayList createFkCommands = new ArrayList();
            Hashtable commands = new Hashtable();

            foreach (EntityMapping entity in _Mapping.Entities)
            {
                string type = String.Empty;

                // if entity table exist
                //		true : return entity table
                //		false : return a new table
                if (!commands.Contains(entity.Table))
                    commands.Add(entity.Table, new CreateTableSQLCommand(entity, entity.Schema, entity.Table));
                CreateTableSQLCommand entityTab = (CreateTableSQLCommand)commands[entity.Table];


                // add a primary key into entity table
                foreach (PrimaryKeyMapping pkm in entity.Ids)
                {
                    AddPrimaryKeysTo(entityTab, entity.GetIdField(pkm), pkm.Generator);
                }

                // if the entity has a discriminator
                //		true : add a field into entity table
                if (entity.DiscriminatorField != null & entity.DiscriminatorValue != null)
                {
                    AddColumnTo(entityTab, entity.DiscriminatorField, DbType.AnsiString, 255, 0, 0, false, null);
                    //GeneratorMapping discriminatorGenerator=new GeneratorMapping();
                    //discriminatorGenerator.Name=GeneratorMapping.GeneratorType.business;
                    //AddPrimaryKeysTo(entityTab, entity.DiscriminatorField, discriminatorGenerator);
                }

                // add all fields
                if (entity.Attributes != null)
                {
                    foreach (AttributeMapping attribute in entity.Attributes)
                    {
                        // if the attribute has a same table to entity
                        //		true : add the field attribute into entity table
                        if (attribute.Table == entity.Table)
                        {
                            AddColumnTo(entityTab, attribute.Field, attribute.DbType, attribute.Size, attribute.Precision, attribute.Scale, attribute.IsNotNull, attribute.DefaultValue);
                        }


                            //		false : create an attribute table
                        else
                        {
                            // if attribute table exist
                            //		true : return attribute table
                            //		false : return a new table
                            if (!commands.Contains(attribute.Table))
                                commands.Add(attribute.Table, new CreateTableSQLCommand(attribute, attribute.Table));
                            CreateTableSQLCommand attributeTab = (CreateTableSQLCommand)commands[attribute.Table];

                            // if the attribute has a discriminator
                            //		true : add fields into attribute table
                            if (attribute.Discriminator != null && attribute.DiscriminatorValue != null)
                            {
                                AddColumnTo(attributeTab, attribute.Discriminator, DbType.AnsiString, 255, 0, 0, true, null);
                                //								type = _Dialect.ConvertDbTypeToString( DbType.AnsiString, 255, 0, 0);
                                //								AddColumnTo(attributeTab, "TYPE", type, true, null);
                            }

                            // add field attribute into attribute table
                            AddColumnTo(attributeTab, attribute.Field, attribute.DbType, attribute.Size, attribute.Precision, attribute.Scale, attribute.IsNotNull, attribute.DefaultValue);

                            // add field containing Id of the entity into attribute table
                            foreach (PrimaryKeyMapping pkm in entity.Ids)
                            {
                                AddForeignKeysTo(attributeTab, attribute.ParentField, entity.GetIdField(pkm), entity.Table, pkm.Generator, null);
                            }
                        }
                    }
                }

                // add all foreign keys
                if (entity.References != null)
                {
                    foreach (ReferenceMapping reference in entity.References)
                    {
                        if (reference.Rules != null)
                        {
                            for (int index = 0; index < reference.Rules.Count; index++)
                            {
                                RuleMapping rule = reference.Rules[index];

                                if (!commands.Contains(rule.ParentTable))
                                    commands.Add(rule.ParentTable, new CreateTableSQLCommand(rule, rule.ParentTable));

                                CreateTableSQLCommand parentTable = (CreateTableSQLCommand)commands[rule.ParentTable];

                                if (!commands.Contains(rule.ChildTable))
                                    commands.Add(rule.ChildTable, new CreateTableSQLCommand(rule, rule.ChildTable));

                                CreateTableSQLCommand childTable = (CreateTableSQLCommand)commands[rule.ChildTable];

                                if (reference.DiscriminatorField != null && index == 0)
                                {
                                    AddColumnTo(childTable, reference.DiscriminatorField, DbType.AnsiString, 36, 0, 0, false, null);
                                }

                                // Creates constraints if specified and handled by the dialect
                                if (_Dialect.GetDisableForeignKeyScope() != DBDialect.ForeignKeyScope.None && rule.Constraint != null && rule.Constraint != String.Empty)
                                {
                                    AlterTableSQLCommand alter = null;

                                    // Detects the parent of the constraints using the position of he PK
                                    if ((index == 0 && rule.ParentField == reference.EntityParent.IdFields) || (index != 0 && rule.ParentField == reference.Rules[index - 1].ChildField))
                                    {
                                        alter = new AlterTableSQLCommand(null, rule.ChildTable, AlterTypeEnum.Add);
                                        alter.ForeignKeys.Add(new ForeignKey(rule.Constraint, new string[] { rule.ChildField }, rule.ParentTable, new string[] { rule.ParentField }));
                                    }
                                    else
                                    {
                                        alter = new AlterTableSQLCommand(null, rule.ParentTable, AlterTypeEnum.Add);
                                        alter.ForeignKeys.Add(new ForeignKey(rule.Constraint, new string[] { rule.ParentField }, rule.ChildTable, new string[] { rule.ChildField }));
                                    }

                                    createFkCommands.Add(alter);

                                    // Generates the DROP command for this constraint
                                    AlterTableSQLCommand drop = alter.Clone() as AlterTableSQLCommand;
                                    drop.AlterType = AlterTypeEnum.Drop;

                                    dropFkCommands.Add(drop);
                                }


                                GeneratorMapping generator = null;

                                for (int i = 0; i < rule.ParentFields.Length; i++)
                                {
                                    string parentField = rule.ParentFields[i];
                                    string childField = rule.ChildFields[i];

                                    // create a parentColumn
                                    if (reference.EntityParent.Table == rule.ParentTable && reference.EntityParent.Ids[parentField] != null)
                                    {
                                        generator = reference.EntityParent.Ids[parentField].Generator;
                                        AddPrimaryKeysTo(parentTable, parentField, generator);
                                        if (!string.IsNullOrEmpty(rule.ChildDefaultValue))
                                            AddForeignKeysTo(childTable, childField, parentField, rule.ParentTable, generator, rule.ChildDefaultValues[i]);
                                        else
                                            AddForeignKeysTo(childTable, childField, parentField, rule.ParentTable, generator, null);

                                    }


                                    // create a childColumn
                                    if (_Mapping.Entities[reference.EntityChild].Table == childTable.TableName
                                        && _Mapping.Entities[reference.EntityChild].Ids[childField] != null)
                                    {
                                        generator = _Mapping.Entities[reference.EntityChild].Ids[childField].Generator;
                                        AddPrimaryKeysTo(childTable, childField, generator);
                                        if (!string.IsNullOrEmpty(rule.ParentDefaultValue))
                                            AddForeignKeysTo(parentTable, parentField, childField, rule.ChildTable, generator, rule.ParentDefaultValues[i]);
                                        else
                                            AddForeignKeysTo(parentTable, parentField, childField, rule.ChildTable, generator, null);

                                    }

                                }
                            }
                        }
                    }
                }
            }

            _InitCommands.AddRange(dropFkCommands);
            _InitCommands.AddRange(commands.Values);
            _InitCommands.AddRange(createFkCommands);

            return _InitCommands;
        }

        #endregion

        #region Functions

        private bool FieldExists(ExpressionCollection selectList, string fieldName)
        {
            foreach (ISQLExpression item in selectList)
            {
                Column col = item as Column;
                if (col == null)
                    continue;
                if (col.Alias == fieldName)
                    return true;
            }
            return false;
        }

        internal CacheEntityEntry GetCacheEntityEntry(string type)
        {
            foreach (CacheEntityEntry entry in _CacheEntries)
            {
                if (((EntityMapping)entry.Mapping).Type == type)
                    return entry;
            }

            return null;
        }

        private void AddPrimaryKeysTo(CreateTableSQLCommand table, string columnName, GeneratorMapping map_generator)
        {
            if (!table.ColumnDefinitions.Contains(columnName))
            {
                DbType type;
                try
                {
                    type = _Dialect.GetDbTypeToPrimaryKey(map_generator);
                }
                catch (NullReferenceException ex)
                {
                    throw new MappingNotFoundException(string.Format("The type is not defined for the field {0} in the table {1}", columnName, table.TableName));
                }
                int size = _Dialect.GetSizeToPrimaryKey(map_generator);
                ColumnDefinition column = new ColumnDefinition(columnName, type, size);
                if (map_generator.Name == GeneratorMapping.GeneratorType.native)
                {
                    int seed = map_generator.GetParam("seed") != null ? Convert.ToInt32(map_generator.GetParam("seed").Value) : 1;
                    int increment = map_generator.GetParam("increment") != null ? Convert.ToInt32(map_generator.GetParam("increment").Value) : 1;
                    column.EnableAutoIncrement(increment, seed);
                }
                table.ColumnDefinitions.Add(columnName, column);

                if (table.PrimaryKey == null)
                    table.PrimaryKey = new PrimaryKey(String.Concat("PK_", table.TableName), new ColumnDefinition[] { column });
                else
                    table.PrimaryKey.Columns.Add(column);
            }
            else
            {
                if (map_generator.Name == GeneratorMapping.GeneratorType.business)
                {
                    ColumnDefinition column = table.ColumnDefinitions[columnName] as ColumnDefinition;
                    if (table.PrimaryKey == null)
                        table.PrimaryKey = new PrimaryKey(String.Concat("PK_", table.TableName), new ColumnDefinition[] { column });
                    else
                    {
                        if (!table.PrimaryKey.Columns.Contains(column))
                            table.PrimaryKey.Columns.Add(column);
                    }
                }
            }
        }

        private void AddForeignKeysTo(CreateTableSQLCommand table, string columnName, string ref_column_name, string ref_table_name, GeneratorMapping map_generator, object defaultValue)
        {
            if (!table.ColumnDefinitions.Contains(columnName))
            {
                DbType type = _Dialect.GetDbTypeToPrimaryKey(map_generator);

                SqlObjectModel.LDD.ColumnConstraint constraint = null;
                if (defaultValue != null)
                    constraint = new SqlObjectModel.LDD.DefaultConstraint(new Constant(defaultValue, type), false);
                else
                    constraint = new SqlObjectModel.LDD.ColumnConstraint(true);

                int size = _Dialect.GetSizeToPrimaryKey(map_generator);
                ColumnDefinition column = new ColumnDefinition(columnName, type, size, constraint);
                table.ColumnDefinitions.Add(columnName, column);

                // Add foreign keys here

            }
        }

        private void AddColumnTo(CreateTableSQLCommand table, string columnName, DbType type, int size, byte precision, byte scale, bool isNotNull, object defaultValue)
        {
            SqlObjectModel.LDD.ColumnConstraint constraint = null;
            if (defaultValue != null)
                constraint = new SqlObjectModel.LDD.DefaultConstraint(new Constant(defaultValue, type), !isNotNull);
            else
                constraint = new SqlObjectModel.LDD.ColumnConstraint(!isNotNull);

            if (!table.ColumnDefinitions.Contains(columnName))
            {

                ColumnDefinition column = new ColumnDefinition(columnName, type, size, precision, scale, constraint);
                table.ColumnDefinitions.Add(columnName, column);
            }
            else
            {
                ColumnDefinition column = table.ColumnDefinitions[columnName] as ColumnDefinition;
                column.ColumnConstraint = constraint;
            }
        }

        public StringCollection GetInheritedAttributes(string type)
        {
            StringCollection attributes = new StringCollection();
            if (_Model.GetEntity(type) == null)
                throw new UniversalStorageException(string.Concat("Cannot find the type ", type, " in meta data"));

            foreach (Evaluant.Uss.Models.Entity e in _Model.GetTree(type))
                foreach (Evaluant.Uss.Models.Attribute a in e.Attributes)
                    if (!attributes.Contains(a.Name))
                        attributes.Add(a.Name);

            return attributes;
        }

        private Hashtable GetReferenceMappings(string entityType, string[] references)
        {
            //ReferenceMappingCollection refs = new ReferenceMappingCollection();
            Hashtable refs = new Hashtable();
            StringCollection processed = new StringCollection();

            EntityMapping currentEM = _Mapping.Entities[entityType, true];

            //	Load all references
            if (references.Length == 0)
            {
                //	Get the references of the parent entities
                if (entityType != "*")
                {
                    if (_Model.GetEntity(entityType) != null)
                    {
                        foreach (Evaluant.Uss.Models.Reference r in _Model.GetInheritedReferences(entityType))
                            if (!processed.Contains(r.Name))
                            {
                                Evaluant.Uss.Models.Entity parent = _Model.GetEntity(currentEM.Type);
                                while (parent != null)
                                {
                                    EntityMapping parentEM = _Mapping.Entities[parent.Type, true];
                                    if (parentEM.References[r.Name] != null)
                                    {
                                        refs.Add(r.Name, parentEM.References[r.Name, true]);
                                        processed.Add(r.Name);
                                    }
                                    parent = _Model.GetParent(_Model.GetEntity(parent.Type));
                                }
                            }
                    }
                }
            }
            else
            {
                //	Load user defined references
                StringCollection userRefs = new StringCollection();
                userRefs.AddRange(references);

                //	Get the references of the current entity
                foreach (string refName in references)
                {
                    ReferenceMapping rm = null;
                    EntityMapping em = currentEM;
                    foreach (string subRefName in refName.Split('.'))
                    {
                        rm = em.References[subRefName];
                        if (rm != null)
                            em = _Mapping.Entities[rm.EntityChild];
                        else
                            throw new MappingNotFoundException(string.Format("The reference [{0}] could not be found for the entity '{1}'", subRefName, em.Type));
                    }

                    //	If refeMap == null, cannot find the reference in the child
                    //	Then try to get it from parent types
                    if (rm == null)
                    {
                        Evaluant.Uss.Models.Entity current = Model.GetEntity(entityType);
                        if (rm == null)
                        {
                            while (Model.GetParent(current) != null)
                            {
                                current = Model.GetParent(current);
                                if (_Mapping.Entities[current.Type, false] != null)
                                {
                                    em = _Mapping.Entities[current.Type, true];
                                    foreach (string subRefName in refName.Split('.'))
                                    {
                                        rm = em.References[subRefName];
                                        if (rm != null)
                                            em = _Mapping.Entities[rm.EntityChild];
                                        else
                                            throw new MappingNotFoundException(string.Format("The reference [{0}] could not be found for the entity '{1}'", subRefName, em.Type));
                                    }
                                    if (rm != null)
                                        break;
                                }
                            }
                        }
                    }

                    //	Try to get a generic reference mapping
                    if (rm == null)
                        throw new MappingNotFoundException(string.Format("The reference mapping {0} not found", refName));

                    refs.Add(refName, rm);
                    processed.Add(refName);
                }

                //	Get the references of the parent entities
                if (entityType != "*")
                {
                    if (_Model.GetEntity(entityType) != null)
                    {
                        foreach (Evaluant.Uss.Models.Reference r in _Model.GetInheritedReferences(entityType))
                            if (!processed.Contains(r.Name) && userRefs.Contains(r.Name))
                            {
                                Evaluant.Uss.Models.Entity parent = _Model.GetParent(_Model.GetEntity(entityType));
                                while (parent != null)
                                {
                                    EntityMapping parentEM = _Mapping.Entities[parent.Type, true];
                                    if (parentEM.References[r.Name] != null)
                                    {
                                        refs.Add(r.Name, parentEM.References[r.Name]);
                                        processed.Add(r.Name);
                                    }
                                    parent = _Model.GetParent(_Model.GetEntity(parent.Type));
                                }
                            }
                    }
                }
            }

            return refs;
        }

        private string GetParentTableName(FromClause from)
        {
            string parentTableName = string.Empty;
            Table t = from[from.Count - 1];
            if (t.GetType() == typeof(TableSource))
                parentTableName = t.TableAlias;
            else
            {
                if (t.GetType() == typeof(JoinedTable))
                {
                    while (t.GetType() == typeof(JoinedTable))
                    {
                        t = ((JoinedTable)t).RigthTable;
                        if (t.GetType() == typeof(TableSource))
                            break;
                    }
                    parentTableName = t.TableAlias;
                }
            }
            return parentTableName;
        }

        #endregion

        #region ProcessCommands

        public override void ProcessCommands(Transaction tx)
        {
            _CommandProcessor = new SqlMapperCommandProcessor(this);
            _Connection.Open();
            _Transaction = Driver.BeginTransaction(_Connection);
            _CommandProcessor.Transaction = _Transaction;

            try
            {

                foreach (Command cmd in tx.PendingCommands)
                {
                    cmd.Accept(_CommandProcessor);
                    tx.NewIds = new Hashtable();
                    foreach (string oldid in _CommandProcessor.NewIds.Keys)
                        tx.NewIds.Add(oldid, _CommandProcessor.NewIds[oldid]);
                }

                _CommandProcessor.EnableForeignKeys();

                _Transaction.Commit();
            }
            catch
            {
                _Transaction.Rollback();
                throw;
            }
            finally
            {
                _Connection.Close();
            }
        }

        #endregion

    }
}



