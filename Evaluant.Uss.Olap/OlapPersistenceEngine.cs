using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.NLinq.Expressions;

namespace Evaluant.Uss.Olap
{
    class OlapPersistenceEngine : PersistenceEngineImplementation
    {
        public OlapPersistenceEngine(OlapPersistenceProvider factory)
            : base(factory)
        {
        }

        public override IList<Domain.Entity> LoadWithId(string type, string[] id)
        {
            throw new NotSupportedException();
        }

        public override void LoadReference(string reference, IEnumerable<Domain.Entity> entities, NLinq.NLinqQuery query)
        {
            throw new NotSupportedException();
        }

        public override void ProcessCommands(Transaction tx)
        {
            throw new NotSupportedException();
        }

        protected override T LoadSingle<T>(NLinq.NLinqQuery query)
        {
            throw new NotSupportedException();
        }

        protected override T LoadMany<T, U>(NLinq.NLinqQuery query)
        {
            throw new NotImplementedException();
        }

        public override void CreateId(Domain.Entity e)
        {
            throw new NotImplementedException();
        }
    }
}
