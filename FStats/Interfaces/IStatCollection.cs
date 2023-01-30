using System.Collections.Generic;

namespace FStats.Interfaces
{
    /// <summary>
    /// Common interface for the Local and Global settings types, which both hold 
    /// </summary>
    public interface IStatCollection
    {
        public T Get<T>() where T : StatController;

        public IEnumerable<StatController> EnumerateActiveStats();
    }
}
