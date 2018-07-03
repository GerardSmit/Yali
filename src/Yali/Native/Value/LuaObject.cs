/*
 * See LICENSE file
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Yali.Extensions;
using Yali.Native.Proxy;
using Yali.Native.Value.Functions;
using Yali.Runtime.Ast;
using Yali.Utils;

namespace Yali.Native.Value
{
    /// <summary>
    /// A Lua object. Can be any of the standard Lua objects
    /// </summary>
    public abstract class LuaObject
    {
        protected LuaObject(LuaType type)
        {
            Type = type;
        }

        //~LuaObject()
        //{
        //    var method = GetMetaMethod(TODO, "__gc");

        //    if (!method.IsNil())
        //    {
        //        method.CallAsync(Lua.Args(this));
        //    }
        //}

        public LuaType Type { get; }

        public virtual IEnumerable<LuaObject> Keys => Enumerable.Empty<LuaObject>();

        public virtual LuaObject Length => throw new LuaException($"attempt to get length of a {Type.ToLuaName()} value");

        public virtual LuaObject GetMetaTable(Engine engine)
        {
            return LuaNil.Instance;
        }

        public virtual void SetMetaTable(Engine engine, LuaObject value)
        {
            throw new LuaException($"attempt to change the metatable of a {Type.ToLuaName()} value");
        }

        public virtual LuaObject GetMetaMethod(Engine engine, LuaObject key)
        {
            var mt = GetMetaTable(engine);

            return mt.IsNil() ? LuaNil.Instance : mt.IndexRaw(key);
        }

        public virtual Task<LuaObject> IndexAsync(Engine engine, LuaObject key, CancellationToken token = default)
        {
            var index = GetMetaMethod(engine, "__index");

            switch (index.Type)
            {
                case LuaType.Function:
                    return index.CallAsync(engine, Lua.Args(this, key), token).FirstAsync();
                case LuaType.Nil:
                    return Task.FromResult(IndexRaw(key));
                default:
                    return Task.FromResult(index.IndexRaw(key));
            }
        }

        public virtual Task NewIndexAsync(Engine engine, LuaObject key, LuaObject value, CancellationToken token = default)
        {
            var newindex = GetMetaMethod(engine, "__newindex");

            switch (newindex.Type)
            {
                case LuaType.Function:
                    return newindex.CallAsync(engine, Lua.Args(this, key, value), token).FirstAsync();
                case LuaType.Nil:
                    NewIndexRaw(key, value);
                    return Task.CompletedTask;
                default:
                    newindex.NewIndexRaw(key, value);
                    return Task.CompletedTask;
            }
        }

        public virtual LuaObject IndexRaw(LuaObject key)
        {
            throw new LuaException($"attempt to index a {Type.ToLuaName()} value");
        }

        public virtual void NewIndexRaw(LuaObject key, LuaObject value)
        {
            throw new LuaException($"attempt to index a {Type.ToLuaName()} value");
        }

        public virtual Task<LuaArguments> CallAsync(Engine engine, LuaArguments args, CancellationToken token = default)
        {
            var call = GetMetaMethod(engine, "__call");

            return call.IsFunction()
                ? call.CallAsync(engine, args, token)
                : throw new LuaException($"attempt to call a {Type.ToLuaName()} value");
        }

        public static async Task<LuaObject> BinaryOperationAsync(Engine engine, BinaryOp op, LuaObject left,
            LuaObject right, CancellationToken token = default)
        {
            // Metatables
            var metaKey = op.ToMetaKey();
            var isDifferent = op == BinaryOp.Different;

            if (isDifferent)
            {
                metaKey = BinaryOp.Equal.ToMetaKey();
            }

            if (!string.IsNullOrEmpty(metaKey) || isDifferent)
            {
                var meta = left.GetMetaMethod(engine, metaKey);

                if (meta.IsNil())
                {
                    meta = right.GetMetaMethod(engine, metaKey);
                }

                if (!meta.IsNil())
                {
                    var result = await meta.CallAsync(engine, Lua.Args(left, right), token).FirstAsync();

                    if (isDifferent)
                    {
                        result = !result.AsBool();
                    }

                    return result;
                }
            }

            // Default implementations
            double leftDbl, rightDbl;

            switch (op)
            {
                // String
                case BinaryOp.Concat:
                    return left.AsString() + right.AsString();

                // Logic
                case BinaryOp.And:
                    return left.AsBool() ? right : left;
                case BinaryOp.Or:
                    return left.AsBool() ? left : right;

                // Relational
                case BinaryOp.Equal:
                    return left.Equals(right);
                case BinaryOp.Different:
                    return !left.Equals(right);

                case BinaryOp.LessThan:
                case BinaryOp.LessOrEqual:
                case BinaryOp.GreaterThan:
                case BinaryOp.GreaterOrEqual:
                    if (left.IsString() && right.IsString())
                    {
                        leftDbl = StringComparer.Ordinal.Compare(left.AsString(), right.AsString());
                        rightDbl = 0;
                    }
                    else if (left.IsNumber() && right.IsNumber())
                    { 
                        leftDbl = left.AsNumber();
                        rightDbl = right.AsNumber();
                    }
                    else
                    {
                        throw new LuaException($"attempt to compare {right.Type.ToLuaName()} with {left.Type.ToLuaName()}");
                    }

                    break;

                // Arithmetic
                case BinaryOp.Addition:
                case BinaryOp.Division:
                case BinaryOp.Subtraction:
                case BinaryOp.Multiplication:
                case BinaryOp.Power:
                case BinaryOp.Modulo:
                    if (!left.TryAsDouble(out leftDbl))
                    {
                        throw new LuaException($"attempt to perform arithmetic on a {left.Type.ToLuaName()} value");
                    }

                    if (!right.TryAsDouble(out rightDbl))
                    {
                        throw new LuaException($"attempt to perform arithmetic on a {right.Type.ToLuaName()} value");
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            switch (op)
            {
                case BinaryOp.Addition:
                    return FromNumber(leftDbl + rightDbl);
                case BinaryOp.Division:
                    return FromNumber(leftDbl / rightDbl);
                case BinaryOp.Subtraction:
                    return FromNumber(leftDbl - rightDbl);
                case BinaryOp.Multiplication:
                    return FromNumber(leftDbl * rightDbl);
                case BinaryOp.Power:
                    return FromNumber(Math.Pow(leftDbl, rightDbl));
                case BinaryOp.Modulo:
                    return FromNumber(leftDbl % rightDbl);
                case BinaryOp.LessThan:
                    return leftDbl < rightDbl;
                case BinaryOp.LessOrEqual:
                    return leftDbl <= rightDbl;
                case BinaryOp.GreaterThan:
                    return leftDbl > rightDbl;
                case BinaryOp.GreaterOrEqual:
                    return leftDbl >= rightDbl;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public abstract object ToObject();

        public object ToObject(Type target)
        {
            if (typeof(LuaObject).IsAssignableFrom(target))
            {
                return this;
            }

            // Check if the object can be used directly.
            var obj = ToObject();

            if (obj == null)
            {
                return target.GetDefault();
            }

            if (target.IsInstanceOfType(obj))
            {
                return obj;
            }

            // Check if the type is nullable.
            var nullableType = Nullable.GetUnderlyingType(target);

            if (nullableType != null)
            {
                target = nullableType;
            }

            if (target == typeof(string)) return AsString();
            if (target == typeof(bool)) return AsBool();
            if (target == typeof(double)) return AsNumber();
            if (target == typeof(float)) return (float)AsNumber();
            if (target == typeof(int)) return (int)AsNumber();
            if (target == typeof(uint)) return (uint)AsNumber();
            if (target == typeof(short)) return (short)AsNumber();
            if (target == typeof(ushort)) return (ushort)AsNumber();
            if (target == typeof(long)) return (long)AsNumber();
            if (target == typeof(ulong)) return (ulong)AsNumber();
            if (target == typeof(byte)) return (byte)AsNumber();
            if (target == typeof(sbyte)) return (sbyte)AsNumber();

            throw new ArgumentException($"Could not transform the Lua Object {GetType().Name} to the type {target.Name}", nameof(target));
        }

        public static LuaObject FromBool(bool bl)
        {
            return bl ? LuaBool.True : LuaBool.False;
        }

        public static LuaObject CreateTable()
        {
            return new LuaTable();
        }

        public static LuaObject FromString(string str)
        {
            return new LuaString(str);
        }

        public static LuaObject FromNumber(double dbl)
        {
            return new LuaNumber(dbl);
        }

        public static LuaObject FromProxy(object obj)
        {
            return new LuaProxy(obj);
        }

        /// <summary>
        /// Creates a Lua object from a .NET object
        /// </summary>
        public static LuaObject FromObject(object obj)
        {
            switch (obj)
            {
                case null:
                    return LuaNil.Instance;
                case LuaObject o:
                    return o;
                case bool b1:
                    return FromBool(b1);
                case string str:
                    return FromString(str);
                case double d:
                    return FromNumber(d);
                case float f:
                    return FromNumber(f);
                case int i:
                    return FromNumber(i);
                case uint u:
                    return FromNumber(u);
                case short s:
                    return FromNumber(s);
                case ushort @ushort:
                    return FromNumber(@ushort);
                case long l:
                    return FromNumber(l);
                case ulong @ulong:
                    return FromNumber(@ulong);
                case byte b:
                    return FromNumber(b);
                case sbyte @sbyte:
                    return FromNumber(@sbyte);
                case Func<Engine, LuaArguments, LuaArguments> directFunc:
                    return FromFunction(directFunc);
                case Func<Engine, LuaArguments, CancellationToken, Task<LuaArguments>> asyncFunc:
                    return FromFunction(asyncFunc);
            }

            if (LuaProxyCache.IsValid(obj.GetType()))
            {
                return FromProxy(obj);
            }

            throw new ArgumentException("Cannot transform the object to a Lua Object", nameof(obj));
        }

        public static LuaObject FromFunction(Func<Engine, LuaArguments, CancellationToken, Task<LuaArguments>> func)
        {
            return new LuaAsyncFunction(func);
        }

        public static LuaObject FromFunction(Func<Engine, LuaArguments, LuaArguments> func)
        {
            return new LuaDirectFunction(func);
        }

        public static LuaObject FromFunction(MethodInfo method)
        {
            return MethodUtils.CreateFunction(method);
        }

        public override string ToString()
        {
            return AsString();
        }

        public virtual string AsString()
        {
            return Type.ToLuaName();
        }

        public virtual LuaObject ToNumber()
        {
            return LuaNil.Instance;
        }

        public virtual double AsNumber()
        {
            return ToNumber().ToObject() is double d ? d : double.NaN;
        }

        public virtual LuaObject ToBoolean()
        {
            return LuaBool.True;
        }

        public virtual bool AsBool()
        {
            return (bool)ToBoolean().ToObject();
        }

        public static implicit operator string(LuaObject obj)
        {
            return obj.AsString();
        }

        public static implicit operator LuaObject(string str)
        {
            return FromString(str);
        }

        public static implicit operator double(LuaObject obj)
        {
            return obj.AsNumber();
        }

        public static implicit operator LuaObject(double dbl)
        {
            return FromNumber(dbl);
        }

        public static implicit operator bool(LuaObject obj)
        {
            return obj.AsBool();
        }

        public static implicit operator LuaObject(bool str)
        {
            return str ? LuaBool.True : LuaBool.False;
        }
    }
}
