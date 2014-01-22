using System;
using System.Collections.Generic;
using System.Reflection;

namespace Evaluant.Uss.Utility
{
    public static class EnumHelper
    {
        public static string[] GetNames(Type enumType)
        {
            List<string> enumNames = new List<string>();

            foreach (FieldInfo fi in enumType.GetFields(BindingFlags.Static | BindingFlags.Public))
                enumNames.Add(fi.Name);

            return enumNames.ToArray();
        }

        public static T[] GetValues<T>()
            where T : struct
        {
            return GetValues<T>(false);
        }

        public static Array GetValues(Type enumType)
        {
            return GetValues(enumType, false);
        }


        public static T[] GetValues<T>(bool ignoreCase)
            where T : struct
        {
            List<T> enumValues = new List<T>();
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new NotSupportedException("The type " + enumType.Name + " is not an enum");
            foreach (FieldInfo fi in enumType.GetFields(BindingFlags.Static | BindingFlags.Public))
                enumValues.Add(EnumHelper.Parse<T>(fi.Name, ignoreCase));

            return enumValues.ToArray();
        }

        public static T Parse<T>(string name, bool ignoreCase)
        {
            return Parse<T>(typeof(T), name, ignoreCase);
        }

        private static T Parse<T>(Type enumType, string name, bool ignoreCase)
        {
            return (T)Enum.Parse(typeof(T), name, ignoreCase);
        }

        public static Array GetValues(Type enumType, bool ignoreCase)
        {
            List<int> enumValues = new List<int>();

            if (!enumType.IsEnum)
                throw new NotSupportedException("The type " + enumType.Name + " is not an enum");

            foreach (FieldInfo fi in enumType.GetFields(BindingFlags.Static | BindingFlags.Public))
                enumValues.Add(EnumHelper.Parse<int>(enumType, fi.Name, ignoreCase));

            return enumValues.ToArray();
        }
    }
}
