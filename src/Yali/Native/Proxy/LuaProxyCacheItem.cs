using System.Collections.Generic;
using Yali.Native.Value;

namespace Yali.Native.Proxy
{
    internal struct LuaProxyCacheItem
    {
        public IDictionary<string, LuaObject> Methods { get; set; }

        public IDictionary<string, LuaProxyCacheItemProperty> Properties { get; set; }

        public IDictionary<string, LuaProxyCacheItemProperty> StaticProperties { get; set; }
    }
}
