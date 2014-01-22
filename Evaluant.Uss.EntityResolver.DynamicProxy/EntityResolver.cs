using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.ObjectContext.Contracts;
using System.Reflection.Emit;
using System.Reflection;
using Evaluant.Uss.Domain;
using System.Collections;
#if SILVERLIGHT
using Evaluant.Uss.Serializer;
#else
using System.Runtime.Serialization;
#endif
using System.Threading;
using Evaluant.Uss.PersistentDescriptors;
using Evaluant.Uss.MetaData;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Collections;

namespace Evaluant.Uss.EntityResolver.Proxy.Dynamic
{
    public class EntityResolver : CacheEntityResolver
    {
        public OpCode? OpCode(Type type)
        {
            if (type == typeof(sbyte))
                return OpCodes.Ldind_I1;
            if (type == typeof(byte))
                return OpCodes.Ldind_U1;
            if (type == typeof(char))
                return OpCodes.Ldind_U2;
            if (type == typeof(short))
                return OpCodes.Ldind_I2;
            if (type == typeof(ushort))
                return OpCodes.Ldind_U2;
            if (type == typeof(int))
                return OpCodes.Ldind_I4;
            if (type == typeof(uint))
                return OpCodes.Ldind_U4;
            if (type == typeof(long))
                return OpCodes.Ldind_I8;
            if (type == typeof(ulong))
                return OpCodes.Ldind_I8;
            if (type == typeof(bool))
                return OpCodes.Ldind_I1;
            if (type == typeof(double))
                return OpCodes.Ldind_R8;
            if (type == typeof(float))
                return OpCodes.Ldind_R4;
            return null;
        }

        private delegate void UpdateEntityHandler(IPersistableProxy parent, string role, IEnumerable values);

        ReflectionDescriptor descriptor = new ReflectionDescriptor();

        public EntityResolver()
            : base()
        {

        }


        public EntityResolver(ObjectContext oc)
            : base(oc)
        {

        }

        public EntityResolver(ObjectContextAsync oc)
            : base(oc)
        {

        }

        #region IEntityResolver Members

        public override Type GetType(Type parentType, Model.Model model)
        {
            // Create an assembly name
            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = parentType.FullName;

            // Create a new assembly with one module
#if SILVERLIGHT
            AssemblyBuilder newAssembly = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#else
            AssemblyBuilder newAssembly = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
#endif
            ModuleBuilder moduleBuilder = newAssembly.DefineDynamicModule(assemblyName.Name);

            TypeBuilder typeBuilder = GenerateType(parentType, moduleBuilder, model);
            // Load the type
            Type type = typeBuilder.CreateType();
#if DEBUG && !SILVERLIGHT
            // Persists the assembly if we are in debug mode to see the resulting code
            //newAssembly.Save(assemblyName.Name);
#endif


            Types.Add(parentType, type);

            return type;
        }

        public TypeBuilder GenerateType(Type parentType, ModuleBuilder moduleBuilder, Model.Model model)
        {
            // Define a public class named "PersistableProxy" in the assembly.
            TypeBuilder typeBuilder = moduleBuilder.DefineType(
                parentType.Name + "Proxy", TypeAttributes.Public, parentType,
                new Type[] { typeof(IPersistable), 
                    typeof(IPersistableProxy), 
#if !SILVERLIGHT
                    typeof(ISerializable) 
#endif
                    
                    });

#if !SILVERLIGHT
            // Add a [Serializable] attribute to the new class
            ConstructorInfo serializableCtorInfo = typeof(SerializableAttribute).GetConstructor(new Type[0]);
            CustomAttributeBuilder serializableCABuilder = new CustomAttributeBuilder(serializableCtorInfo, new object[0]);

            typeBuilder.SetCustomAttribute(serializableCABuilder);
#endif

            ConstructorInfo NonSerializableCtorInfo = typeof(NonSerializedAttribute).GetConstructor(Type.EmptyTypes);
            CustomAttributeBuilder NonSerializableCABuilder = new CustomAttributeBuilder(NonSerializableCtorInfo, new object[0]);

            IDictionary<string, FieldBuilder> fieldTable = new Dictionary<string, FieldBuilder>();

            #region IPersistable implementation

            #region Default constructor
            ConstructorBuilder ctor = typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);

            FieldBuilder entityBldr = CreatePersistableField("entity", typeBuilder, typeof(Entity));
            CreatePersistableProperty("Entity", typeof(Entity), typeBuilder, entityBldr);

            // Mark it as [NonSerialized]
            entityBldr.SetCustomAttribute(NonSerializableCABuilder);

            FieldBuilder ocBldr = CreatePersistableField("context", typeBuilder, typeof(IObjectContext));
            CreatePersistableProperty("ObjectContext", typeof(IPersistenceEngineObjectContext), typeBuilder, ocBldr);

            FieldBuilder ocAsyncBldr = CreatePersistableField("contextAsync", typeBuilder, typeof(IObjectContextAsync));
            CreatePersistableProperty("ObjectContextAsync", typeof(IPersistenceEngineObjectContextAsync), typeBuilder, ocAsyncBldr);


            // Mark it as [NonSerialized]
            ocBldr.SetCustomAttribute(NonSerializableCABuilder);
            ocAsyncBldr.SetCustomAttribute(NonSerializableCABuilder);
            #endregion

            #region TrackChildren()

            //// void TrackChildren()
            //MethodBuilder trackMethod = typeBuilder.DefineMethod(
            //    "IPersistable.TrackChildren",
            //    MethodAttributes.Private | MethodAttributes.Virtual | MethodAttributes.Final |
            //    MethodAttributes.HideBySig | MethodAttributes.NewSlot,
            //    typeof(void),
            //    new Type[0]);

            //ILGenerator trackIL = trackMethod.GetILGenerator();

            //Label retLbl = trackIL.DefineLabel();

            //trackIL.Emit(OpCodes.Ldarg_0);
            //trackIL.Emit(OpCodes.Ldfld, pmBldr);
            //trackIL.Emit(OpCodes.Brfalse, retLbl);

            //foreach (PropertyDescriptor prop in descriptor.GetPersistentProperties(parentType))
            //{
            //    if (!prop.IsEntity)
            //        continue;

            //    Label skipField = trackIL.DefineLabel();

            //    trackIL.Emit(OpCodes.Ldarg_0);
            //    EmitGet(parentType, prop, trackIL);
            //    trackIL.Emit(OpCodes.Brfalse, skipField);
            //    trackIL.Emit(OpCodes.Ldarg_0);
            //    trackIL.Emit(OpCodes.Ldfld, pmBldr);
            //    trackIL.Emit(OpCodes.Ldarg_0);
            //    EmitGet(parentType, prop, trackIL);

            //    MethodInfo trackObj = null;
            //    /*if (prop.IsList)
            //    {
            //        // use the TrackObject (IPersistableCollection) method
            //        trackIL.Emit(OpCodes.Isinst, typeof(IPersistableCollection));
            //        trackObj = typeof(ObjectContext).GetMethod(
            //            "TrackObject",
            //            new Type[] { typeof(IPersistableCollection) });
            //    }
            //    else */ if (prop.IsGenericList)
            //    {
            //        // use the TrackObject<T> (IPersistableCollection<T>) method
            //        Type genArg = prop.Type.GetGenericArguments()[0];
            //        trackIL.Emit(OpCodes.Isinst, typeof(IPersistableCollection<>).MakeGenericType(genArg));
            //        foreach (MethodInfo mi in typeof(ObjectContext).GetMethods())
            //        {
            //            if (mi.Name != "TrackObject")
            //                continue;
            //            ParameterInfo[] parameters = mi.GetParameters();
            //            if (parameters.Length == 1 && parameters[0].ParameterType.IsGenericType)
            //            {
            //                trackObj = mi.MakeGenericMethod(genArg);
            //                break;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        trackIL.Emit(OpCodes.Isinst, typeof(IPersistable));
            //        trackObj = typeof(ObjectContext).GetMethod(
            //            "TrackObject",
            //            new Type[] { typeof(IPersistable) });
            //    }

            //    if (trackObj == null)
            //        throw new EntityResolverException("Cannot find the TrackObject method");

            //    trackIL.Emit(OpCodes.Callvirt, trackObj);

            //    trackIL.MarkLabel(skipField);
            //}

            //trackIL.MarkLabel(retLbl);
            //trackIL.Emit(OpCodes.Ret);

            //typeBuilder.DefineMethodOverride(
            //    trackMethod,
            //    typeof(IPersistable).GetMethod("TrackChildren"));

            #endregion

            #region To-One Lazy loading
            ConstructorInfo referenceCtor = null;

            foreach (PropertyDescriptor prop in descriptor.GetPersistentProperties(parentType))
            {
                if (prop.IsEntity)
                {
                    Type referenceType;
                    if (prop.IsList || prop.IsGenericList)
                        referenceType = typeof(Evaluant.Uss.ObjectContext.Contracts.Reference<>).MakeGenericType(prop.Type.GetGenericArguments()[0]);
                    else
                        referenceType = typeof(Evaluant.Uss.ObjectContext.Contracts.Reference<>).MakeGenericType(prop.Type);
                    referenceCtor = referenceType.GetConstructor(new Type[] { typeof(IPersistable), typeof(string) });
                    PropertyInfo referenceValueProp = referenceType.GetProperty("Value");

                    FieldBuilder referenceField = typeBuilder.DefineField(
                            prop.PropertyName + "Reference",
                            referenceType,
                            FieldAttributes.Private);


                    referenceField.SetCustomAttribute(NonSerializableCABuilder);

                    fieldTable.Add(prop.PropertyName, referenceField);
                    if (!prop.IsList && !prop.IsGenericList)
                    {
                        PropertyInfo parentProp = parentType.GetProperty(prop.PropertyName);

                        MethodBuilder getPropPers = typeBuilder.DefineMethod(
                            parentProp.GetGetMethod().Name,
                            MethodAttributes.Public | MethodAttributes.Virtual |
                            MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                            MethodAttributes.ReuseSlot,
                            prop.Type,
                            new Type[0]);

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

                        ILGenerator getPropPersIL = getPropPers.GetILGenerator();
                        LocalBuilder result = getPropPersIL.DeclareLocal(prop.Type);
                        Label load = getPropPersIL.DefineLabel();
                        Label ret = getPropPersIL.DefineLabel();

                        // if({Prop}Reference==null)
                        getPropPersIL.Emit(OpCodes.Ldarg_0);
                        getPropPersIL.Emit(OpCodes.Ldfld, referenceField);
                        getPropPersIL.Emit(OpCodes.Brtrue, load);

                        // {Prop}Reference=new Reference(this, {Prop});
                        getPropPersIL.Emit(OpCodes.Ldarg_0);
                        getPropPersIL.Emit(OpCodes.Ldarg_0); //this
                        getPropPersIL.Emit(OpCodes.Ldstr, prop.PropertyName);
                        getPropPersIL.Emit(OpCodes.Newobj, referenceCtor);
                        getPropPersIL.Emit(OpCodes.Stfld, referenceField);

                        // if({Prop}Reference.Value!=null)
                        getPropPersIL.MarkLabel(load);
                        // {PropertyName}Reference.Value == null
                        getPropPersIL.Emit(OpCodes.Ldarg_0);
                        getPropPersIL.Emit(OpCodes.Ldfld, referenceField);
                        getPropPersIL.Emit(OpCodes.Call, referenceValueProp.GetGetMethod());
                        getPropPersIL.Emit(OpCodes.Brfalse, ret);

                        //base.{Prop}=Reference.Value;
                        getPropPersIL.Emit(OpCodes.Ldarg_0);
                        getPropPersIL.Emit(OpCodes.Ldarg_0);
                        getPropPersIL.Emit(OpCodes.Ldfld, referenceField);
                        getPropPersIL.Emit(OpCodes.Call, referenceValueProp.GetGetMethod());
                        getPropPersIL.Emit(OpCodes.Call, parentProp.GetSetMethod());

                        //return base.{Prop}
                        getPropPersIL.MarkLabel(ret);
                        getPropPersIL.Emit(OpCodes.Ldarg_0);
                        getPropPersIL.Emit(OpCodes.Call, parentProp.GetGetMethod());
                        getPropPersIL.Emit(OpCodes.Ret);

                        MethodBuilder setPropPers = typeBuilder.DefineMethod(
                            parentProp.GetSetMethod().Name,
                            MethodAttributes.Public | MethodAttributes.Virtual |
                            MethodAttributes.SpecialName | MethodAttributes.HideBySig |
                            MethodAttributes.ReuseSlot,
                            typeof(void),
                            new Type[] { prop.Type });

                        /*
                            One to One Lazy Loaded Setter:

                            set {

                                this.{prop.PropertyName}Reference.Value=value;
                                base.set_{prop.PropertyName} (value);
                            }
                        */

                        ILGenerator setPropPersIL = setPropPers.GetILGenerator();
                        load = setPropPersIL.DefineLabel();
                        //if({Prop}Reference==null)
                        setPropPersIL.Emit(OpCodes.Ldarg_0);
                        setPropPersIL.Emit(OpCodes.Ldfld, referenceField);
                        setPropPersIL.Emit(OpCodes.Brtrue, load);

                        //{Prop}Reference=new Reference(this, {Prop});
                        setPropPersIL.Emit(OpCodes.Ldarg_0);
                        setPropPersIL.Emit(OpCodes.Ldarg_0); //this
                        setPropPersIL.Emit(OpCodes.Ldstr, prop.PropertyName);
                        setPropPersIL.Emit(OpCodes.Newobj, referenceCtor);
                        setPropPersIL.Emit(OpCodes.Stfld, referenceField);

                        //{Prop}Reference.Value=value;
                        setPropPersIL.MarkLabel(load);
                        setPropPersIL.Emit(OpCodes.Ldarg_0);
                        setPropPersIL.Emit(OpCodes.Ldfld, referenceField);
                        setPropPersIL.Emit(OpCodes.Ldarg_1);
                        setPropPersIL.Emit(OpCodes.Call, referenceValueProp.GetSetMethod());

                        // base.{Prop} = value;
                        setPropPersIL.Emit(OpCodes.Ldarg_0);
                        setPropPersIL.Emit(OpCodes.Ldarg_1);
                        setPropPersIL.Emit(OpCodes.Call, parentProp.GetSetMethod());
                        setPropPersIL.Emit(OpCodes.Ret);

                        PropertyBuilder propBuilder = typeBuilder.DefineProperty(prop.PropertyName, PropertyAttributes.None, prop.Type, null);
                        propBuilder.SetGetMethod(getPropPers);
                        propBuilder.SetSetMethod(setPropPers);
                    }
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
                Type.EmptyTypes);

            // From the method, get an ILGenerator. This is used to
            // emit the IL that we want
            ILGenerator setIL = setMethod.GetILGenerator();

            MethodInfo getValueMethod = typeof(Entity).GetMethod("GetValue", new Type[] { typeof(string) });

            if (getValueMethod == null)
                throw new EntityResolverException("Cannot find the Entity.GetValue method");

            // Cyclic reference

            FieldBuilder processingField = typeBuilder.DefineField("processing", typeof(bool), FieldAttributes.Private);

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

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldc_I4_1);
            setIL.Emit(OpCodes.Stfld, processingField);

            // object value;

            LocalBuilder valueBldr = setIL.DeclareLocal(typeof(object));

            //MethodInfo getId = typeof(Entity).GetProperty("Id").GetGetMethod();

            //PropertyDescriptor idPropertyDesc = descriptor.GetIdDescriptor(parentType);
            //if (idPropertyDesc != null)
            //{
            //    /*
            //     * 
            //     * base._Id = _Entity.Id;
            //     * 
            //     or
            //     * 
            //     * base._Id = int.Parse(_Entity.Id);
            //     * 
            //     */

            //    setIL.Emit(OpCodes.Ldarg_0);
            //    setIL.Emit(OpCodes.Ldarg_0);
            //    setIL.Emit(OpCodes.Ldfld, entityBldr);
            //    setIL.EmitCall(OpCodes.Callvirt, getId, null);

            //    if (idPropertyDesc.Type == typeof(Int32))
            //    {
            //        // int
            //        MethodInfo parseIntMethod = typeof(Int32).GetMethod("Parse", new Type[] { typeof(string) });
            //        setIL.EmitCall(OpCodes.Call, parseIntMethod, null);
            //    }
            //    else
            //    {
            //        // string
            //        setIL.Emit(OpCodes.Castclass, typeof(string));
            //    }

            //    EmitSet(parentType, idPropertyDesc, setIL);

            //}

            foreach (PropertyDescriptor propertyDescriptor in descriptor.GetPersistentProperties(parentType))
            {

                if (!propertyDescriptor.IsEntity)
                {

                    /*
                     *	value = _Entity.GetValue("Name");
                     * 
                     * */

                    setIL.Emit(OpCodes.Ldarg_0);
                    setIL.Emit(OpCodes.Ldfld, entityBldr);
                    setIL.Emit(OpCodes.Ldstr, propertyDescriptor.PropertyName);
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

                    if (propertyDescriptor.Type.IsValueType)
                    {
                        OpCode? oc = OpCode(propertyDescriptor.Type);

                        if (oc.HasValue) // Primitive type
                        {
                            setIL.Emit(OpCodes.Ldarg_0);
                            setIL.Emit(OpCodes.Ldloc, valueBldr);
                            setIL.Emit(OpCodes.Unbox, propertyDescriptor.Type);
                            setIL.Emit(oc.Value);
                        }
                        else // Not a primitive type (e.g: DateTime)
                        {
                            if (propertyDescriptor.Type.IsEnum)
                            {
                                setIL.Emit(OpCodes.Ldarg_0);
                                setIL.Emit(OpCodes.Ldtoken, propertyDescriptor.Type);
                                MethodInfo mi = typeof(Type).GetMethod("GetTypeFromHandle");
                                setIL.EmitCall(OpCodes.Call, mi, null);
                                setIL.Emit(OpCodes.Ldloc, valueBldr);

                                //  Always convert the value from the database (int or string) to string
                                //  and call Convert.ToString(object o)
                                mi = typeof(Convert).GetMethod("ToString", new Type[] { typeof(object) });
                                setIL.EmitCall(OpCodes.Call, mi, null);

                                mi = typeof(Enum).GetMethod("Parse", new Type[] { typeof(Type), typeof(string) });
                                setIL.EmitCall(OpCodes.Call, mi, null);
                                setIL.Emit(OpCodes.Unbox_Any, propertyDescriptor.Type);
                            }
                            else
                            {
                                setIL.Emit(OpCodes.Ldarg_0);
                                setIL.Emit(OpCodes.Ldloc, valueBldr);
                                setIL.Emit(OpCodes.Unbox, propertyDescriptor.Type);
                                setIL.Emit(OpCodes.Ldobj, propertyDescriptor.Type);
                            }
                        }
                    }
                    else
                    {
                        setIL.Emit(OpCodes.Ldarg_0);
                        setIL.Emit(OpCodes.Ldloc, valueBldr);
                        setIL.Emit(OpCodes.Castclass, propertyDescriptor.Type);
                    }

                    EmitSet(parentType, propertyDescriptor, setIL);

                    setIL.MarkLabel(valueLabel);
                }
            }

            // _Processing = false;

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldc_I4_0);
            setIL.Emit(OpCodes.Stfld, processingField);

            setIL.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(
                setMethod,
                typeof(IPersistableProxy).GetMethod("Set"));

            #endregion

            #region SetReferences()

            /*
             *  bool _Processing;
             *	SetReferences()
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
                Type.EmptyTypes);

            // From the method, get an ILGenerator. This is used to
            // emit the IL that we want
            setIL = setMethod.GetILGenerator();

            // Cyclic reference

            //processingField = typeBuilder.DefineField("_Processing", typeof(bool), FieldAttributes.Private);

            //processingField.SetCustomAttribute(NonSerializableCABuilder);

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

            // object proxy;

            foreach (PropertyDescriptor propertyDescriptor in descriptor.GetPersistentProperties(parentType))
            {

                if (propertyDescriptor.IsList || propertyDescriptor.IsGenericList)
                {
                    var @ref = fieldTable[propertyDescriptor.PropertyName];

                    setIL.Emit(OpCodes.Ldarg_0);
                    setIL.Emit(OpCodes.Ldarg_0);
                    setIL.Emit(OpCodes.Ldstr, propertyDescriptor.PropertyName);

                    referenceCtor = @ref.FieldType.GetConstructor(new Type[] { typeof(IPersistable), typeof(string) });

                    setIL.Emit(OpCodes.Newobj, referenceCtor);

                    setIL.Emit(OpCodes.Stfld, @ref);

                    setIL.Emit(OpCodes.Ldarg_0);
                    setIL.Emit(OpCodes.Ldarg_0);
                    setIL.Emit(OpCodes.Ldfld, @ref);
                    setIL.Emit(OpCodes.Call, @ref.FieldType.GetProperty("Values").GetGetMethod());
                    setIL.Emit(OpCodes.Call, parentType.GetProperty(propertyDescriptor.PropertyName).GetSetMethod());
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

            updateIL.Emit(OpCodes.Ldarg_0);
            updateIL.Emit(OpCodes.Ldc_I4_1);
            updateIL.Emit(OpCodes.Stfld, processingField);

            // object value;

            valueBldr = updateIL.DeclareLocal(typeof(object));

            foreach (PropertyDescriptor prop in descriptor.GetPersistentProperties(parentType))
            {

                if (prop.IsEntity)
                {
                    if (prop.IsList || prop.IsGenericList)
                    {

                        // this.{OC}.Resolver.UpdateEntitySet(this, {PropertyName}, _partners);

                        MethodInfo uesInfo = new UpdateEntityHandler(EntityResolver.UpdateEntitySet).Method;

                        updateIL.Emit(OpCodes.Ldarg_0);
                        updateIL.Emit(OpCodes.Ldstr, prop.PropertyName);
                        updateIL.Emit(OpCodes.Ldarg_0);

                        EmitGet(parentType, prop, updateIL);

                        updateIL.Emit(OpCodes.Call, uesInfo);

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
                        updateIL.EmitCall(OpCodes.Callvirt, typeof(Entity).GetMethod("Remove", new Type[] { typeof(string) }), null);

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
                        updateIL.EmitCall(OpCodes.Callvirt, typeof(Entity).GetProperty("Item", typeof(Entry), new Type[] { typeof(string) }).GetGetMethod(), null);
                        updateIL.Emit(OpCodes.Brfalse, lblTestAddValue);

                        // if(_name != null)
                        updateIL.Emit(OpCodes.Ldarg_0);

                        EmitGet(parentType, prop, updateIL);

                        updateIL.Emit(OpCodes.Brtrue, lblSetValue);

                        //_Entity.DeleteElement(prop.PropertyName);
                        updateIL.Emit(OpCodes.Ldarg_0);
                        updateIL.Emit(OpCodes.Ldfld, entityBldr);
                        updateIL.Emit(OpCodes.Ldstr, prop.PropertyName);
                        updateIL.EmitCall(OpCodes.Callvirt, typeof(Entity).GetMethod("Remove", new Type[] { typeof(string) }), null);
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

                        updateIL.EmitCall(OpCodes.Callvirt, typeof(Entity).GetMethod("Add", new Type[] { typeof(string), typeof(object) }), null);

                        updateIL.MarkLabel(lblAfterSetValue);
                    }
                    else
                    {
                        if (prop.Type.IsEnum)
                        {
                            updateIL.Emit(OpCodes.Ldarg_0);
                            updateIL.Emit(OpCodes.Ldfld, entityBldr);
                            updateIL.Emit(OpCodes.Ldstr, prop.PropertyName);

                            Evaluant.Uss.Model.Attribute attr = model.GetAttribute(TypeResolver.ConvertNamespaceDomainToEuss(parentType), prop.PropertyName);

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

            updateIL.Emit(OpCodes.Ldarg_0);
            updateIL.Emit(OpCodes.Ldc_I4_0);
            updateIL.Emit(OpCodes.Stfld, processingField);

            updateIL.Emit(OpCodes.Ret);

            typeBuilder.DefineMethodOverride(
                updateMethod,
                typeof(IPersistableProxy).GetMethod("Update"));

            #endregion

            #endregion

            #region ISerializable

            #region GetObjectData()
#if !SILVERLIGHT

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

#endif
            #endregion

            #endregion

            return typeBuilder;
        }

        #region CreatePersistableProperty

        /// <summary>
        /// Generates a property implementation for IPersistable
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="targetType"></param>
        /// <param name="typeBuilder"></param>
        private PropertyBuilder CreatePersistableProperty(string propertyName, Type targetType, TypeBuilder typeBuilder, FieldBuilder fieldBldr)
        {
            PropertyBuilder entityPropBldr = typeBuilder.DefineProperty(
                "IPersistable." + propertyName,
                PropertyAttributes.SpecialName,
                targetType,
                Type.EmptyTypes);

            // First, we'll define the behavior of the "get" property for Entity as a method.
            MethodBuilder entityGetPropMthdBldr = typeBuilder.DefineMethod(
                "IPersistable.get_" + propertyName,
                MethodAttributes.HideBySig |
                MethodAttributes.Private |
                MethodAttributes.NewSlot |
                MethodAttributes.SpecialName |
                MethodAttributes.Final |
                MethodAttributes.Virtual,
                CallingConventions.Standard | CallingConventions.HasThis,
                targetType,
                Type.EmptyTypes);

            ILGenerator entityGetIL = entityGetPropMthdBldr.GetILGenerator();

            entityGetIL.Emit(OpCodes.Ldarg_0);
            entityGetIL.Emit(OpCodes.Ldfld, fieldBldr);
            entityGetIL.Emit(OpCodes.Ret);

            // Now, we'll define the behavior of the "set" property for CustomerName.
            MethodBuilder entitySetPropMthdBldr = typeBuilder.DefineMethod(
                "IPersistable.set_" + propertyName,
                MethodAttributes.HideBySig |
                MethodAttributes.Private |
                MethodAttributes.NewSlot |
                MethodAttributes.SpecialName |
                MethodAttributes.Final |
                MethodAttributes.Virtual,
                CallingConventions.Standard | CallingConventions.HasThis,
                typeof(void),
                new Type[] { targetType });
            entitySetPropMthdBldr.DefineParameter(1, ParameterAttributes.In, "value");

            ILGenerator entitySetIL = entitySetPropMthdBldr.GetILGenerator();

            entitySetIL.Emit(OpCodes.Ldarg_0);
            entitySetIL.Emit(OpCodes.Ldarg_1);
            entitySetIL.Emit(OpCodes.Stfld, fieldBldr);
            entitySetIL.Emit(OpCodes.Ret);

            // Last, we must map the two methods created above to our PropertyBuilder to 
            // their corresponding behaviors, "get" and "set" respectively. 
            entityPropBldr.SetGetMethod(entityGetPropMthdBldr);
            entityPropBldr.SetSetMethod(entitySetPropMthdBldr);

            typeBuilder.DefineMethodOverride(
                entityGetPropMthdBldr,
                typeof(IPersistable).GetProperty(propertyName).GetGetMethod());

            typeBuilder.DefineMethodOverride(
                entitySetPropMthdBldr,
                typeof(IPersistable).GetProperty(propertyName).GetSetMethod());

            return entityPropBldr;
        }

        #endregion

        #region CreatePersitableField

        FieldBuilder CreatePersistableField(string name, TypeBuilder type, Type fieldType)
        {
            return type.DefineField(
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

                if (method == null)
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


        #endregion

        /// <summary>
        /// Updates an Entity with an external list.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="role">The role.</param>
        /// <param name="values">The values.</param>
        public static void UpdateEntitySet(IPersistableProxy parent, string role, IEnumerable values)
        {
            // don't load reference if not needed
            if (!parent.Entity.InferredReferences.Contains(role))
                return;

            HashedList<Entity> list = new HashedList<Entity>();

            foreach (object item in values)
            {
                if (item is IPersistableProxy)
                {
                    ((IPersistableProxy)item).Update();
                }

                if (parent.ObjectContextAsync != null)
                    list.Add(parent.ObjectContextAsync.Resolver.Resolve(item, parent.ObjectContextAsync.PersistenceEngineAsync.FactoryAsync.Model));
                else
                    list.Add(parent.ObjectContext.Resolver.Resolve(item, parent.ObjectContext.PersistenceEngine.Factory.Model));
            }

            Entry entry = parent.Entity[role];
            MultipleEntry currentList;
            if (!entry.IsMultiple)
            {
                currentList = new MultipleEntry(entry.Name);
                currentList.Add((Entry<Entity>)entry);
            }
            else
                currentList = (MultipleEntry)entry;

            // Detect removed relationships
            foreach (Entry<Entity> e in currentList)
                if (!list.Contains(e.TypedValue))
                    parent.Entity.Remove(role, e.TypedValue);

            // Add new relationships
            foreach (Entity e in list)
                if (!currentList.Contains(e))
                {
                    parent.Entity.Add(role, e);
                }
        }

        public override void Resolve<T>(T entity, Model.Model model, Entity entityToUpdate)
        {
            foreach (var property in this.descriptor.GetPersistentProperties(typeof(T)))
            {
                if (property.IsList || property.IsGenericList || property.IsEntity)
                {
                    IEntityResolver resolver;
                    if (oc == null)
                        resolver = this.asyncOc.Resolver;
                    else
                        resolver = this.oc.Resolver;

                    if (!property.IsList && !property.IsGenericList)
                    {
                        Entry<Entity> entry = (Entry<Entity>)entityToUpdate[property.PropertyName];
                        object value = property.GetValue(typeof(T), entity);
                        if (!(value is IPersistable) && entry != null && value != null)
                            resolver.Resolve(value, model, entry.TypedValue);
                    }
                    else
                    {
                        MultipleEntry entry = (MultipleEntry)entityToUpdate[property.PropertyName];
                        List<Entity> entities = new List<Entity>();
                        List<Entry<Entity>> entriesToRemove = new List<Entry<Entity>>();
                        var value = property.GetValue(typeof(T), entity);
                        if (value != null)
                        {
                            foreach (var item in (IEnumerable)value)
                            {
                                Entity e;
                                if (!(item is IPersistable))
                                    e = resolver.Resolve(item, model);
                                else
                                    e = ((IPersistable)item).Entity;
                                //Entity e = resolver.Resolve(item, model);
                                entities.Add(e);
                                if (!entry.Contains(e))
                                    entry.Add(e);
                            }

                            foreach (Entry<Entity> entryToRemove in entry)
                            {
                                if (!entities.Contains(entryToRemove.TypedValue))
                                    entriesToRemove.Add(entryToRemove);
                            }
                            foreach (Entry<Entity> entryToRemove in entriesToRemove)
                                entry.Remove(entryToRemove);
                        }
                    }

                }
                else
                {
                    object value = property.GetValue(typeof(T), entity);
                    if (value != null)
                        entityToUpdate.SetValue(property.PropertyName, value);
                }
            }
        }
    }


}
