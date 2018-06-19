using System.Reflection;

namespace Yali.Native.Proxy
{
    internal class LuaProxyCacheItemProperty
    {
        public string Name { get; set; }

        public PropertyInfo Info { get; set; }

        public bool Writeable { get; set; }

        public bool Readable { get; set; }
    }
}
