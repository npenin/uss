using System;

namespace Evaluant.Uss.PersistentDescriptors
{

	/// <summary>
	/// Used to indicate the AttributeDescriptor to persist the property
	/// </summary>
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Interface)]
	public class NotSerializedAttribute : Attribute 
	{
        public NotSerializedAttribute()
        {
        }
	}
}
