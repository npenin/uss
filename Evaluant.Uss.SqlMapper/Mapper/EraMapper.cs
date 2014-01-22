using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.Model;
using Evaluant.Uss.SqlMapper.Mapping;

namespace Evaluant.Uss.SqlMapper.Mapper
{
    class EraMapper : IMapper
    {
        private IDriver driver;

        public EraMapper(IDriver driver)
        {
            this.driver = driver;
        }

        public EraMapper(IDriver driver, Mapping.Mapping mapping)
            : this(driver)
        {
            this.driver = driver;
            this.Mapping = mapping;
        }

        public NLinq.Expressions.BinaryExpression On(Mapping.Reference rm)
        {
            throw new NotImplementedException();
        }

        public SqlExpressions.IAliasedExpression Join(Mapping.Reference rm, out SqlExpressions.TableAlias firstAlias)
        {
            throw new NotImplementedException();
        }

        public void Map(Model.Model model, params IMapperOption[] options)
        {
            Mapping.Mapping mapping = new Mapping.Mapping();
            mapping.Model = model;
            foreach (Model.Entity e in model)
            {
                Mapping.Entity entityMapping = new Mapping.Entity();
                entityMapping.TableName = "Entity";
                entityMapping.Add(new Mapping.Attribute("Id") { IsPrimaryKey = true, Generator = SqlMapper.Mapping.Generator.Native, DbType = System.Data.DbType.UInt32 });
                entityMapping.Constraint = "Type=='" + e.Type + "'";
                foreach (Model.Attribute attribute in e.Attributes.Values)
                {
                    entityMapping.Add(new Mapping.Attribute(attribute.Name)
                        {
                            TableName = "Attribute",
                            ColumnName = "Name",
                            DbType = driver.GetDbType(attribute.Type)
                        });
                }

                foreach (Model.Reference reference in e.References.Values)
                {
                    Mapping.Reference referenceMapping = new Mapping.Reference();
                    referenceMapping.Rules.Add(new Rule()
                    {
                        ParentTableName = "Entity",
                        ParentFieldNames = "Id",
                        ChildTableName = "Relation",
                        ChildFieldNames = "FK_ParentID,Name='" + reference.Name + "'",
                    });
                    referenceMapping.Rules.Add(new Rule()
                    {
                        ParentTableName = "Relation",
                        ParentFieldNames = "FK_ChildID,Name='" + reference.Name + "'",
                        ChildTableName = "Entity",
                        ChildFieldNames = "Id",
                    });
                }
            }
        }

        public SqlExpressions.IDbStatement Create(Mapping.Reference reference)
        {
            throw new NotImplementedException();
        }

        public SqlExpressions.IDbStatement Delete(Mapping.Reference reference)
        {
            throw new NotImplementedException();
        }

        public SqlExpressions.InsertStatement Create(Mapping.Entity reference)
        {
            throw new NotImplementedException();
        }

        public void Create(Mapping.Entity entity, SqlExpressions.InsertStatement insert)
        {
            throw new NotImplementedException();
        }

        public SqlExpressions.UpdateStatement Update(Mapping.Entity reference)
        {
            throw new NotImplementedException();
        }

        public SqlExpressions.DeleteStatement Delete(Mapping.Entity reference)
        {
            throw new NotImplementedException();
        }

        public SqlExpressions.CaseTestExpression[] TestCases(string type, out SqlExpressions.TableAlias alias)
        {
            throw new NotImplementedException();
        }

        public SqlExpressions.IDbStatement Create(Mapping.Reference reference, SqlExpressions.ValuedParameter childId)
        {
            throw new NotImplementedException();
        }

        public SqlExpressions.CaseTestExpression[] TestCases(string p, SqlExpressions.TableAlias currentAlias)
        {
            throw new NotImplementedException();
        }

        public Mapping.Mapping Mapping { get; set; }
    }
}
