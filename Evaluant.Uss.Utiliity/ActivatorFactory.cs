using System;
using System.Collections.Generic;
using System.Text;
//using System.Reflection.Emit;
using System.Collections;
using System.Reflection;
using System.Threading;

namespace Evaluant.Uss.Utility
{
    /// <summary>
    /// Generates IActivator classes implementations dynamically
    /// </summary>
    public class ActivatorFactory
    {
        private class Activator<T> : IActivator
            where T : new()
        {
            public object CreateInstance()
            {
                return new T();
            }
        }

        private Dictionary<Type, IActivator> _ActivatorCache = new Dictionary<Type, IActivator>(20);

        public IActivator CreateActivator<T>()
        {
            return CreateActivator(typeof(T));
        }

        public IActivator CreateActivator(Type typeToActivate)
        {
            IActivator activator;

            if (_ActivatorCache.ContainsKey(typeToActivate))
            {
                return _ActivatorCache[typeToActivate];
            }

            lock (_ActivatorCache)
            {
                if (_ActivatorCache.ContainsKey(typeToActivate))
                {
                    return _ActivatorCache[typeToActivate];
                }

                activator = typeof(Activator<>).MakeGenericType(typeToActivate) as IActivator;

                _ActivatorCache.Add(typeToActivate, activator);

            }

            // newAssembly.Save(assemblyName.Name + ".dll");

            return activator;
        }
    }
}
