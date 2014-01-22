using System;
using Evaluant.Uss.PersistentDescriptors;
using System.Collections.Generic;
using System.Reflection;
using Evaluant.Uss.Utility;
using System.Collections;

namespace Evaluant.Uss.MetaData
{
    public class LightMetaDataFactory
    {

        public static IMetaData[] FromPropertyDescriptor(Type type, IPersistentDescriptor persDescriptor, bool derivedTypes)
        {
            List<IMetaData> metadata = new List<IMetaData>();

            string typeName = TypeResolver.ConvertNamespaceDomainToEuss(type);

            metadata.Add(new TypeMetaData(typeName));
            foreach (PropertyDescriptor descriptor in persDescriptor.GetPersistentProperties(type))
            {
                // Relation
                if (descriptor.IsEntity)
                {
                    Type propType = descriptor.Type;

                    if (descriptor.IsList)
                    {
                        //	Get the child type when the reference is a collection
                        PersistentPropertyAttribute[] attrs = type.GetProperty(descriptor.PropertyName).GetCustomAttributes(
                            typeof(PersistentPropertyAttribute), false) as PersistentPropertyAttribute[];

                        if (attrs != null && attrs.Length > 0)
                        {
                            PersistentPropertyAttribute ppa = attrs[0];
                            propType = ppa.Type;
                        }
                    }

                    if (descriptor.IsGenericList)
                    {
                        propType = DescriptorHelper.GetGenericType(descriptor.Type);
                    }

                    bool isList = descriptor.IsList || descriptor.IsGenericList;

                    metadata.Add(new ReferenceMetaData(typeName, descriptor.PropertyName, TypeResolver.ConvertNamespaceDomainToEuss(propType), descriptor.IsComposition, !descriptor.IsComposition, isList));
                }
                else
                {
                    if (descriptor.Type.IsEnum)
                    {
                        Type enumType = typeof(string);

                        //  Try to get an attribute for this property which defines the type of the enum (i.e.: int or string)
                        PropertyInfo pi = type.GetProperty(descriptor.PropertyName, descriptor.Type);
                        if (pi != null)
                        {
                            PersistentPropertyAttribute[] attrs = pi.GetCustomAttributes(
                            typeof(PersistentPropertyAttribute), false) as PersistentPropertyAttribute[];

                            // Add this property if more than one Attribute
                            if (attrs != null && attrs.Length > 0)
                            {
                                // Takes the first attribute even if more are set
                                PersistentPropertyAttribute ppa = attrs[0];
                                if (ppa.Type != null)
                                    enumType = ppa.Type;
                            }
                        }

                        List<string> values = new List<string>();
                        foreach (object val in EnumHelper.GetValues(descriptor.Type))
                            values.Add(val.ToString());

                        PropertyMetaData pm = new PropertyMetaData(typeName, descriptor.PropertyName, enumType, false);
                        pm.Values = values.ToArray();

                        metadata.Add(pm);
                    }
                    else
                        metadata.Add(new PropertyMetaData(typeName, descriptor.PropertyName, descriptor.Type, false));
                }

            }

            if (derivedTypes)
                foreach (Type dt in type.Assembly.GetTypes())
                    if (dt.BaseType == type)
                    {
                        metadata.AddRange(FromPropertyDescriptor(dt, persDescriptor, true));
                        metadata.Add(new TypeMetaData(TypeResolver.ConvertNamespaceDomainToEuss(dt), TypeResolver.ConvertNamespaceDomainToEuss(type), type.IsInterface));
                    }

            return metadata.ToArray();
        }

        public static IMetaData[] FromAssembly(string assemblyName)
        {
            return FromAssembly(assemblyName, new ReflectionDescriptor());
        }

        public static IMetaData[] FromAssembly(Assembly assembly, params string[] namespaces)
        {
            return FromAssembly(assembly, new ReflectionDescriptor(), namespaces);
        }

        public static IMetaData[] FromAssembly(string assemblyName, IPersistentDescriptor persDescriptor)
        {
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");

            string[] assemblyNameAndNamespaces = new string[] { assemblyName };
            if (assemblyName.Contains(":"))
                assemblyNameAndNamespaces = assemblyName.Split(':');
            assemblyName = assemblyNameAndNamespaces[assemblyNameAndNamespaces.Length - 1];
            string[] namespaces = new string[assemblyNameAndNamespaces.Length - 1];
            for (int i = 0; i < assemblyNameAndNamespaces.Length - 1; i++)
                namespaces[i] = assemblyNameAndNamespaces[i];

            return FromAssembly(Assembly.Load(assemblyName), persDescriptor, namespaces);
        }

        public static IMetaData[] FromAssembly(Assembly assembly, IPersistentDescriptor persDescriptor, params string[] namespaces)
        {
            List<IMetaData> metadata = new List<IMetaData>();
            List<string> processed = new List<string>();

            List<string> namespaceList;
            if (namespaces != null)
                namespaceList = new List<string>(namespaces);
            else
                namespaceList = new List<string>(0);

            foreach (Type type in assembly.GetExportedTypes())
            {
                if (namespaceList.Count > 0 && !namespaceList.Contains(type.Namespace))
                    continue;

                if (!type.IsPublic)
                {
                    continue;
                }

                // Ignore none business types
                if (typeof(ICollection).IsAssignableFrom(type))
                {
                    continue;
                }

                // Ignore types from System assembly
                if (type.Assembly == typeof(String).Assembly)
                {
                    continue;
                }

#if !EUSS11
                // Ignores generic types declared globally as they can't be persisted. Only use those which are used as relationships (really used)
                // i.e: Gen<T> is ignored, but not Gen<string>
                if (type.IsGenericType)
                {
                    continue;
                }
#endif

                string typeName = TypeResolver.ConvertNamespaceDomainToEuss(type);

                if (type.IsInterface)
                {
                    metadata.Add(new InterfaceMetadata(typeName));
                }

                if (processed.Contains(typeName) || (type.Attributes & TypeAttributes.Sealed) == TypeAttributes.Sealed)
                    continue;

                object[] attributes = type.GetCustomAttributes(typeof(NotSerializedAttribute), true);
                if (attributes.Length > 0 && attributes[0] is NotSerializedAttribute)
                    continue;

                ImportType(persDescriptor, metadata, processed, type, typeName);

            }

            return metadata.ToArray();
        }


#if !SILVERLIGHT
        public static IMetaData[] FromModelFile(string filename)
        {
            XmlReader xmlTextReader = null;

            try
            {
                xmlTextReader = XmlReader.Create(filename);
                return FromModelFile(xmlTextReader);
            }
            finally
            {
                if (xmlTextReader != null)
                    xmlTextReader.Close();
            }
        }

        public static IMetaData[] FromModelFile(XmlReader inStream)
        {
            //ITypeResolver typeResolver = new TypeResolverImpl(); 
            List<IMetaData> metadata = new List<IMetaData>(100);

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(inStream);

            // The reason for needing a prefix for the default namespace in our XPath query is due to the fact that in XPath, there is no concept of a default namespace.
            XmlNamespaceManager nsm = new XmlNamespaceManager(xDoc.NameTable);
            nsm.AddNamespace("ns", "http://euss.evaluant.com/schemas/GenerationModel.xsd");

            foreach (XmlNode n in xDoc.SelectNodes("//ns:Class", nsm))
            {
                string package = TypeResolver.ConvertNamespaceDomainToEuss(n.ParentNode.Attributes["name"].Value.Trim()) + ":";
                string typeName = package + n.Attributes["name"].Value;
                metadata.Add(new TypeMetaData(typeName, n.Attributes["inherit"] != null ? package + n.Attributes["inherit"].Value : String.Empty, false));

                string[] implement = n.Attributes["implement"] != null ? n.Attributes["implement"].Value.Split(',') : new string[0];
                foreach (string interf in implement)
                {
                    metadata.Add(new TypeMetaData(typeName, package + interf.Trim(), true));
                }
            }

            foreach (XmlNode n in xDoc.SelectNodes("//ns:Property", nsm))
            {
                string package = TypeResolver.ConvertNamespaceDomainToEuss(n.ParentNode.ParentNode.Attributes["name"].Value.Trim()) + ".";
                metadata.Add(new PropertyMetaData(package + n.ParentNode.Attributes["name"].Value, n.Attributes["name"].Value, TypeResolver.GetType(n.Attributes["type"].Value), Convert.ToBoolean(n.Attributes["isId"].Value)));
            }

            foreach (XmlNode n in xDoc.SelectNodes("//ns:Relationship", nsm))
            {
                string package = TypeResolver.ConvertNamespaceDomainToEuss(n.ParentNode.Attributes["name"].Value.Trim()) + ":";
                bool composition = n.Attributes["type"].Value == "composition";
                string typeName = package + n.SelectSingleNode("./ns:Parent/@name", nsm).Value;
                bool toMany = n.SelectSingleNode("./ns:Child/@multiplicity", nsm).Value.IndexOf("*") != -1;
                bool fromMany = n.SelectSingleNode("./ns:Parent/@multiplicity", nsm).Value.IndexOf("*") != -1;
                string role = n.SelectSingleNode("./ns:Child/@role", nsm).Value;
                string childTypeName = n.SelectSingleNode("./ns:Child/@name", nsm).Value;

                if (childTypeName.Trim() == String.Empty)
                    throw new MetadataException("A relationship has no child type in a model file.");

                if (typeName.Trim() == String.Empty)
                    throw new MetadataException("A relationship has no parent type in a model file.");

                // Is it an enumeration ?
                XmlNode xmlEnum = xDoc.SelectSingleNode(String.Concat("//ns:Enumeration[@name='", childTypeName, "']"), nsm);

                if (xmlEnum != null)
                {
                    ArrayList values = new ArrayList();
                    foreach (XmlNode valNode in xmlEnum.SelectNodes("./ns:Literal/@name", nsm))
                        values.Add(valNode.Value);

                    PropertyMetaData pm = new PropertyMetaData(typeName, role, typeof(string), false);
                    pm.Values = (string[])values.ToArray(typeof(string));

                    metadata.Add(pm);
                }
                else
                {
                    metadata.Add(new ReferenceMetaData(typeName, role, package + childTypeName, composition, fromMany, toMany));
                }
            }

            return metadata.ToArray();
        }


        public static IMetaData[] FromMetaDataFile(string filename)
        {
            XmlReader xmlTextReader = null;

            try
            {
                xmlTextReader = XmlReader.Create(filename);
                return FromMetaDataFile(xmlTextReader);
            }
            finally
            {
                if (xmlTextReader != null)
                    xmlTextReader.Close();
            }
        }

        public static IMetaData[] FromMetaDataFile(XmlReader inStream)
        {
            List<IMetaData> metadata = new List<IMetaData>(100);
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(inStream);

            // The reason for needing a prefix for the default namespace in our XPath query is due to the fact that in XPath, there is no concept of a default namespace.
            XmlNamespaceManager nsm = new XmlNamespaceManager(xDoc.NameTable);
            nsm.AddNamespace("ns", "http://euss.evaluant.com/schemas/MetaDataModel.xsd");

            foreach (XmlNode n in xDoc.SelectNodes("//ns:Entity", nsm))
            {
                TypeMetaData tmd = new TypeMetaData(n.Attributes["type"].Value, n.Attributes["inherit"] != null ? n.Attributes["inherit"].Value : String.Empty, false);
                tmd.Ignore = n.Attributes["ignore"] != null ? bool.Parse(n.Attributes["ignore"].Value) : false;
                metadata.Add(tmd);

                if (n.Attributes["implement"] != null)
                {
                    foreach (string intf in n.Attributes["implement"].Value.Split(','))
                    {
                        TypeMetaData tmdi = new TypeMetaData(n.Attributes["type"].Value, intf.Trim(), true);
                        metadata.Add(tmd);
                    }
                }

                // Processing sub nodes in the same order as they are declared

                foreach (XmlNode s in n.SelectNodes("ns:*", nsm))
                {
                    switch (s.Name)
                    {
                        case "Attribute":
                            string typeName = TypeResolver.ConvertNamespaceEussToDomain(s.Attributes["type"].Value);

                            Type type = TypeResolver.GetType(typeName);

                            if (type == null)
                                throw new TypeLoadException(String.Concat("The type ", typeName, " could not be found. You should register the assembly containing it."));

                            PropertyMetaData pm = new PropertyMetaData(s.ParentNode.Attributes["type"].Value, s.Attributes["name"].Value, type, s.Attributes["idId"] == null ? false : Convert.ToBoolean(s.Attributes["isId"].Value));

                            if (s.Attributes["values"] != null)
                                pm.Values = s.Attributes["values"].Value.Split();

                            pm.Ignore = s.Attributes["ignore"] != null ? bool.Parse(s.Attributes["ignore"].Value) : false;

                            metadata.Add(pm);
                            break;

                        case "Reference":

                            // fromMany, toMany and composition are false if not specified
                            bool composition = s.Attributes["composition"] == null ? false : bool.Parse(s.Attributes["composition"].Value);
                            bool fromMany = s.Attributes["fromMany"] == null ? false : bool.Parse(s.Attributes["fromMany"].Value);
                            bool toMany = s.Attributes["toMany"] == null ? false : bool.Parse(s.Attributes["toMany"].Value);
                            ReferenceMetaData rmd = new ReferenceMetaData(s.ParentNode.Attributes["type"].Value, s.Attributes["name"].Value, s.Attributes["type"].Value, composition, fromMany, toMany);

                            rmd.Ignore = s.Attributes["ignore"] != null ? bool.Parse(s.Attributes["ignore"].Value) : false;
                            metadata.Add(rmd);
                            break;
                    }

                }
            }
            return metadata.ToArray();
        }

        //public static IMetaData[] FromAssembly(string assemblyName)
        //{
        //    return FromAssembly(assemblyName, new ReflectionDescriptor());
        //}

        //public static IMetaData[] FromAssembly(string assemblyName, IPersistentDescriptor descriptor)
        //{
        //    return FromAssembly()
        //}

#endif

        private static void ImportType(IPersistentDescriptor persDescriptor, IList<IMetaData> metadata, ICollection<string> processed, Type type, string typeName)
        {
            if (processed.Contains(typeName) || type == typeof(object))
            {
                return;
            }

            processed.Add(typeName);

            PropertyDescriptor[] properties = persDescriptor.GetPersistentProperties(type);

            string parentTypeName = type.BaseType == typeof(object) || type.IsInterface ? String.Empty : TypeResolver.ConvertNamespaceDomainToEuss(type.BaseType);
            metadata.Add(new TypeMetaData(typeName, parentTypeName, false));

            foreach (Type intf in type.GetInterfaces())
            {
                metadata.Add(new TypeMetaData(typeName, TypeResolver.ConvertNamespaceDomainToEuss(intf), true));
            }

            foreach (PropertyDescriptor descriptor in properties)
            {
                // Relation
                if (descriptor.IsEntity)
                {
                    Type propType = descriptor.Type;

                    if (descriptor.IsList)
                    {
                        //	Get the child type when the reference is a collection
                        PersistentPropertyAttribute[] attrs = type.GetProperty(descriptor.PropertyName).GetCustomAttributes(
                            typeof(PersistentPropertyAttribute), false) as PersistentPropertyAttribute[];

                        if (attrs != null && attrs.Length > 0)
                        {
                            PersistentPropertyAttribute ppa = attrs[0];
                            propType = ppa.Type;
                        }
                    }

#if !EUSS11
                    if (descriptor.IsGenericList)
                    {
                        propType = DescriptorHelper.GetGenericType(descriptor.Type);
                    }

                    bool isList = descriptor.IsList || descriptor.IsGenericList;
#else
                        bool isList = descriptor.IsList;
#endif

                    // Ignores relationships which are not IList
                    if (descriptor.IsList && descriptor.Type != typeof(IEnumerable))
                        continue;

                    // Ignores relationships to undefined Generic types (a relationship to Generic<T>, when the class is itsel generic)
                    if (propType.FullName == null)
                        continue;

                    // Ignore types from System assembly
                    if (propType.Assembly == typeof(String).Assembly)
                    {
                        continue;
                    }

                    // Ignore none business types
                    if (propType.IsPublic && !typeof(IEnumerable).IsAssignableFrom(propType))
                    {
                        // Ignore types from System assembly
                        if (propType.Assembly != typeof(String).Assembly)
                        {
                            metadata.Add(new ReferenceMetaData(typeName, descriptor.PropertyName, TypeResolver.ConvertNamespaceDomainToEuss(propType), descriptor.IsComposition, !descriptor.IsComposition, isList));
                        }

                        ImportType(persDescriptor, metadata, processed, propType, TypeResolver.ConvertNamespaceDomainToEuss(propType));
                    }
                }
                else
                {
                    if (descriptor.Type.IsEnum)
                    {
                        Type enumType = typeof(string);

                        //  Try to get an attribute for this property which defines the type of the enum (i.e.: int or string)
                        PropertyInfo pi = type.GetProperty(descriptor.PropertyName, descriptor.Type);
                        if (pi != null)
                        {
                            PersistentPropertyAttribute[] attrs = pi.GetCustomAttributes(
                            typeof(PersistentPropertyAttribute), false) as PersistentPropertyAttribute[];

                            // Add this property if more than one Attribute
                            if (attrs != null && attrs.Length > 0)
                            {
                                // Takes the first attribute even if more are set
                                PersistentPropertyAttribute ppa = attrs[0];
                                if (ppa.Type != null)
                                    enumType = ppa.Type;
                            }
                        }

                        List<string> values = new List<string>();
                        foreach (object val in EnumHelper.GetValues(descriptor.Type))
                            values.Add(val.ToString());

                        PropertyMetaData pm = new PropertyMetaData(typeName, descriptor.PropertyName, enumType, false);
                        pm.Values = values.ToArray();

                        metadata.Add(pm);
                    }
                    else
                        metadata.Add(new PropertyMetaData(typeName, descriptor.PropertyName, descriptor.Type, false));
                }
            }
        }
    }
}
