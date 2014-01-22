using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.Domain;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Reflection;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    public interface IEntityResolver
    {
        void Initialize(IPersistenceEngineObjectContext engine);
        T Resolve<T>(Entity entity, Model.Model model);
        IList<T> Resolve<T>(IList<Entity> entities, Model.Model model);
        Entity Resolve(object entity, Model.Model model);
        void Resolve<T>(T entity, Model.Model model, Entity entityToUpdate);
    }
}
