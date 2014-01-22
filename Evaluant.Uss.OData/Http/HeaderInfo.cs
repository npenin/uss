using System;

namespace Evaluant.Uss.OData.Http
{
    internal class HeaderInfo
    {
        // Fields
        internal readonly string HeaderName;
        internal readonly bool IsRequestRestricted;

        // Methods
        internal HeaderInfo(string name, bool requestRestricted)
        {
            this.HeaderName = name;
            this.IsRequestRestricted = requestRestricted;
        }
    }
}
