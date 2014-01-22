using System;
using System.Collections;
using Evaluant.Uss;
using System.Collections.Generic;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.Domain;
using System.Runtime.CompilerServices;
using System.Reflection;
using Evaluant.Uss.EntityResolver;

namespace Evaluant.Uss.ObjectContext
{
    public class IdentityMap : IEntityResolver
    {
        static IdentityMap()
        {
            foreach (MethodInfo mi in typeof(IdentityMap).GetMethods())
            {
                if (mi.Name == "Resolve" && mi.IsGenericMethodDefinition)
                {
                    if (mi.GetParameters()[0].ParameterType == typeof(Entity))
                        genericEntityToObject = mi;
                    if (mi.GetParameters()[0].ParameterType.IsGenericParameter && mi.GetParameters().Length == 2)
                        genericObjectToEntity = mi;
                }
            }
        }

        static MethodInfo genericObjectToEntity;
        static MethodInfo genericEntityToObject;


        public IdentityMap(IEntityResolver resolver)
        {
            this.resolver = resolver;
        }

        IEntityResolver resolver;

        IDictionary<string, WeakReference> entityToObject = new Dictionary<string, WeakReference>();
        IDictionary<int, Entity> objectToEntity = new Dictionary<int, Entity>();

        #region IEntityResolver Members

        public T Resolve<T>(Domain.Entity entity, Model.Model model)
        {
            WeakReference entityWR;
            if (entityToObject.TryGetValue(entity.Type + entity.Id, out entityWR))
            {
                if (entityWR.IsAlive)
                    return (T)entityWR.Target;
                Clean();
            }
            T resolvedEntity = resolver.Resolve<T>(entity, model);
            entityWR = new WeakReference(resolvedEntity);
            entityToObject.Add(entity.Type + entity.Id, entityWR);
            return resolvedEntity;
        }

        public void Clean()
        {
            List<string> deadKeys = new List<string>();
            foreach (KeyValuePair<string, WeakReference> wr in entityToObject)
            {
                if (!wr.Value.IsAlive)
                    deadKeys.Add(wr.Key);
            }
            foreach (string deadKey in deadKeys)
                entityToObject.Remove(deadKey);
        }

        public IList<T> Resolve<T>(IList<Domain.Entity> entities, Model.Model model)
        {
            IList<T> list = new List<T>();
            foreach (Domain.Entity entity in entities)
            {
                list.Add(Resolve<T>(entity, model));
            }
            return list;
        }

        public object Resolve(Entity entity, Model.Model model)
        {
            return (Domain.Entity)genericEntityToObject.MakeGenericMethod(entity.GetType()).Invoke(this, new object[] { entity, model });
        }

        public Evaluant.Uss.Domain.Entity Resolve(object entity, Model.Model model)
        {
            Entity e;
            int hashCode = RuntimeHelpers.GetHashCode(entity);
            if (objectToEntity.ContainsKey(hashCode))
            {
                e = objectToEntity[hashCode];
                if (entity is IPersistable)
                    return e;
                resolver.Resolve(entity, model, e);
                return e;
            }
            e = resolver.Resolve(entity, model);
            objectToEntity.Add(RuntimeHelpers.GetHashCode(entity), e);
            return e;
        }

        #endregion

        #region IEntityResolver Members

        public void Initialize(IPersistenceEngineObjectContext engine)
        {
            resolver.Initialize(engine);
        }

        #endregion

        internal bool Contains(int hashCode)
        {
            return objectToEntity.ContainsKey(hashCode);
        }

        internal void Clean(object persistable)
        {
            int hashCode = RuntimeHelpers.GetHashCode(persistable);
            objectToEntity.Remove(hashCode);
        }

        internal void Attach<T>(T item, Evaluant.Uss.Model.Model model)
        {
            Resolve(item, model);
        }

        internal void Import(IPersistable item, Model.Model model)
        {
            if (Contains(item.GetHashCode()))
                return;

            item.Entity.State = State.New;

            objectToEntity.Add(RuntimeHelpers.GetHashCode(item), item.Entity);

            foreach (Entry entry in item.Entity)
            {
                entry.State = State.New;
                if (entry.IsEntity)
                {
                    Entity related = (Entity)entry.Value;
                    IPersistable relatedItem = Resolve<IPersistable>(related, model);
                    if (related != relatedItem.Entity)
                        throw new EntityResolverException("Items should be the same.");

                    Import(relatedItem, model);
                }
            }
        }

        #region IEntityResolver Members


        public void Resolve<T>(T entity, Model.Model model, Entity entityToUpdate)
        {
            resolver.Resolve<T>(entity, model, entityToUpdate);
        }

        #endregion

        public IEnumerable GetAll()
        {
            foreach (var item in entityToObject.Values)
            {
                if (item.IsAlive)
                    yield return item.Target;
            }
        }
    }
}