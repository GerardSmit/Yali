using System;
using System.Collections.Generic;
using System.Text;
using Yali.Attributes;

namespace Yali.Tests.Proxies
{
    [LuaClass(DefaultMethodVisible = true)]
    public class UserManager
    {
        public static User Create(string firstName, string lastName)
        {
            return new User
            {
                FirstName = firstName,
                LastName = lastName
            };
        }
    }
}
