using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.SqlMapper.Mapper
{
    public interface IMapper
    {
        BinaryExpression On(Mapping.Reference rm);
        IAliasedExpression Join(Mapping.Reference rm, out TableAlias firstAlias);

        void Map(Model.Model model, params IMapperOption[] options);

        IDbStatement Create(Mapping.Reference reference);
        IDbStatement Delete(Mapping.Reference reference);
        InsertStatement Create(Mapping.Entity reference);
        void Create(Mapping.Entity entity, InsertStatement insert);
        UpdateStatement Update(Mapping.Entity reference);
        DeleteStatement Delete(Mapping.Entity reference);

        CaseTestExpression[] TestCases(string type, out TableAlias alias);

        //List<AliasedExpression> GetColumns(string type);

        IDbStatement Create(Mapping.Reference reference, ValuedParameter childId);

        CaseTestExpression[] TestCases(string p, TableAlias currentAlias);
    }
}
