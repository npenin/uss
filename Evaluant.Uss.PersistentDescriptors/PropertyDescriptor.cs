using System;
using System.Reflection;

namespace Evaluant.Uss.PersistentDescriptors
{
	public class PropertyDescriptor
	{
		public PropertyDescriptor(string fieldName, string propertyName, bool isEntity,
            bool isList, bool isGenericList, bool isComposition, Type type, bool usePublicProperty)
		{
			_FieldName = fieldName;
			_PropertyName = propertyName;
			_IsEntity = isEntity;
			_IsList = isList;
			_IsGenericList = isGenericList;
            _IsComposition = isComposition;
			_Type = type;
            _UsePublicProperty = usePublicProperty;
		}

		private string _FieldName;
		public string FieldName
		{
			get { return _FieldName; }
			set { _FieldName = value; }
		}

		private string _PropertyName;
		public string PropertyName
		{
			get { return _PropertyName; }
			set { _PropertyName = value; }
		}

		private bool _IsEntity;
		public bool IsEntity
		{
			get { return _IsEntity; }
			set { _IsEntity = value; }
		}

		private bool _IsList;
		public bool IsList
		{
			get { return _IsList; }
			set { _IsList = value; }
		}

		private bool _IsGenericList;
		public bool IsGenericList
		{
			get { return _IsGenericList; }
			set { _IsGenericList = value; }
		}

        private bool _IsComposition = false;
        public bool IsComposition
        {
            get { return _IsComposition; }
            set { _IsComposition = value; }
        }

        private Type _Type;
		public Type Type
		{
			get { return _Type; }
			set { _Type = value; }
		}

        private bool _UsePublicProperty = false;
        public bool UsePublicProperty
        {
            get { return _UsePublicProperty; }
            set { _UsePublicProperty = value; }
        }

        public object GetValue(Type type, object target)
        {
            if (_UsePublicProperty)
            {
                return type.GetProperty(_PropertyName).GetValue(target, null);
            }
            else
            {
                return type.GetField(_FieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
            }

        }

        public void SetValue(Type type, object target, object value)
        {
            if (_UsePublicProperty)
            {
                type.GetProperty(_PropertyName).SetValue(target, value, null);
            }
            else
            {
                type.GetField(_FieldName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
            }

        }

    }
}
