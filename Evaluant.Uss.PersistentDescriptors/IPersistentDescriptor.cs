using System;
using System.Reflection;

namespace Evaluant.Uss.PersistentDescriptors
{
	public interface IPersistentDescriptor
	{
		PropertyDescriptor[] GetPersistentProperties(Type targetType);
        //PropertyDescriptor GetIdDescriptor(Type targetType);
        //void SetIdDescriptor(Type target, string propertyName, string fieldName, Type propertyType, bool usePublicProperty);
	}
}
