using System;

namespace FStats.Attributes
{
    /// <summary>
    /// Any subclass of <see cref="StatController"/> with this attribute will not have the results 
    /// of <see cref="StatController.GetDisplayInfos"/> shown on the ending screen unless
    /// the user has manually set the value in the FStatsMod global settings to true.
    /// </summary>
    public class DefaultHiddenScreenAttribute : Attribute
    {
    }
}
