using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.MetaData
{
    public static class NamespaceConverter
    {
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
    }
}
