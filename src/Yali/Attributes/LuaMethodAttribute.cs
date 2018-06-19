using System;

namespace Yali.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LuaMethodAttribute : Attribute
    {
        public LuaMethodAttribute(string name = null, bool visible = true)
        {
            Name = name;
            Visible = visible;
        }

        public string Name { get; set; }

        public bool Visible { get; set; }
    }
}
