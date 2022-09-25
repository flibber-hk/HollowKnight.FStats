using System;

namespace FStats.Attributes
{
    /// <summary>
    /// Any subclass of <see cref="StatController"/> with this attribute will be excluded from
    /// the FStats Global Settings, and will always have <see cref="StatController.GetDisplayInfos"/>
    /// called by FStats.
    /// </summary>
    public class GlobalSettingsExcludeAttribute : Attribute
    {
    }
}
