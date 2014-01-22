using System;

namespace Evaluant.Uss.EntityResolver.Proxy
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ProxyAssemblyAttribute : Attribute
    {
        public ProxyAssemblyAttribute() { }
    }
}
