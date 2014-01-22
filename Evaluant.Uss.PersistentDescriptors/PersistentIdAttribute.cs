using System;

namespace Evaluant.Uss.PersistentDescriptors
{

	/// <summary>
	/// Used to indicate the AttributeDescriptor to persist the property
	/// </summary>
	[AttributeUsage (AttributeTargets.Property)]
	public class PersistentIdAttribute : Attribute 
	{
		private string _FieldName;

		/// <summary>
		/// The name of the field associated to the property
		/// </summary>
		public string FieldName {
			get { return _FieldName; }
		}

		/// <summary>
		/// Create a PersistentPropertyAttribute
		/// </summary>
		/// <param name="fieldName">The name of the field associated to the property</param>
		public PersistentIdAttribute (string fieldName) {
			_FieldName = fieldName;
		}
	}
}
