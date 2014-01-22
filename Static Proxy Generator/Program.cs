using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.EntityResolver.Proxy.Dynamic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.IO;
using Evaluant.Uss.MetaData;
using Evaluant.Uss.Model;
using Evaluant.Uss.EntityResolver.Proxy;

namespace Static_Proxy_Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Environment.CurrentDirectory = args[1];
            }
            else
                Environment.CurrentDirectory = args[0].Substring(0, args[0].Length - Path.GetFileName(args[0]).Length);

            if (!File.Exists(args[0]))
                throw new FileNotFoundException(args[0]);
            if (!Path.IsPathRooted(args[0]))
                args[0] = Path.Combine(Environment.CurrentDirectory, args[0]);

            Assembly assembly = Assembly.LoadFile(args[0]);
            Model model = new Model();
            ModelMetaDataVisitor visitor = new ModelMetaDataVisitor(model);
            foreach (IMetaData metadata in MetaDataFactory.FromAssembly(assembly, args.Length > 2 ? args[2] : null))
                metadata.Accept(visitor);

            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = assembly.GetName().Name + ".Proxies";

            // Create a new assembly with one module
            AssemblyBuilder newAssembly = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave, Environment.CurrentDirectory);
            newAssembly.SetCustomAttribute(new CustomAttributeBuilder(typeof(ProxyAssemblyAttribute).GetConstructor(Type.EmptyTypes), new object[0]));

            ModuleBuilder moduleBuilder = newAssembly.DefineDynamicModule(assemblyName.Name, assemblyName.Name + ".dll", true);
            EntityResolver resolver = new EntityResolver();
            foreach (Entity e in model.Entities.Values)
            {
                Type t = assembly.GetType(e.Type);
                if (t.IsInterface || t.IsAbstract)
                    continue;
                resolver.GenerateType(t, moduleBuilder, model).CreateType();
            }

            newAssembly.Save(assemblyName.Name + ".dll");
        }
    }
}
