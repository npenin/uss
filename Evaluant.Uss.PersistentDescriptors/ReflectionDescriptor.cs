using System;
using System.Collections;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;
using Evaluant.Uss.Utility;

namespace Evaluant.Uss.PersistentDescriptors
{
    /// <summary>
    /// Provides an IPersistentDescriptor implementation based on Reflection returning
    /// all public properties
    /// </summary>
    public class ReflectionDescriptor : IPersistentDescriptor
    {

        private IDictionary<Type, PropertyDescriptor[]> _CachedDescriptors = new Dictionary<Type, PropertyDescriptor[]>();
        private IDictionary<Type, PropertyDescriptor> _CachedIDs = new Dictionary<Type, PropertyDescriptor>();

        private static ReaderWriterLock _RWL = new ReaderWriterLock();

        public PropertyDescriptor[] GetPersistentProperties(Type targetType)
        {
            _RWL.AcquireReaderLock();

            try
            {
                if (_CachedDescriptors.ContainsKey(targetType))
                    return (PropertyDescriptor[])_CachedDescriptors[targetType];
            }
            finally
            {
                _RWL.ReleaseReaderLock();
            }

            _RWL.AcquireWriterLock();

            try
            {

                List<PropertyDescriptor> properties = new List<PropertyDescriptor>();

                Type parentType = targetType;

                while (parentType != null && parentType != typeof(object))
                {
                    foreach (PropertyInfo pi in parentType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                        if (DescriptorHelper.IsValidProperty(pi.Name))
                        {
                            if (pi.GetCustomAttributes(typeof(NotSerializedAttribute), true).Length > 0)
                                continue;

                            //if (parentType != targetType && pi.DeclaringType.Assembly == targetType.Assembly)
                            //    continue;

                            //  Try to get an attribute for this property
                            PersistentPropertyAttribute[] attrs = pi.GetCustomAttributes(
                                typeof(PersistentPropertyAttribute), false) as PersistentPropertyAttribute[];

                            FieldInfo fi = null;
                            bool serializeAsAttribute = false;
                            bool isComposition = false;

                            // Add this property if more than one Attribute
                            if (attrs != null && attrs.Length > 0)
                            {
                                // Takes the first attribute even if more are set
                                PersistentPropertyAttribute ppa = attrs[0];

                                serializeAsAttribute = ppa.SerializeAsAttribute;
                                isComposition = ppa.Composition;

                                if (!string.IsNullOrEmpty(ppa.FieldName))
                                {
                                    fi = parentType.GetField(ppa.FieldName, BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                                }
                            }

                            if (fi == null)
                            {
                                // Searches for a Field with the same name but a different case
                                fi = parentType.GetField(pi.Name, BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                                // If no field was found, searches for _{PropertyName}
                                if (fi == null)
                                    fi = parentType.GetField("_" + pi.Name, BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                                // If no field was found, searches for m_{PropertyName}
                                if (fi == null)
                                    fi = parentType.GetField("m_" + pi.Name, BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                                // If no field was found, searches for <{PropertyName}>k__BackingField (used by the C# 3.0 compiler for syntactic properties)
                                if (fi == null)
                                    fi = parentType.GetField("<" + pi.Name + ">k__BackingField", BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                            }

                            if (fi != null)
                            {
                                bool isEntity = DescriptorHelper.IsEntity(fi.FieldType);

                                if (serializeAsAttribute)
                                    isEntity = false;

                                properties.Add(new PropertyDescriptor(fi.Name, pi.Name, isEntity, DescriptorHelper.IsList(fi.FieldType),
                                    DescriptorHelper.IsGenericList(fi.FieldType), isComposition, fi.FieldType, fi.IsPrivate));
                            }
                            else if (parentType.IsInterface)
                            {
                                bool isEntity = DescriptorHelper.IsEntity(pi.PropertyType);

                                if (serializeAsAttribute)
                                    isEntity = false;

                                properties.Add(new PropertyDescriptor(pi.Name, pi.Name, isEntity, DescriptorHelper.IsList(pi.PropertyType),
                                    DescriptorHelper.IsGenericList(pi.PropertyType), isComposition, pi.PropertyType, false));
                            }
                        }

                    parentType = parentType.BaseType;
                }

                // Removes the Id from the list
                //PropertyDescriptor idDescriptor = GetIdDescriptor_old(targetType);
                //if (idDescriptor != null)
                //{
                //    for(int i=0; i<properties.Count;i++)
                //    {
                //        if (((PropertyDescriptor)properties[i]).PropertyName == idDescriptor.PropertyName)
                //        {
                //            properties.RemoveAt(i);
                //            break;
                //        }
                //    }
                //}

                PropertyDescriptor[] ret = properties.ToArray();
                _CachedDescriptors.Add(targetType, ret);

                return ret;
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }
        }

        public PropertyDescriptor GetIdDescriptor_old(Type targetType)
        {
            _RWL.AcquireReaderLock();

            try
            {
                if (_CachedIDs.ContainsKey(targetType))
                    return (PropertyDescriptor)_CachedIDs[targetType];
            }
            finally
            {
                _RWL.ReleaseReaderLock();
            }

            _RWL.AcquireWriterLock();

            try
            {

                foreach (PropertyInfo pi in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    FieldInfo fi = null;

                    //  Try to get an attribute for this property
                    PersistentIdAttribute[] attrs = pi.GetCustomAttributes(
                        typeof(PersistentIdAttribute), false) as PersistentIdAttribute[];

                    // Add this property if more than one Attribute
                    if (attrs != null && attrs.Length > 0)
                    {
                        // Takes the first attribute even if more are set
                        PersistentIdAttribute pia = attrs[0];

                        if (pia.FieldName != null && pia.FieldName != string.Empty)
                            fi = targetType.GetField(pia.FieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    }

                    if (fi == null)
                    {
                        string propertyname = String.Concat(targetType.Name, "Id");
                        if (pi.Name != propertyname && pi.Name != "Id")
                            continue;
                        else
                            if (pi.Name == "Id")
                                propertyname = "Id";

                        // Searches for a Field with the default id name but a different case
                        fi = targetType.GetField(propertyname, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        // If no field was found, searches for _{TypeName}Id
                        if (fi == null)
                            fi = targetType.GetField("_" + propertyname, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        // If no field was found, searches for m_{TypeName}Id
                        if (fi == null)
                            fi = targetType.GetField("m_" + propertyname, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        // If no field was found, searches for <{PropertyName}>k__BackingField (used by the C# 3.0 compiler for syntactic properties)
                        if (fi == null)
                            fi = targetType.GetField("<" + pi.Name + ">k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    }

                    if (fi != null)
                    {
                        PropertyDescriptor pid = new PropertyDescriptor(fi.Name, pi.Name, DescriptorHelper.IsEntity(fi.FieldType), DescriptorHelper.IsList(fi.FieldType), DescriptorHelper.IsGenericList(fi.FieldType), false, fi.FieldType, fi.IsPrivate);
                        _CachedIDs.Add(targetType, pid);
                        return pid;
                    }
                }
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }

            if (targetType.BaseType != null)
                return GetIdDescriptor_old(targetType.BaseType);

            //If no Id property / _Id field found
            return null;
        }

        public void SetIdDescriptor(Type target, string propertyName, string fieldName, Type propertyType, bool usePublicProperty)
        {
            _RWL.AcquireWriterLock();

            try
            {
                _CachedIDs.Add(target, new PropertyDescriptor(fieldName, propertyName, false, false, false, false, propertyType, usePublicProperty));
            }
            finally
            {
                _RWL.ReleaseWriterLock();
            }
        }
    }
}
