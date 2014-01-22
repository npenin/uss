using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.EntityResolver.DynamicObject
{
    public class DynamicProxy<T> : System.Dynamic.DynamicObject, INotifyPropertyChanged
         where T : new()
    {
        static IDictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
        private Entity entity;
        private T model;

        static DynamicProxy()
        {
            foreach (var prop in typeof(T).GetProperties().Select(pi => new KeyValuePair<string, PropertyInfo>(pi.Name, pi)))
                properties.Add(prop);
        }


        public DynamicProxy(Entity entity)
            : this(entity, new T())
        {
        }

        public DynamicProxy(Entity entity, T model)
        {
            this.entity = entity;
            this.model = model;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return properties.Keys;
        }

        public override bool TryConvert(System.Dynamic.ConvertBinder binder, out object result)
        {
            if (binder.Type == typeof(INotifyPropertyChanged))
            {
                result = this;
                return true;
            }
            if (typeof(T).IsAssignableFrom(binder.Type))
            {
                result = model;
                return true;
            }
            if (typeof(Entity).IsAssignableFrom(binder.Type))
            {
                result = entity;
                return true;
            }
            return base.TryConvert(binder, out result);
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            Entry e;
            if (entity.TryGet(binder.Name, out e))
            {
                result = e.Value;
                return true;
            }
            result = null;
            return false;
        }

        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
        {
            Entry e;
            if (entity.TryGet(binder.Name, out e))
            {
                e.Value = value;
                properties[binder.Name].SetValue(model, value, null);
                OnPropertyChanged(binder.Name);
                return true;
            }
            return false;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
