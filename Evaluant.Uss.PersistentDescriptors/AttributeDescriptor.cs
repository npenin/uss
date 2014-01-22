using System;
using System.Collections;
using System.Reflection;

namespace Evaluant.Uss.PersistentDescriptors
{
	/// <summary>
	/// Provides an IPersistentDescriptor implementation based on reflection
	/// and PersistentProperty 
	/// </summary>
	public class AttributeDescriptor : IPersistentDescriptor {

		private Hashtable _CachedDescriptors = new Hashtable ();
		private Hashtable _CachedIDs = new Hashtable();

		public PropertyDescriptor [] GetPersistentProperties (Type targetType)
		{
			if (_CachedDescriptors.ContainsKey (targetType))
				return (PropertyDescriptor []) _CachedDescriptors [targetType];

			ArrayList properties = new ArrayList();

			foreach (PropertyInfo pi in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)) {

				if (!DescriptorHelper.IsValidProperty (pi.Name))
					continue;

				PersistentPropertyAttribute [] attrs = pi.GetCustomAttributes (
					typeof (PersistentPropertyAttribute), false) as PersistentPropertyAttribute [];

				// Add this property if more than one Attribute
				if (attrs != null && attrs.Length > 0)
				{
					// Takes the first attribute even if more are set
					PersistentPropertyAttribute ppa = attrs [0];
					properties.Add (new PropertyDescriptor (
						ppa.FieldName, pi.Name, DescriptorHelper.IsEntity (pi.PropertyType), DescriptorHelper.IsList(pi.PropertyType),
						DescriptorHelper.IsGenericList (pi.PropertyType), ppa.Composition, pi.PropertyType));
				}
			}

			// Removes the Id from the list
			PropertyDescriptor idDescriptor = GetIdDescriptor(targetType);
			if(idDescriptor != null && properties.Contains(idDescriptor))
				properties.Remove(idDescriptor);

			PropertyDescriptor [] ret = (PropertyDescriptor []) properties.ToArray (typeof (PropertyDescriptor));
			_CachedDescriptors.Add (targetType, ret);

			return ret;
		}

        public PropertyDescriptor GetIdDescriptor(Type targetType)
        {
			if (_CachedIDs.ContainsKey (targetType))
				return (PropertyDescriptor) _CachedIDs [targetType];
			
			foreach (PropertyInfo pi in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                PersistentIdAttribute[] attrs = pi.GetCustomAttributes(
                    typeof(PersistentIdAttribute), false) as PersistentIdAttribute[];

                if (attrs != null && attrs.Length > 0)
				{
					PersistentIdAttribute pia = attrs[0];
					PropertyDescriptor pid = new PropertyDescriptor(pia.FieldName, pi.Name, DescriptorHelper.IsEntity(pi.PropertyType), DescriptorHelper.IsList(pi.PropertyType), DescriptorHelper.IsGenericList(pi.PropertyType), false, pi.PropertyType);
					_CachedIDs.Add(targetType, pid);

					return pid;
				}
            }

            return null;
        }

        #region IPersistentDescriptor Members


        public void SetIdDescriptor(Type target, string propertyName, string fieldName, Type propertyType, bool usePublicProperty)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
