using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Services
{
    public class ServicesCollection : IList<IService>, IService
    {
        IList<IService> services = new List<IService>();

        public IService this[Type serviceType]
        {
            get
            {
                foreach (IService service in services)
                {
                    if (service.GetType() == serviceType)
                        return service;
                }
                return null;
            }
        }

        #region IList<IService> Membres

        public int IndexOf(IService item)
        {
            return services.IndexOf(item);
        }

        public void Insert(int index, IService item)
        {
            item.ServiceAdded();
            services.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            services[index].ServiceRemoved();
            services.RemoveAt(index);
        }

        public IService this[int index]
        {
            get
            {
                return services[index];
            }
            set
            {
                services[index] = value;
            }
        }

        #endregion

        #region ICollection<IService> Membres

        public void Add(IService item)
        {
            item.ServiceAdded();
            services.Add(item);
        }

        public void Clear()
        {
            foreach (IService service in services)
            {
                service.ServiceRemoved();
            }
            services.Clear();
        }

        public bool Contains(IService item)
        {
            return services.Contains(item);
        }

        public bool Contains(Type type)
        {
            foreach (IService service in services)
            {
                if (service.GetType() == type)
                    return true;
            }
            return false;
        }

        public void CopyTo(IService[] array, int arrayIndex)
        {
            services.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return services.Count; }
        }

        public bool IsReadOnly
        {
            get { return services.IsReadOnly; }
        }

        public bool Remove(IService item)
        {
            item.ServiceRemoved();
            return services.Remove(item);
        }

        #endregion

        #region IEnumerable<IService> Membres

        public IEnumerator<IService> GetEnumerator()
        {
            return services.GetEnumerator();
        }

        #endregion

        #region IEnumerable Membres

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)services).GetEnumerator();
        }

        #endregion

        #region IService Membres

        public void ServiceAdded()
        {
            foreach (IService service in services)
                service.ServiceAdded();
        }

        public void ServiceRemoved()
        {
            foreach (IService service in services)
                service.ServiceRemoved();
        }

#if EUSS12

        public void Visit<T>(T item, ServiceContext context)
            where T:class
        {
            foreach (IService service in services)
                service.Visit<T>(item, context);
        }

        public void Visit<Parent, Child>(Parent parent, Child child, ReferenceServiceContext<Child> context)
            where Parent : class
            where Child : class
        {
            foreach (IService service in services)
                service.Visit<Parent, Child>(parent, child, context);
        }

#else

        public void Visit(object item, ServiceContext context)
        {
            foreach (IService service in services)
                service.Visit(item, context);
        }

        public void Visit(object parent, object child, ReferenceServiceContext context)
        {
            foreach (IService service in services)
                service.Visit(parent, child, context);
        }

#endif

        #endregion
    }
}
