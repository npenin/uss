using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.PersistentDescriptors;
using Evaluant.Uss.Domain;
using System.Collections;
using System.Runtime.CompilerServices;
using Evaluant.Uss.MetaData;
using Evaluant.Uss.Utility;

namespace Evaluant.Uss.EntityResolver.Proxy
{
    public abstract class CacheEntityResolver : IEntityResolver
    {
        protected IPersistenceEngineObjectContext oc;
        protected IPersistenceEngineObjectContextAsync asyncOc;

        private static object _SynLock = new object();

        protected CacheEntityResolver()
        {
            Types = new Dictionary<Type, Type>();
        }

        public CacheEntityResolver(IPersistenceEngineObjectContextAsync oc)
            : this()
        {
            Initialize(oc);
        }

        public CacheEntityResolver(IPersistenceEngineObjectContext oc)
            : this()
        {
            Initialize(oc);
        }

        public void Initialize(IPersistenceEngineObjectContext oc)
        {
            this.oc = oc;
            this.asyncOc = oc;
        }

        public void Initialize(IPersistenceEngineObjectContextAsync oc)
        {
            this.asyncOc = oc;
        }

        #region CreatePersistableProxy

        public IPersistableProxy CreatePersistableProxy(Type parentType)
        {
            lock (_SynLock)
            {
                Type type = GetType(parentType, oc.Factory.PersistenceEngineFactory.Model);
                IPersistableProxy proxy = (IPersistableProxy)oc.Factory.Activator.CreateActivator(type).CreateInstance();

                return proxy;
            }
        }

        #endregion

        public Dictionary<Type, Type> Types { get; set; }

        #region IEntityResolver Members

        public T Resolve<T>(Evaluant.Uss.Domain.Entity entity, Evaluant.Uss.Model.Model model)
        {
            Type entityType;
            if (oc != null)
                entityType = oc.Factory.GetDomainType(entity.Type);
            else
                entityType = asyncOc.Factory.GetDomainType(entity.Type);
            if (entityType == null)
#if !SILVERLIGHT
                throw new UniversalStorageException(string.Format("The type {0} could not be found", entity.Type));
#else
                throw new UniversalStorageException(string.Format("The type {0} could not be found", entity.Type));
#endif
            Type resolvedEntityType;
            if (!Types.TryGetValue(entityType, out resolvedEntityType))
                resolvedEntityType = GetType(entityType, model);

            IPersistableProxy instance = (IPersistableProxy)Activator.CreateInstance(resolvedEntityType);
            instance.ObjectContext = oc;
            instance.ObjectContextAsync = asyncOc;
            instance.Entity = entity;
            instance.Set();
            instance.SetReferences();
            return (T)instance;
        }

        public abstract Type GetType(Type type, Model.Model model);

        public IList<T> Resolve<T>(IList<Evaluant.Uss.Domain.Entity> entities, Model.Model model)
        {
            IList<T> items = new List<T>();
            foreach (var entity in entities)
                items.Add(Resolve<T>(entity, model));
            return items;
        }

        Dictionary<object, Entity> localProcessing;

        public Evaluant.Uss.Domain.Entity Resolve(object entity, Evaluant.Uss.Model.Model model)
        {
            bool setLocalProcessing = localProcessing == null;
            if (setLocalProcessing)
                localProcessing = new Dictionary<object, Entity>();

            Entity e = Get(entity, model, localProcessing);
            if (setLocalProcessing)
                localProcessing = null;
            return e;
        }

        public Entity Get(object target, Model.Model model, Dictionary<object, Entity> locallyProcessed)
        {
            if (locallyProcessed.ContainsKey(target))
                return locallyProcessed[target];

            ReflectionDescriptor descriptor = new ReflectionDescriptor();
            Type targetType = target.GetType();

            Entity e = null;

            if (target is IPersistableProxy)
            {
                IPersistableProxy proxy = target as IPersistableProxy;

                // Makes an update on the current object so that the entity reflects its private values
                // Also calls Update() recursively on all IPersistable only related objects
                proxy.Update();

                e = proxy.Entity;

                // A cyclic reference can already process the current item
                if (!locallyProcessed.ContainsKey(target))
                    locallyProcessed.Add(target, e);

                // Explicitly updates for none IPersistable references
                foreach (PropertyDescriptor prop in descriptor.GetPersistentProperties(targetType))
                {
                    if (!prop.IsEntity)
                    {
                        continue;
                    }

                    // Ignores this member (attribute or reference) if not in metadata
                    if (model.GetAttribute(e.Type, prop.PropertyName) == null
                        && model.GetReference(e.Type, prop.PropertyName) == null)
                    {
                        continue;
                    }

                    object fieldValue = prop.GetValue(targetType, target);

                    if (fieldValue == null)
                    {
                        continue;
                    }

                    if (prop.IsList || prop.IsGenericList)
                    {
                        foreach (object o in (IEnumerable)fieldValue)
                        {
                            if (o is IPersistable)
                            {
                                continue;
                            }

                            oc.Resolver.Resolve(o, model);
                        }
                    }
                    else
                    {
                        if (fieldValue is IPersistable)
                        {
                            continue;
                        }

                        oc.Resolver.Resolve(fieldValue, model);
                    }
                }

                return e;
            }
            else if (target is IPersistable)
            {
                e = ((IPersistable)target).Entity;

                //globallyProcessed.Add(RuntimeHelpers.GetHashCode(target), e);
                locallyProcessed.Add(target, e);

                return e;
            }

            // the new object has already been processed ?
            //if (globallyProcessed.ContainsKey(RuntimeHelpers.GetHashCode(target)))
            //{
            //    e = globallyProcessed[RuntimeHelpers.GetHashCode(target)];
            //    if (e.State == State.UpToDate)
            //        return e;
            //}
            //else
            //{
            e = new Entity(TypeResolver.ConvertNamespaceDomainToEuss(targetType));
            //e.Model = model.Entities[e.Type];
            //    globallyProcessed.Add(RuntimeHelpers.GetHashCode(target), e);
            //}

            locallyProcessed.Add(target, e);

            foreach (PropertyDescriptor prop in descriptor.GetPersistentProperties(targetType))
            {
                // Ignores this member (attribute or reference) if not in metadata
                if (model.GetAttribute(e.Type, prop.PropertyName) == null
                    && model.GetReference(e.Type, prop.PropertyName) == null)
                    continue;

                object fieldValue = prop.GetValue(targetType, target);

                if (fieldValue == null)
                    continue;

                if (!prop.IsEntity)
                {
                    Model.Attribute attribute = model.GetAttribute(TypeResolver.ConvertNamespaceDomainToEuss(targetType.FullName), prop.PropertyName);
                    if (prop.Type.IsEnum)
                        if (attribute.Type == typeof(string))
                            e.SetValue(attribute.Name, Enum.GetName(prop.Type, fieldValue));
                        else
                            e.SetValue(attribute.Name, (int)fieldValue);
                    else
                        e.SetValue(prop.PropertyName, fieldValue);
                }
                else
                {
                    if (e.State == State.New)
                    {
                        if (prop.IsList || prop.IsGenericList)
                        {
                            MultipleEntry entry = new MultipleEntry(prop.PropertyName);
                            e.Add(prop.PropertyName, entry);
                            foreach (object o in (IEnumerable)fieldValue)
                            {
                                Entity r;
                                if (oc != null)
                                    r = oc.Resolver.Resolve(o, model);
                                else
                                    r = asyncOc.Resolver.Resolve(o, model);
                                entry.Add(r);
                            }
                        }
                        else
                        {
                            if (oc != null)
                                e.SetValue(prop.PropertyName, oc.Resolver.Resolve(fieldValue, model));
                            else
                                e.SetValue(prop.PropertyName, asyncOc.Resolver.Resolve(fieldValue, model));
                        }
                    }
                    else
                    {
                        if (prop.IsList || prop.IsGenericList)
                        {
                            EntitySet newRels = new EntitySet();

                            foreach (object o in (IEnumerable)fieldValue)
                            {
                                Entity r = oc.Resolver.Resolve(o, model);
                                newRels.Add(r);
                            }

                            // Removes removed entities
                            foreach (Entity r in (IEnumerable<Entity>)e[prop.PropertyName])
                            {
                                if (!newRels.Contains(r))
                                    e.RemoveReference(prop.PropertyName, r.Id);
                            }

                            ICollection<Entity> oldRels = (MultipleEntry)e[prop.PropertyName];

                            // Add new references
                            foreach (Entity r in newRels)
                            {
                                if (!oldRels.Contains(r))
                                    e.Add(prop.PropertyName, r, true);
                            }
                        }
                        else
                        {
                            Entity r = oc.Resolver.Resolve(fieldValue, model);

                            if (r.State == State.New)
                                e.Add(prop.PropertyName, r, false);
                            else
                            {
                                Entity old = e.GetEntity(prop.PropertyName);

                                if (old == null || r.Id != old.Id)
                                {
                                    if (old != null)
                                        e.RemoveReference(prop.PropertyName, old.Id);
                                    e.Add(prop.PropertyName, r);
                                }
                            }

                        }

                    }
                }
            }

            // Updates the Entity's id to be serialized with the one in the object (for assigned values)
            //PropertyDescriptor idPropertyDesc = descriptor.GetIdDescriptor(targetType);
            //if (idPropertyDesc != null && e.State == State.New)
            //{
            //    object idFieldValue = idPropertyDesc.GetValue(targetType, target);

            //    if (idPropertyDesc.Type == typeof(string))
            //    {
            //        if (idFieldValue != null)
            //        {
            //            e.Id = idFieldValue.ToString();
            //        }
            //        else
            //        {
            //            idPropertyDesc.SetValue(targetType, target, e.Id);
            //        }
            //    }
            //    else if (idPropertyDesc.Type == typeof(Int32) && (int)idFieldValue != 0)
            //    {
            //        e.Id = idFieldValue.ToString();
            //    }
            //}
            if (e.State == State.New)
            {
                if (oc != null)
                    oc.PersistenceEngine.CreateId(e);
                else
                    asyncOc.PersistenceEngineAsync.CreateId(e);
            }

            return e;
        }

        #endregion

        #region IEntityResolver Members


        public abstract void Resolve<T>(T entity, Model.Model model, Entity entityToUpdate);

        #endregion
    }
}
