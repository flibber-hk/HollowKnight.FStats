using System.Collections.Generic;

namespace FStats.EndScreen
{
    public enum NavigationDirection
    {
        None,
        Left,
        Right,
        Up,
        Down,
    }

    /// <summary>
    /// Class to manage the screens and navigation between them.
    /// </summary>
    public class NavigationManager
    {
        private List<DisplayInfo> displayInfos;
        private int current = 0;

        public NavigationManager(List<DisplayInfo> infos, out DisplayInfo initial)
        {
            displayInfos = new(infos);
            initial = infos[0];
        }

        public bool TryMove(NavigationDirection dir, out DisplayInfo next)
        {
            switch (dir)
            {
                case NavigationDirection.Left:
                    current = (current - 1 + displayInfos.Count) % displayInfos.Count;
                    next = displayInfos[current];
                    return true;
                case NavigationDirection.Right:
                    current = (current + 1) % displayInfos.Count;
                    next = displayInfos[current];
                    return true;
                case NavigationDirection.None:
                case NavigationDirection.Up:
                case NavigationDirection.Down:
                    next = default;
                    return false;
            }

            next = default;
            return false;
        }
    }
}
