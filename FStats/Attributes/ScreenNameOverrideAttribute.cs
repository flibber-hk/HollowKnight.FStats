using System;

namespace FStats.Attributes
{
    /// <summary>
    /// If this attribute is applied to a class, then it will be treated as identical
    /// to the provided type for the purposes of global settings exclusion.
    /// </summary>
    public class ScreenNameOverrideAttribute : Attribute
    {
        internal Type Type { get; set; }
        public ScreenNameOverrideAttribute(Type type)
        {
            Type = type;
        }
    }
}
