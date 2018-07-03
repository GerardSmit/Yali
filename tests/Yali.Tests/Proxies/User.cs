using System;
using System.Collections.Generic;
using System.Text;
using Yali.Attributes;

namespace Yali.Tests.Proxies
{
    [LuaClass(DefaultMethodVisible = true, DefaultPropertyAccess = LuaPropertyAccess.Both)]
    public class User
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Greet()
        {
            return $"Hello {FirstName} {LastName}";
        }
    }
}
