using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Evaluant.Uss.MetaData;
using Evaluant.Uss.ObjectContext.Contracts;
using System;
using Evaluant.Uss.Domain;
using Evaluant.Uss.PersistentDescriptors;
using System.Text;

namespace Evaluant.Uss.EntityResolver.Proxy
{
    public class AnonymousTypeResolver : IEntityResolver
    {
        Dictionary<Type, IAnonymousBuilder> builders = new Dictionary<Type, IAnonymousBuilder>();

        private readonly CacheEntityResolver resolver;
        public AnonymousTypeResolver(CacheEntityResolver resolver)
        {
            this.resolver = resolver;
        }

        public void Initialize(IPersistenceEngineObjectContext engine)
        {
            resolver.Initialize(engine);
        }

        public void Resolve<T>(T entity, Model.Model model, Entity entityToUpdate)
        {
            resolver.Resolve(entity, model, entityToUpdate);
        }

        public Entity Resolve(object entity, Model.Model model)
        {
            return resolver.Resolve(entity, model);
        }

        public IList<T> Resolve<T>(IList<Evaluant.Uss.Domain.Entity> entities, Model.Model model)
        {
            IList<T> items = new List<T>();
            foreach (var entity in entities)
                items.Add(Resolve<T>(entity, model));
            return items;
        }

        public T Resolve<T>(Entity entity, Model.Model model)
        {
            if (TypeResolver.IsAnonymous(typeof(T)))
            {
                IAnonymousBuilder builder;
                if (!builders.TryGetValue(typeof(T), out builder))
                {
                    builders.Add(typeof(T), builder = new AnonymousBuilder(typeof(T)));
                }
                return builder.GetTarget<T>(entity);
            }
            return resolver.Resolve<T>(entity, model);
        }
    }
}
