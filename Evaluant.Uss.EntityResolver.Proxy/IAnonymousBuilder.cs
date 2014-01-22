using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.EntityResolver.Proxy
{
    public interface IAnonymousBuilder : IPersistableProxy
    {
        T GetTarget<T>(Entity entity);
    }

    public class AnonymousBuilder : IAnonymousBuilder
    {
        private ConstructorInfo ctor;

        public AnonymousBuilder(Type t)
        {
            this.ctor = t.GetConstructors()[0];
        }

        public T GetTarget<T>(Entity entity)
        {
            object[] values = new object[entity.Count];
            entity.Id = Guid.NewGuid().ToString();
            foreach (ParameterInfo parameter in ctor.GetParameters())
                values[parameter.Position] = entity.GetValue(parameter.Name);

            return (T)ctor.Invoke(values);
        }

        public void Set()
        {

        }

        public void SetReferences()
        {
        }

        public void Update()
        {
        }

        public Domain.Entity Entity
        {
            get;
            set;
        }

        public ObjectContext.Contracts.IPersistenceEngineObjectContext ObjectContext
        {
            get;
            set;
        }

        public ObjectContext.Contracts.IPersistenceEngineObjectContextAsync ObjectContextAsync
        {
            get;
            set;
        }
    }

}
