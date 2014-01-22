using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using Evaluant.OPath;
using Evaluant.OPath.Expressions;
using SQLObject;

namespace Evaluant.Uss.SqlMapper
{
    public class SqlMapperTransformer : OPathVisitor
    {
        public const string TYPE_ALIAS = "EntityType";

        #region Members

        private Mapping _Mapping;
        private Evaluant.Uss.Models.Model _Model;
        private DBDialect _Dialect;

        private Stack entityModelContext = new Stack();

        private Stack entityMappingContext = new Stack();
        private Stack queryContext = new Stack();
        private Stack tableContext = new Stack();
        private Stack sqlExpressionContext = new Stack();
        private Stack linkTableContext = new Stack();

        private string _BaseType = string.Empty;
        bool _IsFirstAttribute = false;

        private Stack _ExprLevel;   // level in opath : expression(link different path) or path
        private static readonly int EXPR = 0;
        private static readonly int PATH = 1;

        private int _AliasColumnIndexNumber = 0;    // used to generate alias column name
        private Hashtable _AliasColumnMapping;      // correspondance between alias/column name in select list and attribute name of entity
        private bool _IsAliasColumnComputed;        // if alias has been computed (only for the first select query in case of union)
        private StringCollection _UserAttributes;   // used to store full attribute names

        private static readonly char DOT = '.';
        #endregion

        #region Ctor

        public SqlMapperTransformer(Mapping mapping, Evaluant.Uss.Models.Model model)
        {
            _Mapping = mapping;
            _Model = model;
            _ExprLevel = new Stack();

            _AliasColumnMapping = new Hashtable();
            _IsAliasColumnComputed = false;

            // Add "Type" and "Id" field. Avoid duplicated field if mapping model contains these two keyword fields.
            //            _AliasColumnMapping.Add("Type", "Type");

            foreach (EntityMapping em in mapping.Entities)
            {
                if (em.DiscriminatorField == null || em.DiscriminatorField == string.Empty)
                    continue;

                if (_AliasColumnMapping.ContainsKey(em.DiscriminatorField))
                    continue;

                _AliasColumnMapping.Add(em.DiscriminatorField, em.DiscriminatorField);
            }

            //_AliasColumnMapping.Add("Id", "Id");
        }

        #endregion

        #region Properties

        public Evaluant.Uss.Models.Model Model
        {
            get { return _Model; }
            set { _Model = value; }
        }

        public DBDialect Dialect
        {
            get { return _Dialect; }
            set { _Dialect = value; }
        }

        /// <summary>
        /// Get a Hashtable containing correspondance between attribute name (key) and its column/alias name in select list (value)
        /// </summary>
        public Hashtable ColumnAliasMapping
        {
            get { return _AliasColumnMapping; }
        }

        /// <summary>
        /// Get an array containnig the fullname attribute (with type) of attributes to load
        /// </summary>
        public string[] FullNameAttributes
        {
            get
            {
                if (_UserAttributes.Count == 0)
                    return null;

                string[] atts = new string[_UserAttributes.Count];
                _UserAttributes.CopyTo(atts, 0);
                return atts;
            }
        }

        #endregion

        #region Aliases

        // Indexes for table's aliases
        private int _AttributesIndex;
        private int _EntitiesIndex;

        private static readonly string TABLEPREFIX = "e";
        private static readonly string ATTRIBUTEPREFIX = "a";

        /// <summary>
        /// Creates a unique table alias.
        /// </summary>
        /// <returns></returns>
        private string CreateUniqueTableAlias()
        {
            return String.Concat(TABLEPREFIX, _EntitiesIndex++);
        }


        /// <summary>
        /// Creates a unique attribute alias.
        /// </summary>
        /// <returns></returns>
        private string CreateUniqueAttributeAlias()
        {
            return String.Concat(ATTRIBUTEPREFIX, _AttributesIndex++);
        }

        #endregion

        #region Process Entity

        public void ProcessEntity(Identifier ident, bool isInRef)
        {
            // Restore current entity mapping context
            EntityMapping entityMapping = (EntityMapping)entityMappingContext.Peek();

            // Initialization
            Table currentTable = new TableSource(entityMapping, entityMapping.Table, CreateUniqueTableAlias());
            Column currentItem = new Column(entityMapping.Ids[0], currentTable.TableAlias, entityMapping.GetIdField(entityMapping.Ids[0]), entityMapping.GetIdFieldAs(entityMapping.Ids[0]));
            SelectStatement currentQuery = new SelectStatement(entityMapping);
            if (!isInRef)
                currentQuery.TableAlias = currentTable.TableAlias;

            // Save current context
            tableContext.Push(currentTable);
            sqlExpressionContext.Push(currentItem);
            queryContext.Push(currentQuery);

            if (entityMapping.DiscriminatorField != null && !isInRef)
            {
                InPredicate ip = new InPredicate(new Column(entityMapping, currentTable.TableAlias, entityMapping.DiscriminatorField));

                string parentType = entityMapping.Type;

                if (_BaseType != null && _BaseType != parentType)
                {
                    Evaluant.Uss.Models.Entity parentEntity = Model.GetEntity(_BaseType);

                    while (Model.GetParent(parentEntity) != null)
                        parentEntity = Model.GetParent(parentEntity);

                    parentType = parentEntity.Type;
                }

                //Evaluant.Uss.Models.Entity parentType = Model.GetEntity(parentType);

                //Get all the children of the queried type
                IList children = _Model.GetTree(parentType);

                ip.SubQueries.Add(new Constant(entityMapping.DiscriminatorValue, DbType.AnsiString));

                StringCollection processed = new StringCollection();

                foreach (Evaluant.Uss.Models.Entity e in children)
                {
                    if (e.Type == parentType)
                        continue;

                    EntityMapping current = _Mapping.Entities[e.Type, true];

                    string currentType = current.DiscriminatorValue;
                    if (processed.Contains(currentType))
                        continue;

                    processed.Add(currentType);

                    ip.SubQueries.Add(new Constant(currentType, DbType.AnsiString));
                }

                currentQuery.WhereClause.SearchCondition.Add(ip);
            }

            // Process constraint : an attribute constraint can modify the context
            if (ident.Constraint != null)
            {
                int stackSizeBefore = entityMappingContext.Count;

                ProcessConstraint(ident.Constraint);

                int stackSizeAfter = entityMappingContext.Count;

                for (int i = stackSizeAfter - 1; i > stackSizeBefore - 1; i--)
                {
                    entityMappingContext.Pop();
                    entityModelContext.Pop();
                }
            }
        }

        #endregion

        #region ConvertToSQL

        public void ConvertToSQL(Path path, bool firstIsType, bool isInConstraint, bool lastIsAttribute)
        {
            ConvertToSQL(path, firstIsType, isInConstraint, lastIsAttribute, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstIsType"></param>
        /// <param name="isInConstraint"></param>
        /// <param name="lastIsAttribute"></param>
        /// <param name="overrideId">True, if the Path is into a constraint and queried an attribute through Reference 
        /// (Person[Brother.MyKind.Name = 'nice'])</param>
        public void ConvertToSQL(Path path, bool firstIsType, bool isInConstraint, bool lastIsAttribute, bool overrideId)
        {
            int index = 0;

            // Process the first entity
            if (firstIsType)
            {
                Identifier ident = path.Identifiers[index++];

                //	Exception management : entity mapping
                EntityMapping entityMapping = _Mapping.Entities[ident.Value, true];

                // Save entity mapping current context
                entityMappingContext.Push(entityMapping);

                entityModelContext.Push(_Model.GetEntity(ident.Value, true));

                ProcessEntity(ident, false);
            }

            // Restore the first entity context
            EntityMapping firstMapping = (EntityMapping)entityMappingContext.Peek();
            Table firstTable = (Table)tableContext.Peek();

            // Process references
            int lastIndex = lastIsAttribute ? path.Identifiers.Count - 1 : path.Identifiers.Count;
            bool isFirstReference = true;
            ArrayList collEpression = new ArrayList();

            int stackCount = entityMappingContext.Count;

            Evaluant.Uss.Models.Entity previousEntityModel = (Evaluant.Uss.Models.Entity)entityModelContext.Peek();

            for (; index < lastIndex; index++)
            {
                Identifier ident = path.Identifiers[index];

                UnionStatement childrenSubExp = new UnionStatement();
                //childrenSubExp.TableAlias = ident.Value;

                Evaluant.Uss.Models.Reference reference =
                    _Model.GetReference(previousEntityModel.Type, ident.Value, true);

                //	ident.Value isn't a reference but an attribute
                if (_Model.GetReference(previousEntityModel.Type, ident.Value) == null &&
                    _Model.GetAttribute(previousEntityModel.Type, ident.Value) != null)
                {
                    EntityMapping tmpMap = _Mapping.Entities[previousEntityModel.Type];
                    entityMappingContext.Push(tmpMap);

                    entityModelContext.Push(_Model.GetEntity(previousEntityModel.Type));

                    continue;
                }

                Evaluant.Uss.Models.Entity currentModel = previousEntityModel;	//Model.GetEntity(previousEntityModel.Type);
                while (_Model.GetParent(currentModel) != null && reference == null)
                {
                    currentModel = Model.GetParent(currentModel);

                    //	ident.Value isn't a reference but an attribute
                    if (_Model.GetReference(currentModel.Type, ident.Value) == null &&
                        _Model.GetAttribute(currentModel.Type, ident.Value) != null)
                        continue;

                    reference = _Model.GetReference(currentModel.Type, ident.Value);
                    if (reference != null)
                        break;
                }

                // Restore parent context
                EntityMapping parentMapping = index == 0 && isInConstraint ? (EntityMapping)entityMappingContext.Peek() : (EntityMapping)entityMappingContext.Pop();
                Evaluant.Uss.Models.Entity parentModel =
                    index == 0 && isInConstraint ? (Evaluant.Uss.Models.Entity)entityModelContext.Peek() : (Evaluant.Uss.Models.Entity)entityModelContext.Pop();

                string cType = reference.ChildType;
                previousEntityModel = _Model.GetEntity(cType, true);

                Table parentTable = (Table)tableContext.Pop();
                SelectStatement parentQuery = (SelectStatement)queryContext.Pop();

                //	Get tree for reference.ChildType
                IList childrenSubTypes = _Model.GetTree(cType);

                // Get all fullname attributes
                StringCollection attributes = new StringCollection();
                attributes.AddRange(_Model.GetFullAttributesNames(cType));

                Hashtable subQueries = new Hashtable();

                foreach (Evaluant.Uss.Models.Entity child in childrenSubTypes)
                {
                    if (_Mapping.Entities[child.Type] == null)
                        continue;

                    string refName = ident.Value;

                    ReferenceMapping referenceMapping = parentMapping.References[refName];

                    Evaluant.Uss.Models.Entity current = Model.GetEntity(parentMapping.Type);
                    if (referenceMapping == null)
                    {
                        while (_Model.GetParent(current) != null)
                        {
                            current = Model.GetParent(current);
                            referenceMapping = _Mapping.Entities[current.Type].References[refName];
                            if (referenceMapping != null)
                                break;
                        }
                    }

                    if (referenceMapping == null)
                        throw new MappingNotFoundException(String.Format("Reference [{0}] not found in Type [{1}]", ident.Value, parentMapping.Type));

                    RuleMappingCollection ruleMapping = referenceMapping.Rules;

                    // Save child mapping context
                    EntityMapping childMapping = _Mapping.Entities[child.Type];
                    entityMappingContext.Push(childMapping);

                    entityModelContext.Push(_Model.GetEntity(child.Type));

                    // Process child mapping reference
                    ProcessEntity(ident, true);

                    // Restore child context
                    Table childTable = (Table)tableContext.Pop();
                    SelectStatement childQuery = (SelectStatement)queryContext.Pop();
                    childQuery.TableAlias = childTable.TableAlias;
                    childQuery.SelectList = new ExpressionCollection();

                    // Begin to the first INNER JOIN 
                    Table righFirstTableJoinTable = null;

                    // la référence précedente a une contrainte avec Attribute Table de type INNER JOIN
                    if (isFirstReference && parentTable.GetType() == typeof(JoinedTable))
                        righFirstTableJoinTable = parentQuery;

                        // la référence précedente suit une autre référence de type INNER JOIN
                    else if (!isFirstReference && parentTable.GetType() == typeof(JoinedTable))
                    {
                        righFirstTableJoinTable = parentTable;
                        foreach (ILogicExpression exp in parentQuery.WhereClause.SearchCondition)
                        {
                            childQuery.WhereClause.SearchCondition.Add(exp);
                        }
                    }


                        // autre cas
                    else if (parentTable.GetType() == typeof(TableSource))
                    {
                        righFirstTableJoinTable = parentTable;
                        // Ajoute les clauses Wheres de la table courante
                        if (!isInConstraint)
                            foreach (ILogicExpression exp in parentQuery.WhereClause.SearchCondition)
                            {
                                childQuery.WhereClause.SearchCondition.Add(exp);
                            }
                    }
                    else
                    {
                        righFirstTableJoinTable = parentTable;
                        if (righFirstTableJoinTable.TableAlias == null || righFirstTableJoinTable.TableAlias == string.Empty)
                            righFirstTableJoinTable.TableAlias = refName;
                    }

                    Table leftFirstJoinTable = ruleMapping.Count != 1 ? new TableSource(ruleMapping[0], ruleMapping[0].ChildTable, "rule" + index) : childTable;

                    string parentFieldTable = righFirstTableJoinTable.TableAlias;

                    for (int indexId = 0; indexId < referenceMapping.EntityParent.Ids.Count; indexId++)
                    {
                        PrimaryKeyMapping pmk = referenceMapping.EntityParent.Ids[indexId];

                        string idfield = string.Empty;
                        if (righFirstTableJoinTable.GetType() == typeof(TableSource))
                            idfield = referenceMapping.EntityParent.GetIdField(pmk);
                        else
                            idfield = referenceMapping.EntityParent.GetIdFieldAs(pmk);

                        string alias = GetParentIdAlias(referenceMapping.EntityParent, pmk.Field);
                        childQuery.SelectList.Add(new Column(ruleMapping[0], parentFieldTable, idfield, alias));
                    }

                    // Multiple Key
                    foreach (PrimaryKeyMapping pkm in childMapping.Ids)
                        childQuery.SelectList.Add(new Column(ruleMapping[ruleMapping.Count - 1], childQuery.TableAlias, childMapping.GetIdField(pkm), childMapping.GetIdFieldAs(pkm)));


                    // adds all foreign keys contained in the current table that will be used by the next reference
                    if (index + 1 < lastIndex)
                    {
                        Identifier nextIdent = path.Identifiers[index + 1];
                        Models.Entity type = child;

                        while (_Mapping.Entities[type.Type].References[nextIdent.Value] == null)
                            type = Model.GetEntity(type.Inherit, true);

                        if (_Mapping.Entities[type.Type].References[nextIdent.Value] == null)
                            break;

                        RuleMappingCollection rules = _Mapping.Entities[type.Type].References[nextIdent.Value].Rules;

                        if (rules.Count == 1
                            && rules[0].ParentField != referenceMapping.EntityParent.IdFields
                            && rules[0].ParentField != childMapping.IdFields)
                        {
                            foreach (string parentField in rules[0].ParentField.Split(SqlMapperProvider.IDSEP))
                                childQuery.SelectList.Add(new Column(rules[0], childQuery.TableAlias, parentField, parentField));


                        }
                    }

                    // specify relation role name
                    if (!string.IsNullOrEmpty(referenceMapping.DiscriminatorField))
                    {
                        childQuery.WhereClause.SearchCondition.Add(
                            new BinaryLogicExpression(
                            new Column(referenceMapping, leftFirstJoinTable.TableAlias, referenceMapping.DiscriminatorField),
                            BinaryLogicOperator.Equals,
                            new Constant(referenceMapping.DiscriminatorValue, DbType.AnsiString)
                            ));
                    }

                    // inner join on index table FK_Parent
                    JoinedTable firstJoinTable = new JoinedTable(leftFirstJoinTable, TypeJoinedTable.Inner, righFirstTableJoinTable);
                    firstJoinTable.TableAlias = leftFirstJoinTable.TableAlias;

                    for (int indexId = 0; indexId < ruleMapping[0].ParentFields.Length; indexId++)
                    {
                        string childField = ruleMapping[0].ChildFields[indexId];
                        string parentField = ruleMapping[0].ParentFields[indexId];
                        string idField = parentMapping.Ids.Count > indexId ? parentMapping.Ids[indexId].Field : string.Empty;

                        string parentid = ruleMapping[0].GetParentFieldAs();
                        if (righFirstTableJoinTable.GetType() == typeof(TableSource) || (ruleMapping.Count == 1 && parentField != idField))
                            parentid = parentField;

                        firstJoinTable.SearchConditions.Add(new BinaryLogicExpression(
                            new Column(ruleMapping[0], righFirstTableJoinTable.TableAlias, parentid),
                            BinaryLogicOperator.Equals,
                            new Column(ruleMapping[0], leftFirstJoinTable.TableAlias, childField)));
                    }

                    if (!string.IsNullOrEmpty(childMapping.DiscriminatorField))
                    {
                        BinaryLogicExpression discriminator = new BinaryLogicExpression(
                            new Column(childMapping, childQuery.TableAlias, childMapping.DiscriminatorField),
                            BinaryLogicOperator.Equals,
                            new Constant(childMapping.DiscriminatorValue, DbType.AnsiStringFixedLength)
                            );
                        childQuery.WhereClause.SearchCondition.Add(discriminator);
                    }

                    if (ruleMapping.Count == 1)
                        childQuery.FromClause.Add(firstJoinTable);

                    // Fin Construction du premier INNER JOIN 
                    if (isInConstraint && ruleMapping.Count == 1 && index == 0)
                    {
                        // Restore the first entity context
                        Table mainTable = (Table)tableContext.Peek();

                        if (ruleMapping[0].ParentFields.Length > 1)
                        {
                            MultipledKey parentKey = new MultipledKey(ruleMapping[0]);
                            MultipledKey childKey = new MultipledKey(ruleMapping[0]);
                            foreach (string key in ruleMapping[0].ParentFields)
                            {
                                parentKey.Collection.Add(new Column(ruleMapping[0], firstTable.TableAlias, key));       // parent table/column link
                                childKey.Collection.Add(new Column(ruleMapping[0], mainTable.TableAlias, key));        // child table/column link   
                            }
                            linkTableContext.Push(parentKey);       // parent table/column link
                            linkTableContext.Push(childKey);        // child table/column link
                        }
                        else
                        {
                            linkTableContext.Push(new Column(ruleMapping[0], firstTable.TableAlias, ruleMapping[0].ParentField));       // parent table/column link
                            linkTableContext.Push(new Column(ruleMapping[0], mainTable.TableAlias, ruleMapping[0].ParentField));        // child table/column link
                        }
                    }

                    tableContext.Push(firstJoinTable);

                    for (int i = 1; i < ruleMapping.Count; i++)
                    {
                        firstJoinTable = (JoinedTable)tableContext.Pop();

                        JoinedTable lastJoinTable = null;

                        // la référence courante possède une contrainte qui est de type INNER JOIN
                        if (ident.Constraint != null && childTable.GetType() == typeof(JoinedTable))
                        {
                            SelectStatement query = new SelectStatement(referenceMapping);
                            query.TableAlias = childTable.TableAlias;

                            lastJoinTable = new JoinedTable(childQuery, TypeJoinedTable.Inner, firstJoinTable);
                            lastJoinTable.TableAlias = childQuery.TableAlias;
                            lastJoinTable.SearchConditions.Add(new BinaryLogicExpression(
                                new Column(ruleMapping[i], childQuery.TableAlias, ruleMapping[i].ChildField),
                                BinaryLogicOperator.Equals,
                                new Column(ruleMapping[i], leftFirstJoinTable.TableAlias, ruleMapping[i].ParentField)));


                            query.SelectList.Add(new Column(childMapping.Ids[0], childQuery.TableAlias, childMapping.GetIdField(childMapping.Ids[0])));

                            childQuery = query;
                            tableContext.Push(lastJoinTable);
                        }
                        else
                        {
                            Table leftTable = i == ruleMapping.Count - 1 ? childTable : new TableSource(ruleMapping[i], ruleMapping[i].ChildTable, String.Concat("rule", index + i));
                            Table rightTable = firstJoinTable;

                            lastJoinTable = new JoinedTable(leftTable, TypeJoinedTable.Inner, rightTable);
                            lastJoinTable.TableAlias = leftTable.TableAlias;

                            // inner join on index table FK_Child
                            lastJoinTable.SearchConditions.Add(new BinaryLogicExpression(
                                new Column(ruleMapping[i], leftTable.TableAlias, ruleMapping[i].ChildField),
                                BinaryLogicOperator.Equals,
                                new Column(ruleMapping[i], rightTable.TableAlias, ruleMapping[i].ParentField)));

                            tableContext.Push(lastJoinTable);
                        }

                        // it's a last rule
                        if (i == ruleMapping.Count - 1)
                        {
                            lastJoinTable = (JoinedTable)tableContext.Pop();

                            if (isInConstraint && index == lastIndex - 1)
                            {
                                // Restore the first entity context
                                EntityMapping mainMapping = (EntityMapping)entityMappingContext.Peek();
                                Table mainTable = (Table)tableContext.Peek();

                                string parentField = firstMapping.GetIdField(firstMapping.Ids[0]);
                                if (path.Identifiers.Count > 1 && (!lastIsAttribute || overrideId))
                                    parentField = "ParentId";

                                linkTableContext.Push(new Column(firstMapping.Ids[0], parentTable.TableAlias, parentField));                // parent table/column link
                                linkTableContext.Push(new Column(firstMapping.Ids[0], mainTable.TableAlias, firstMapping.GetIdField(firstMapping.Ids[0])));        // child table/column link
                            }

                            if (referenceMapping.DiscriminatorField != null && referenceMapping.Name != "*")
                            {
                                BinaryLogicExpression condition = new BinaryLogicExpression(
                                    new Column(referenceMapping, leftFirstJoinTable.TableAlias, referenceMapping.DiscriminatorField),
                                    BinaryLogicOperator.Equals,
                                    new Constant(ident.Value, DbType.AnsiString));

                                collEpression.Add(condition);
                            }

                            childQuery.FromClause.Add(lastJoinTable);
                            tableContext.Push(lastJoinTable);
                        }
                    }

                    childQuery.SelectList.Insert(0, new Constant(child.Type, DbType.AnsiString, TYPE_ALIAS));

                    int attributeTableIndex = 0;

                    foreach (string attName in attributes)
                    {
                        string attNameAlias = attName.Substring(attName.LastIndexOf(DOT) + 1);
                        string shortAttName = attNameAlias;

                        AttributeMapping am = childMapping.Attributes[shortAttName];

                        // if duplicated attribute name, add an incremented index at the end  (if not already done)
                        if (!_IsAliasColumnComputed)
                        {
                            foreach (string colName in _AliasColumnMapping.Values)
                            {
                                if (colName == shortAttName)
                                {
                                    if (!_AliasColumnMapping.Contains(attName))
                                    {
                                        attNameAlias += ++_AliasColumnIndexNumber;
                                        _AliasColumnMapping.Add(attName, attNameAlias);
                                    }
                                    else
                                        attNameAlias = _AliasColumnMapping[attName].ToString();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // get the previous computed alias
                            attNameAlias = _AliasColumnMapping[attName].ToString();
                        }

                        if (am != null)
                        {
                            if (am.Table == childMapping.Table)
                            {
                                childQuery.SelectList.Add(new Column(am, childQuery.TableAlias, am.Field, attNameAlias));
                            }
                            if (am.Table != childMapping.Table && childMapping.Ids[0].Generator.Name == GeneratorMapping.GeneratorType.inherited)
                            {
                                //	Add the Left Join for parent attribute table

                                Table left = childQuery.FromClause[0];
                                Table right = new TableSource(am.ParentEntity.Ids[0], am.Table, string.Concat("a", attributeTableIndex++));

                                JoinedTable jt = new JoinedTable(left, TypeJoinedTable.LeftOuter, right);

                                jt.SearchConditions.Add(new BinaryLogicExpression(
                                    new Column(null, right.TableAlias, am.ParentField),
                                    BinaryLogicOperator.Equals,
                                    new Column(null, left.TableAlias, childMapping.GetIdField(childMapping.Ids[0]))
                                    ));

                                childQuery.FromClause[0] = jt;

                                childQuery.SelectList.Add(new Column(am, right.TableAlias, am.Field, attNameAlias));

                                //	Get the parent table and insert a clause and then we don't return the
                                //	records which have a record in a child table

                                string parentType = string.Empty;
                                if (_Model.GetParent(_Model.GetEntity(child.Type)) != null)
                                    parentType = _Model.GetParent(_Model.GetEntity(child.Type)).Type;

                                if (parentType == string.Empty)
                                    continue;

                                EntityMapping parentEntityMapping = _Mapping.Entities[parentType];

                                SelectStatement sel = (SelectStatement)subQueries[parentType];

                                SelectStatement exludeClause = new SelectStatement(null);
                                exludeClause.SelectedAllColumns = true;
                                exludeClause.FromClause.Add(new TableSource(childMapping, childMapping.Table));

                                exludeClause.WhereClause.SearchCondition.Add(new BinaryLogicExpression(
                                    new Column(parentEntityMapping.Ids[0], sel.TableAlias, parentEntityMapping.GetIdField(parentEntityMapping.Ids[0])),
                                    BinaryLogicOperator.Equals,
                                    new Column(childMapping.Ids[0], childMapping.Table, childMapping.GetIdField(childMapping.Ids[0]))));

                                ExistsPredicate ex = new ExistsPredicate(exludeClause);
                                UnaryLogicExpression ua = new UnaryLogicExpression(UnaryLogicOperator.Not, ex);

                                sel.WhereClause.SearchCondition.Add(ua);
                            }
                        }
                        else
                        {
                            childQuery.SelectList.Add(new Constant(DBNull.Value, DbType.AnsiStringFixedLength, attNameAlias));
                        }

                        // Store the correspondance between the alias/column name and the attribute name 
                        if (!_IsAliasColumnComputed && !_AliasColumnMapping.Contains(attName))
                            _AliasColumnMapping.Add(attName, attNameAlias);
                    }

                    //					entityMappingContext.Pop();
                    //					entityModelContext.Pop();
                    tableContext.Pop();

                    //childrenSubExp.SelectExpressions.Add(childQuery);
                    subQueries.Add(child.Type, childQuery);
                }

                foreach (SelectStatement sel in subQueries.Values)
                    childrenSubExp.SelectExpressions.Add(sel);

                childrenSubExp.TableAlias = parentQuery.TableAlias;

                if (parentQuery.SelectList.Count == 0)
                {
                    //parentQuery.SelectedAllColumns = true;
                }

                //isFirstReference = false;
                queryContext.Push(childrenSubExp);

                //entityMappingContext.Push(_Mapping.Entities.GetEntity(cType));

                //entityModelContext.Push(_Model.GetEntity(cType));

                //if(!isInConstraint)
                tableContext.Push(childrenSubExp);

                //				queryContext.Push(childQuery);
            }


            // if only one attribute
            if (lastIsAttribute && index == 0)
            {
                //FROM  [Course] as [e2] 
                //WHERE ([e2].[CourseId] = [e1].[CourseId])

                SelectStatement childQuery = (SelectStatement)queryContext.Pop();
                firstTable = (Table)tableContext.Pop();
                childQuery.FromClause.Add(firstTable);

                Table mainTable = (Table)tableContext.Peek();
                childQuery.WhereClause.SearchCondition.Add(
                        new BinaryLogicExpression(new Column(null, firstTable.TableAlias, firstMapping.GetIdField(firstMapping.Ids[0])),
                        BinaryLogicOperator.Equals,
                        new Column(null, mainTable.TableAlias, firstMapping.GetIdField(firstMapping.Ids[0]))));

                queryContext.Push(childQuery);
                tableContext.Push(firstTable);
            }

            if (isInConstraint)
            {
                int max = entityMappingContext.Count;
                if (lastIsAttribute)
                    max--;

                for (int i = max; i > stackCount; i--)
                {
                    entityMappingContext.Pop();
                    entityModelContext.Pop();
                }
            }
        }

        #endregion

        public static string GetParentIdAlias(EntityMapping entity, string field)
        {
            string alias = "ParentId";
            if (entity.Ids.Count == 1)
                return alias;

            for (int i = 0; i < entity.Ids.Count; i++)
            {
                if (entity.Ids[i].Field == field)
                    return alias + i.ToString();
            }

            return alias;
        }


        #region Process Constraint

        public void ProcessConstraint(Evaluant.OPath.Expressions.Constraint constraint)
        {
            _IsFirstAttribute = true;
            constraint.Accept(this);

            ISQLExpression currentItem = (ISQLExpression)sqlExpressionContext.Pop();
            SelectStatement currentQuery = (SelectStatement)queryContext.Pop();

            if (currentItem != null && currentItem is ILogicExpression)
                currentQuery.WhereClause.SearchCondition.Add((ILogicExpression)currentItem);

            sqlExpressionContext.Push(currentItem);

            EntityMapping em = (EntityMapping)entityMappingContext.Peek();

            //bool idFieldInserted = false;
            //foreach (ISQLExpression exp in currentQuery.SelectList)
            //{
            //    Column col = exp as Column;
            //    if (col.ColumnName == em.GetIdField(em.Ids[0]))
            //    {
            //        idFieldInserted = true;
            //        break;
            //    }
            //}

            //if (!idFieldInserted && em.Ids[0].Generator.Name == GeneratorMapping.GeneratorType.inherited)
            //    currentQuery.SelectList.Add(new Column(em.Ids[0], em.GetIdField(em.Ids[0]))); //currentQuery.TableAlias, xxx , em.GetIdFieldAs(em.Ids[0])

            foreach (AttributeMapping am in em.Attributes)
                if (am.Table == em.Table && em.Ids[0].Generator.Name == GeneratorMapping.GeneratorType.inherited)
                    currentQuery.SelectList.Add(new Column(am, currentQuery.FromClause[0].TableAlias, am.Field));

            queryContext.Push(currentQuery);
        }

        #endregion

        #region Process Attributes

        private ISQLExpression ProcessAttributesWithLoadAttribute(SelectStatement mainQuery, SelectStatement currentQuery, Table currentTable, EntityMapping entityMap, Table attributeTable, AttributeMapping attributeMapping, string atrbId)
        {
            if (_IsFirstAttribute)
            {
                _IsFirstAttribute = false;

                // if current table is a JoinedTable : 
                //		from clause = mainQuery INNER JOIN attributeTable
                if (currentTable is JoinedTable)
                {
                    JoinedTable lastJoinTable = new JoinedTable(mainQuery, TypeJoinedTable.LeftOuter, attributeTable);
                    lastJoinTable.TableAlias = mainQuery.TableAlias;

                    BinaryLogicExpression onCondition = new BinaryLogicExpression(
                        new Column(entityMap.Ids[0], mainQuery.TableAlias, entityMap.GetIdField(entityMap.Ids[0])),
                        BinaryLogicOperator.Equals,
                        new Column(attributeMapping, attributeTable.TableAlias, atrbId));

                    if (mainQuery.WhereClause.SearchCondition.Count != 0)
                    {
                        foreach (ILogicExpression expression in mainQuery.WhereClause.SearchCondition)
                        {
                            onCondition = new BinaryLogicExpression(onCondition, BinaryLogicOperator.And, expression);
                        }
                    }

                    lastJoinTable.SearchConditions.Add(new BinaryLogicExpression(
                        new Column(entityMap.Ids[0], mainQuery.TableAlias, entityMap.GetIdField(entityMap.Ids[0])),
                        BinaryLogicOperator.Equals,
                        new Column(entityMap.Ids[0], attributeTable.TableAlias, atrbId)));

                    currentQuery.TableAlias = mainQuery.TableAlias;
                    currentQuery.FromClause.Add(lastJoinTable);

                    tableContext.Push(lastJoinTable);

                    for (int i = 0; i < 2; i++)
                    {
                        Column column = mainQuery.SelectList[i] as Column;
                        if (column != null && column.ColumnName == entityMap.Ids[0].Field && entityMap.Ids[0].Generator.Name == GeneratorMapping.GeneratorType.inherited)
                        {
                            column.Alias = null;
                            currentQuery.SelectList.Add(new Column(column.TagMapping, lastJoinTable.TableAlias, column.ColumnName, entityMap.GetIdFieldAs(entityMap.Ids[0])));
                        }
                        else
                        {
                            currentQuery.SelectList.Add(mainQuery.SelectList[i]);
                        }
                    }

                    if (entityMap.Ids[0].Generator.Name == GeneratorMapping.GeneratorType.inherited)
                    {
                        for (int i = 2; i < mainQuery.SelectList.Count; i++)
                        {
                            Column column = mainQuery.SelectList[i] as Column;
                            if (column != null)
                            {
                                currentQuery.SelectList.Add(new Column(column.TagMapping, lastJoinTable.TableAlias, column.GetSelectName()));
                            }
                        }
                    }

                    if (attributeMapping.Discriminator != null)
                    {
                        currentQuery.SelectList.Add(new Column(attributeMapping, attributeTable.TableAlias, attributeMapping.Discriminator));
                    }

                    queryContext.Push(currentQuery);

                    string alias = (attributeMapping.Field != attributeMapping.Name) ? attributeMapping.Name : String.Empty;
                    return new Column(attributeMapping, attributeTable.TableAlias, attributeMapping.Field);

                }
                // else 
                //		from clause = currentTable INNER JOIN attributeTable
                else
                {
                    JoinedTable joinTable = new JoinedTable(currentTable, TypeJoinedTable.LeftOuter, attributeTable);
                    joinTable.SearchConditions.Add(new BinaryLogicExpression(
                        new Column(entityMap.Ids[0], currentTable.TableAlias, entityMap.GetIdField(entityMap.Ids[0])),
                        BinaryLogicOperator.Equals,
                        new Column(attributeMapping, attributeTable.TableAlias, atrbId)));

                    currentQuery.TableAlias = currentTable.TableAlias;
                    currentQuery.FromClause.Add(joinTable);
                    currentQuery.WhereClause.SearchCondition = mainQuery.WhereClause.SearchCondition;

                    foreach (ISQLExpression exp in mainQuery.SelectList)
                    {
                        if (!currentQuery.SelectList.Contains(exp))
                            currentQuery.SelectList.Add(exp);
                    }

                    tableContext.Push(joinTable);
                }
            }
            else
            {
                Table rightTable = ((JoinedTable)currentTable).RigthTable;

                JoinedTable joinTable = new JoinedTable(rightTable, TypeJoinedTable.LeftOuter, attributeTable);
                joinTable.SearchConditions.Add(new BinaryLogicExpression(
                    new Column(attributeMapping, rightTable.TableAlias, atrbId),
                    BinaryLogicOperator.Equals,
                    new Column(attributeMapping, attributeTable.TableAlias, atrbId)));

                ((JoinedTable)currentTable).RigthTable = joinTable;

                currentQuery.TableAlias = currentTable.TableAlias;
                currentQuery.FromClause.Add(currentTable);
                currentQuery.WhereClause.SearchCondition = mainQuery.WhereClause.SearchCondition;

                tableContext.Push(currentTable);
            }



            foreach (ISQLExpression exp in mainQuery.SelectList)
            {
                if (!currentQuery.SelectList.Contains(exp))
                {
                    currentQuery.SelectList.Add(exp);
                }
            }


            if (attributeMapping.Discriminator != null)
            {
                currentQuery.SelectList.Add(new Column(attributeMapping, attributeTable.TableAlias, attributeMapping.Discriminator));
            }
            queryContext.Push(currentQuery);

            string alias1 = (attributeMapping.Field != attributeMapping.Name) ? attributeMapping.Name : String.Empty;
            return new Column(attributeMapping, attributeTable.TableAlias, attributeMapping.Field, alias1);

        }

        public ISQLExpression ProcessAttributes(EntityMapping entityMap, string name, bool loadAttributes)
        {
            return ProcessAttributes(entityMap, name, loadAttributes, false);
        }

        /// <summary>
        /// Processes the attributes.
        /// </summary>
        /// <param name="entityMap">Entity mapping whitch contains the attribute</param>
        /// <param name="name">Name of the attribute</param>
        /// <param name="loadAttributes"></param>
        /// <param name="checkAlias">Indicate if returned column must have an computed alias name or not</param>
        /// <returns>Return a <see cref="Column"/></returns>
        public ISQLExpression ProcessAttributes(EntityMapping entityMap, string name, bool loadAttributes, bool checkAlias)
        {
            if (name == "Id")
            {
                Table currentTable = (Table)tableContext.Peek();
                return new Column(entityMap.Ids[0], currentTable.TableAlias, entityMap.GetIdField(entityMap.Ids[0]));
            }
            else
            {
                // Exception management : attribute mapping
                AttributeMapping attributeMapping = entityMap.Attributes[name];

                //Attempt to get the attribute in the children types
                if (attributeMapping == null)
                    foreach (Evaluant.Uss.Models.Entity e in _Model.GetTree(entityMap.Type))
                    {
                        EntityMapping em = _Mapping.Entities[e.Type];
                        if ((attributeMapping = em.Attributes[name]) != null)
                            break;
                    }

                //Attempt to get the attribute in the parent types
                if (attributeMapping == null)
                {
                    Evaluant.Uss.Models.Entity cur = _Model.GetEntity(entityMap.Type);

                    while (_Model.GetParent(cur) != null)
                    {
                        cur = _Model.GetParent(cur);
                        EntityMapping current = _Mapping.Entities[cur.Type];

                        if (current.Attributes[name] != null)
                            attributeMapping = current.Attributes[name];
                    }
                }

                if (attributeMapping == null)
                    throw new UniversalStorageException(String.Format("The attribute '{0}' of the entity '{1}' is not defined in your mapping file", name, entityMap.Type));

                // If the attribute is contained in entity's table : the current query doesn't change
                if (attributeMapping.Table == entityMap.Table)
                {
                    Table currentTable = (Table)tableContext.Peek();

                    Column c = new Column(attributeMapping, currentTable.TableAlias, attributeMapping.Field);
                    //if (entityMap.Ids[attributeMapping.Field] != null)
                    //    c.ColumnName = entityMap.GetIdField(entityMap.Ids[attributeMapping.Field]);

                    if (checkAlias)
                    {
                        string attributeFullName = _Model.GetFullAttributeName(attributeMapping.Name, attributeMapping.ParentEntity.Type);
                        //if (entityMap.Ids[attributeMapping.Field] != null)
                        //    attributeFullName = entityMap.GetIdFieldAs(entityMap.Ids[attributeMapping.Field]);
                        //c.ColumnName = (_AliasColumnMapping.Contains(attributeFullName)) ? _AliasColumnMapping[attributeFullName].ToString() : "";
                        if (_AliasColumnMapping.Contains(attributeFullName))
                            c.ColumnName = _AliasColumnMapping[attributeFullName].ToString();
                    }
                    return c;
                }
                // Else : create a new select query
                else
                {
                    // Get the current context
                    Table currentTable = (Table)tableContext.Pop();
                    SelectStatement mainQuery = (SelectStatement)queryContext.Pop();

                    // Create a new query
                    SelectStatement currentQuery = new SelectStatement(attributeMapping);

                    Table attributeTable = new TableSource(attributeMapping, attributeMapping.Table, CreateUniqueAttributeAlias());
                    string atrbId = attributeMapping.Table == null || attributeMapping.Table == entityMap.Table ? entityMap.GetIdField(entityMap.Ids[0]) : attributeMapping.ParentField;

                    if (loadAttributes)
                    {
                        return this.ProcessAttributesWithLoadAttribute(mainQuery, currentQuery, currentTable, entityMap, attributeTable, attributeMapping, atrbId);
                    }
                    else
                    {
                        if (_IsFirstAttribute)
                        {
                            _IsFirstAttribute = false;

                            // if current table is a JoinedTable : 
                            //		from clause = mainQuery INNER JOIN attributeTable
                            if (currentTable.GetType() == typeof(JoinedTable))
                            {
                                JoinedTable lastJoinTable = new JoinedTable(mainQuery, loadAttributes ? TypeJoinedTable.LeftOuter : TypeJoinedTable.Inner, attributeTable);
                                lastJoinTable.TableAlias = mainQuery.TableAlias;

                                BinaryLogicExpression onCondition = new BinaryLogicExpression(
                                    new Column(entityMap.Ids[0], mainQuery.TableAlias, entityMap.GetIdField(entityMap.Ids[0])),
                                    BinaryLogicOperator.Equals,
                                    new Column(attributeMapping, attributeTable.TableAlias, atrbId));

                                if (mainQuery.WhereClause.SearchCondition.Count != 0)
                                {
                                    foreach (ILogicExpression expression in mainQuery.WhereClause.SearchCondition)
                                    {
                                        onCondition = new BinaryLogicExpression(onCondition, BinaryLogicOperator.And, expression);
                                    }
                                }

                                lastJoinTable.SearchConditions.Add(new BinaryLogicExpression(
                                    new Column(entityMap.Ids[0], mainQuery.TableAlias, entityMap.GetIdField(entityMap.Ids[0])),
                                    BinaryLogicOperator.Equals,
                                    new Column(entityMap.Ids[0], attributeTable.TableAlias, atrbId)));

                                currentQuery.TableAlias = mainQuery.TableAlias;
                                currentQuery.FromClause.Add(lastJoinTable);

                                currentQuery.WhereClause.SearchCondition.Add(new BinaryLogicExpression(
                                    new Column(attributeMapping, attributeTable.TableAlias, attributeMapping.Discriminator),
                                    BinaryLogicOperator.Equals,
                                    new Constant(name, DbType.AnsiString)));


                                tableContext.Push(lastJoinTable);
                                queryContext.Push(currentQuery);

                                return new Column(attributeMapping, attributeTable.TableAlias, attributeMapping.Field);

                            }
                            // else 
                            //		from clause = currentTable INNER JOIN attributeTable
                            else
                            {

                                JoinedTable joinTable = new JoinedTable(currentTable, loadAttributes ? TypeJoinedTable.LeftOuter : TypeJoinedTable.Inner, attributeTable);
                                joinTable.SearchConditions.Add(new BinaryLogicExpression(
                                    new Column(entityMap.Ids[0], currentTable.TableAlias, entityMap.GetIdField(entityMap.Ids[0])),
                                    BinaryLogicOperator.Equals,
                                    new Column(attributeMapping, attributeTable.TableAlias, atrbId)));


                                currentQuery.TableAlias = CreateUniqueTableAlias();
                                currentQuery.FromClause.Add(joinTable);
                                currentQuery.WhereClause.SearchCondition = mainQuery.WhereClause.SearchCondition;

                                tableContext.Push(joinTable);
                            }
                        }
                        else
                        {
                            Table rightTable = ((JoinedTable)currentTable).RigthTable;

                            JoinedTable joinTable = new JoinedTable(attributeTable, TypeJoinedTable.Inner, rightTable);
                            joinTable.SearchConditions.Add(new BinaryLogicExpression(
                                new Column(attributeMapping, rightTable.TableAlias, atrbId),
                                BinaryLogicOperator.Equals,
                                new Column(attributeMapping, attributeTable.TableAlias, atrbId)));

                            ((JoinedTable)currentTable).RigthTable = joinTable;

                            currentQuery.SelectList.Add(new Column(entityMap.Ids[0], currentTable.TableAlias, entityMap.GetIdField(entityMap.Ids[0])));

                            currentQuery.TableAlias = currentTable.TableAlias;
                            currentQuery.FromClause.Add(currentTable);

                            tableContext.Push(currentTable);
                        }

                        queryContext.Push(currentQuery);
                        return new Column(attributeMapping, attributeTable.TableAlias, attributeMapping.Field);

                    }
                }
            }
        }

        #endregion

        #region TransformToSql

        /// <summary>
        /// Get an ISqlExpression which correspond to one entity mapping
        /// This method process the opath constraint
        /// This method doesn't not process the children of an entity
        /// </summary>
        /// <param name="query"></param>
        /// <param name="attributes"></param>
        /// <param name="baseType">In the case of a table per hierarchy mapping, the "in" predicates computes all the children from the base</param>
        /// <param name="orderById"></param>
        /// <returns></returns>
        public ISQLExpression TransformToSql(Path path, StringCollection attributes, bool orderById, bool lastIsAttribute)
        {

            // Exception management : path
            if (path == null)
                throw new ArgumentNullException("path");

            // Exception management : identifier
            if (path.Identifiers.Count == 0)
                throw new ArgumentNullException("path", "Must have at least one Identifier");

            ConvertToSQL(path, true, false, lastIsAttribute);

            // Restore context
            SelectStatement currentQuery = (SelectStatement)queryContext.Peek();
            Table currentTable = (Table)tableContext.Peek();

            EntityMapping entityMapping = (EntityMapping)entityMappingContext.Peek();

            //	Always add the Type and Id in the SQL query
            SelectStatement s = (SelectStatement)queryContext.Peek();

            string childType = string.Empty;
            string parenttype = string.Empty;

            if (path.Identifiers.Count == 1)
                childType = path.Identifiers[0].Value;
            else
            {
                string refName = path.Identifiers[path.Identifiers.Count - 1].Value;
                parenttype = string.Empty;	//= query.Path.Identifiers[0].Value;

                //	Get the parent type: the type which corresponds to the refName
                string nextRef = string.Empty;// query.Path.Identifiers[1].Value;
                string nextType = path.Identifiers[0].Value;

                for (int i = 1; i < path.Identifiers.Count - 1; i++)
                {
                    nextRef = path.Identifiers[i].Value;

                    Evaluant.Uss.Models.Reference nextRefModel = _Model.GetReference(nextType, nextRef);

                    //	Try to get the reference model in the parent type
                    if (nextRefModel == null)
                    {
                        Evaluant.Uss.Models.Entity currentE = _Model.GetEntity(nextType);

                        while (_Model.GetParent(currentE) != null)
                        {
                            currentE = _Model.GetParent(currentE);
                            nextRefModel = _Model.GetReference(currentE.Type, nextRef);
                            if (nextRefModel != null)
                                break;
                        }
                    }

                    if (nextRefModel == null)
                        throw new SqlMapperException(string.Concat("Cannot find the reference ", refName, " in the model of the entity ", nextType));

                    nextType = nextRefModel.ChildType;
                }

                parenttype = nextType;

                //if (_Model.GetReference(parenttype, refName) == null)
                //{
                //    Evaluant.Uss.Models.Entity current = Model.GetEntity(parenttype);

                //    while (Model.GetParent(current) != null)
                //    {
                //        current = Model.GetParent(current);
                //        parenttype = current.Type;
                //        if (_Model.GetReference(parenttype, refName) != null)
                //            break;
                //    }
                //}

                Evaluant.Uss.Models.Reference r = _Model.GetReference(parenttype, refName);

                if (r == null)
                {
                    Evaluant.Uss.Models.Attribute a = _Model.GetAttribute(parenttype, refName);

                    if (a == null)
                    {
                        throw new SqlMapperException("Cannot find " + refName + " in the '" + parenttype + "' model");
                    }
                    childType = parenttype;
                }
                else
                {
                    childType = r.ChildType;
                }

                if (_Model.GetReference(parenttype, refName) == null && _Model.GetAttribute(parenttype, refName) == null)
                    throw new SqlMapperException("Cannot find " + refName + " in the '" + parenttype + "' model");

                if (_Model.GetReference(parenttype, refName) != null)
                    childType = _Model.GetReference(parenttype, refName).ChildType;

                if (_Model.GetAttribute(parenttype, refName) != null)
                    childType = parenttype;
            }


            if (path.Identifiers.Count == 1 || lastIsAttribute)
            {
                // Generates a specific query if a discriminator field is set
                if (entityMapping.DiscriminatorField != null && entityMapping.DiscriminatorField != string.Empty)
                {

                    CaseExpression caseExp = new CaseExpression(null, new Column(entityMapping, currentTable.TableAlias, entityMapping.DiscriminatorField), TYPE_ALIAS);
                    caseExp.DefaultResult = new Column(entityMapping, currentTable.TableAlias, entityMapping.DiscriminatorField);

                    // Generates a case for each discriminator value which is different to the real type
                    foreach (Models.Entity e in _Model.GetTree(entityMapping.Type))
                    {
                        string discriminatorValue = _Mapping.Entities[e.Type].DiscriminatorValue;
                        if (discriminatorValue != e.Type)
                        {
                            caseExp.TestExpressions.Add(new CaseTest(new Constant(discriminatorValue, DbType.String), new Constant(e.Type, DbType.String)));
                        }
                    }

                    // Determines wether the select should transform the discriminator values to real types (needed by the entity isntanciator)
                    if (caseExp.TestExpressions.Count > 0)
                    {
                        s.SelectList.Insert(0, caseExp);
                    }
                    else // If no type conversion has to be done, just write the discriminator-field alias
                    {
                        s.SelectList.Insert(0, new Column(entityMapping, currentTable.TableAlias, entityMapping.DiscriminatorField, TYPE_ALIAS));
                    }
                }
                else
                {
                    s.SelectList.Insert(0, new Constant(childType, DbType.AnsiStringFixedLength, TYPE_ALIAS));
                }

                bool idFieldInserted = false;

                foreach (ISQLExpression exp in s.SelectList)
                {
                    Column id = exp as Column;
                    if (id != null && id.ColumnName == entityMapping.GetIdField(entityMapping.Ids[0]))
                    {
                        idFieldInserted = true;
                        if (id.Alias == null || id.Alias == String.Empty)
                            id.Alias = entityMapping.GetIdFieldAs(entityMapping.Ids[0]);
                        break;
                    }
                }

                if (!idFieldInserted)
                {
                    for (int i = 0; i < entityMapping.Ids.Count; i++)
                        s.SelectList.Insert(Math.Min(1 + i, s.SelectList.Count), new Column(entityMapping.Ids[i], currentTable.TableAlias, entityMapping.GetIdField(entityMapping.Ids[i]), entityMapping.GetIdFieldAs(entityMapping.Ids[i])));
                }

            }
            else
            {
                s.SelectList.Insert(0, new Column(null, currentTable.TableAlias, TYPE_ALIAS, TYPE_ALIAS));

                EntityMapping parentMapping = _Mapping.Entities[parenttype];
                int pos = 2;
                if (parentMapping != null)
                {
                    pos += parentMapping.Ids.Count - 1;
                    for (int i = 0; i < parentMapping.Ids.Count; i++)
                    {
                        PrimaryKeyMapping pmk = parentMapping.Ids[i];
                        string parentIdAlias = GetParentIdAlias(parentMapping, pmk.Field);
                        s.SelectList.Insert(1 + i, new Column(pmk, currentTable.TableAlias, parentIdAlias, parentIdAlias));

                    }
                }

                for (int i = 0; i < entityMapping.Ids.Count; i++)
                {
                    PrimaryKeyMapping pmk = entityMapping.Ids[i];
                    if (currentTable is SelectStatement)
                        s.SelectList.Insert(pos + i, new Column(pmk, currentTable.TableAlias, entityMapping.GetIdFieldAs(pmk), entityMapping.GetIdFieldAs(pmk)));
                    else
                        s.SelectList.Insert(pos + i, new Column(pmk, currentTable.TableAlias, entityMapping.GetIdField(pmk), entityMapping.GetIdFieldAs(pmk)));

                }


            }

            // FROM entity_table e{n}
            if (!currentQuery.FromClause.Contains(currentTable))
                currentQuery.FromClause.Add(currentTable);

            //	If attributes == null => don't load
            if (attributes != null)
            {
                _IsFirstAttribute = true;

                StringCollection processedAttributes = new StringCollection();

                // rule1: alias in selectList are specified only if field name is different than attribute name 
                //        if it is the case we use the attribute name as alias
                // rule2: if more than one item have the same attribute name, add an index number at the end of 2nd, ..., n
                // rule3: use a hashtable to do the correspondance between the alias and the attributeName
                // rule4: above rules has to be done only for the first select list in case of union query

                foreach (string attName in attributes)
                {
                    string attNameAlias = attName.Substring(attName.LastIndexOf(DOT) + 1);
                    string shortAttName = attNameAlias;

                    AttributeMapping am = entityMapping.Attributes[shortAttName];

                    if (am == null)
                        continue;

                    PrimaryKeyMapping pkm = entityMapping.Ids[am.Field, false];

                    if (pkm == null || pkm.Generator.Name != GeneratorMapping.GeneratorType.business)
                        continue;

                    attNameAlias = entityMapping.GetIdFieldAs(pkm);

                    bool exists = false;

                    foreach (ISQLExpression expr in currentQuery.SelectList)
                    {
                        if (expr as Column == null)
                            continue;

                        if ((expr as Column).Alias == attNameAlias)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (exists)
                        continue;

                    ISQLExpression expression = ProcessAttributes(entityMapping, shortAttName, true);

                    if (expression is Column)
                        ((Column)expression).Alias = attNameAlias;

                    currentQuery = (SelectStatement)queryContext.Peek();

                    int index = -1;
                    foreach (ISQLExpression expr in currentQuery.SelectList)
                    {
                        Column col = expr as Column;
                        if (col != null && col.Alias == attNameAlias)
                        {
                            index = currentQuery.SelectList.IndexOf(expr);
                            break;
                        }

                        Constant cst = expr as Constant;
                        if (cst != null && cst.Alias == attNameAlias)
                        {
                            index = currentQuery.SelectList.IndexOf(expr);
                            break;
                        }
                    }

                    if (index != -1)
                        currentQuery.SelectList.RemoveAt(index);

                    currentQuery.SelectList.Add(expression);
                }

                foreach (string attName in attributes)
                {
                    string attNameAlias = attName.Substring(attName.LastIndexOf(DOT) + 1);
                    string shortAttName = attNameAlias;

                    AttributeMapping am = entityMapping.Attributes[shortAttName];
                    EntityMapping em = entityMapping;

                    // if am is null, ask child mapping
                    if (am == null)
                    {
                        string attType = attName.Substring(0, attName.IndexOf(DOT));

                        // if the attribute was found in another hierarchy, use a NULL value (case of TPCC when two hierarchies inherit from an absctract class)
                        if (_Model.GetTree(entityMapping.Type).Contains(attType))
                        {

                            foreach (Models.Entity entity in _Model.GetTree(attType))
                            {
                                EntityMapping childMapping = _Mapping.Entities[entity.Type];

                                if (childMapping != null)
                                {
                                    AttributeMapping tmpAm = childMapping.Attributes[shortAttName];
                                    if (tmpAm != null)
                                    {
                                        am = tmpAm;
                                        em = childMapping;
                                        break;
                                    }
                                }
                            }
                        }

                        //if (childMapping.Type != entityMapping.Type && childMapping.Table == entityMapping.Table)
                        //{
                        //    am = childMapping.Attributes[shortAttName];
                        //    if (am != null)
                        //        em = childMapping;
                        //}
                    }

                    // if duplicated attribute name, add an incremented index at the end  (if not already done)
                    if (!_IsAliasColumnComputed)
                    {
                        foreach (string colName in _AliasColumnMapping.Values)
                        {
                            if (colName == shortAttName)
                            {
                                if (!_AliasColumnMapping.Contains(attName))
                                {
                                    attNameAlias += ++_AliasColumnIndexNumber;
                                    _AliasColumnMapping.Add(attName, attNameAlias);
                                }
                                else
                                    attNameAlias = _AliasColumnMapping[attName].ToString();
                                break;
                            }
                        }
                    }
                    else
                    {
                        // get the previous computed alias
                        if (_AliasColumnMapping[attName] != null)
                            attNameAlias = _AliasColumnMapping[attName].ToString();
                    }

                    if (am != null)
                    {
                        //	If attribute exists in mapping: add a column to the query

                        ISQLExpression expression = null;
                        // check if is imbricate table
                        if (currentTable is SelectStatement)
                            expression = ProcessAttributes(em, shortAttName, true, true);
                        else
                            expression = ProcessAttributes(em, shortAttName, true);

                        // check if alias is needed or not
                        if (attNameAlias != am.Field)
                        {
                            if (expression is Column)
                            {
                                ((Column)expression).Alias = attNameAlias;
                            }
                            else if (expression is Constant)
                            {
                                ((Constant)expression).Alias = attNameAlias;
                            }
                        }

                        currentQuery = (SelectStatement)queryContext.Peek();

                        int index = -1;
                        foreach (ISQLExpression expr in currentQuery.SelectList)
                        {
                            Column col = expr as Column;
                            if (col != null && col.Alias == attNameAlias)
                            {
                                index = currentQuery.SelectList.IndexOf(expr);
                                break;
                            }

                            Constant cst = expr as Constant;
                            if (cst != null && cst.Alias == attNameAlias)
                            {
                                index = currentQuery.SelectList.IndexOf(expr);
                                break;
                            }
                        }

                        if (index != -1)
                            currentQuery.SelectList.RemoveAt(index);

                        currentQuery.SelectList.Add(expression);
                    }
                    else
                    {
                        //	If attribute doesn't exists in mapping: add a constant instead of
                        //	a column so that we can build a UNION

                        currentQuery = (SelectStatement)queryContext.Peek();

                        Models.Attribute found = null;
                        Models.Entity foundEntity = null;

                        foreach (Models.Entity e in _Model.GetTree(entityMapping.Type))
                        {
                            foreach (Models.Attribute a in e.Attributes)
                            {
                                if (a.Name == attNameAlias)
                                {
                                    found = a;
                                    foundEntity = e;
                                    break;
                                }
                            }

                            if (found != null)
                            {
                                break;
                            }
                        }

                        // If the attribute is in a sub class, select NULL for the current UNION, the column instead
                        if (found != null && foundEntity.Type == entityMapping.Type)
                        {
                            AttributeMapping foundMapping = _Mapping.Entities[foundEntity.Type].Attributes[found.Name];
                            if (foundMapping == null)
                                continue;
                            currentQuery.SelectList.Add(new Column(null, currentTable.TableAlias, foundMapping.Field, foundMapping.Name));
                        }
                        else
                        {
                            currentQuery.SelectList.Add(new Constant(DBNull.Value, DbType.AnsiStringFixedLength, attNameAlias));
                        }
                    }

                    // Store the correspondance between the alias/column name and the attribute name 
                    if (!_IsAliasColumnComputed && !_AliasColumnMapping.Contains(attName))
                        _AliasColumnMapping.Add(attName, attNameAlias);
                }

            }

            return (SelectStatement)queryContext.Peek();
        }


        public ISQLExpression TransformToSql(Path path, string[] attributes, string[] orderby, bool lastIsAttribute)
        {
            //	Get all the entity of the hierarchy (parent and children)

            string baseType = path.Identifiers[0].Value;

            IList tree = _Model.GetTree(baseType);

            _UserAttributes = new StringCollection();

            //	attributes == null => don't get attributes

            if (attributes != null)
            {
                //	Get the specified class and store all the inherited classes 
                Evaluant.Uss.Models.Entity parent = null;
                Evaluant.Uss.Models.EntityCollection parents = new Evaluant.Uss.Models.EntityCollection();

                if (path.Identifiers.Count == 1)
                    parent = _Model.GetEntity(path.Identifiers[0].Value);
                else
                {
                    string refName = path.Identifiers[path.Identifiers.Count - 1].Value;
                    string firstType = path.Identifiers[0].Value;
                    string lastType = firstType;
                    string nextRefName = string.Empty;

                    for (int i = 0; i < path.Identifiers.Count - 1; i++)
                    {
                        nextRefName = path.Identifiers[i + 1].Value;

                        //	The next identifier isn't a reference but an attribute
                        if (_Model.GetReference(lastType, nextRefName) == null &&
                            _Model.GetAttribute(lastType, nextRefName) != null)
                            continue;

                        lastType = _Model.GetReference(lastType, nextRefName, true).ChildType;
                    }
                    parent = _Model.GetEntity(lastType);
                }

                if (parent == null)
                    throw new Evaluant.Uss.Models.ModelElementNotFoundException(String.Format("The type [{0}] was not found. Check your metadata.", baseType));

                if (attributes.Length == 0)
                {
                    //	attributes.Length == 0 => get all attributes
                    _UserAttributes.AddRange(_Model.GetFullAttributesNames(parent.Type));
                }
                else
                {
                    //	attributes.Length > 0 => get the user defined attributes
                    foreach (string attName in attributes)
                    {
                        _UserAttributes.Add(_Model.GetFullAttributeName(attName, parent.Type));
                    }
                }
            }

            UnionStatement sqlQuery = new UnionStatement();

            //	Process each entity of the tree and put the corresponding sql query
            //	into a UnionStatement

            StringCollection processedTables = new StringCollection();

            _BaseType = path.Identifiers[0].Value;

            Hashtable expressions = new Hashtable();

            string commonIdField = string.Empty;

            foreach (Evaluant.Uss.Models.Entity entity in tree)
            {
                //	If the entity doesn't have a mapping => don't process it
                if (_Mapping.Entities[entity.Type] == null)
                    continue;

                path.Identifiers[0].Value = entity.Type;

                EntityMapping entityMapping = _Mapping.Entities[entity.Type];

                if (processedTables.Contains(entityMapping.Table))
                    continue;

                processedTables.Add(entityMapping.Table);

                _ExprLevel.Push(PATH);
                SelectStatement exp = (SelectStatement)TransformToSql(path, _UserAttributes, true, lastIsAttribute);
                _ExprLevel.Pop();

                // process alias name calculation only once  
                _IsAliasColumnComputed = true;

                expressions.Add(entity.Type, exp);

                string parentType = string.Empty;
                if (_Model.GetParent(_Model.GetEntity(entity.Type)) != null)
                    parentType = _Model.GetParent(_Model.GetEntity(entity.Type)).Type;

                if (parentType == string.Empty || entityMapping.Ids[0].Generator.Name != GeneratorMapping.GeneratorType.inherited || parentType != _BaseType)
                    continue;

                EntityMapping parentMapping = _Mapping.Entities[parentType];

                SelectStatement sel = (SelectStatement)expressions[parentType];

                SelectStatement exludeClause = new SelectStatement(null);
                exludeClause.SelectedAllColumns = true;
                exludeClause.FromClause.Add(new TableSource(entityMapping, entityMapping.Table));

                string parentIdField = parentMapping.GetIdField(parentMapping.Ids[0]);
                if (path.Identifiers.Count > 1)
                    parentIdField = "ParentId";

                exludeClause.WhereClause.SearchCondition.Add(new BinaryLogicExpression(
                    new Column(parentMapping.Ids[0], sel.TableAlias, parentIdField),
                    BinaryLogicOperator.Equals,
                    new Column(entityMapping.Ids[0], entityMapping.Table, entityMapping.GetIdField(entityMapping.Ids[0]))));

                ExistsPredicate ex = new ExistsPredicate(exludeClause);
                UnaryLogicExpression ua = new UnaryLogicExpression(UnaryLogicOperator.Not, ex);

                sel.WhereClause.SearchCondition.Add(ua);

                //sqlQuery.SelectExpressions.Add(exp);
            }


            //If there are many tables in the union statement, make the id fields aliases identical
            bool isFirstSelect = true;
            ArrayList idAliases = new ArrayList();
            foreach (ISQLExpression exp in expressions.Values)
            {
                SelectStatement innerSelect = (SelectStatement)exp;
                if (isFirstSelect)
                {
                    foreach (ISQLExpression innerExp in innerSelect.SelectList)
                    {
                        if (innerExp is Column && ((Column)innerExp).Alias.StartsWith(EntityMapping.PREFIX_ID))
                            idAliases.Add(((Column)innerExp).Alias);
                    }
                    isFirstSelect = false;
                }
                else
                {
                    int i = 0;
                    foreach (ISQLExpression innerExp in innerSelect.SelectList)
                    {
                        if (innerExp is Column && ((Column)innerExp).Alias.StartsWith(EntityMapping.PREFIX_ID))
                        {
                            if (idAliases.Count > i)
                                ((Column)innerExp).Alias = (string)idAliases[i];
                            else
                                throw new UniversalStorageException(string.Format("entity Id fields do not meet the one of its parent '{0}'", _BaseType));
                            i++;
                        }
                    }
                }

                //Add the SelectStatement to the unionStatement
                sqlQuery.SelectExpressions.Add(innerSelect);
            }

            if (entityMappingContext.Count == 0)
            {
                throw new MappingNotFoundException(String.Format("Type not found [{0}]", baseType));
            }

            EntityMapping em = (EntityMapping)entityMappingContext.Peek();

            if (orderby != null)
            {
                for (int i = 0; i < orderby.Length; i++)
                {
                    string order = orderby[i].Trim();
                    if (order != string.Empty)
                    {
                        bool isDesc = false;
                        OrderByClauseColumn col = null;
                        int indexOfDirection = 0;
                        if ((indexOfDirection = order.ToLower().IndexOf(" desc")) > 0)
                        {
                            order = order.Substring(0, indexOfDirection).Trim();
                            isDesc = true;
                        }
                        else if ((indexOfDirection = order.ToLower().IndexOf(" asc")) > 0)
                            order = order.Substring(0, indexOfDirection).Trim();

                        if (order == "Id")
                        {
                            col = new OrderByClauseColumn(em.GetIdField(em.Ids[0]), true);
                        }
                        else
                        {
                            AttributeMapping orderMapping = em.Attributes[order, true];
                            string orderColName = orderMapping.Name;
                            string fullAttributeName = _Model.GetFullAttributeName(orderMapping.Name, orderMapping.ParentEntity.Type);
                            if (_AliasColumnMapping.Contains(fullAttributeName))
                                orderColName = _AliasColumnMapping[fullAttributeName].ToString();

                            col = new OrderByClauseColumn(orderColName, isDesc);
                        }
                        sqlQuery.OrderByClause.Add(col);
                    }
                }
            }

            if (sqlQuery.OrderByClause.Count == 0 && !lastIsAttribute && orderby != null)
            {
                //  Id position = 1 if loading single Entity (Person or Person[Name = 'p1']
                //  Id position = 2 if loading referenced Entity (Person.Partners or Person[Name = 'p1'].Partners)
                int idPosition = (path.Identifiers.Count == 1) ? 1 : 2;
                if (idPosition == 2)
                    idPosition = _Mapping.Entities[path.Identifiers[0].Value].Ids.Count + 1;
                Column columnId = (Column)sqlQuery.SelectExpressions[0].SelectList[idPosition];
                // get the Id alias of the first SelectStatement in case of UnionStatement
                if (sqlQuery.SelectExpressions[0] is UnionStatement)
                {
                    columnId = (Column)((UnionStatement)sqlQuery.SelectExpressions[0]).SelectExpressions[0].SelectList[idPosition];
                }

                EntityMapping entityMap = null;
                if (columnId.TagMapping is PrimaryKeyMapping)
                    entityMap = ((PrimaryKeyMapping)columnId.TagMapping).ParentEntity;
                if (columnId.TagMapping is RuleMapping)
                {
                    if (idPosition == 2)
                        entityMap = _Mapping.Entities[((RuleMapping)columnId.TagMapping).ParentReference.EntityChild];
                    else
                        entityMap = ((RuleMapping)columnId.TagMapping).ParentReference.EntityParent;

                }

                if (entityMap != null)
                {
                    SelectStatement selectQuery = sqlQuery;
                    if (sqlQuery is UnionStatement && ((UnionStatement)sqlQuery).SelectExpressions.Count > 0)
                        selectQuery = ((UnionStatement)sqlQuery).SelectExpressions[0] as SelectStatement;
                    foreach (PrimaryKeyMapping pkm in entityMap.Ids)
                    {
                        OrderByClauseColumn orderCol = null;
                        foreach (ISQLExpression exp in selectQuery.SelectList)
                        {
                            if (exp is Column && exp.TagMapping == pkm)
                            {
                                Column col = exp as Column;
                                orderCol = new OrderByClauseColumn(col.Alias);
                                break;
                            }
                        }
                        if (orderCol == null)
                        {

                            AttributeMapping attribute = entityMap.Attributes.FindByField(pkm.Field);

                            string idFieldAlias = pkm.ParentEntity.GetIdFieldAs(pkm);

                            foreach (string alias in _AliasColumnMapping.Keys)
                            {
                                if (alias.Contains(".") && attribute != null)
                                {
                                    if (alias.Split('.')[1] == attribute.Name)
                                    {
                                        orderCol = new OrderByClauseColumn(_AliasColumnMapping[alias].ToString());
                                        break;
                                    }
                                }
                                else if (alias == idFieldAlias)
                                {
                                    orderCol = new OrderByClauseColumn(_AliasColumnMapping[alias].ToString());
                                    break;
                                }
                            }

                            if (orderCol == null)
                                orderCol = new OrderByClauseColumn(em.GetIdFieldAs(pkm));

                        }
                        if (orderCol != null)
                            sqlQuery.OrderByClause.Add(orderCol);
                    }
                }


            }

            return sqlQuery;
        }

        #endregion

        #region TransformScalar

        public ISQLExpression TransformScalar(OPathQuery query)
        {
            if (query.Expression == null)
                throw new ArgumentNullException("query.Expression");

            if (query.Expression.Operands.Count == 0)
                throw new ArgumentNullException("query.Expression", "Must have at least one Operand");

            _ExprLevel.Push(EXPR);
            ISQLExpression select = ConvertToSQL(query.Expression, true, false, false);

            if (select.GetType() != typeof(SelectStatement))
            {
                ISQLExpression exp = select;
                select = new SelectStatement(null);
                ((SelectStatement)select).SelectList.Add(exp);
                if (exp is SelectStatement)
                    ((SelectStatement)select).FromClause = ((SelectStatement)exp).FromClause;

            }

            return select;
        }

        private ISQLExpression ConvertToSQL(Call call, bool firstIsType, bool isInConstraint, bool lastIsAttribute)
        {
            _ExprLevel.Push(_ExprLevel.Peek());

            ISQLExpression result = null;

            if (call.Name == "eval")
            {
                call.Operands[0].Accept(this);
                result = (ISQLExpression)sqlExpressionContext.Peek();
            }

            _ExprLevel.Pop();

            return result;
        }

        #endregion

        #region Functions EntityMapping

        public EntityMapping TransformToEntityMapping(OPathQuery query)
        {
            if (query.Path == null)
                throw new ArgumentNullException("path");

            if (query.Path.Identifiers.Count == 0)
                throw new ArgumentNullException("path", "Must have at least one Identifier");

            return _Mapping.Entities[query.Path.Identifiers[0].Value];
        }

        public EntityMapping GetCurrentEntityMapping()
        {
            return (EntityMapping)entityMappingContext.Peek();
        }

        #endregion

        #region Visit

        /// <summary>
        /// Process an attribute like following : 
        ///         Person[Brother.Kind.Name = 'nice']
        /// </summary>
        /// <returns>The column corresponding to the required attribute, i.e. : Name</returns>
        public ISQLExpression ProcessReferenceAttribute(Path path, EntityMapping baseEntity, string[] references, string attributeName)
        {
            bool firstIsType = ((int)_ExprLevel.Peek() == EXPR);
            _ExprLevel.Push(_ExprLevel.Peek());

            SelectStatement subQuery = new SelectStatement(baseEntity);
            int lastIndex = path.Identifiers.Count - 1;

            int referenceCounter = 0;

            if (!firstIsType)
            {
                for (int index = 0; index < lastIndex; index++)
                {
                    string refName = path.Identifiers[index].Value;
                    ReferenceMapping refeMap = baseEntity.References[refName];

                    //	If refeMap == null, cannot find the reference in the child
                    //	Then try to get it from parent types
                    if (refeMap == null)
                    {
                        Evaluant.Uss.Models.Entity current = Model.GetEntity(baseEntity.Type);
                        if (refeMap == null)
                        {
                            while (Model.GetParent(current) != null)
                            {
                                current = Model.GetParent(current);
                                refeMap = _Mapping.Entities[current.Type].References[refName];
                                if (refeMap != null)
                                    break;
                            }
                        }
                    }

                    if (refeMap == null)
                        throw new SqlMapperException(string.Concat("Cannot find the reference", refName, " from the entity ", baseEntity.Type));

                    referenceCounter += 1;

                    //entityMap = _Mapping.Entities.GetEntity(refeMap.EntityChild);
                }
            }

            TableSource currentTable = new TableSource(baseEntity, baseEntity.Table, CreateUniqueTableAlias());
            tableContext.Push(currentTable);
            queryContext.Push(subQuery);

            bool overrideId = false;
            if (referenceCounter > 1)
                overrideId = true;

            ConvertToSQL(path, false, true, true, overrideId);

            entityMappingContext.Pop();

            subQuery = (SelectStatement)queryContext.Peek();

            //// the exists function just need ids fields 
            //// do not list attribute field to avoid problems with data type fields (text, ntext, image for sql server)

            //// Remove none needed selected item, according to the path

            //// no references: keep Type, Id (2 fields)
            //// references: keep Type, IdParent, Id (3 fields)
            int keepFieldNb = (path.Identifiers.Count == 1) ? 2 : 3;

            if (subQuery is UnionStatement)
            {
                foreach (SelectStatement select in ((UnionStatement)subQuery).SelectExpressions)
                {
                    for (int i = select.SelectList.Count - 1; i >= keepFieldNb; i--)
                        select.SelectList.RemoveAt(i);

                    // link tables for inline views
                    select.WhereClause.SearchCondition.Add(new BinaryLogicExpression(linkTableContext.Pop() as Column, BinaryLogicOperator.Equals, linkTableContext.Pop() as Column));
                }
            }
            else
            {
                for (int i = subQuery.SelectList.Count - 1; i >= keepFieldNb; i--)
                    subQuery.SelectList.RemoveAt(i);

                // link tables for inline views
                subQuery.WhereClause.SearchCondition.Add(new BinaryLogicExpression(linkTableContext.Pop() as Column, BinaryLogicOperator.Equals, linkTableContext.Pop() as Column));
            }

            UnionStatement us = subQuery as UnionStatement;
            if (us != null)
            {
                foreach (SelectStatement s in us.SelectExpressions)
                {
                    s.SelectList.Clear();
                    s.SelectList.Add(new Column(null, s.TableAlias, attributeName));
                }
            }
            else
            {
                subQuery.SelectList.Clear();
                subQuery.SelectList.Add(new Column(null, attributeName, subQuery.TableAlias));
            }

            //subQuery.SelectedAllColumns = true;
            //expression = new ExistsPredicate(subQuery);
            //sqlExpressionContext.Push(subQuery);

            tableContext.Pop();
            queryContext.Pop();

            _ExprLevel.Pop();

            //return expression;
            return subQuery;
        }

        // Process constraints
        public override void Visit(Path path)
        {
            bool firstIsType = ((int)_ExprLevel.Peek() == EXPR);

            _ExprLevel.Push(PATH);

            Identifier lastident = path.Identifiers[path.Identifiers.Count - 1];
            if (lastident.Constraint != null)
                throw new PersistenceEngineException("Constraint not allowed here");

            EntityMapping entityMap = (EntityMapping)entityMappingContext.Peek();

            if (path.Identifiers.Count == 1)
            {
                //  Attribute concerns base type
                sqlExpressionContext.Push(ProcessAttributes(entityMap, lastident.Value, false));
            }
            else
            {
                //  Attribute concers reference of base type
                string[] references = new string[path.Identifiers.Count - 1];
                for (int i = 0; i < path.Identifiers.Count - 1; i++)
                    references[i] = path.Identifiers[i].Value;

                sqlExpressionContext.Push(ProcessReferenceAttribute(path, entityMap, references, lastident.Value));
            }

            _ExprLevel.Pop();

        }

        public override void Visit(Identifier identifier)
        {
            throw new NotImplementedException();
        }

        public override void Visit(Function function)
        {
            bool firstIsType = ((int)_ExprLevel.Peek() == EXPR);
            _ExprLevel.Push(_ExprLevel.Peek());

            ISQLExpression expression = null;

            EntityMapping entityMap = null;
            if (!firstIsType)
                entityMap = (EntityMapping)entityMappingContext.Peek();

            SelectStatement subQuery = new SelectStatement(entityMap);
            int lastIndex = function.Path.Identifiers.Count - 1;

            if (!firstIsType)
            {
                for (int index = 0; index < lastIndex; index++)
                {
                    string refName = function.Path.Identifiers[index].Value;
                    ReferenceMapping refeMap = entityMap.References[refName];

                    //	If refeMap == null, cannot find the reference in the child
                    //	Then try to get it from parent types
                    if (refeMap == null)
                    {
                        Evaluant.Uss.Models.Entity current = Model.GetEntity(entityMap.Type);
                        if (refeMap == null)
                        {
                            while (Model.GetParent(current) != null)
                            {
                                current = Model.GetParent(current);
                                refeMap = _Mapping.Entities[current.Type].References[refName];
                                if (refeMap != null)
                                {
                                    entityMap = refeMap.EntityParent;
                                    break;
                                }
                            }
                        }
                    }

                    if (refeMap == null)
                        throw new SqlMapperException(string.Concat("Cannot find the reference", refName, " from the entity ", entityMap.Type));

                    //entityMap = _Mapping.Entities[refeMap.EntityChild];
                }
            }

            Table currentTable = null;

            switch (function.Type)
            {
                case FunctionEnum.Average:
                    if (firstIsType)
                    {
                        subQuery = (SelectStatement)TransformToSql(function.Path, new string[0], new string[0], true); ;
                        subQuery.TableAlias = CreateUniqueTableAlias();
                        if (subQuery is UnionStatement)
                        {
                            SelectStatement select = new SelectStatement(null);
                            select.FromClause.Add(subQuery as Table);
                            subQuery = select;
                        }

                        tableContext.Push(subQuery as Table);
                        _IsFirstAttribute = true;
                        expression = ProcessAttributes((EntityMapping)entityMappingContext.Peek(), function.Path.Identifiers[lastIndex].Value, false, true);

                        subQuery.SelectList = new ExpressionCollection();
                        subQuery.SelectList.Add(new AggregateFunction(null, AggregateFunctionEnum.Avg, expression));
                        sqlExpressionContext.Push(subQuery);
                    }
                    else
                    {
                        currentTable = new TableSource(entityMap, ((TableSource)tableContext.Peek()).TableName, CreateUniqueTableAlias());
                        tableContext.Push(currentTable);
                        queryContext.Push(subQuery);

                        ConvertToSQL(function.Path, false, true, true);
                        expression = ProcessAttributes(entityMap, function.Path.Identifiers[lastIndex].Value, false, true);

                        subQuery = (SelectStatement)queryContext.Peek();

                        if (subQuery.GetType() == typeof(UnionStatement))
                        {
                            SelectStatement sel = new SelectStatement(null);

                            subQuery.TableAlias = CreateUniqueTableAlias();
                            sel.FromClause.Add(subQuery);

                            sel.SelectList = new ExpressionCollection();
                            sel.SelectList.Add(new AggregateFunction(entityMap, AggregateFunctionEnum.Avg, expression));
                            sqlExpressionContext.Push(sel);
                        }
                        else
                        {
                            subQuery.SelectList = new ExpressionCollection();
                            subQuery.SelectList.Add(new AggregateFunction(entityMap, AggregateFunctionEnum.Avg, expression));
                            sqlExpressionContext.Push(subQuery);
                        }

                        // link tables for inline views
                        ((SelectStatement)sqlExpressionContext.Peek()).WhereClause.SearchCondition.Add(new BinaryLogicExpression(new Column(null, subQuery.TableAlias, "ParentId"), BinaryLogicOperator.Equals, linkTableContext.Pop() as Column));

                        tableContext.Pop();
                        queryContext.Pop();
                    }
                    break;
                case FunctionEnum.Count:
                    if (firstIsType)
                    {
                        subQuery = (SelectStatement)TransformToSql(function.Path, new string[0], null, false);
                        subQuery.TableAlias = CreateUniqueTableAlias();
                        if (subQuery is UnionStatement)
                        {
                            SelectStatement select = new SelectStatement(null);
                            select.FromClause.Add(subQuery as Table);
                            subQuery = select;
                        }

                        subQuery.SelectList = new ExpressionCollection();
                        subQuery.SelectList.Add(new AggregateFunction(null, AggregateFunctionEnum.Count, new Constant("*", DbType.StringFixedLength)));
                        sqlExpressionContext.Push(subQuery);
                    }
                    else
                    {
                        currentTable = new TableSource(entityMap, entityMap.Table, CreateUniqueTableAlias());
                        tableContext.Push(currentTable);
                        queryContext.Push(subQuery);

                        ConvertToSQL(function.Path, false, true, false);
                        currentTable = (Table)tableContext.Peek();
                        subQuery = (SelectStatement)queryContext.Peek();

                        if (subQuery.GetType() == typeof(UnionStatement))
                        {
                            SelectStatement sel = new SelectStatement(null);

                            subQuery.TableAlias = CreateUniqueTableAlias();
                            sel.FromClause.Add(subQuery);

                            sel.SelectList = new ExpressionCollection();
                            sel.SelectList.Add(new AggregateFunction(entityMap, AggregateFunctionEnum.Count, new Constant("*", DbType.StringFixedLength)));
                            sqlExpressionContext.Push(sel);
                        }
                        else
                        {
                            subQuery.SelectList = new ExpressionCollection();
                            subQuery.SelectList.Add(new AggregateFunction(entityMap, AggregateFunctionEnum.Count, new Constant("*", DbType.StringFixedLength)));
                            sqlExpressionContext.Push(subQuery);
                        }

                        // link tables for inline views
                        ((SelectStatement)sqlExpressionContext.Peek()).WhereClause.SearchCondition.Add(new BinaryLogicExpression(new Column(null, subQuery.TableAlias, "ParentId"), BinaryLogicOperator.Equals, linkTableContext.Pop() as Column));

                        tableContext.Pop();
                        queryContext.Pop();
                    }
                    break;

                case FunctionEnum.Exists:
                    currentTable = new TableSource(entityMap, entityMap.Table, CreateUniqueTableAlias());
                    tableContext.Push(currentTable);
                    queryContext.Push(subQuery);

                    ConvertToSQL(function.Path, false, true, false);
                    subQuery = (SelectStatement)queryContext.Peek();

                    // the exists function just need ids fields 
                    // do not list attribute field to avoid problems with data type fields (text, ntext, image for sql server)

                    // Remove none needed selected item, according to the path

                    // no references: keep Type, Id (2 fields)
                    // references: keep Type, IdParent, Id (3 fields)
                    int keepFieldNb = (function.Path.Identifiers.Count == 1) ? 2 : 3;

                    if (subQuery is UnionStatement)
                    {
                        foreach (SelectStatement select in ((UnionStatement)subQuery).SelectExpressions)
                        {
                            for (int i = select.SelectList.Count - 1; i >= keepFieldNb; i--)
                                select.SelectList.RemoveAt(i);

                            // link tables for inline views
                            Column cLeft = linkTableContext.Pop() as Column;
                            Column cRight = linkTableContext.Pop() as Column;

                            if (cLeft is MultipledKey)
                            {
                                for (int index = 0; index < ((MultipledKey)cLeft).Collection.Count; index++)
                                {
                                    select.WhereClause.SearchCondition.Add(new BinaryLogicExpression(((MultipledKey)cLeft).Collection[index], BinaryLogicOperator.Equals, ((MultipledKey)cRight).Collection[index]));
                                }
                            }
                            else
                            {
                                select.WhereClause.SearchCondition.Add(new BinaryLogicExpression(cLeft, BinaryLogicOperator.Equals, cRight));
                            }
                        }
                    }
                    else
                    {
                        for (int i = subQuery.SelectList.Count - 1; i >= keepFieldNb; i--)
                            subQuery.SelectList.RemoveAt(i);

                        // link tables for inline views
                        subQuery.WhereClause.SearchCondition.Add(new BinaryLogicExpression(linkTableContext.Pop() as Column, BinaryLogicOperator.Equals, linkTableContext.Pop() as Column));
                    }

                    subQuery.SelectedAllColumns = true;
                    expression = new ExistsPredicate(subQuery);
                    sqlExpressionContext.Push(expression);

                    tableContext.Pop();
                    queryContext.Pop();
                    break;

                case FunctionEnum.IsNull:
                    _IsFirstAttribute = true;
                    expression = new IsNullPredicate(ProcessAttributes(entityMap, function.Path.Identifiers[lastIndex].Value, false));
                    sqlExpressionContext.Push(expression);
                    break;

                case FunctionEnum.Max:
                    if (firstIsType)
                    {
                        subQuery = (SelectStatement)TransformToSql(function.Path, new string[0], new string[0], true);
                        subQuery.TableAlias = CreateUniqueTableAlias();
                        if (subQuery is UnionStatement)
                        {
                            SelectStatement select = new SelectStatement(null);
                            select.FromClause.Add(subQuery as Table);
                            subQuery = select;
                        }

                        tableContext.Push(subQuery as Table);
                        _IsFirstAttribute = true;
                        expression = ProcessAttributes((EntityMapping)entityMappingContext.Peek(), function.Path.Identifiers[lastIndex].Value, false, true);

                        subQuery.SelectList = new ExpressionCollection();
                        subQuery.SelectList.Add(new AggregateFunction(null, AggregateFunctionEnum.Max, expression));
                        sqlExpressionContext.Push(subQuery);
                    }
                    else
                    {
                        currentTable = new TableSource(entityMap, ((TableSource)tableContext.Peek()).TableName, CreateUniqueTableAlias());
                        tableContext.Push(currentTable);
                        queryContext.Push(subQuery);
                        entityMappingContext.Push(entityMap);
                        ConvertToSQL(function.Path, false, true, true);
                        
                        EntityMapping lastEM = (EntityMapping)entityMappingContext.Peek();

                        //entityModelContext.Pop();
                        entityModelContext.Peek();

                        expression = ProcessAttributes(lastEM, function.Path.Identifiers[lastIndex].Value, false, true);

                        subQuery = (SelectStatement)queryContext.Peek();

                        if (subQuery.GetType() == typeof(UnionStatement))
                        {
                            SelectStatement sel = new SelectStatement(null);

                            subQuery.TableAlias = CreateUniqueTableAlias();
                            sel.FromClause.Add(subQuery);

                            sel.SelectList = new ExpressionCollection();
                            sel.SelectList.Add(new AggregateFunction(lastEM, AggregateFunctionEnum.Max, expression));
                            sqlExpressionContext.Push(sel);
                        }
                        else
                        {
                            subQuery.SelectList = new ExpressionCollection();
                            subQuery.SelectList.Add(new AggregateFunction(lastEM, AggregateFunctionEnum.Max, expression));
                            sqlExpressionContext.Push(subQuery);
                        }

                        // link tables for inline views
                        if (linkTableContext.Count > 0)
                            ((SelectStatement)sqlExpressionContext.Peek()).WhereClause.SearchCondition.Add(
                                    new BinaryLogicExpression(new Column(null, subQuery.TableAlias, "ParentId"), BinaryLogicOperator.Equals, linkTableContext.Pop() as Column));

                        tableContext.Pop();
                        queryContext.Pop();
                        entityMappingContext.Pop();
                    }
                    break;

                case FunctionEnum.Min:
                    if (firstIsType)
                    {
                        subQuery = (SelectStatement)TransformToSql(function.Path, new string[0], new string[0], true); ;
                        subQuery.TableAlias = CreateUniqueTableAlias();
                        if (subQuery is UnionStatement)
                        {
                            SelectStatement select = new SelectStatement(null);
                            select.FromClause.Add(subQuery as Table);
                            subQuery = select;
                        }

                        tableContext.Push(subQuery as Table);
                        _IsFirstAttribute = true;
                        expression = ProcessAttributes((EntityMapping)entityMappingContext.Peek(), function.Path.Identifiers[lastIndex].Value, false, true);

                        subQuery.SelectList = new ExpressionCollection();
                        subQuery.SelectList.Add(new AggregateFunction(null, AggregateFunctionEnum.Min, expression));
                        sqlExpressionContext.Push(subQuery);
                    }
                    else
                    {
                        currentTable = new TableSource(entityMap, ((TableSource)tableContext.Peek()).TableName, CreateUniqueTableAlias());
                        tableContext.Push(currentTable);
                        queryContext.Push(subQuery);

                        ConvertToSQL(function.Path, false, true, true);
                        expression = ProcessAttributes(entityMap, function.Path.Identifiers[lastIndex].Value, false, true);

                        subQuery = (SelectStatement)queryContext.Peek();

                        if (subQuery.GetType() == typeof(UnionStatement))
                        {
                            SelectStatement sel = new SelectStatement(null);

                            subQuery.TableAlias = CreateUniqueTableAlias();
                            sel.FromClause.Add(subQuery);

                            sel.SelectList = new ExpressionCollection();
                            sel.SelectList.Add(new AggregateFunction(entityMap, AggregateFunctionEnum.Min, expression));
                            sqlExpressionContext.Push(sel);
                        }
                        else
                        {
                            subQuery.SelectList = new ExpressionCollection();
                            subQuery.SelectList.Add(new AggregateFunction(entityMap, AggregateFunctionEnum.Min, expression));
                            sqlExpressionContext.Push(subQuery);
                        }

                        // link tables for inline views
                        ((SelectStatement)sqlExpressionContext.Peek()).WhereClause.SearchCondition.Add(new BinaryLogicExpression(new Column(null, subQuery.TableAlias, "ParentId"), BinaryLogicOperator.Equals, linkTableContext.Pop() as Column));

                        tableContext.Pop();
                        queryContext.Pop();
                    }
                    break;

                case FunctionEnum.Sum:
                    if (firstIsType)
                    {
                        subQuery = (SelectStatement)TransformToSql(function.Path, new string[0], new string[0], true);
                        subQuery.TableAlias = CreateUniqueTableAlias();

                        if (subQuery is UnionStatement)
                        {
                            SelectStatement select = new SelectStatement(null);
                            select.FromClause.Add(subQuery as Table);
                            subQuery = select;
                        }

                        tableContext.Push(subQuery as Table);
                        _IsFirstAttribute = true;
                        expression = ProcessAttributes((EntityMapping)entityMappingContext.Peek(), function.Path.Identifiers[lastIndex].Value, false, true);

                        subQuery.SelectList = new ExpressionCollection();
                        subQuery.SelectList.Add(new AggregateFunction(null, AggregateFunctionEnum.Sum, expression));
                        sqlExpressionContext.Push(subQuery);
                    }
                    else
                    {
                        currentTable = new TableSource(entityMap, ((TableSource)tableContext.Peek()).TableName, CreateUniqueTableAlias());
                        tableContext.Push(currentTable);
                        queryContext.Push(subQuery);

                        ConvertToSQL(function.Path, false, true, true);
                        expression = ProcessAttributes(entityMap, function.Path.Identifiers[lastIndex].Value, false, true);

                        subQuery = (SelectStatement)queryContext.Peek();
                        if (subQuery.GetType() == typeof(UnionStatement))
                        {
                            SelectStatement sel = new SelectStatement(null);

                            subQuery.TableAlias = CreateUniqueTableAlias();
                            sel.FromClause.Add(subQuery);

                            sel.SelectList = new ExpressionCollection();
                            sel.SelectList.Add(new AggregateFunction(entityMap, AggregateFunctionEnum.Sum, expression));
                            sqlExpressionContext.Push(sel);
                        }
                        else
                        {
                            subQuery.SelectList = new ExpressionCollection();
                            subQuery.SelectList.Add(new AggregateFunction(entityMap, AggregateFunctionEnum.Sum, expression));
                            sqlExpressionContext.Push(subQuery);
                        }

                        // link tables for inline views
                        ((SelectStatement)sqlExpressionContext.Peek()).WhereClause.SearchCondition.Add(new BinaryLogicExpression(new Column(null, subQuery.TableAlias, "ParentId"), BinaryLogicOperator.Equals, linkTableContext.Pop() as Column));

                        tableContext.Pop();
                        queryContext.Pop();
                    }
                    break;
            }

            _ExprLevel.Pop();
        }

        public override void Visit(BinaryOperator binaryop)
        {
            bool firstIsType = ((int)_ExprLevel.Peek() == EXPR);
            _ExprLevel.Push(_ExprLevel.Peek());

            ISQLExpression expression = null;

            EntityMapping entityMap = null;
            if (!firstIsType)
                entityMap = (EntityMapping)entityMappingContext.Peek();

            //	Try to get the queried attribute. If it is a date type and the 
            //	corresponding mapping is generic, we have to convert the value as ticks
            //	instead of the standard representation

            Value val = binaryop.LeftOperand as Value;
            Path path = binaryop.LeftOperand as Path;

            if (val == null)
                val = binaryop.RightOperand as Value;

            if (path == null)
                path = binaryop.RightOperand as Path;

            if (!firstIsType)
            {
                if (val != null && path != null && path.Identifiers.Count == 1)
                {
                    AttributeMapping am = entityMap.Attributes[path.Identifiers[path.Identifiers.Count - 1].Value, true];

                    if (val.Type == ValueEnum.Date)
                    {
                        //	If the field type isn't date type, convert the value as ticks
                        if (am.DbType != DbType.Date && am.DbType != DbType.DateTime && am.DbType != DbType.Time)
                        {
                            DateTime dt = Convert.ToDateTime(val.Text);
                            val.Text = Common.Utils.ConvertToString(dt, dt.GetType());
                        }
                    }
                }
            }

            ISQLExpression discriminatorConstraint = null;
            Path p = binaryop.LeftOperand as Path;
            if (p == null)
                p = binaryop.RightOperand as Path;

            if (p != null && !firstIsType)
            {
                AttributeMapping am = entityMap.Attributes[p.Identifiers[p.Identifiers.Count - 1].Value];

                if (am != null &&
                    am.Discriminator != null &&
                    am.Discriminator != string.Empty &&
                    am.DiscriminatorValue != null &&
                    am.DiscriminatorValue != string.Empty)
                {
                    Column left = new Column(am, am.Discriminator);
                    Constant right = new Constant(path.Identifiers[path.Identifiers.Count - 1].Value, DbType.AnsiStringFixedLength);
                    discriminatorConstraint = new BinaryLogicExpression(
                        left,
                        BinaryLogicOperator.Equals,
                        right);
                }
            }

            switch (binaryop.Type)
            {
                case BinaryOperatorEnum.And:
                    binaryop.LeftOperand.Accept(this);
                    ISQLExpression leftValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    binaryop.RightOperand.Accept(this);
                    ISQLExpression rightValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    expression = new BinaryLogicExpression(leftValueExpression, BinaryLogicOperator.And, rightValueExpression);

                    if (discriminatorConstraint != null)
                        expression = new BinaryLogicExpression(
                            expression,
                            BinaryLogicOperator.And,
                            discriminatorConstraint);

                    sqlExpressionContext.Push(expression);

                    break;
                case BinaryOperatorEnum.Or:
                    binaryop.LeftOperand.Accept(this);
                    leftValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    binaryop.RightOperand.Accept(this);
                    rightValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    expression = new BinaryLogicExpression(leftValueExpression, BinaryLogicOperator.Or, rightValueExpression);

                    if (discriminatorConstraint != null)
                        expression = new BinaryLogicExpression(
                            expression,
                            BinaryLogicOperator.And,
                            discriminatorConstraint);

                    sqlExpressionContext.Push(expression);

                    break;
                case BinaryOperatorEnum.Equal:

                    binaryop.LeftOperand.Accept(this);
                    leftValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    binaryop.RightOperand.Accept(this);
                    rightValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    expression = new BinaryLogicExpression(leftValueExpression, BinaryLogicOperator.Equals, rightValueExpression);

                    if (discriminatorConstraint != null)
                        expression = new BinaryLogicExpression(
                            expression,
                            BinaryLogicOperator.And,
                            discriminatorConstraint);

                    sqlExpressionContext.Push(expression);

                    break;
                case BinaryOperatorEnum.Greater:
                    binaryop.LeftOperand.Accept(this);
                    leftValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    binaryop.RightOperand.Accept(this);
                    rightValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    expression = new BinaryLogicExpression(leftValueExpression, BinaryLogicOperator.Greater, rightValueExpression);

                    if (discriminatorConstraint != null)
                        expression = new BinaryLogicExpression(
                            expression,
                            BinaryLogicOperator.And,
                            discriminatorConstraint);

                    sqlExpressionContext.Push(expression);
                    break;

                case BinaryOperatorEnum.GreaterOrEqual:
                    binaryop.LeftOperand.Accept(this);
                    leftValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    binaryop.RightOperand.Accept(this);
                    rightValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    expression = new BinaryLogicExpression(leftValueExpression, BinaryLogicOperator.GreaterOrEquals, rightValueExpression);

                    if (discriminatorConstraint != null)
                        expression = new BinaryLogicExpression(
                            expression,
                            BinaryLogicOperator.And,
                            discriminatorConstraint);

                    sqlExpressionContext.Push(expression);
                    break;
                case BinaryOperatorEnum.Lesser:
                    binaryop.LeftOperand.Accept(this);
                    leftValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    binaryop.RightOperand.Accept(this);
                    rightValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    expression = new BinaryLogicExpression(leftValueExpression, BinaryLogicOperator.Lesser, rightValueExpression);

                    if (discriminatorConstraint != null)
                        expression = new BinaryLogicExpression(
                            expression,
                            BinaryLogicOperator.And,
                            discriminatorConstraint);

                    sqlExpressionContext.Push(expression);
                    break;
                case BinaryOperatorEnum.LesserOrEqual:
                    binaryop.LeftOperand.Accept(this);
                    leftValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    binaryop.RightOperand.Accept(this);
                    rightValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    expression = new BinaryLogicExpression(leftValueExpression, BinaryLogicOperator.LesserOrEquals, rightValueExpression);

                    if (discriminatorConstraint != null)
                        expression = new BinaryLogicExpression(
                            expression,
                            BinaryLogicOperator.And,
                            discriminatorConstraint);

                    sqlExpressionContext.Push(expression);
                    break;

                case BinaryOperatorEnum.NotEqual:
                    binaryop.LeftOperand.Accept(this);
                    leftValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    binaryop.RightOperand.Accept(this);
                    rightValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    expression = new BinaryLogicExpression(leftValueExpression, BinaryLogicOperator.NotEquals, rightValueExpression);

                    if (discriminatorConstraint != null)
                        expression = new BinaryLogicExpression(
                            expression,
                            BinaryLogicOperator.And,
                            discriminatorConstraint);

                    sqlExpressionContext.Push(expression);
                    break;

                case BinaryOperatorEnum.Plus:
                case BinaryOperatorEnum.Times:
                case BinaryOperatorEnum.Div:
                case BinaryOperatorEnum.Minus:
                case BinaryOperatorEnum.Modulo:
                    binaryop.LeftOperand.Accept(this);
                    leftValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    binaryop.RightOperand.Accept(this);
                    rightValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    expression = new BinaryLogicExpression(leftValueExpression, BinaryOperatorToBinaryLogicOperator(binaryop.Type), rightValueExpression);

                    if (discriminatorConstraint != null)
                        expression = new BinaryLogicExpression(expression, BinaryLogicOperator.And, discriminatorConstraint);

                    sqlExpressionContext.Push(expression);
                    break;

                case BinaryOperatorEnum.Contains:
                    binaryop.LeftOperand.Accept(this);
                    leftValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    binaryop.RightOperand.Accept(this);
                    rightValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    expression = new LikePredicate(leftValueExpression, String.Format("%{0}%", (string)((Constant)rightValueExpression).Value));

                    if (discriminatorConstraint != null)
                        expression = new BinaryLogicExpression(
                            expression,
                            BinaryLogicOperator.And,
                            discriminatorConstraint);

                    sqlExpressionContext.Push(expression);
                    break;

                case BinaryOperatorEnum.BeginsWith:
                    binaryop.LeftOperand.Accept(this);
                    leftValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    binaryop.RightOperand.Accept(this);
                    rightValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    expression = new LikePredicate(leftValueExpression, String.Format("{0}%", (string)((Constant)rightValueExpression).Value));

                    if (discriminatorConstraint != null)
                        expression = new BinaryLogicExpression(
                            expression,
                            BinaryLogicOperator.And,
                            discriminatorConstraint);

                    sqlExpressionContext.Push(expression);

                    break;
                case BinaryOperatorEnum.EndsWith:
                    binaryop.LeftOperand.Accept(this);
                    leftValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    binaryop.RightOperand.Accept(this);
                    rightValueExpression = (ISQLExpression)sqlExpressionContext.Pop();

                    expression = new LikePredicate(leftValueExpression, String.Format("%{0}", (string)((Constant)rightValueExpression).Value));

                    if (discriminatorConstraint != null)
                        expression = new BinaryLogicExpression(
                            expression,
                            BinaryLogicOperator.And,
                            discriminatorConstraint);

                    sqlExpressionContext.Push(expression);

                    break;
            }

            _ExprLevel.Pop();
        }

        public override void Visit(UnaryOperator unaryop)
        {
            ISQLExpression expression = null;
            UnaryLogicOperator op = UnaryLogicOperator.Unknown;
            switch (unaryop.Type)
            {
                case UnaryOperatorEnum.Minus:
                    op = UnaryLogicOperator.Minus;
                    break;
                case UnaryOperatorEnum.Not:
                    op = UnaryLogicOperator.Not;
                    break;
            }

            unaryop.Operand.Accept(this);
            expression = new UnaryLogicExpression(op, (ISQLExpression)sqlExpressionContext.Pop());
            sqlExpressionContext.Push(expression);
        }

        private ValueEnum GetValueEnum(DbType type)
        {
            switch (type)
            {
                case DbType.Boolean:
                    return ValueEnum.Boolean;
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                    return ValueEnum.Integer;
                case DbType.Time:
                case DbType.Date:
                case DbType.DateTime:
                    return ValueEnum.Date;
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return ValueEnum.String;
                case DbType.Double:
                case DbType.Decimal:
                    return ValueEnum.Float;
                default:
                    return ValueEnum.Unknown;
            }
        }

        public override void Visit(Evaluant.OPath.Expressions.Constraint constraint)
        {
            string s = string.Empty;
        }

        public override void Visit(Call call)
        {
            if (call.Name == "id")
            {
                Table table = (Table)tableContext.Peek();

                EntityMapping em = (EntityMapping)entityMappingContext.Peek();

                if (em.Ids.Count == 1)
                {
                    Column opRight = new Column(em.Ids[0], table.TableAlias, em.GetIdField(em.Ids[0]));

                    InPredicate ip = new InPredicate(opRight);

                    DbType type = DbType.Object;
                    if (em.Ids[0].Generator.Name == GeneratorMapping.GeneratorType.guid)
                        type = DbType.AnsiString;
                    else
                    {
                        // If generator is inherited => Get the parameters of the parent type
                        EntityMapping parentEM = em;
                        if (em.Ids[0].Generator.Name == GeneratorMapping.GeneratorType.inherited)
                        {
                            Evaluant.Uss.Models.Entity current = Model.GetEntity(em.Type);

                            while (Model.GetParent(current) != null)
                            {
                                current = Model.GetParent(current);
                                parentEM = _Mapping.Entities[current.Type];
                                if (parentEM.Ids[0].Generator.Name != GeneratorMapping.GeneratorType.inherited)
                                {
                                    parentEM.Ids[0].Generator.Params = _Mapping.Entities[current.Type].Ids[0].Generator.Params;
                                    break;
                                }
                            }
                        }

                        if (parentEM.Ids[0].Generator.Name == GeneratorMapping.GeneratorType.guid)
                            type = DbType.AnsiString;
                        else
                            type = (DbType)Enum.Parse(typeof(DbType), parentEM.Ids[0].Generator.GetParam(DBDialect.DBTYPE).Value);
                    }

                    foreach (Evaluant.OPath.Expressions.Constraint constraint in call.Operands)
                    {
                        if ((constraint as Value) != null)
                        {
                            if (((Value)constraint).Text == string.Empty)
                            {
                                ((Value)constraint).Text = null;
                                ((Value)constraint).Type = GetValueEnum(type);
                            }
                        }

                        constraint.Accept(this);

                        if (((Constant)sqlExpressionContext.Peek()).Value == null)
                        {
                            sqlExpressionContext.Pop();
                            sqlExpressionContext.Push(new Constant(DBNull.Value, type));
                        }

                        ip.SubQueries.Add(new Constant(((Constant)sqlExpressionContext.Pop()).Value, type));
                    }

                    sqlExpressionContext.Push(ip);
                }
                else
                {
                    // TODO : Logique des clés primaires
                    for (int indexConstraint = 0; indexConstraint < call.Operands.Count; indexConstraint++)
                    {
                        Evaluant.OPath.Expressions.Constraint constraint = call.Operands[indexConstraint];

                        for (int i = 0; i < em.Ids.Count; i++)
                        {
                            PrimaryKeyMapping pkm = em.Ids[i];
                            DbType dbType = _Dialect.GetDbTypeToPrimaryKey(pkm.Generator);

                            Value value = constraint as Value;
                            if ((constraint as Value) != null)
                            {
                                if (value.Text == string.Empty)
                                {
                                    value.Text = null;
                                    value.Type = GetValueEnum(dbType);
                                }
                            }

                            string[] values = value.Text.Split(SqlMapperProvider.IDSEP);

                            if (i >= values.Length)
                                break;
                            Value newValue = new Value(values[i], GetValueEnum(_Dialect.GetDbTypeToPrimaryKey(pkm.Generator)));
                            newValue.Accept(this);

                            if (((Constant)sqlExpressionContext.Peek()).Value == null)
                            {
                                sqlExpressionContext.Pop();
                                sqlExpressionContext.Push(new Constant(DBNull.Value, dbType));
                            }

                            Column field = new Column(pkm, table.TableAlias, em.GetIdField(pkm));

                            sqlExpressionContext.Push(new BinaryLogicExpression(field, BinaryLogicOperator.Equals, (Constant)sqlExpressionContext.Pop()));

                            if (i != 0)
                            {
                                BinaryLogicExpression leftOperand = (BinaryLogicExpression)sqlExpressionContext.Pop();
                                BinaryLogicExpression rightOperand = (BinaryLogicExpression)sqlExpressionContext.Pop();

                                sqlExpressionContext.Push(new BinaryLogicExpression(leftOperand, BinaryLogicOperator.And, rightOperand));
                            }

                        }

                        if (indexConstraint != 0)
                        {
                            BinaryLogicExpression leftOp = (BinaryLogicExpression)sqlExpressionContext.Pop();
                            BinaryLogicExpression rightOp = (BinaryLogicExpression)sqlExpressionContext.Pop();

                            sqlExpressionContext.Push(new BinaryLogicExpression(leftOp, BinaryLogicOperator.Or, rightOp));
                        }
                    }
                }
            }
        }

        public override void Visit(Value val)
        {
            ISQLExpression value = null;
            switch (val.Type)
            {
                case ValueEnum.Boolean:
                    value = new Constant(val.Text, DbType.Boolean);
                    break;
                case ValueEnum.Date:
                    value = new Constant(val.Text, DbType.Date);
                    break;
                case ValueEnum.Float:
                    value = new Constant(val.Text, DbType.Double);
                    break;
                case ValueEnum.Integer:
                    value = new Constant(val.Text, DbType.Int32);
                    break;
                case ValueEnum.String:
                    value = new Constant(val.Text, DbType.AnsiString);
                    break;
                case ValueEnum.Unknown:
                    value = null;
                    break;
            }
            sqlExpressionContext.Push(value);
        }

        #endregion

        #region Functions

        private static BinaryLogicOperator BinaryOperatorToBinaryLogicOperator(BinaryOperatorEnum binaryop)
        {
            switch (binaryop)
            {
                case BinaryOperatorEnum.And: return BinaryLogicOperator.And;
                case BinaryOperatorEnum.Div: return BinaryLogicOperator.Div;
                case BinaryOperatorEnum.Equal: return BinaryLogicOperator.Equals;
                case BinaryOperatorEnum.Greater: return BinaryLogicOperator.Greater;
                case BinaryOperatorEnum.GreaterOrEqual: return BinaryLogicOperator.GreaterOrEquals;
                case BinaryOperatorEnum.Lesser: return BinaryLogicOperator.Lesser;
                case BinaryOperatorEnum.LesserOrEqual: return BinaryLogicOperator.LesserOrEquals;
                case BinaryOperatorEnum.Minus: return BinaryLogicOperator.Minus;
                case BinaryOperatorEnum.Modulo: return BinaryLogicOperator.Modulo;
                case BinaryOperatorEnum.NotEqual: return BinaryLogicOperator.NotEquals;
                case BinaryOperatorEnum.Or: return BinaryLogicOperator.Or;
                case BinaryOperatorEnum.Plus: return BinaryLogicOperator.Plus;
                case BinaryOperatorEnum.Times: return BinaryLogicOperator.Times;
            }

            throw new PersistenceEngineException("Operator not managed: " + binaryop.ToString());
        }

        public StringCollection GetInheritedAttributes_(string type)
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

        #endregion

    }
}
