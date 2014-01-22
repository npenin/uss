using System;
using System.Collections;
using System.Collections.Generic;
//using System.Reflection.Emit;

namespace Evaluant.Uss.PersistentDescriptors
{

    /// <summary>
    /// Helper class dedicated to PersistentDescriptor
    /// </summary>
    public static class DescriptorHelper
    {

        private static IList<Type> _StandardTypes = new List<Type>();
        //private static IDictionary<Type, OpCode> _OpCodes = new Dictionary<Type, OpCode>();

#if !EUSS11
        private static List<Type> _NullableTypes = new List<Type>();
#endif

        static DescriptorHelper()
        {
            _StandardTypes.Add(typeof(sbyte));
            _StandardTypes.Add(typeof(byte));
            _StandardTypes.Add(typeof(char));
            _StandardTypes.Add(typeof(short));
            _StandardTypes.Add(typeof(ushort));
            _StandardTypes.Add(typeof(int));
            _StandardTypes.Add(typeof(uint));
            _StandardTypes.Add(typeof(long));
            _StandardTypes.Add(typeof(ulong));
            _StandardTypes.Add(typeof(bool));
            _StandardTypes.Add(typeof(double));
            _StandardTypes.Add(typeof(float));
            _StandardTypes.Add(typeof(string));
            _StandardTypes.Add(typeof(decimal));
            _StandardTypes.Add(typeof(DateTime));
            _StandardTypes.Add(typeof(Guid));
            _StandardTypes.Add(typeof(TimeSpan));

            _NullableTypes.Add(typeof(sbyte?));
            _NullableTypes.Add(typeof(byte?));
            _NullableTypes.Add(typeof(char?));
            _NullableTypes.Add(typeof(short?));
            _NullableTypes.Add(typeof(ushort?));
            _NullableTypes.Add(typeof(int?));
            _NullableTypes.Add(typeof(uint?));
            _NullableTypes.Add(typeof(long?));
            _NullableTypes.Add(typeof(ulong?));
            _NullableTypes.Add(typeof(bool?));
            _NullableTypes.Add(typeof(double?));
            _NullableTypes.Add(typeof(float?));
            _NullableTypes.Add(typeof(decimal?));
            _NullableTypes.Add(typeof(DateTime?));
            _NullableTypes.Add(typeof(Guid?));

            //_OpCodes[typeof(sbyte)] = OpCodes.Ldind_I1;
            //_OpCodes[typeof(byte)] = OpCodes.Ldind_U1;
            //_OpCodes[typeof(char)] = OpCodes.Ldind_U2;
            //_OpCodes[typeof(short)] = OpCodes.Ldind_I2;
            //_OpCodes[typeof(ushort)] = OpCodes.Ldind_U2;
            //_OpCodes[typeof(int)] = OpCodes.Ldind_I4;
            //_OpCodes[typeof(uint)] = OpCodes.Ldind_U4;
            //_OpCodes[typeof(long)] = OpCodes.Ldind_I8;
            //_OpCodes[typeof(ulong)] = OpCodes.Ldind_I8;
            //_OpCodes[typeof(bool)] = OpCodes.Ldind_I1;
            //_OpCodes[typeof(double)] = OpCodes.Ldind_R8;
            //_OpCodes[typeof(float)] = OpCodes.Ldind_R4;
        }

        //public static IDictionary<Type, OpCode> OpCode
        //{
        //    get { return _OpCodes; }
        //}

        /// <summary>
        /// Checks if a type is usable whithin a entity
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>Whether the type is usable or not</returns>
        public static bool IsEntity(Type type)
        {
            if (_NullableTypes.Contains(type))
            {
                return false;
            }
            return !(_StandardTypes.Contains(type) || type.IsEnum || type.IsArray);
        }

        /// <summary>
        /// Checks if a type is a collection
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>Whether the type is implements IList or not</returns>
        public static bool IsList(Type type)
        {
            return typeof(IList).IsAssignableFrom(type);
        }

        /// <summary>
        /// Checks if a type is a generic collection
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>Whether the type is a generic collection or not</returns>
        public static bool IsGenericList(Type type)
        {
            Type currentType = type;

            do
            {
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() != null &&
                     typeof(IList<>).IsAssignableFrom(currentType.GetGenericTypeDefinition()))
                    return true;

                currentType = currentType.BaseType;
            } while (currentType != null && currentType != typeof(object));

            return false;

            //return type.IsGenericType && type.GetGenericTypeDefinition () != null
            //    && typeof(IList<>).IsAssignableFrom(type.GetGenericTypeDefinition());
        }

        public static Type GetGenericType(Type type)
        {
            Type currentType = type;

            do
            {
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() != null &&
                    typeof(IList<>).IsAssignableFrom(currentType.GetGenericTypeDefinition()))
                    return currentType.GetGenericArguments()[0];

                currentType = currentType.BaseType;
            } while (currentType != null && currentType != typeof(object));

            return null;
        }

        /// <summary>
        /// Checks if a property name is valid
        /// </summary>
        /// <param name="propertyName">The property name to check</param>
        /// <returns>Whether the property name is valid or not</returns>
        public static bool IsValidProperty(string propertyName)
        {
            return propertyName != "Entity" && propertyName != "ObjectContext";
        }
    }
}