using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.ObjectContext.Contracts;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;

namespace Evaluant.Uss.EntityFramework
{
    public class ObjectContext : IObjectContextWithTracking, ILinqableContext
    {
        private readonly DbContext oc;

        public ObjectContext(ObjectService factory)
        {
            Factory = factory;
            this.oc = new DbContext("");
        }

        public ObjectService Factory
        {
            get;
            set;
        }

        #region IDisposable Members

        public void Dispose()
        {
            oc.Dispose();
        }

        #endregion

        #region IObjectContextBase Members


        public void Attach<T>(T item)
            where T : class
        {
            oc.Set<T>().Attach(item);
        }

        public T Detach<T>(T graph)
            where T : class
        {
            var formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, graph);
            ms.Position = 0;
            return (T)formatter.Deserialize(ms);

        }

        public bool IsNull(object item, string property)
        {
            throw new NotSupportedException();
        }

        public void SetNull(object item, string property)
        {
            throw new NotSupportedException();
        }

        public string GetId(object entity)
        {
            return string.Join(",", ((IEntityWithKey)entity).EntityKey.EntityKeyValues.Select(key => key.Value));
        }

        public void SetId(object entity, string id)
        {
            string[] ids = id.Split(',');
            if (ids.Length != ((IEntityWithKey)entity).EntityKey.EntityKeyValues.Length)
                throw new ArgumentOutOfRangeException();
            for (int i = 0; i < ((IEntityWithKey)entity).EntityKey.EntityKeyValues.Length; i++)
                ((IEntityWithKey)entity).EntityKey.EntityKeyValues[0].Value = ids[i];
        }

        public void AddService(Services.IService service, params Services.Operation[] operations)
        {
            throw new NotSupportedException();
        }

        public void RemoveService(Services.IService service, params Services.Operation[] operations)
        {
            throw new NotSupportedException();
        }

        public Services.ServicesCollection GetServices(Services.Operation operation)
        {
            throw new NotSupportedException();
        }

        public T GetService<T>()
        {
            throw new NotSupportedException();
        }

        public bool IsLazyLoadingEnabled
        {
            get { return oc.Configuration.LazyLoadingEnabled; }
            set { oc.Configuration.LazyLoadingEnabled = value; }
        }

        #endregion

        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IObjectContextSyncBase Members

        public void InitializeRepository()
        {
            throw new NotSupportedException();
        }

        public List<T> Load<T>(NLinq.NLinqQuery query)
        {
            throw new NotImplementedException();
        }

        public List<T> Load<T>(NLinq.Expressions.Expression query)
        {
            throw new NotImplementedException();
        }

        public T LoadSingle<T>()
        {
            throw new NotImplementedException();
        }

        public T LoadSingle<T>(Type type)
        {
            throw new NotImplementedException();
        }

        public T LoadSingle<T>(string constraint)
        {
            throw new NotImplementedException();
        }

        public T LoadSingle<T>(NLinq.Expressions.Expression constraint)
        {
            throw new NotImplementedException();
        }

        public T LoadSingle<T>(NLinq.NLinqQuery constraint)
        {
            throw new NotImplementedException();
        }

        public T LoadSingle<T>(Type type, string constraint)
        {
            throw new NotImplementedException();
        }

        public double LoadScalar(string query)
        {
            throw new NotImplementedException();
        }

        public T LoadScalar<T>(string query)
        {
            throw new NotImplementedException();
        }

        public T LoadScalar<T, U>(string query) where T : class, IEnumerable<U>
        {
            throw new NotImplementedException();
        }

        public double LoadScalar(NLinq.NLinqQuery query)
        {
            throw new NotImplementedException();
        }

        public T LoadScalar<T>(NLinq.NLinqQuery query)
        {
            throw new NotImplementedException();
        }

        public T LoadScalar<T, U>(NLinq.NLinqQuery query) where T : class, IEnumerable<U>
        {
            throw new NotImplementedException();
        }

        public double LoadScalar(NLinq.Expressions.Expression query)
        {
            throw new NotImplementedException();
        }

        public T LoadScalar<T>(NLinq.Expressions.Expression query)
        {
            throw new NotImplementedException();
        }

        public T LoadScalar<T, U>(NLinq.Expressions.Expression query) where T : class, IEnumerable<U>
        {
            throw new NotImplementedException();
        }

        public List<T> Load<T>()
        {
            throw new NotImplementedException();
        }

        public List<T> Load<T>(string constraint)
        {
            throw new NotImplementedException();
        }

        public List<T> Load<T>(Type type)
        {
            throw new NotImplementedException();
        }

        public List<T> Load<T>(Type type, string constraint)
        {
            throw new NotImplementedException();
        }

        public T LoadWithId<T>(string id)
        {
            throw new NotImplementedException();
        }

        public List<T> LoadWithId<T>(string[] ids)
        {
            throw new NotImplementedException();
        }

        public void Import<T>(T entity) where T : class,  IPersistable
        {
            oc.Set<T>().Add(entity);
        }

        public void Import<T>(IEnumerable<T> entities) where T : class, IPersistable
        {
            foreach (T entity in entities)
            {
                Import(entity);
            }
        }

        public void CommitChanges()
        {
            oc.SaveChanges();
        }

        #endregion

        #region ILinqableContext Members

        public IQueryable<T> Cast<T>() where T : class
        {
            return oc.Set<T>();
        }

        #endregion

        #region IObjectContextWithTracking Members

        public void Delete(object o)
        {
            oc.Set(o.GetType()).Remove(o);
        }

        public void Delete<T>(IEnumerable<T> persistables)
        {
            foreach (T item in persistables)
            {
                Delete(item);
            }
        }

        public void RemoveFromCache(object persistable)
        {
            oc.Set(persistable.GetType()).Local.Remove(persistable);
        }

        #endregion
    }
}
