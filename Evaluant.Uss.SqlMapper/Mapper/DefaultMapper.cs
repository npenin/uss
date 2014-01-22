using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlExpressions;
//using Evaluant.Uss.Mapping;
using Evaluant.Uss.SqlExpressions.Mapping;
using Evaluant.Uss.Model;
using Evaluant.Uss.Era;
using Evaluant.Uss.SqlMapper.DbExpressionVisitors;

namespace Evaluant.Uss.SqlMapper.Mapper
{
    public class DefaultMapper : IMapper
    {
        private IDriver driver;

        public DefaultMapper(IDriver driver)
        {
            this.driver = driver;
        }

        public DefaultMapper(IDriver driver, Mapping.Mapping mapping)
            : this(driver)
        {
            this.driver = driver;
            this.Mapping = mapping;
        }


        private Model.Model model;

        public void Map(Model.Model model, params IMapperOption[] options)
        {
            this.model = model;
            foreach (Model.Entity e in model)
                GetEntityMapping(e, options);
        }

        private Mapping.Entity GetEntityMapping(Model.Entity e, params IMapperOption[] options)
        {
            if (Mapping.Entities.ContainsKey(e.Type))
                return Mapping.Entities[e.Type];

            Mapping.Entity em = new Mapping.Entity();
            em.Type = e.Type;
            em.EntityModel = e;
            Mapping.Entities.Add(em.Type, em);
            //DefaultStrategy : TablePerHierarchy
            if (!string.IsNullOrEmpty(e.Inherit))
            {
                Mapping.Entity parentEm = GetEntityMapping(model.Entities[e.Inherit], options);
                em.TableName = parentEm.TableName;
                em.Schema = parentEm.Schema;
                em.Inherit = parentEm;
                parentEm.Derived.Add(em);
                SqlMapper.Mapping.InheritanceMappings inheritanceType = SqlMapper.Mapping.InheritanceMappings.TablePerHierarchy;
                if (em.Inheritance != null)
                {
                    inheritanceType = em.Inheritance.Type;
                }

                switch (inheritanceType)
                {
                    case Evaluant.Uss.SqlMapper.Mapping.InheritanceMappings.TablePerClass:
                        break;
                    case Evaluant.Uss.SqlMapper.Mapping.InheritanceMappings.TablePerHierarchy:
                        foreach (Mapping.Reference rm in em.Inherit.References.Values)
                            em.Add(rm.Clone(em.Inherit.TableName, em));
                        foreach (Mapping.Attribute am in em.Inherit.Attributes.Values)
                            em.Add(am.Clone(em.Inherit.TableName, em));
                        if (em.Inheritance.Discriminator == null)
                            em.Inheritance.Discriminator = "Type=='" + em.Type + "'";
                        Field[] fields;
                        if (parentEm.Inheritance.Discriminator == null)
                        {
                            parentEm.Inheritance.Discriminator = "Type=='" + parentEm.Type + "'";
                            fields = GetFields(parentEm.Inheritance.DiscriminatorExpression);
                            foreach (Field field in fields)
                            {
                                parentEm.Add(field);
                            }
                        }
                        fields = GetFields(em.Inheritance.DiscriminatorExpression);
                        foreach (Field field in fields)
                        {
                            em.Add(field);
                        }
                        break;
                    case Evaluant.Uss.SqlMapper.Mapping.InheritanceMappings.TablePerConcreteClass:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                em.Schema = e.Type.Substring(0, e.Type.LastIndexOf('.'));
                em.TableName = e.Type.Substring(em.Schema.Length + 1);
            }
            foreach (KeyValuePair<string, Model.Attribute> attribute in e.Attributes)
            {
                Mapping.Attribute field = new Mapping.Attribute(attribute.Key);
                field.Model = attribute.Value;
                field.ColumnName = attribute.Value.Name;
                field.IsId = attribute.Value.Name == "Id" || attribute.Value.Name == e.Type.Substring(e.Type.LastIndexOf('.') + 1) + "Id";
                field.Name = attribute.Value.Name;
                field.IsNullable = true;
                //if (attribute.Key != "#Id")
                driver.GetTypeInformation(attribute.Value, field);
                //else
                //{
                //    field.DbType = System.Data.DbType.Guid;
                //    field.DefaultValue = Guid.Empty.ToString();
                //}
                em.Add(field);
            }

            foreach (KeyValuePair<string, Model.Reference> reference in e.References)
            {
                Mapping.Reference rm = new Mapping.Reference();
                rm.Name = reference.Value.Name;
                rm.Model = reference.Value;
                rm.ChildType = reference.Value.ChildType;
                //rm.Target = GetEntityMapping(model.Entities[reference.Value.ChildType]);
                //rm.Cardinality = reference.Value.Cardinality;
                //rm.Inherited = reference.Value.Inherited;
                if (reference.Value.Cardinality.IsToMany)
                {
                    string[] parentFields = new string[em.Ids.Keys.Count];
                    em.Ids.Keys.CopyTo(parentFields, 0);
                    var target = GetEntityMapping(model.Entities[rm.ChildType]);
                    if (target.Ids.Count == 0)
                        throw new Mapping.MappingException("No Id could be found on the " + target.Type + " entity you need to either specify one or specify manually the way you want to map this reference " + reference.Value);
                    string[] childFields = new string[target.Ids.Keys.Count];
                    target.Ids.Keys.CopyTo(childFields, 0);
                    string indexSchema = e.Type.Substring(0, e.Type.LastIndexOf('.'));
                    string indexTableName = e.Type.Substring(indexSchema.Length + 1) + "_" + rm.Name;
                    rm.Rules.Add(new Mapping.Rule() { ParentFieldNames = string.Join(",", parentFields), ChildSchema = indexSchema, ChildTableName = indexTableName, ChildFieldNames = "FK_Parent" + string.Join(",FK_Parent", parentFields) });
                    rm.Rules.Add(new Mapping.Rule() { ParentSchema = indexSchema, ParentTableName = indexTableName, ParentFieldNames = "FK_Child" + string.Join(",FK_Child", childFields), ChildSchema = target.Schema, ChildTableName = target.TableName, ChildFieldNames = string.Join(",", childFields) });
                }
                else
                {
                    if (reference.Value.Cardinality.IsOne)
                    {
                        var target = GetEntityMapping(model.Entities[rm.ChildType]);
                        string[] parentFields = new string[em.Ids.Keys.Count];
                        em.Ids.Keys.CopyTo(parentFields, 0);
                        rm.Rules.Add(new Mapping.Rule() { ParentFieldNames = string.Join(",", parentFields), ChildSchema = target.Schema, ChildTableName = target.TableName, ChildFieldNames = "FK_" + em.TableName + string.Join(",FK_" + em.TableName, parentFields) });
                    }
                    if (reference.Value.Cardinality.IsMany)
                    {
                        var target = GetEntityMapping(model.Entities[rm.ChildType]);
                        string[] childFields = new string[target.Ids.Keys.Count];
                        target.Ids.Keys.CopyTo(childFields, 0);
                        rm.Rules.Add(new Mapping.Rule() { ParentFieldNames = "FK_" + target.TableName + string.Join(",FK_" + target.TableName, childFields), ChildSchema = target.Schema, ChildTableName = target.TableName, ChildFieldNames = string.Join(",", childFields) });
                    }

                }
                em.Add(rm);
            }
            return em;
        }

        private Field[] GetFields(Expression expression)
        {
            FieldExtractionVisitor fev = new FieldExtractionVisitor(driver);
            fev.Visit(expression);
            return fev.Fields.ToArray();
        }

        private void BuildConstraint(Evaluant.Uss.SqlMapper.Mapping.Reference rm, out TableAlias firstAlias)
        {
            BinaryExpression on = null;
            JoinedTableExpression join = null;
            TableAlias lastAlias = null;
            firstAlias = new LazyTableAlias(rm.Rules[0].ParentTable);
            foreach (Mapping.Rule rule in rm.Rules)
            {
                JoinedTableExpression tempJoin = new JoinedTableExpression(new TableSourceExpression(lastAlias ?? firstAlias, rule.ParentTable), new TableSourceExpression(new TableAlias(), rule.ChildTable));

                BinaryExpression tempOn = null;
                for (int i = 0; i < rule.ParentFields.Count; i++)
                {
                    Field parentField = rule.ParentFields[i];
                    Field childField = rule.ChildFields[i];
                    BinaryExpression localOn = new BinaryExpression(BinaryExpressionType.Equal,
                        new ColumnExpression(tempJoin.LeftTable.Alias, parentField.ColumnName),
                        new ColumnExpression(tempJoin.RightTable.Alias, childField.ColumnName));
                    if (tempOn == null)
                        tempOn = localOn;
                    else
                        tempOn = new BinaryExpression(BinaryExpressionType.And, tempOn, localOn);
                }
                tempJoin.On = tempOn;
                if (join != null)
                    tempJoin.LeftTable = join;
                lastAlias = tempJoin.RightTable.Alias;
                join = tempJoin;
                if (on == null)
                    on = tempOn;
                else
                    on = new BinaryExpression(BinaryExpressionType.And, on, tempOn);

            }
            joins[rm] = join;
            ons[rm] = on;
        }

        private IDictionary<Mapping.Reference, BinaryExpression> ons = new Dictionary<Mapping.Reference, BinaryExpression>();
        private IDictionary<Mapping.Reference, IAliasedExpression> joins = new Dictionary<Mapping.Reference, IAliasedExpression>();
        private IDictionary<Mapping.Reference, TableAlias> alias = new Dictionary<Mapping.Reference, TableAlias>();

        public Mapping.Mapping Mapping { get; set; }

        #region IMapper Members

        public BinaryExpression On(Mapping.Reference rm)
        {
            TableAlias firstTableAlias;
            if (!ons.ContainsKey(rm))
                BuildConstraint(rm, out firstTableAlias);
            return ons[rm];
        }

        public IAliasedExpression Join(Mapping.Reference rm, out TableAlias firstTableAlias)
        {
            if (!joins.ContainsKey(rm) || !alias.ContainsKey(rm))
                BuildConstraint(rm, out firstTableAlias);
            else
                firstTableAlias = alias[rm];
            return joins[rm];
        }



        public IDbStatement Create(Mapping.Reference reference)
        {
            return Create(reference, null);
        }
        public IDbStatement Create(Mapping.Reference reference, ValuedParameter idParameter)
        {
            //Case of single rule and FK is in the child table
            if (reference.Model.Cardinality.IsOne && reference.Model.Cardinality is Cardinality.ToMany)
            {
                Mapping.Rule rule = reference.Rules[0];
                TableAlias alias = null;
                UpdateStatement update = new UpdateStatement() { From = new Evaluant.Uss.SqlExpressions.FromClause(new TableSourceExpression(alias, rule.ChildTable)) };
                foreach (Field field in rule.ChildFields)
                    update.Set.Add(new ColumnExpression(alias, field.ColumnName), new Evaluant.Uss.SqlExpressions.DbParameter(field.ColumnName.Text, field.DbType, System.Data.ParameterDirection.Input));
                BinaryExpression constraint = null;
                foreach (Mapping.Attribute attribute in reference.Target.Ids.Values)
                {
                    Field field = attribute.Field;
                    BinaryExpression fieldConstraint = new BinaryExpression(BinaryExpressionType.Equal, new ColumnExpression(alias, field.ColumnName), new SqlExpressions.DbParameter(field.ColumnName.Text, field.DbType, System.Data.ParameterDirection.Input));
                    if (constraint == null)
                        constraint = fieldConstraint;
                    else
                        constraint = new BinaryExpression(BinaryExpressionType.And, constraint, fieldConstraint);
                }
                update.Where = new WhereClause(constraint);
                return update;
            }
            //Case of single rule and FK is in the parent table
            if (reference.Model.Cardinality.IsMany && reference.Model.Cardinality is Cardinality.ToOne)
            {
                Mapping.Rule rule = reference.Rules[0];
                TableAlias alias = null;
                UpdateStatement update = new UpdateStatement() { From = new Evaluant.Uss.SqlExpressions.FromClause(new TableSourceExpression(alias, rule.ParentTable)) };
                foreach (Field field in rule.ParentFields)
                    update.Set.Add(new ColumnExpression(alias, field.ColumnName), new Evaluant.Uss.SqlExpressions.DbParameter(field.ColumnName.Text, field.DbType, System.Data.ParameterDirection.Input));
                BinaryExpression constraint = null;
                foreach (Mapping.Attribute attribute in reference.Parent.Ids.Values)
                {
                    Field field = attribute.Field;
                    BinaryExpression fieldConstraint = new BinaryExpression(BinaryExpressionType.Equal, new ColumnExpression(alias, field.ColumnName), new SqlExpressions.DbParameter(field.ColumnName.Text, field.DbType, System.Data.ParameterDirection.Input));
                    if (constraint == null)
                        constraint = fieldConstraint;
                    else
                        constraint = new BinaryExpression(BinaryExpressionType.And, constraint, fieldConstraint);
                }
                update.Where = new WhereClause(constraint);
                return update;
            }
            //Case of 2 rules with an index table
            if (reference.Model.Cardinality.IsMany && reference.Model.Cardinality is Cardinality.ToMany)
            {
                TableAlias alias = null;
                Table indexTable = reference.Rules[0].ChildTable;
                Dictionary<ColumnExpression, Expression> values = new Dictionary<ColumnExpression, Expression>();
                InsertStatement insert = new InsertStatement(new TableSourceExpression(alias, indexTable), values);

                foreach (Mapping.Rule rule in reference.Rules)
                {
                    if (rule.ParentTable == indexTable)
                    {
                        foreach (Field field in rule.ParentFields)
                            values.Add(new ColumnExpression(alias, field.ColumnName), new SqlExpressions.DbParameter(field.ColumnName.Text, field.DbType, System.Data.ParameterDirection.Input));
                    }
                    if (rule.ChildTable == indexTable)
                    {
                        foreach (Field field in rule.ChildFields)
                        {
                            if (field.IsIdentity)

                                values.Add(new ColumnExpression(alias, field.ColumnName), new LazyParameter(field.ColumnName.Text, idParameter, System.Data.ParameterDirection.Input));
                            else
                                values.Add(new ColumnExpression(alias, field.ColumnName), new SqlExpressions.DbParameter(field.ColumnName.Text, field.DbType, System.Data.ParameterDirection.Input));
                        }
                    }
                }
                return insert;
            }
            throw new NotImplementedException();
        }

        public IDbStatement Delete(Evaluant.Uss.SqlMapper.Mapping.Reference reference)
        {
            throw new NotImplementedException();
        }

        private void Create(Mapping.Entity entity, InsertStatement insert, List<string> columnNamesToInsert)
        {
            foreach (Mapping.Attribute field in entity.Attributes.Values)
            {
                if (field.Generator == SqlMapper.Mapping.Generator.Native)
                {
                    insert.Identity = new ValuedParameter("outIdentity", null, field.DbType, System.Data.ParameterDirection.Output);
                    continue;
                }
                if (columnNamesToInsert.Contains(field.ColumnName))
                    continue;
                columnNamesToInsert.Add(field.ColumnName);
                SqlExpressions.DbParameter param = new SqlExpressions.DbParameter(field.Name, field.DbType, System.Data.ParameterDirection.Input);
                insert.Values.Add(new ColumnExpression(insert.Table.Alias, field.ColumnName), param);
            }

            foreach (Mapping.Reference reference in entity.References.Values)
            {
                if (!reference.IsComposite)
                    continue;

                if (reference.Target.Table == entity.Table)
                    Create(reference.Target, insert, columnNamesToInsert);
            }

            foreach (Field field in entity.NotMappedFields)
            {
                if (columnNamesToInsert.Contains(field.ColumnName.Text))
                    continue;
                insert.Values.Add(new ColumnExpression(insert.Table.Alias, field.ColumnName), new Constant(field.DefaultValue, field.DbType));
            }
        }

        public void Create(Mapping.Entity entity, InsertStatement insert)
        {
            List<string> columnNamesToInsert = new List<string>();

            foreach (ColumnExpression column in insert.Values.Keys)
                columnNamesToInsert.Add(column.ColumnName.Text);

            Create(entity, insert, columnNamesToInsert);
        }

        public InsertStatement Create(Mapping.Entity entity)
        {
            InsertStatement insert = new InsertStatement(new TableSourceExpression(null, entity.Table), new Dictionary<ColumnExpression, NLinq.Expressions.Expression>());

            Create(entity, insert, new List<string>());

            return insert;
        }

        public UpdateStatement Update(Evaluant.Uss.SqlMapper.Mapping.Entity entity)
        {
            UpdateStatement update = new UpdateStatement() { From = new SqlExpressions.FromClause(new TableSourceExpression(null, entity.Table)) };

            foreach (var item in entity.Ids.Values)
            {
                var idConstraint = new BinaryExpression(BinaryExpressionType.Equal, new ColumnExpression(null, item.ColumnName), new DbParameter(item.Name, item.DbType, System.Data.ParameterDirection.Input));

                if (update.Where == null)
                    update.Where = new WhereClause(idConstraint);
                else
                    update.Where.Expression = new BinaryExpression(BinaryExpressionType.And, update.Where.Expression, idConstraint);
            }
            return update;
        }

        public DeleteStatement Delete(Evaluant.Uss.SqlMapper.Mapping.Entity reference)
        {
            throw new NotImplementedException();
        }
        public CaseTestExpression[] TestCases(string type, TableAlias alias)
        {
            List<CaseTestExpression> cases = new List<CaseTestExpression>();
            var tem = new DbExpressionVisitors.Mutators.ThisExpressionMutator(type, alias);
            alias = tem.This;
            Mapping.Entity e = Mapping.Entities[type];
            TestCases(e, cases);
            for (int i = 0; i < cases.Count; i++)
                cases[i] = tem.Visit(cases[i]);
            return cases.ToArray();
        }

        public CaseTestExpression[] TestCases(string type, out TableAlias alias)
        {
            List<CaseTestExpression> cases = new List<CaseTestExpression>();
            var tem = new DbExpressionVisitors.Mutators.ThisExpressionMutator(type);
            alias = tem.This;
            Mapping.Entity e = Mapping.Entities[type];
            TestCases(e, cases);
            for (int i = 0; i < cases.Count; i++)
                cases[i] = tem.Visit(cases[i]);
            return cases.ToArray();
        }
        public void TestCases(Mapping.Entity e, List<CaseTestExpression> cases)
        {
            foreach (Mapping.Entity em in e.Derived)
            {
                cases.Add(new CaseTestExpression(em.Inheritance.DiscriminatorExpression, new Constant(em.Type, System.Data.DbType.AnsiString)));
                if (em.Derived.Count > 0)
                {
                    TestCases(em, cases);
                }
            }
        }

        #endregion
    }
}
