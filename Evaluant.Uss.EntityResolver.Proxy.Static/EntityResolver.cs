using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Evaluant.Uss.ObjectContext.Contracts;

namespace Evaluant.Uss.EntityResolver.Proxy.Static
{
    public class EntityResolver : CacheEntityResolver
    {
        Assembly staticProxyAssembly;
        Dictionary<Type, Type> resolvedTypes = new Dictionary<Type, Type>();

        public override Type GetType(Type type, Model.Model model)
        {
            if (typeof(IPersistable).IsAssignableFrom(type))
                return type;
            Type resolvedType = null;
            if (!resolvedTypes.TryGetValue(type, out resolvedType))
            {
                lock (resolvedTypes)
                {
                    if (!resolvedTypes.TryGetValue(type, out resolvedType))
                    {
                        foreach (Assembly assembly in oc.Factory.RegisteredAssemblies)
                        {
                            if (assembly.GetCustomAttributes(typeof(ProxyAssemblyAttribute), false) != null)
                            {
                                staticProxyAssembly = assembly;
                                break;
                            }
                        }
                        if (staticProxyAssembly == null)
                            throw new InvalidOperationException("No static proxy assembly has been registered");
                        foreach (Type t in staticProxyAssembly.GetExportedTypes())
                        {
                            if (t.BaseType == type)
                            {
                                resolvedType = t;
                                break;
                            }
                        }
                        if (resolvedType == null)
                            throw new InvalidCastException("No static proxy was defined for type " + type.FullName);
                        resolvedTypes.Add(type, resolvedType);
                    }
                }
            }
            return resolvedType;
        }

        public override void Resolve<T>(T entity, Model.Model model, Domain.Entity entityToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
