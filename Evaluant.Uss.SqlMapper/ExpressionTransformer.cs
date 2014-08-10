using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.CommonVisitors;
using Evaluant.NLinq.Expressions;
using Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators;
using Evaluant.Uss.SqlMapper.DbExpressionVisitors;
using Evaluant.Uss.SqlMapper.DbExpressionVisitors.Mutators.MappingMutators;
using Evaluant.Uss.Domain;
using Evaluant.Uss.SqlExpressions.Visitors;
using Evaluant.Uss.PersistenceEngine.Contracts.Instrumentation;
using Evaluant.Uss.SqlMapper.DbExpressionVisitors.Optimizers;
using Evaluant.Uss.SqlMapper.DbExpressionVisitors.Sanitizers;

namespace Evaluant.Uss.SqlMapper
{
    public class ExpressionTransformer : DbExpressionVisitor
    {
        static ExpressionTransformer()
        {
            TraceHelper.AddSource(traceSource);
        }

        const string traceSource = "Evaluant.Uss.Mapper.Visitors";

        protected IList<IVisitor<Expression>> visitors = new List<IVisitor<Expression>>();

        public ExpressionTransformer(SqlMapperEngine engine)
        {
            //visitors.Add(new MethodCallMutator());
            visitors.Add(new ValueExpressionMutator(engine.Driver));
            visitors.Add(new StringOperations());
            visitors.Add(new IdentifierToEntityMutator());
            visitors.Add(new ToManyIsNotNull(engine));
            visitors.Add(new EntityToSelectMutator(engine));
            if (!engine.Driver.IsOrm)
            {
                visitors.Add(new ArrayToUnionAll(engine.Driver));
                //visitors.Add(new AggregateMutator());
                visitors.Add(new EntityToTableMutator(engine.Provider.Mapping, engine.Driver));
                ReferenceToColumnMutator rtcm = new ReferenceToColumnMutator(engine.Provider.Mapping);
                visitors.Add(rtcm);
                visitors.Add(new DateOperations(engine.Provider.Mapping));
                visitors.Add(new AttributeToColumnMutator(engine.Provider.Mapping));
                visitors.Add(new LazyAliasResolver(rtcm.AliasesMapping));
                visitors.Add(new InheritanceMappingMutator(engine.Provider.Mapping));

                visitors.Add(new RowNumberSanitizer());

                visitors.Add(new RemoveUselessRootSelectIfPossible());
                visitors.Add(new RemoveUselessColumnsIfNotNeeded());
                visitors.Add(new RemoveOrderByWhenCounting());
                visitors.Add(new EnsureStartsWithSelect());
            }
        }

        public override Expression Visit(Expression exp)
        {
            foreach (IVisitor<Expression> visitor in visitors)
            {
                TraceHelper.TraceEvent(traceSource, System.Diagnostics.TraceEventType.Start, 1, "{0} process begin...", visitor.GetType().Name);
                exp = visitor.Visit(exp);
                TraceHelper.TraceEvent(traceSource, System.Diagnostics.TraceEventType.Start, 1, "{0} process succeed...", visitor.GetType().Name);
            }
            return exp;
        }
    }
}
