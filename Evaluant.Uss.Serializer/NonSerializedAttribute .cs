using System;

namespace Evaluant.Uss.Serializer
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class NonSerializedAttribute : Attribute
    {
        // This is a positional argument
        public NonSerializedAttribute()
        {
        }
    }
}
