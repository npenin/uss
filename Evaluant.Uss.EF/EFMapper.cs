using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Evaluant.Uss.SqlMapper.Mapper;

namespace Evaluant.Uss.SqlMapper.EF
{
    class EFMapper : IMapper
    {
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
            throw new NotImplementedException();
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

        public SqlExpressions.IDbStatement Create(Mapping.Reference reference, SqlExpressions.ValuedParameter childId)
        {
            throw new NotImplementedException();
        }


        public SqlExpressions.CaseTestExpression[] TestCases(string type, out SqlExpressions.TableAlias alias)
        {
            throw new NotImplementedException();
        }

        public SqlExpressions.CaseTestExpression[] TestCases(string p, SqlExpressions.TableAlias currentAlias)
        {
            throw new NotImplementedException();
        }
    }
}
