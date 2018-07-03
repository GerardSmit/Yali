using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yali.Extensions;
using Yali.Utils;

namespace Yali.Native.Value
{
    public class LuaTable : LuaObject, IEnumerable<KeyValuePair<LuaObject, LuaObject>>
    {
        private readonly ConcurrentDictionary<LuaObject, LuaObject> _table;
        private readonly string _hash;
        private LuaObject _metaTable;

        internal readonly LuaObject Parent;

        internal LuaTable(LuaObject parent) : base(LuaType.Table)
        {
            _table = new ConcurrentDictionary<LuaObject, LuaObject>();
            _hash = StringUtils.GetRandomHexNumber(6);
            _metaTable = LuaNil.Instance;
            Parent = parent;
        }

        public LuaTable() : this(LuaNil.Instance)
        {
        }

        public override IEnumerable<LuaObject> Keys => _table.Keys.Where(k => !IndexRaw(k).IsNil());

        public override LuaObject Length => FromNumber(Keys.Count());

        public override LuaObject GetMetaTable(Engine engine)
        {
            return _metaTable;
        }

        public override void SetMetaTable(Engine engine, LuaObject value)
        {
            if (!(value.IsNil() || value.IsTable()))
            {
                throw new LuaException($"attempt to change the metatable to a {value.Type} value");
            }

            _metaTable = value;
        }

        public override object ToObject()
        {
            return _table;
        }

        public override string AsString()
        {
            return $"table: 0x{_hash}";
        }

        public override double AsNumber()
        {
            return Length;
        }

        public virtual bool ContainsKey(LuaObject key)
        {
            return _table.ContainsKey(key);
        }

        public override Task<LuaObject> IndexAsync(Engine engine, LuaObject key, CancellationToken token = default)
        {
            if (ContainsKey(key))
            {
                return Task.FromResult(IndexRaw(key));
            }

            var index = GetMetaMethod(engine, "__index");

            switch (index.Type)
            {
                case LuaType.Nil when !Parent.IsNil():
                    return Parent.IndexAsync(engine, key, token);
                case LuaType.Nil:
                    return Task.FromResult(LuaNil.Instance);
                case LuaType.Function:
                    return index.CallAsync(engine, Lua.Args(this, key), token).FirstAsync();
                default:
                    return Task.FromResult(index.IndexRaw(key));
            }
        }

        public override Task NewIndexAsync(Engine engine, LuaObject key, LuaObject value,
            CancellationToken token = default)
        {
            var newindex = GetMetaMethod(engine, "__newindex");
            var contains = ContainsKey(key);

            switch (newindex.Type)
            {
                case LuaType.Nil when !Parent.IsNil() && !contains:
                    return Parent.NewIndexAsync(engine, key, value, token);
                case LuaType.Function when !contains:
                    return newindex.CallAsync(engine, Lua.Args(this, key), token);
                case LuaType.Table when !contains:
                    newindex.NewIndexRaw(key, value);
                    return Task.CompletedTask;
                default:
                    NewIndexRaw(key, value);
                    return Task.CompletedTask;
            }
        }

        public override LuaObject IndexRaw(LuaObject key)
        {
            return _table.TryGetValue(key, out var value) ? value : LuaNil.Instance;
        }

        public override void NewIndexRaw(LuaObject key, LuaObject value)
        {
            _table.AddOrUpdate(key, value, (k, v) => value);
        }

        protected bool Equals(LuaTable other)
        {
            return string.Equals(_hash, other._hash);
        }

        public IEnumerator<KeyValuePair<LuaObject, LuaObject>> GetEnumerator()
        {
            return _table.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LuaTable) obj);
        }

        public override int GetHashCode()
        {
            return (_hash != null ? _hash.GetHashCode() : 0);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
