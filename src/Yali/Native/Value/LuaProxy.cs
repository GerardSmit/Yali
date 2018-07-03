using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yali.Extensions;
using Yali.Native.Proxy;

namespace Yali.Native.Value
{
    public class LuaProxy : LuaTable
    {
        private readonly object _instance;
        private readonly LuaProxyCacheItem _cacheItem;

        public LuaProxy(object instance)
        {
            _instance = instance ?? throw new ArgumentNullException(nameof(instance));
            _cacheItem = LuaProxyCache.Get(instance.GetType());
        }

        public override IEnumerable<LuaObject> Keys => base.Keys
            .Concat(_cacheItem.Properties.Keys.Select(FromString))
            .Where(k => !IndexRaw(k).IsNil());

        public override LuaObject GetMetaTable(Engine engine)
        {
            return engine.GetProxyMetaTable(_instance.GetType());
        }

        public override void SetMetaTable(Engine engine, LuaObject value)
        {
            throw new LuaException($"attempt to change metatable of a {Type.ToLuaName()} value");
        }

        public override object ToObject()
        {
            return _instance;
        }

        public override bool ContainsKey(LuaObject key)
        {
            var str = key.AsString();

            return _cacheItem.Properties.ContainsKey(str) || base.ContainsKey(key);
        }

        public override LuaObject IndexRaw(LuaObject key)
        {
            if (base.ContainsKey(key))
            {
                return base.IndexRaw(key);
            }

            var str = key.AsString();

            if (!_cacheItem.Properties.ContainsKey(str))
            {
                return LuaNil.Instance;
            }

            var property = _cacheItem.Properties[str];

            if (!property.Writeable)
            {
                throw new LuaException($"the index {str} is not readable");
            }

            return FromObject(property.Info.GetValue(_instance));
        }

        public override void NewIndexRaw(LuaObject key, LuaObject value)
        {
            var str = key.AsString();

            if (!_cacheItem.Properties.ContainsKey(str))
            {
                base.NewIndexRaw(key, value);
                return;
            }

            var property = _cacheItem.Properties[str];

            if (!property.Writeable)
            {
                throw new LuaException($"the index {str} is not writeable");
            }

            property.Info.SetValue(_instance, value.ToObject(property.Info.PropertyType));
        }
    }
}
