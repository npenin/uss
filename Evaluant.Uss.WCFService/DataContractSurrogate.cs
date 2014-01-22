using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using Evaluant.Uss.Domain;
using Evaluant.Uss.Commands;

namespace Evaluant.Uss.WCFService
{
    class DataContractSurrogate : IDataContractSurrogate
    {
        #region IDataContractSurrogate Members

        public object GetCustomDataToExport(Type clrType, Type dataContractType)
        {
            return null;
        }

        public object GetCustomDataToExport(System.Reflection.MemberInfo memberInfo, Type dataContractType)
        {
            return null;
        }

        public Type GetDataContractType(Type type)
        {
            if (type == typeof(MultipleEntry))
                return typeof(Surrogates.MultipleEntry);
            if (typeof(Command).IsAssignableFrom(type) || type.IsAssignableFrom((typeof(Command))))
                return typeof(Surrogates.Command);
            return type;
        }

        public object GetDeserializedObject(object obj, Type targetType)
        {
            Surrogates.MultipleEntry entry = obj as Surrogates.MultipleEntry;
            if (entry != null)
            {
                MultipleEntry entryToDeserialize = new MultipleEntry(entry.Name);
                entryToDeserialize.TypedValue = entry.Entities;
                return entryToDeserialize;
            }
            Surrogates.Command command = obj as Surrogates.Command;
            if (command != null)
            {
                return null;
            }
            return obj;
        }

        public void GetKnownCustomDataTypes(System.Collections.ObjectModel.Collection<Type> customDataTypes)
        {
        }

        public object GetObjectToSerialize(object obj, Type targetType)
        {
            MultipleEntry entryToSerialize = obj as MultipleEntry;
            if (entryToSerialize != null)
            {
                Surrogates.MultipleEntry entry = new Surrogates.MultipleEntry();
                entry.Name = entryToSerialize.Name;
                entry.Entities = entryToSerialize.TypedValue;
                return entry;
            }
            Command command = obj as Command;
            if (command != null)
            {
                return new Surrogates.Command();
            }
            return obj;
        }

        public Type GetReferencedTypeOnImport(string typeName, string typeNamespace, object customData)
        {
            if (typeName == "MultipleEntry")
                return typeof(Surrogates.MultipleEntry);
            if (typeName == "Command")
                return typeof(Surrogates.Command);
            return null;
        }

        public System.CodeDom.CodeTypeDeclaration ProcessImportedType(System.CodeDom.CodeTypeDeclaration typeDeclaration, System.CodeDom.CodeCompileUnit compileUnit)
        {
            return typeDeclaration;
        }

        #endregion
    }
}
