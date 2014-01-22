using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.SqlMapper.Mapper
{
    public class ProxyMapper : IMapper
    {
        IMapper mapper;

        public ProxyMapper(IMapper mapper)
        {
            this.mapper = mapper;
        }

        #region IMapper Members

        public virtual Evaluant.NLinq.Expressions.BinaryExpression On(Mapping.Reference rm)
        {
            return mapper.On(rm);
        }

        public virtual Evaluant.Uss.SqlExpressions.IAliasedExpression Join(Mapping.Reference rm, out SqlExpressions.TableAlias firstAlias)
        {
            return mapper.Join(rm, out firstAlias);
        }

        public virtual SqlExpressions.IDbStatement Create(Mapping.Reference reference)
        {
            return mapper.Create(reference);
        }

        public virtual SqlExpressions.IDbStatement Delete(Mapping.Reference reference)
        {
            return mapper.Delete(reference);
        }

        public virtual SqlExpressions.InsertStatement Create(Mapping.Entity entity)
        {
            return mapper.Create(entity);
        }

        public virtual SqlExpressions.UpdateStatement Update(Mapping.Entity entity)
        {
            return mapper.Update(entity);
        }

        public virtual SqlExpressions.DeleteStatement Delete(Mapping.Entity entity)
        {
            return mapper.Delete(entity);
        }

        public virtual SqlExpressions.CaseTestExpression[] TestCases(string type, out TableAlias alias)
        {
            return mapper.TestCases(type, out alias);
        }

        public void Create(Mapping.Entity entity, SqlExpressions.InsertStatement insert)
        {
            mapper.Create(entity, insert);
        }

        #endregion

        #region IMapper Members


        public SqlExpressions.IDbStatement Create(Mapping.Reference reference, SqlExpressions.ValuedParameter childId)
        {
            return mapper.Create(reference, childId);
        }

        #endregion

        #region IMapper Members


        public void Map(Model.Model model, params IMapperOption[] options)
        {
            mapper.Map(model, options);
        }

        #endregion


        public CaseTestExpression[] TestCases(string type, TableAlias currentAlias)
        {
            return mapper.TestCases(type, currentAlias);
        }
    }
}
