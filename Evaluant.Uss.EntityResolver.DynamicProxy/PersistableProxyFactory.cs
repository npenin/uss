using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using Evaluant.Uss.ObjectContext.Descriptors;
using Evaluant.Uss.ObjectContext.DynamicProxy;
using Evaluant.Uss.Collections;
using Evaluant.Uss.Models;
using Evaluant.Uss.Common;

namespace Evaluant.Uss.ObjectContext.DynamicProxy
{
	class PersistableProxyFactory : IPersistableProxyFactory
    {
        #region Members

        private Dictionary<Type, Type> _Types;
        private IPersistentDescriptor _Descriptor;
		private object _SynLock = new object();

        #endregion

        #region Ctor

        private Model _Model;

        public PersistableProxyFactory(IPersistentDescriptor descriptor, Model model)
		{
			_Types = new Dictionary<Type, Type> ();
			_Descriptor = descriptor;
            _Model = model;
        }

        #endregion

        #region CreatePersistableProxy

        public IPersistableProxy CreatePersistableProxy(Type parentType)
		{
			lock(_SynLock)
			{
				Type type = GetType(parentType);
				IPersistableProxy proxy = (IPersistableProxy)ActivatorFactory.CreateActivator(type).CreateInstance();
            
				return proxy;
			}
        }

        #endregion

        #region CreatePersistableProperty

        /// <summary>
		/// Generates a property implementation for IPersistable
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="targetType"></param>
		/// <param name="typeBuilder"></param>
		private PropertyBuilder CreatePersistableProperty(string propertyName, Type targetType, TypeBuilder typeBuilder, FieldBuilder entityBldr)
		{
			PropertyBuilder entityPropBldr = typeBuilder.DefineProperty(
				"IPersistable." + propertyName,
				PropertyAttributes.SpecialName,
				targetType,
				new Type [0]);

			// First, we'll define the behavior of the "get" property for Entity as a method.
			MethodBuilder entityGetPropMthdBldr = typeBuilder.DefineMethod (
                "IPersistable.get_" + propertyName,
                MethodAttributes.HideBySig |
                MethodAttributes.NewSlot |
                MethodAttributes.Private |
                MethodAttributes.SpecialName |
                MethodAttributes.Final |
                MethodAttributes.Virtual,
                CallingConventions.Standard | CallingConventions.HasThis,
                targetType,
				new Type [0]);

			ILGenerator entityGetIL = entityGetPropMthdBldr.GetILGenerator();

			entityGetIL.Emit(OpCodes.Ldarg_0);
			entityGetIL.Emit(OpCodes.Ldfld, entityBldr);
			entityGetIL.Emit(OpCodes.Ret);

			// Now, we'll define the behavior of the "set" property for CustomerName.
			MethodBuilder entitySetPropMthdBldr = typeBuilder.DefineMethod (
				"IPersistable.set_" + propertyName,
                MethodAttributes.HideBySig |
                MethodAttributes.NewSlot |
                MethodAttributes.Private |
                MethodAttributes.SpecialName |
                MethodAttributes.Final |
                MethodAttributes.Virtual,
                CallingConventions.Standard | CallingConventions.HasThis,
                typeof(void),
				new Type [] {targetType});

			ILGenerator entitySetIL = entitySetPropMthdBldr.GetILGenerator();

			entitySetIL.Emit(OpCodes.Ldarg_0);
			entitySetIL.Emit(OpCodes.Ldarg_1);
			entitySetIL.Emit(OpCodes.Stfld, entityBldr);
			entitySetIL.Emit(OpCodes.Ret);

			// Last, we must map the two methods created above to our PropertyBuilder to 
			// their corresponding behaviors, "get" and "set" respectively. 
			entityPropBldr.SetGetMethod(entityGetPropMthdBldr);
			entityPropBldr.SetSetMethod(entitySetPropMthdBldr);

			typeBuilder.DefineMethodOverride (
				entityGetPropMthdBldr,
				typeof (IPersistable).GetProperty (propertyName).GetGetMethod ());

			typeBuilder.DefineMethodOverride (
				entitySetPropMthdBldr,
				typeof (IPersistable).GetProperty (propertyName).GetSetMethod ());

			return entityPropBldr;
        }

        #endregion

        #region CreatePersitableField

        FieldBuilder CreatePersistableField (string name, TypeBuilder type, Type fieldType)
		{
			return type.DefineField (
				name,
				fieldType,
				FieldAttributes.Private);
        }

        #endregion

        #region GetFieldInfo

        public FieldInfo GetFieldInfo(Type type, string field)
		{
			return type.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
        }

        public PropertyInfo GetPropertyInfo(Type type, string property)
        {
            return type.GetProperty(property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        }

        public void EmitGet(Type type, PropertyDescriptor descriptor, ILGenerator generator)
        {
            if (descriptor.UsePublicProperty)
            {
                generator.Emit(OpCodes.Call, GetPropertyInfo(type, descriptor.PropertyName).GetGetMethod());
            }
            else
            {
                generator.Emit(OpCodes.Ldfld, GetFieldInfo(type, descriptor.FieldName));
            }
        }

        public void EmitSet(Type type, PropertyDescriptor descriptor, ILGenerator generator)
        {
            if (descriptor.UsePublicProperty)
            {
                MethodInfo method = GetPropertyInfo(type, descriptor.PropertyName).GetSetMethod();
                
                if ( method == null)
                {
                    throw new UniversalStorageException("A property does not have a protected member and its setter can't be defined. Check the property has a public setter or define its member as protected : " + descriptor.PropertyName);
                }

                generator.Emit(OpCodes.Call, method);
            }
            else
            {
                generator.Emit(OpCodes.Stfld, GetFieldInfo(type, descriptor.FieldName));
            }
        }

        #endregion

        #region GetType

        protected virtual Type GetType(Type parentType)
		{
			// Gets the type from the cache or generates it
			if(_Types.ContainsKey(parentType))
				return _Types[parentType];

			// Create an assembly name
			AssemblyName assemblyName = new AssemblyName();
			assemblyName.Name = parentType.FullName;

			// Create a new assembly with one module
			AssemblyBuilder newAssembly = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = newAssembly.DefineDynamicModule(assemblyName.Name);
	
			// Define a public class named "PersistableProxy" in the assembly.
			TypeBuilder typeBuilder = moduleBuilder.DefineType (
				parentType.Name + "Proxy", TypeAttributes.Public, parentType,
				new Type [] {typeof (IPersistable), typeof (IPersistableProxy), typeof(ISerializable)});

            // Add a [Serializable] attribute to the new class
            ConstructorInfo serializableCtorInfo = typeof(SerializableAttribute).GetConstructor(new Type[0]);
            CustomAttributeBuilder serializableCABuilder = new CustomAttributeBuilder(serializableCtorInfo, new object[0]);

            typeBuilder.SetCustomAttribute(serializableCABuilder);

            ConstructorInfo NonSerializableCtorInfo = typeof(NonSerializedAttribute).GetConstructor(new Type[0]);
            CustomAttributeBuilder NonSerializableCABuilder = new CustomAttributeBuilder(NonSerializableCtorInfo, new object[0]);

			Hashtable fieldTable = new Hashtable();

			#region IPersistable implementation

			#region Default constructor
			typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

			FieldBuilder entityBldr = CreatePersistableField ("_Entity", typeBuilder, typeof (Entity));
			CreatePersistableProperty ("Entity", typeof (Entity), typeBuilder, entityBldr);

            // Mark it as [NonSerialized]
            entityBldr.SetCustomAttribute(NonSerializableCABuilder);

			FieldBuilder pmBldr = CreatePersistableField ("_PersistenceManager", typeBuilder, typeof (ObjectContext));
			CreatePersistableProperty ("ObjectContext", typeof (ObjectContext), typeBuilder, pmBldr);

            // Mark it as [NonSerialized]
            pmBldr.SetCustomAttribute(NonSerializableCABuilder);

			#endregion

			#region TrackChildren()

			// void TrackChildren()
			MethodBuilder trackMethod = typeBuilder.DefineMethod (
				"IPersistable.TrackChildren",
                MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.Final |
                MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                typeof(void), 
				new Type [0]);

			ILGenerator trackIL = trackMethod.GetILGenerator();

			Label retLbl = trackIL.DefineLabel();

			trackIL.Emit(OpCodes.Ldarg_0);
			trackIL.Emit(OpCodes.Ldfld, pmBldr);
			trackIL.Emit(OpCodes.Brfalse, retLbl);

			foreach (PropertyDescriptor prop in _Descriptor.GetPersistentProperties (parentType)) 
			{
				if (!prop.IsEntity)
					continue;

				Label skipField = trackIL.DefineLabel ();

				trackIL.Emit (OpCodes.Ldarg_0);
                EmitGet(parentType, prop, trackIL);
				trackIL.Emit (OpCodes.Brfalse, skipField);
				trackIL.Emit (OpCodes.Ldarg_0);
				trackIL.Emit (OpCodes.Ldfld, pmBldr);
				trackIL.Emit (OpCodes.Ldarg_0);
                EmitGet(parentType, prop, trackIL);

				MethodInfo trackObj = null;
				if (prop.IsList) {
					// use the TrackObject (IPersistableCollection) method
					trackIL.Emit (OpCodes.Isinst, typeof (IPersistableCollection));
					trackObj = typeof (ObjectContext).GetMethod (
						"TrackObject",
						new Type [] { typeof (IPersistableCollection) });
				} else if (prop.IsGenericList) {
					// use the TrackObject<T> (IPersistableCollection<T>) method
					Type genArg = prop.Type.GetGenericArguments () [0];
					trackIL.Emit (OpCodes.Isinst, typeof (IPersistableCollection<>).MakeGenericType (genArg));
					foreach (MethodInfo mi in typeof (ObjectContext).GetMethods ()) {
						if (mi.Name != "TrackObject")
							continue;
						ParameterInfo [] parameters = mi.GetParameters ();
						if (parameters.Length == 1 && parameters [0].ParameterType.IsGenericType) {
							trackObj = mi.MakeGenericMethod (genArg);
							break;
						}
					}
				} else {
					trackIL.Emit (OpCodes.Isinst, typeof (IPersistable));
					trackObj = typeof (ObjectContext).GetMethod (
						"TrackObject",
						new Type [] { typeof (IPersistable) });
				}

				if (trackObj == null)
					throw new PersistenceManagerException ("Cannot find the TrackObject method");

				trackIL.Emit (OpCodes.Callvirt, trackObj);

				trackIL.MarkLabel (skipField);
			}

			trackIL.MarkLabel (retLbl);
			trackIL.Emit (OpCodes.Ret);

			typeBuilder.DefineMethodOverride (
				trackMethod,
				typeof (IPersistable).GetMethod ("TrackChildren"));

			#endregion

			#region To-One Lazy loading
			
			foreach(PropertyDescriptor prop in _Descriptor.GetPersistentProperties(parentType))
			{
				if(prop.IsEntity && !prop.IsList && !prop.IsGenericList)
				{
					PropertyInfo parentProp = parentType.GetProperty (prop.PropertyName);
					
					FieldBuilder isFieldLoaded = typeBuilder.DefineField (
						string.Format ("_Is{0}Loaded", prop.PropertyName),
						typeof (bool),
						FieldAttributes.Private);

                    isFieldLoaded.SetCustomAttribute(NonSerializableCABuilder);

					fieldTable.Add(prop.PropertyName, isFieldLoaded);

					MethodBuilder getPropPers = typeBuilder.DefineMethod (
						parentProp.GetGetMethod ().Name,
						MethodAttributes.Public | MethodAttributes.Virtual | 
						MethodAttributes.SpecialName | MethodAttributes.HideBySig |
						MethodAttributes.ReuseSlot,
						prop.Type,
						new Type [0]);

                    /*
                        One to One Lazy Loaded Getter:

                        get {

                            if(!_Is{prop.PropertyName}Loaded && _PersistenceManager != null)
                            {
                                _Is{prop.PropertyName}Loaded = true;
                                IList items = _PersistenceManager.LoadReference(_Entity, "{prop.PropertyName}");
                                if(items.Count > 0)
                                {
                                    base._{prop.FieldName} = ({prop.Type}) items[0];
                                }
                            }

                            return base.get_{prop.PropertyName} ();
                        }
                    */

                    ILGenerator getPropPersIL = getPropPers.GetILGenerator ();
					LocalBuilder varItems = getPropPersIL.DeclareLocal (typeof (IList));
					Label setFieldLoaded = getPropPersIL.DefineLabel ();
                    Label enabledLabel = getPropPersIL.DefineLabel();
                    Label ret = getPropPersIL.DefineLabel();

                    // !_Is{prop.PropertyName}Loaded
					getPropPersIL.Emit (OpCodes.Ldarg_0);
					getPropPersIL.Emit (OpCodes.Ldfld, isFieldLoaded);
					getPropPersIL.Emit (OpCodes.Brtrue, ret); 

                    // _PersistenceManager != null
					getPropPersIL.Emit (OpCodes.Ldarg_0);
					getPropPersIL.Emit (OpCodes.Ldfld, pmBldr);
                    getPropPersIL.Emit (OpCodes.Brfalse, setFieldLoaded);

                    /* Should be useless as only the fields are accessed, and not the properties
                        // _PersistenceManager.EnableLazyLoading
                        getPropPersIL.Emit(OpCodes.Ldarg_0);
                        getPropPersIL.Emit(OpCodes.Ldfld, pmBldr);
                        getPropPersIL.Emit(OpCodes.Callvirt,
                            typeof(ObjectContext).GetProperty("EnableLazyLoading").GetGetMethod());
                        getPropPersIL.Emit(OpCodes.Brfalse, enabledLabel); 
                    */

                    // _Is{prop.PropertyName}Loaded = true;
                    getPropPersIL.Emit(OpCodes.Ldarg_0);
                    getPropPersIL.Emit(OpCodes.Ldc_I4_1);
                    getPropPersIL.Emit(OpCodes.Stfld, isFieldLoaded);

                    // IList items = _PersistenceManager.LoadReference(_Entity, "{prop.PropertyName}");
                    getPropPersIL.Emit(OpCodes.Ldarg_0);
					getPropPersIL.Emit (OpCodes.Ldfld, pmBldr);
					getPropPersIL.Emit (OpCodes.Ldarg_0);
                    getPropPersIL.Emit (OpCodes.Ldfld, entityBldr);
					getPropPersIL.Emit (OpCodes.Ldstr, prop.PropertyName);
					getPropPersIL.Emit (OpCodes.Callvirt,
						typeof (ObjectContext).GetMethod ("LoadReference",
						new Type [] {typeof (Entity), typeof (string)}));
					getPropPersIL.Emit (OpCodes.Stloc, varItems);

                    
                    getPropPersIL.Emit (OpCodes.Ldloc, varItems);
					getPropPersIL.Emit (OpCodes.Callvirt,
						typeof (ICollection).GetProperty ("Count", typeof (int)).GetGetMethod ());
					getPropPersIL.Emit (OpCodes.Brfalse, setFieldLoaded); // abandon if no items
					getPropPersIL.Emit (OpCodes.Ldarg_0);
					getPropPersIL.Emit (OpCodes.Ldloc, varItems);
					getPropPersIL.Emit (OpCodes.Ldc_I4_0);
					getPropPersIL.Emit (OpCodes.Callvirt,
						typeof (IList).GetProperty ("Item", typeof (object)).GetGetMethod ());
					getPropPersIL.Emit (OpCodes.Castclass, prop.Type);
                    EmitSet(parentType, prop, getPropPersIL);

                    getPropPersIL.MarkLabel(setFieldLoaded);
                    getPropPersIL.MarkLabel(enabledLabel);

                    getPropPersIL.MarkLabel(ret);
					getPropPersIL.Emit (OpCodes.Ldarg_0);
					getPropPersIL.Emit (OpCodes.Call, parentProp.GetGetMethod ());
					getPropPersIL.Emit (OpCodes.Ret);

					MethodBuilder setPropPers = typeBuilder.DefineMethod (
						parentProp.GetSetMethod ().Name,
						MethodAttributes.Public | MethodAttributes.Virtual | 
						MethodAttributes.SpecialName | MethodAttributes.HideBySig |
						MethodAttributes.ReuseSlot,
						typeof (void),
						new Type [] {prop.Type});

					/*
						One to One Lazy Loaded Setter:

						set {

							if (this.{prop.PropertyName} != null)
							{
								_Entity.DeleteElement("{prop.PropertyName}");
							}

							if (value != null)
							{
								_Entity.AddValue("{prop.PropertyName}", _PersistenceManager.ExtractEntity(value));
							}
					 
							_Is[PropertyName]Loaded = true;
							base.set_{prop.PropertyName} (value);
						}
					*/ 

					ILGenerator setPropPersIL = setPropPers.GetILGenerator ();
					Label isPersistableLabel = setPropPersIL.DefineLabel();
					Label isParamNull = setPropPersIL.DefineLabel ();
					Label setValue = setPropPersIL.DefineLabel ();

					// if (this.{prop.PropertyName} != null) {
					setPropPersIL.Emit (OpCodes.Ldarg_0);
					setPropPersIL.Emit (OpCodes.Callvirt, getPropPers);
					setPropPersIL.Emit (OpCodes.Brfalse_S, isParamNull);
					setPropPersIL.Emit (OpCodes.Ldarg_0);
					setPropPersIL.Emit (OpCodes.Ldfld, entityBldr);
					setPropPersIL.Emit (OpCodes.Ldstr, prop.PropertyName);
					setPropPersIL.Emit (OpCodes.Callvirt,
						typeof (Entity).GetMethod ("DeleteElement",
						new Type [] {typeof (string)}));
					setPropPersIL.MarkLabel (isParamNull);
					setPropPersIL.Emit (OpCodes.Ldarg_1);
					setPropPersIL.Emit (OpCodes.Brfalse_S, setValue);
					setPropPersIL.Emit (OpCodes.Ldarg_0);
					setPropPersIL.Emit (OpCodes.Ldfld, entityBldr);
					setPropPersIL.Emit (OpCodes.Ldstr, prop.PropertyName);
					setPropPersIL.Emit(OpCodes.Ldarg_0);
					setPropPersIL.Emit(OpCodes.Ldfld, pmBldr);
					setPropPersIL.Emit (OpCodes.Ldarg_1);
					setPropPersIL.Emit(OpCodes.Callvirt, 
						typeof(ObjectContext).GetMethod("ExtractEntity"));
					setPropPersIL.Emit (OpCodes.Callvirt,
						typeof (Entity).GetMethod ("AddValue",
						new Type [] {typeof (string), typeof (Entity)}));
					setPropPersIL.MarkLabel (setValue);
					setPropPersIL.MarkLabel(isPersistableLabel);

					// _Is[PropertyName]Loaded = true;
					setPropPersIL.Emit(OpCodes.Ldarg_0);
					setPropPersIL.Emit(OpCodes.Ldc_I4_1);
					setPropPersIL.Emit(OpCodes.Stfld, isFieldLoaded);
						
					// base.set_{prop.PropertyName} (value);
					setPropPersIL.Emit(OpCodes.Ldarg_0);
					setPropPersIL.Emit (OpCodes.Ldarg_1);
					setPropPersIL.Emit(OpCodes.Call, parentProp.GetSetMethod());
					setPropPersIL.Emit (OpCodes.Ret);
				}
			}

			#endregion

			#endregion

			#region IPersistableProxy implementation

			#region Set()

            /*
			 *  bool _Processing;
			 *	Set()
			 *	{
			 *		// Prevent cyclic references
			 *		if(_Processing) 
			 *			return;
			 *		
			 *		_Processing = true;
			 *		object value = _Entity.GetValue("Name");
			 *		
			 *		// beginloop: for each property
			 * 
			 *		[
			 *		// if !IsEntity
			 *		if(value != null)
			 *			_name = (string)value;
			 *		]
			 *		
			 *		// endloop
			 * 
			 *		_Processing = false;
			 * }
			 * 
			 * */

            MethodBuilder setMethod = 
				typeBuilder.DefineMethod("IPersistableProxy.Set",
                MethodAttributes.HideBySig |
                MethodAttributes.NewSlot |
                MethodAttributes.Private |
                MethodAttributes.SpecialName |
                MethodAttributes.Final |
                MethodAttributes.Virtual,
                null, 
				new Type[0]);

			// From the method, get an ILGenerator. This is used to
			// emit the IL that we want
			ILGenerator setIL = setMethod.GetILGenerator();

			MethodInfo getValueMethod = typeof(Entity).GetMethod("GetValue", new Type[] { typeof(string) } );

			if(getValueMethod == null)
				throw new PersistenceManagerException("Cannot find the Entity.GetValue method");

			// Cyclic reference
			
			FieldBuilder processingField = typeBuilder.DefineField("_Processing", typeof(bool), FieldAttributes.Private);

            processingField.SetCustomAttribute(NonSerializableCABuilder);

			/*
			 *		if(_Processing) 
			 *			return;
			*/

			setIL.Emit(OpCodes.Ldarg_0);
			setIL.Emit(OpCodes.Ldfld, processingField);
			Label processingLabel = setIL.DefineLabel();
			setIL.Emit(OpCodes.Brfalse, processingLabel);
			setIL.Emit(OpCodes.Ret);
			setIL.MarkLabel(processingLabel);

			/*
			 * _Processing = true;
			*/

			setIL.Emit (OpCodes.Ldarg_0);
			setIL.Emit (OpCodes.Ldc_I4_1);
			setIL.Emit (OpCodes.Stfld, processingField);

			// object value;

			LocalBuilder valueBldr = setIL.DeclareLocal(typeof(object));

            MethodInfo getId = typeof(Entity).GetProperty("Id").GetGetMethod();

            PropertyDescriptor idPropertyDesc = _Descriptor.GetIdDescriptor(parentType);
            if (idPropertyDesc != null)
            {
                /*
                 * 
                 * base._Id = _Entity.Id;
                 * 
                 or
                 * 
                 * base._Id = int.Parse(_Entity.Id);
                 * 
                 */

                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldarg_0);
                setIL.Emit(OpCodes.Ldfld, entityBldr);
                setIL.EmitCall(OpCodes.Callvirt, getId, null);

                if (idPropertyDesc.Type == typeof(Int32))
                {
                    // int
                    MethodInfo parseIntMethod = typeof(Int32).GetMethod("Parse", new Type[] { typeof(string) });
                    setIL.EmitCall(OpCodes.Call, parseIntMethod, null);
                }
                else
                {
                    // string
                    setIL.Emit(OpCodes.Castclass, typeof(string));
                }

                EmitSet(parentType, idPropertyDesc, setIL);
                
            }

            foreach (PropertyDescriptor descriptor in _Descriptor.GetPersistentProperties(parentType))
            {

                if (!descriptor.IsEntity)
                {

                    /*
                     *	value = _Entity.GetValue("Name");
                     * 
                     * */

                    setIL.Emit(OpCodes.Ldarg_0);
                    setIL.Emit(OpCodes.Ldfld, entityBldr);
                    setIL.Emit(OpCodes.Ldstr, descriptor.PropertyName);
                    setIL.EmitCall(OpCodes.Callvirt, getValueMethod, null);

                    setIL.Emit(OpCodes.Stloc_S, valueBldr);

                    //  If value is not an Enumeration
                    /*
                     *	if(value != null)
                     *		_name = (string)value;
                     * */

                    //  If value is an Enumeration
                    /*
                     *  if(value != null)
                     *      _name = (EnumType)Enum.Parse(typeof(EnumType), (string)value);
                     * */

                    setIL.Emit(OpCodes.Ldloc, valueBldr);
                    Label valueLabel = setIL.DefineLabel();
                    setIL.Emit(OpCodes.Brfalse, valueLabel);

                    if (descriptor.Type.IsValueType)
                    {
                        if (DescriptorHelper.OpCode[descriptor.Type] != null) // Primitive type
                        {
                            setIL.Emit(OpCodes.Ldarg_0);
                            setIL.Emit(OpCodes.Ldloc, valueBldr);
                            setIL.Emit(OpCodes.Unbox, descriptor.Type);
                            setIL.Emit((OpCode)DescriptorHelper.OpCode[descriptor.Type]);
                        }
                        else // Not a primitive type (e.g: DateTime)
                        {
                            if (descriptor.Type.IsEnum)
                            {
                                setIL.Emit(OpCodes.Ldarg_0);
                                setIL.Emit(OpCodes.Ldtoken, descriptor.Type);
                                MethodInfo mi = typeof(Type).GetMethod("GetTypeFromHandle");
                                setIL.EmitCall(OpCodes.Call, mi, null);
                                setIL.Emit(OpCodes.Ldloc, valueBldr);

                                //  Always convert the value from the database (int or string) to string
                                //  and call Convert.ToString(object o)
                                mi = typeof(Convert).GetMethod("ToString", new Type[] { typeof(object) });
                                setIL.EmitCall(OpCodes.Call, mi, null);

                                mi = typeof(Enum).GetMethod("Parse", new Type[] { typeof(Type), typeof(string) });
                                setIL.EmitCall(OpCodes.Call, mi, null);
                                setIL.Emit(OpCodes.Unbox_Any, descriptor.Type);
                            }
                            else
                            {
                                setIL.Emit(OpCodes.Ldarg_0);
                                setIL.Emit(OpCodes.Ldloc, valueBldr);
                                setIL.Emit(OpCodes.Unbox, descriptor.Type);
                                setIL.Emit(OpCodes.Ldobj, descriptor.Type);
                            }
                        }
                    }
                    else
                    {
                        setIL.Emit(OpCodes.Ldarg_0);
                        setIL.Emit(OpCodes.Ldloc, valueBldr);
                        setIL.Emit(OpCodes.Castclass, descriptor.Type);
                    }

                    EmitSet(parentType, descriptor, setIL);

                    setIL.MarkLabel(valueLabel);
                }
            }

			// _Processing = false;

			setIL.Emit (OpCodes.Ldarg_0);
			setIL.Emit (OpCodes.Ldc_I4_0);
			setIL.Emit (OpCodes.Stfld, processingField);

			setIL.Emit(OpCodes.Ret);

			typeBuilder.DefineMethodOverride (
				setMethod,
				typeof(IPersistableProxy).GetMethod("Set"));

			#endregion

            #region SetReferences()

            /*
			 *  bool _Processing;
			 *	Set()
			 *	{
			 *		// Prevent cyclic references
			 *		if(_Processing) 
			 *			return;
			 *		
			 *		_Processing = true;
			 *		
			 *		[
			 *		// IsEntity then IsList ?
			 *		/// TODO : cast "value" as EntitySet and give it to the constructor for lazy initialization
			 *		_partners = new IPersistableCollection( _PersistenceManager, this, prop.PropertyName, null));
			 *		] 
			 * 
			 *		[
			 *		// else (to-one)
			 * 		if(value != null)
			 * 		{
			 *			IPersistableProxy proxy = _PersistenceManager.Factory.CreatePersistableProxy(prop.Type);
			 *			
			 *			proxy.Entity = (Entity)value;
			 *			proxy.ObjectContext = _PersistenceManager; // Should be done in TrackChildren while processing children
			 *			proxy.Set();
			 *			_IsAddressLoaded = true; 
			 *  
			 *			_address = (Address)proxy;              
			 *		}
			 *		else if(_Entity.InferredReferences.Contains("Name")
			 *		{
			 *			_IsAddressLoaded = true;
			 *			_address = null;
			 *		}             * 
			 *		]
			 *	
			 *		// endloop
			 * 
			 *		_Processing = false;
			 * }
			 * 
			 * 
			 * */

            setMethod =
                typeBuilder.DefineMethod("IPersistableProxy.SetReferences",
                MethodAttributes.HideBySig |
                MethodAttributes.NewSlot |
                MethodAttributes.Private |
                MethodAttributes.SpecialName |
                MethodAttributes.Final |
                MethodAttributes.Virtual,
                null,
                new Type[0]);

            // From the method, get an ILGenerator. This is used to
            // emit the IL that we want
            setIL = setMethod.GetILGenerator();

            // Cyclic reference

            processingField = typeBuilder.DefineField("_Processing", typeof(bool), FieldAttributes.Private);

            processingField.SetCustomAttribute(NonSerializableCABuilder);

            /*
             *		if(_Processing) 
             *			return;
            */

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldfld, processingField);
            processingLabel = setIL.DefineLabel();
            setIL.Emit(OpCodes.Brfalse, processingLabel);
            setIL.Emit(OpCodes.Ret);
            setIL.MarkLabel(processingLabel);

            /*
             * _Processing = true;
            */

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldc_I4_1);
            setIL.Emit(OpCodes.Stfld, processingField);

            // object value
            valueBldr = setIL.DeclareLocal(typeof(object));

            // object proxy;
            LocalBuilder proxyBldr = setIL.DeclareLocal(typeof(IPersistableProxy));
            
            foreach (PropertyDescriptor descriptor in _Descriptor.GetPersistentProperties(parentType))
            {

                if (descriptor.IsEntity)
                {
                    /*
                     *	value = _Entity.GetValue("Name");
                     * 
                     * */

                    setIL.Emit(OpCodes.Ldarg_0);
                    setIL.Emit(OpCodes.Ldfld, entityBldr);
                    setIL.Emit(OpCodes.Ldstr, descriptor.PropertyName);
                    setIL.EmitCall(OpCodes.Callvirt, getValueMethod, null);

                    setIL.Emit(OpCodes.Stloc_S, valueBldr);

                    if (descriptor.IsList || descriptor.IsGenericList)
                    {

                        ConstructorInfo iPersistableCollectionCtor = null;
                        Type expectedType = null;
                        if (descriptor.IsGenericList)
                        {
                            Type genArgType = descriptor.Type.GetGenericArguments()[0];
                            Type genType = typeof(IPersistableCollection<>).MakeGenericType(genArgType);
                            foreach (ConstructorInfo ci in genType.GetConstructors())
                            {
                                if (ci.GetParameters().Length > 1)
                                {
                                    iPersistableCollectionCtor = ci;
                                    break;
                                }
                            }

                            expectedType = typeof(IList<>).MakeGenericType(genArgType);
                        }
                        else
                        {
                            iPersistableCollectionCtor = typeof(IPersistableCollection).GetConstructor(new Type[] { typeof(ObjectContext), typeof(IPersistable), typeof(string), typeof(IEnumerable) });
                            expectedType = typeof(IList);
                        }

                        if (iPersistableCollectionCtor == null)
                            throw new PersistenceManagerException("Cannot find IPersistableCollection.Ctor Method");

                        /*
                         *	_partners = new IPersistableCollection( _PersistenceManager, this, prop.PropertyName, null));
                         * 
                         * */

                        setIL.Emit(OpCodes.Ldarg_0);
                        setIL.Emit(OpCodes.Ldarg_0);
                        setIL.Emit(OpCodes.Ldfld, pmBldr); // _PersistenceManager
                        setIL.Emit(OpCodes.Ldarg_0); // this
                        setIL.Emit(OpCodes.Ldstr, descriptor.PropertyName);
                        setIL.Emit(OpCodes.Ldnull);
                        setIL.Emit(OpCodes.Newobj, iPersistableCollectionCtor);
                        setIL.Emit(OpCodes.Isinst, expectedType);

                        EmitSet(parentType, descriptor, setIL);
                    }
                    else
                    {
                        // if(value != null)

                        Label oneLabel = setIL.DefineLabel();
                        Label elseStatementLabel = setIL.DefineLabel();

                        setIL.Emit(OpCodes.Ldloc_S, valueBldr);
                        setIL.Emit(OpCodes.Brfalse, elseStatementLabel); // {

                        setIL.Emit(OpCodes.Ldarg_0);

                        setIL.Emit(OpCodes.Ldfld, (FieldBuilder)fieldTable[descriptor.PropertyName]);
                        setIL.Emit(OpCodes.Brtrue, elseStatementLabel); // {

                        // proxy = _ObjectContext.Factory.PersistableProxyFactory.CreatePersistableProxy(prop.Type);

                        setIL.Emit(OpCodes.Ldarg_0);
                        setIL.Emit(OpCodes.Ldfld, pmBldr);
                        setIL.EmitCall(OpCodes.Callvirt, typeof(ObjectContext).GetProperty("Factory").GetGetMethod(), null);
                        setIL.EmitCall(OpCodes.Callvirt, typeof(ObjectService).GetProperty("PersistableProxyFactory").GetGetMethod(), null);
                        setIL.Emit(OpCodes.Ldtoken, descriptor.Type);
                        setIL.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) }));
                        setIL.EmitCall(OpCodes.Callvirt, typeof(IPersistableProxyFactory).GetMethod("CreatePersistableProxy"), null);

                        setIL.Emit(OpCodes.Stloc_S, proxyBldr);

                        // proxy.Entity = (Entity)value;

                        setIL.Emit(OpCodes.Ldloc_S, proxyBldr);

                        setIL.Emit(OpCodes.Ldloc_S, valueBldr);
                        setIL.Emit(OpCodes.Castclass, typeof(Entity));

                        setIL.EmitCall(OpCodes.Callvirt, typeof(IPersistable).GetProperty("Entity").GetSetMethod(), null);

                        // proxy.ObjectContext = _ObjectContext;

                        setIL.Emit(OpCodes.Ldloc_S, proxyBldr);

                        setIL.Emit(OpCodes.Ldarg_0);
                        setIL.Emit(OpCodes.Ldfld, pmBldr);

                        setIL.EmitCall(OpCodes.Callvirt, typeof(IPersistable).GetProperty("ObjectContext").GetSetMethod(), null);

                        // proxy.Set();

                        setIL.Emit(OpCodes.Ldloc_S, proxyBldr);
                        setIL.EmitCall(OpCodes.Callvirt, typeof(IPersistableProxy).GetMethod("Set"), null);

                        // proxy.SetReferences();

                        setIL.Emit(OpCodes.Ldloc_S, proxyBldr);
                        setIL.EmitCall(OpCodes.Callvirt, typeof(IPersistableProxy).GetMethod("SetReferences"), null);

                        // _IsAddressLoaded = true;      

                        setIL.Emit(OpCodes.Ldarg_0);
                        setIL.Emit(OpCodes.Ldc_I4_1);
                        setIL.Emit(OpCodes.Stfld, (FieldBuilder)fieldTable[descriptor.PropertyName]);

                        // _address = (Address)proxy;

                        setIL.Emit(OpCodes.Ldarg_0);
                        setIL.Emit(OpCodes.Ldloc_S, proxyBldr);
                        setIL.Emit(OpCodes.Castclass, descriptor.Type);

                        EmitSet(parentType, descriptor, setIL);

                        setIL.Emit(OpCodes.Br, oneLabel);

                        /*
                        *		else if(_Entity.InferredReferences.Contains("Name")
                        *		{
                        *			_IsAddressLoaded = true;
                        *			_address = null;
                        *		}
                        */

                        setIL.MarkLabel(elseStatementLabel);

                        //_Entity.InferredReferences.Contains("Name")
                        setIL.Emit(OpCodes.Ldarg_0);
                        setIL.Emit(OpCodes.Ldfld, entityBldr);
                        setIL.EmitCall(OpCodes.Callvirt, typeof(Entity).GetProperty("InferredReferences").GetGetMethod(), null);
                        setIL.Emit(OpCodes.Ldstr, descriptor.PropertyName);
                        setIL.EmitCall(OpCodes.Callvirt, typeof(System.Collections.Specialized.StringCollection).GetMethod("Contains", new Type[] { typeof(string) }), null);
                        setIL.Emit(OpCodes.Brfalse, oneLabel);

                        // _IsAddressLoaded = true;      
                        setIL.Emit(OpCodes.Ldarg_0);
                        setIL.Emit(OpCodes.Ldc_I4_1);
                        setIL.Emit(OpCodes.Stfld, (FieldBuilder)fieldTable[descriptor.PropertyName]);

                        setIL.MarkLabel(oneLabel); // }
                    }
                }
            }

            // _Processing = false;

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldc_I4_0);
            setIL.Emit(OpCodes.Stfld, processingField);

            setIL.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(
                setMethod,
                typeof(IPersistableProxy).GetMethod("SetReferences"));

            #endregion


            #region Update()

            /*
			 *	// Prevent cyclic references
			 * 		
			 *	if(_Processing) 
			 *		return;
			 *
			 *	_Processing = true;
			 * 
			 *  object value;
			 * 
			 *  // beginloop: foreach property descriptor
			 * 
			 *  [if prop.IsEntity]
			 *		[if prop.IsList]
			 *			_PersistenceManager.UpdateEntitySet(this, {PropertyName}, _partners);
			 *		[else]
			 *			if(_address is IPersistable)
			 *			{
			 *				if(_address == null)
			 *				{
			 *					value = _Entity.GetValue({PropertyName});
			 *					if(value != null)
			 *						_Entity.DeleteElement({PropertyName});
			 *				}
			 *				else
			 *					_address.Update();
			 *			}
			 *	[else]
             *        [if prop.Type == typeof(string) || !prop.Type.IsValueType]
             *            if(_Entity.FindEntry(prop.PropertyName) != null)
             *             {   
             *                 if(_name != null)
             *                     _Entity.SetValue(prop.PropertyName, _name);
             *                 else
             *                     _Entity.DeleteElement(prop.PropertyName);
             *             }
             *             else
             *             {
             *                 if(_name != null)
             *                     _Entity.AddValue(prop.PropertyName, _name);
             *             }
             *        [else]
             *             _Entity.SetValue(prop.PropertyName, _name);
			 * 
			 *  [endloop]
			 * 
			 *	_Processing = false;
			 * 
			 * */

            MethodBuilder updateMethod =
                typeBuilder.DefineMethod("IPersistableProxy.Update",
                MethodAttributes.HideBySig |
                MethodAttributes.NewSlot |
                MethodAttributes.Private |
                MethodAttributes.SpecialName |
                MethodAttributes.Final |
                MethodAttributes.Virtual,
                null, 
				new Type[0]);

			ILGenerator updateIL = updateMethod.GetILGenerator();

			/*
			 *		if(_Processing) 
			 *			return;
			*/

			updateIL.Emit(OpCodes.Ldarg_0);
			updateIL.Emit(OpCodes.Ldfld, processingField);
			processingLabel = updateIL.DefineLabel();
			updateIL.Emit(OpCodes.Brfalse, processingLabel);
			updateIL.Emit(OpCodes.Ret);
			updateIL.MarkLabel(processingLabel);

			/*
			 * _Processing = true;
			*/

			updateIL.Emit (OpCodes.Ldarg_0);
			updateIL.Emit (OpCodes.Ldc_I4_1);
			updateIL.Emit (OpCodes.Stfld, processingField);

			// object value;

			valueBldr = updateIL.DeclareLocal(typeof(object));

			foreach(PropertyDescriptor prop in _Descriptor.GetPersistentProperties(parentType))
			{

				if(prop.IsEntity)
				{
					if(prop.IsList || prop.IsGenericList)
					{
						
						// this._PersistenceManager.UpdateEntitySet(this, {PropertyName}, _partners);

						MethodInfo uesInfo = typeof(ObjectContext).GetMethod("UpdateEntitySet");

						updateIL.Emit(OpCodes.Ldarg_0);
						updateIL.Emit(OpCodes.Ldfld, pmBldr);

						updateIL.Emit(OpCodes.Ldarg_0);
						updateIL.Emit(OpCodes.Ldstr, prop.PropertyName);
						updateIL.Emit(OpCodes.Ldarg_0);

                        EmitGet(parentType, prop, updateIL);

						updateIL.Emit(OpCodes.Callvirt, uesInfo);
					}
					else
					{
						// if(_address is IPersistable)

						Label isPersistableLabel = updateIL.DefineLabel();

						updateIL.Emit(OpCodes.Ldarg_0);

                        EmitGet(parentType, prop, updateIL);

						updateIL.Emit(OpCodes.Isinst, typeof(IPersistable));
						updateIL.Emit(OpCodes.Brfalse_S, isPersistableLabel);

						// if(_address == null)

						updateIL.Emit(OpCodes.Ldarg_0);

                        EmitGet(parentType, prop, updateIL);

						Label fieldNullLabel = updateIL.DefineLabel();
						updateIL.Emit(OpCodes.Brtrue, fieldNullLabel);
							
							// value = _Entity.GetValue("Name");

							updateIL.Emit(OpCodes.Ldarg_0);
							updateIL.Emit(OpCodes.Ldfld, entityBldr);
							updateIL.Emit(OpCodes.Ldstr, prop.PropertyName);
							updateIL.EmitCall(OpCodes.Callvirt, getValueMethod, null);

							updateIL.Emit(OpCodes.Stloc_S, valueBldr);

							// if(value != null)

							updateIL.Emit(OpCodes.Ldloc_S, valueBldr);
							Label valueNullLabel = updateIL.DefineLabel();
							updateIL.Emit(OpCodes.Brfalse, valueNullLabel);
								
							// _Entity.DeleteElement({PropertyName});
							
							updateIL.Emit(OpCodes.Ldarg_0);
							updateIL.Emit(OpCodes.Ldfld, entityBldr);
							updateIL.Emit(OpCodes.Ldstr, prop.PropertyName);
							updateIL.EmitCall(OpCodes.Callvirt, typeof(Entity).GetMethod("DeleteElement", new Type[] { typeof(string) }), null);
							
							updateIL.MarkLabel(valueNullLabel);

						Label valueNullElseLabel = updateIL.DefineLabel();
						updateIL.Emit(OpCodes.Br_S, valueNullElseLabel);
						updateIL.MarkLabel(fieldNullLabel);

						// else
						
						updateIL.Emit(OpCodes.Ldarg_0);

                        EmitGet(parentType, prop, updateIL);

						updateIL.Emit(OpCodes.Isinst, typeof(IPersistableProxy));
						updateIL.EmitCall(OpCodes.Callvirt, typeof(IPersistableProxy).GetMethod("Update"), null);

						updateIL.MarkLabel(valueNullElseLabel);
						updateIL.MarkLabel(isPersistableLabel);
					}
				}
				else
				{
                    // [if prop.Type == typeof(string)]
                    //      if(_Entity.FindEntry(prop.PropertyName) != null)
                    //      {   
                    //          if(_name != null)
                    //              _Entity.SetValue(prop.PropertyName, _name);
                    //          else
                    //              _Entity.DeleteElement(prop.PropertyName);
                    //      }
                    //      else
                    //      {
                    //          if(_name != null)
                    //              _Entity.AddValue(prop.PropertyName, _name);
                    //      }
                    // [else]
                    //      _Entity.SetValue(prop.PropertyName, _name);


                    Label lblSetValue = updateIL.DefineLabel();
                    Label lblAfterSetValue = updateIL.DefineLabel();
                    Label lblDeleteElement = updateIL.DefineLabel();
                    Label lblAddValue = updateIL.DefineLabel();
                    Label lblTestAddValue = updateIL.DefineLabel();
                    
                    if (prop.Type == typeof(string) || !prop.Type.IsValueType)
                    {
                        //if (_Entity.FindEntry(prop.PropertyName) != null)
                        updateIL.Emit(OpCodes.Ldarg_0);
                        updateIL.Emit(OpCodes.Ldfld, entityBldr);
                        updateIL.Emit(OpCodes.Ldstr, prop.PropertyName);
                        updateIL.EmitCall(OpCodes.Callvirt, typeof(Entity).GetMethod("FindEntry", new Type[] { typeof(string) }), null);
                        updateIL.Emit(OpCodes.Brfalse, lblTestAddValue);

                        // if(_name != null)
                        updateIL.Emit(OpCodes.Ldarg_0);

                        EmitGet(parentType, prop, updateIL);

                        updateIL.Emit(OpCodes.Brtrue, lblSetValue);

                        //_Entity.DeleteElement(prop.PropertyName);
                        updateIL.Emit(OpCodes.Ldarg_0);
                        updateIL.Emit(OpCodes.Ldfld, entityBldr);
                        updateIL.Emit(OpCodes.Ldstr, prop.PropertyName);
                        updateIL.EmitCall(OpCodes.Callvirt, typeof(Entity).GetMethod("DeleteElement", new Type[] { typeof(string) }), null);                        
                        updateIL.Emit(OpCodes.Br, lblAfterSetValue);

                        updateIL.MarkLabel(lblSetValue);

                        // _Entity.SetValue(prop.PropertyName, _name);
                        updateIL.Emit(OpCodes.Ldarg_0);
                        updateIL.Emit(OpCodes.Ldfld, entityBldr);
                        updateIL.Emit(OpCodes.Ldstr, prop.PropertyName);
                        updateIL.Emit(OpCodes.Ldarg_0);

                        EmitGet(parentType, prop, updateIL);

                        updateIL.EmitCall(OpCodes.Callvirt, typeof(Entity).GetMethod("SetValue", new Type[] { typeof(string), typeof(object) }), null);
                        updateIL.Emit(OpCodes.Br, lblAfterSetValue);


                        updateIL.MarkLabel(lblTestAddValue);

                        // if(_name != null)
                        updateIL.Emit(OpCodes.Ldarg_0);

                        EmitGet(parentType, prop, updateIL);

                        updateIL.Emit(OpCodes.Brfalse, lblAfterSetValue);

                        //_Entity.AddValue(prop.PropertyName, _name);
                        updateIL.Emit(OpCodes.Ldarg_0);
                        updateIL.Emit(OpCodes.Ldfld, entityBldr);
                        updateIL.Emit(OpCodes.Ldstr, prop.PropertyName);
                        updateIL.Emit(OpCodes.Ldarg_0);

                        EmitGet(parentType, prop, updateIL);

                        updateIL.EmitCall(OpCodes.Callvirt, typeof(Entity).GetMethod("AddValue", new Type[] { typeof(string), typeof(object) }), null);

                        updateIL.MarkLabel(lblAfterSetValue);
                    }
                    else
                    {
                        if (prop.Type.IsEnum)
                        {
                            updateIL.Emit(OpCodes.Ldarg_0);
                            updateIL.Emit(OpCodes.Ldfld, entityBldr);
                            updateIL.Emit(OpCodes.Ldstr, prop.PropertyName);

                            Evaluant.Uss.Models.Attribute attr = _Model.GetAttribute(ObjectContext.ConvertNamespaceDomainToEuss(parentType), prop.PropertyName);

                            if (attr.Type == typeof(string))
                            {
                                updateIL.Emit(OpCodes.Ldtoken, prop.Type);
                                MethodInfo mi = typeof(Type).GetMethod("GetTypeFromHandle");
                                updateIL.EmitCall(OpCodes.Call, mi, null);
                                updateIL.Emit(OpCodes.Ldarg_0);

                                EmitGet(parentType, prop, updateIL);

                                updateIL.Emit(OpCodes.Box, prop.Type);
                                mi = typeof(Enum).GetMethod("GetName");
                                updateIL.EmitCall(OpCodes.Call, mi, null);
                            }
                            else
                            {
                                updateIL.Emit(OpCodes.Ldarg_0);

                                EmitGet(parentType, prop, updateIL);

                                updateIL.Emit(OpCodes.Box, typeof(int));
                            }

                            updateIL.EmitCall(OpCodes.Callvirt, typeof(Entity).GetMethod("SetValue", new Type[] { typeof(string), typeof(object) }), null);
                        }
                        else
                        {
                            // _Entity.SetValue(prop.PropertyName, _name);
                            updateIL.Emit(OpCodes.Ldarg_0);
                            updateIL.Emit(OpCodes.Ldfld, entityBldr);
                            updateIL.Emit(OpCodes.Ldstr, prop.PropertyName);
                            updateIL.Emit(OpCodes.Ldarg_0);

                            EmitGet(parentType, prop, updateIL);

                            if (prop.Type.IsValueType)
                                updateIL.Emit(OpCodes.Box, prop.Type);

                            updateIL.EmitCall(OpCodes.Callvirt, typeof(Entity).GetMethod("SetValue", new Type[] { typeof(string), typeof(object) }), null);
                        }
                    }
				}
			}

			// _Processing = false;

			updateIL.Emit (OpCodes.Ldarg_0);
			updateIL.Emit (OpCodes.Ldc_I4_0);
			updateIL.Emit (OpCodes.Stfld, processingField);

			updateIL.Emit(OpCodes.Ret);

			typeBuilder.DefineMethodOverride (
				updateMethod,
				typeof(IPersistableProxy).GetMethod("Update"));

			#endregion

			#endregion

            #region ISerializable

            #region GetObjectData()

            /*
			 * 			
			 *  info.SetType(parentType);
			 * 
			 *  // beginloop: foreach property descriptor
			 * 
			 *  info.AddValue( {Field} );
			 * 
			 *  [endloop]
			 * 
			 * */

            MethodBuilder getObjectDataMethod =
                typeBuilder.DefineMethod("ISerializable.GetObjectData",
                MethodAttributes.HideBySig |
                MethodAttributes.NewSlot |
                MethodAttributes.Private |
                MethodAttributes.SpecialName |
                MethodAttributes.Final |
                MethodAttributes.Virtual,
                null,
                new Type[] { typeof(SerializationInfo), typeof(StreamingContext) });

            ILGenerator getObjectDataIL = getObjectDataMethod.GetILGenerator();

            // info.SetType(parentType);

            //			getObjectDataIL.Emit(OpCodes.Ldarg_1);
            //			getObjectDataIL.Emit(OpCodes.Ldtoken, parentType);
            //			getObjectDataIL.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null);
            //			getObjectDataIL.EmitCall(OpCodes.Callvirt, typeof(SerializationInfo).GetMethod("SetType"), null);
            //
            //			foreach(PropertyDescriptor prop in _Descriptor.GetPersistentProperties(parentType))
            //			{
            //				// info.AddValue( {Field} );
            //
            //				getObjectDataIL.Emit(OpCodes.Ldarg_1);
            //				getObjectDataIL.Emit(OpCodes.Ldstr, prop.FieldName);
            //				getObjectDataIL.Emit(OpCodes.Ldarg_0);
            //				getObjectDataIL.Emit(OpCodes.Ldfld, GetFieldInfo(parentType, prop.FieldName));
            //				getObjectDataIL.EmitCall(OpCodes.Callvirt, typeof(SerializationInfo).GetMethod("AddValue", new Type[] { typeof(string), typeof(object) }), null);
            //			}

            Label lbl1 = getObjectDataIL.DefineLabel();
            Label lbl2 = getObjectDataIL.DefineLabel();

            LocalBuilder memberInfoBldr = getObjectDataIL.DeclareLocal(typeof(System.Reflection.MemberInfo[]));
            LocalBuilder objeArrayBldr = getObjectDataIL.DeclareLocal(typeof(object[]));
            LocalBuilder iBldr = getObjectDataIL.DeclareLocal(typeof(int));

            getObjectDataIL.Emit(OpCodes.Ldtoken, parentType);
            getObjectDataIL.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null);
            getObjectDataIL.EmitCall(OpCodes.Call, typeof(FormatterServices).GetMethod("GetSerializableMembers", new Type[] { typeof(Type) }), null);

            getObjectDataIL.Emit(OpCodes.Stloc_0);
            getObjectDataIL.Emit(OpCodes.Ldarg_0);
            getObjectDataIL.Emit(OpCodes.Ldloc_0);
            getObjectDataIL.EmitCall(OpCodes.Call, typeof(FormatterServices).GetMethod("GetObjectData"), null);

            getObjectDataIL.Emit(OpCodes.Stloc_1);
            getObjectDataIL.Emit(OpCodes.Ldc_I4_0);
            getObjectDataIL.Emit(OpCodes.Stloc_2);
            getObjectDataIL.Emit(OpCodes.Br_S, lbl2);

            getObjectDataIL.MarkLabel(lbl1);

            getObjectDataIL.Emit(OpCodes.Ldarg_1);
            getObjectDataIL.Emit(OpCodes.Ldloc_0);
            getObjectDataIL.Emit(OpCodes.Ldloc_2);
            getObjectDataIL.Emit(OpCodes.Ldelem_Ref);

            getObjectDataIL.EmitCall(OpCodes.Callvirt, typeof(MemberInfo).GetMethod("get_Name"), null);
            getObjectDataIL.Emit(OpCodes.Ldloc_1);
            getObjectDataIL.Emit(OpCodes.Ldloc_2);
            getObjectDataIL.Emit(OpCodes.Ldelem_Ref);

            getObjectDataIL.EmitCall(OpCodes.Callvirt, typeof(SerializationInfo).GetMethod("AddValue", new Type[] { typeof(string), typeof(object) }), null);
            getObjectDataIL.Emit(OpCodes.Ldloc_2);
            getObjectDataIL.Emit(OpCodes.Ldc_I4_1);
            getObjectDataIL.Emit(OpCodes.Add);
            getObjectDataIL.Emit(OpCodes.Stloc_2);

            getObjectDataIL.MarkLabel(lbl2);

            getObjectDataIL.Emit(OpCodes.Ldloc_2);
            getObjectDataIL.Emit(OpCodes.Ldloc_0);
            getObjectDataIL.Emit(OpCodes.Ldlen);
            getObjectDataIL.Emit(OpCodes.Conv_I4);
            getObjectDataIL.Emit(OpCodes.Blt_S, lbl1);

            // info.SetType(parentType);

            getObjectDataIL.Emit(OpCodes.Ldarg_1);
            getObjectDataIL.Emit(OpCodes.Ldtoken, parentType);
            getObjectDataIL.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null);
            getObjectDataIL.EmitCall(OpCodes.Callvirt, typeof(SerializationInfo).GetMethod("SetType"), null);

            getObjectDataIL.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(
                getObjectDataMethod,
                typeof(ISerializable).GetMethod("GetObjectData"));

            #endregion

            #endregion

			// Load the type
			Type type = typeBuilder.CreateType();
			_Types.Add(parentType, type);

#if DEBUG
			// Persists the assembly if we are in debug mode to see the resulting code
            //newAssembly.Save(assemblyName.Name);
#endif
			return type;
        }

        #endregion
    }
}
