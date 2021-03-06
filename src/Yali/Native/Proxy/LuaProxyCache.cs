﻿using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Yali.Attributes;
using Yali.Native.Value;

namespace Yali.Native.Proxy
{
    internal static class LuaProxyCache
    {
        private static readonly ConcurrentDictionary<Type, LuaProxyCacheItem> CacheItems = new ConcurrentDictionary<Type, LuaProxyCacheItem>();

        public static bool IsValid(Type type)
        {
            return type.IsClass && type.GetCustomAttribute<LuaClassAttribute>() != null;
        }

        public static LuaProxyCacheItem Get(Type type)
        {
            if (!type.IsClass)
            {
                throw new ArgumentException("Expected the argument to be a class", nameof(type));
            }

            if (type.GetCustomAttribute<LuaClassAttribute>() == null)
            {
                throw new ArgumentException($"The proxy should should have the {nameof(LuaClassAttribute)}", nameof(type));
            }

            return CacheItems.GetOrAdd(type, CacheItemFactory);
        }

        private static LuaProxyCacheItem CacheItemFactory(Type type)
        {
            var classAttr = type.GetCustomAttribute<LuaClassAttribute>();
            var propertyDefaultVisible = classAttr?.DefaultPropertyAccess ?? LuaPropertyAccess.None;
            var methodDefaultVisible = classAttr?.DefaultMethodVisible ?? false;

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Concat(type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Select(m =>
                {
                    var attr = m.GetCustomAttributeDeep<LuaMethodAttribute>();

                    return new
                    {
                        Name = attr?.Name ?? m.Name.ToLower(CultureInfo.InvariantCulture),
                        Visible = attr?.Visible ?? methodDefaultVisible,
                        Info = m
                    };
                })
                .Where(m => m.Visible);

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Select(p =>
                {
                    var attr = p.GetCustomAttributeDeep<LuaPropertyAttribute>();
                    var access = attr?.Access ?? propertyDefaultVisible;

                    return new LuaProxyCacheItemProperty
                    {
                        Name = attr?.Name ?? p.Name.ToLower(CultureInfo.InvariantCulture),
                        Writeable = p.CanWrite && access.HasFlag(LuaPropertyAccess.Writeable),
                        Readable = p.CanRead && access.HasFlag(LuaPropertyAccess.Readable),
                        IsStatic = p.GetAccessors(true)[0].IsStatic,
                        Info = p
                    };
                })
                .Where(p => p.Readable || p.Writeable)
                .ToArray();

            return new LuaProxyCacheItem
            {
                Methods = methods.ToDictionary(m => m.Name, m => LuaObject.FromFunction(m.Info)),
                Properties = properties.Where(p => !p.IsStatic).ToDictionary(p => p.Name, p => p),
                StaticProperties = properties.Where(p => p.IsStatic).ToDictionary(p => p.Name, p => p)
            };
        }
    }
}
