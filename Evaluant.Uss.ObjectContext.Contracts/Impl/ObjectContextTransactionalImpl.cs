using System;
using System.Collections.Generic;
using Evaluant.Uss.Commands;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Runtime.CompilerServices;
using Evaluant.Uss.Domain;
using System.Collections;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    public abstract class ObjectContextTransactionalImpl : ObjectContextImpl, IObjectContextTransactionalBase
    {
        protected Transaction _CurrentTransaction;

        /// <summary>
        /// Ensures the transaction is created.
        /// </summary>
        protected void EnsureTransactionCreated()
        {
            if (_CurrentTransaction == null)
                throw new UniversalStorageException("BeginTransaction() must be called before");
        }

        /// <summary>
        /// Ensures the transaction is disposed.
        /// </summary>
        protected void EnsureTransactionDisposed()
        {
            if (_CurrentTransaction != null)
                throw new UniversalStorageException("CommitTransaction() or RollbackTransaction() must be called before");
        }

        /// <summary>
        /// Serializes the specified persistable object.
        /// </summary>
        /// <param name="persistable">Persistable object.</param>
        public void Serialize(IPersistable persistable)
        {
            if (persistable == null)
                throw new ArgumentNullException("persistable");

            if (persistable.Entity == null)
                throw new NullReferenceException("Entity cannot be null");

            EnsureTransactionCreated();

            _CurrentTransaction.Serialize(persistable.Entity);

            // Adds the Entity to the cache after Serialize for it can throw an exception or change the Id's value
            //TrackObject(persistable);
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="item">Object to serialize.</param>
        public override void Serialize(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            EnsureTransactionCreated();

            // Stores the object in a temporary list to extract its entity during CommitTransaction
            if (!_PendingObjects.Contains(item))
            {
                if (resolver.Contains(item.GetHashCode()))
                    resolver.Resolve(item, Factory.PersistenceEngineFactory.Model).State = State.Modified;
                _PendingObjects.Add(item);
            }
        }

        /// <summary>
        /// Deletes the specified object.
        /// </summary>
        /// <param name="item">Object to delete.</param>
        public override void Delete(object o)
        {


            if (o == null)
                throw new ArgumentNullException("o");

            IPersistable persistable = o as IPersistable;

            if (persistable == null && !resolver.Contains(RuntimeHelpers.GetHashCode(o)))
                throw new ArgumentException("This object is not persisted.");

            Entity e = persistable == null ? (Entity)resolver.Resolve(o, Factory.PersistenceEngineFactory.Model) : persistable.Entity;

            EnsureTransactionCreated();

            resolver.Clean(o);
            _CurrentTransaction.Delete(e);
        }


        /// <summary>
        /// Deletes the specified object with the specified Id.
        /// </summary>
        /// <param name="id">Object Id to delete.</param>
        public override void DeleteWithId<T>(string id)
        {
            DeleteWithId(typeof(T), id);
        }

        /// <summary>
        /// Deletes the specified object with the specified Id.
        /// </summary>
        /// <param name="type">Object type to delete.</param>
        /// <param name="id">Object Id to delete.</param>
        public override void DeleteWithId(Type type, string id)
        {
            EnsureTransactionCreated();

            string eussNamespace = Evaluant.Uss.MetaData.TypeResolver.ConvertNamespaceDomainToEuss(type);

            //UnTrackObject(eussNamespace, id);
            DeleteEntityCommand dec = new DeleteEntityCommand(id, eussNamespace);
            _CurrentTransaction.PushCommand(dec);
        }


        /// <summary>
        /// Serializes the specified persistable objects.
        /// </summary>
        /// <param name="persistables">Persistable objects.</param>
        public override void Serialize(IEnumerable persistables)
        {
            if (persistables == null)
                throw new ArgumentNullException("persistables");

            foreach (object persistable in persistables)
                Serialize(persistable);
        }

        /// <summary>
        /// Serializes the specified persistable objects.
        /// </summary>
        /// <param name="persistables">Persistable objects.</param>
        public override void Serialize<T>(IEnumerable<T> persistables)
        {
            if (persistables == null)
                throw new ArgumentNullException("persistables");

            foreach (T persistable in persistables)
                Serialize(persistable);
        }

        /// <summary>
        /// Deletes the specified persistable objects.
        /// </summary>
        /// <param name="persistables">Persistable objects.</param>
        public override void Delete<T>(IEnumerable<T> persistables)
        {
            if (persistables == null)
                throw new ArgumentNullException("persistables");

            foreach (T persistable in persistables)
                Delete(persistable);
        }

        /// <summary>
        /// Attaches a detached object.
        /// </summary>
        public override void SerializeDetached(object item)
        {
            SerializeDetached(item, null);
        }

        /// <summary>
        /// Attaches a detached object.
        /// </summary>
        public override void SerializeDetached(object item, string id)
        {
            //if (id == null)
            //{
            //    Descriptors.PropertyDescriptor pd = Factory.PersistentDescriptor.GetIdDescriptor(item.GetType());

            //    if (pd == null)
            //    {
            //        throw new ArgumentException("The object to attach must have an identifier. Please use SerializeDetached(object, string) instead.");
            //    }

            //    id = pd.GetValue(item.GetType(), item).ToString();
            //}

            Dictionary<object, Entity> local = new Dictionary<object, Entity>();
            Entity e = resolver.Resolve(item, Factory.PersistenceEngineFactory.Model);

            //e.Id = id;
            e.State = State.Modified;

            for (int i = e.EntityEntries.Count - 1; i >= 0; i--)
            {
                Entry ee = (Entry)e.EntityEntries[i];

                if (ee.IsEntity)
                {
                    e.EntityEntries.Remove(ee);
                }
                else
                {
                    ee.State = State.Modified;
                }
            }

            _CurrentTransaction.Serialize(e);
        }
    }
}
