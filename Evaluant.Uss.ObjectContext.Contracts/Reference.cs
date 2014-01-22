using System;
using System.Collections.Generic;
using Evaluant.Uss.Domain;
using System.ComponentModel;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    public class Reference<T> : INotifyPropertyChanged
    {
        private bool isLoaded;
        private IPersistable parent;
        private T value;
        private IPersistableCollection<T> values;

        public Reference(IPersistable parent, Model.Reference reference)
        {
            this.parent = parent;
            this.Model = reference;
        }

        public Reference(IPersistable parent, string referenceName)
            : this(parent, parent.ObjectContextAsync.PersistenceEngineAsync.FactoryAsync.Model.GetReference(parent.Entity.Type, referenceName))
        {
        }


        public bool IsLoaded { get { return isLoaded; } }

        public Model.Reference Model { get; private set; }

        public void Load()
        {
            EnsureLoaded();
        }

        private void EnsureLoaded()
        {
            if (isLoaded)
                return;

            isLoaded = true;

            if (parent.Entity.InferredReferences.Contains(Model.Name))
            {
                if (!Model.Cardinality.IsToMany)
                    value = parent.ObjectContextAsync.Resolver.Resolve<T>((Entity)parent.Entity[Model.Name].Value, parent.ObjectContextAsync.PersistenceEngineAsync.FactoryAsync.Model);
                else
                {
                    List<Entity> entities = new List<Entity>();
                    foreach (Entry<Entity> entry in (MultipleEntry)parent.Entity[Model.Name])
                        entities.Add(entry.TypedValue);
                    if (parent.ObjectContext == null)
                        values = new IPersistableCollection<T>(parent.ObjectContextAsync,
                            parent,
                            Model.Name,
                            parent.ObjectContextAsync.Resolver.Resolve<T>(entities, parent.ObjectContextAsync.PersistenceEngineAsync.FactoryAsync.Model));
                    else
                        values = new IPersistableCollection<T>(parent.ObjectContext,
                            parent,
                            Model.Name,
                            parent.ObjectContextAsync.Resolver.Resolve<T>(entities, parent.ObjectContextAsync.PersistenceEngineAsync.FactoryAsync.Model));
                }
                return;
            }
            if (!Model.Cardinality.IsToMany)
            {
                var result = parent.ObjectContext.LoadReference<T>(parent.Entity, Model.Name);
                if (result.Count > 0)
                {
                    value = result[0];
                    foreach (T item in result)
                        parent.Entity.Add(Entry.Create(Model.Name, State.UpToDate, parent.ObjectContextAsync.Resolver.Resolve(item, parent.ObjectContextAsync.PersistenceEngineAsync.FactoryAsync.Model)));
                }
                parent.Entity.InferredReferences.Add(Model.Name);
            }
            else
            {
                values = new IPersistableCollection<T>(parent.ObjectContext, parent, Model.Name, null);
            }

        }

        public T Value
        {
            get
            {
                EnsureLoaded();

                return value;
            }
            set
            {
                if (isLoaded && this.Value != null)
                    parent.Entity.Remove(Model.Name);

                if (value != null)
                    parent.Entity.Add(Model.Name, parent.ObjectContext.Resolver.Resolve(value, parent.ObjectContext.Factory.PersistenceEngineFactory.Model));

                isLoaded = true;
            }
        }

        public IList<T> Values
        {
            get
            {
                EnsureLoaded();

                return values;
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
