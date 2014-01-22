using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Evaluant.Uss.MetaData
{
    public static class TypeResolver //: ITypeResolver
    {
        public static string ConvertNamespaceDomainToEuss(string type)
        {
            return type;
            //return type.Replace('.', ':');
        }

        public static string ConvertNamespaceEussToDomain(string eussType)
        {
            return eussType;
            //return eussType.Replace(':', '.');
        }

        public static string ConvertNamespaceDomainToEuss(Type type)
        {
            StringBuilder sb = new StringBuilder();

            if (type.IsGenericType)
            {
                string genericType = type.GetGenericTypeDefinition().FullName;
                sb.Append(ConvertNamespaceDomainToEuss(genericType.Substring(0, genericType.IndexOf('`'))));

                foreach (Type parameterType in type.GetGenericArguments())
                {
                    sb.Append("{");
                    sb.Append(ConvertNamespaceDomainToEuss(parameterType));
                    sb.Append("}");
                }
            }
            else
            {
                sb.Append(ConvertNamespaceDomainToEuss(type.FullName));
            }
            return sb.ToString();
        }

        static Regex genericRepresentation = new Regex(@"(?<genericType>[^{]+)\{(?<innerType>[^}]+)\}");

        public static Type ConvertNamespaceEussToType(string type)
        {
            Match m = genericRepresentation.Matches(type)[0];
            //for (int i = matches.Count - 1; i >= 0; i--)
            //{
            //    Match m = matches[i];
            Type genericType = Type.GetType(m.Groups["genericType"].Value);
            List<Type> innerTypes = new List<Type>();
            foreach (string innerTypeName in m.Groups["innerType"].Value.Split(','))
            {
                Type innerType = ConvertNamespaceEussToType(innerTypeName);
                innerTypes.Add(innerType);
            }
            //}
            if (genericType.IsGenericTypeDefinition)
                return genericType.MakeGenericType(innerTypes.ToArray());
            return genericType;
        }

        /// <summary>
        /// Gets a type.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public static Type GetType(string typeName)
        {
            // If it's a nullable type, gets the underlying type
            if (typeName[typeName.Length - 1] == '?')
                typeName = typeName.Substring(0, typeName.Length - 1);

            Type type = Type.GetType(typeName);

            if (type != null)
            {
                return type;
            }

            switch (typeName.ToLower())
            {
                case "string": return typeof(string);
                case "datetime": return typeof(DateTime);
                case "int": return typeof(int);
                case "sbyte": return typeof(sbyte);
                case "byte": return typeof(byte);
                case "char": return typeof(char);
                case "short": return typeof(short);
                case "ushort": return typeof(ushort);
                case "uint": return typeof(uint);
                case "long": return typeof(long);
                case "ulong": return typeof(ulong);
                case "bool": return typeof(bool);
                case "double": return typeof(double);
                case "float": return typeof(float);
                case "guid": return typeof(Guid);
                case "byte[]": return typeof(byte[]);
            }

            string[] values = typeName.Split(',');

            if (values.Length >= 2)
            {
                Assembly assembly = Assembly.Load(values[1].Trim());
                if (assembly != null)
                    return assembly.GetType(values[0].Trim());

                throw new NullReferenceException("The type " + typeName + " was not found");
            }

            return typeof(object);
        }

        #region ITypeResolver Members


        public static bool IsPrimitive(string type)
        {
            return IsPrimitive(GetType(type));
        }

        public static bool IsPrimitive(Type type)
        {
            return type.IsPrimitive
                || type == typeof(string)
                || type == typeof(Guid)
                || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static bool IsAnonymous(string type)
        {
            return IsAnonymous(GetType(type));
        }

        public static bool IsAnonymous(Type type)
        {
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
               && type.IsGenericType && type.Name.Contains("AnonymousType")
               && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) ||
                   type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
               && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }

        public static bool IsId(string name, Type type, Type parentType)
        {
            if (name == "Id" || name == parentType.Name + "Id")
            {
                return IsPrimitive(type);
            }
            return false;
        }

        public static bool IsId(string name, string type, Type parentType)
        {
            return IsId(name, GetType(type), parentType);
        }

        #endregion
    }
}
