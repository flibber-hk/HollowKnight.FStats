using System;
using System.Collections.Generic;
using System.Linq;

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
        private readonly List<DisplayInfo> _displayInfos;
        private readonly List<DisplayInfo> _globalDisplayInfos;

        private int currentIndex = 0;
        private bool currentGlobal = false;

        private List<DisplayInfo> CurrentList => currentGlobal ? _globalDisplayInfos : _displayInfos;
        private List<DisplayInfo> OtherList => currentGlobal ? _displayInfos : _globalDisplayInfos;

        private DisplayInfo Current => CurrentList[currentIndex];

        public NavigationManager(List<DisplayInfo> infos, List<DisplayInfo> globalInfos, out DisplayInfo initial)
        {
            _displayInfos = new(infos);
            _globalDisplayInfos = new(globalInfos);

            initial = infos[0];
        }

        public bool TryMove(NavigationDirection dir, out DisplayInfo next)
        {
            switch (dir)
            {
                case NavigationDirection.Left:
                    currentIndex = (currentIndex - 1 + CurrentList.Count) % CurrentList.Count;
                    next = CurrentList[currentIndex];
                    return true;
                case NavigationDirection.Right:
                    currentIndex = (currentIndex + 1) % CurrentList.Count;
                    next = CurrentList[currentIndex];
                    return true;
                case NavigationDirection.Up:
                case NavigationDirection.Down:
                    return TrySwitch(out next);
            }

            next = default;
            return false;
        }

        private bool TrySwitch(out DisplayInfo next)
        {
            if (_globalDisplayInfos.Count == 0)
            {
                next = default;
                return false;
            }

            // Cringe code but I'm not sure what the best way to do this is :zota:
            double currentPriority = Current.Priority;
            double priorityOffset = OtherList.Select(x => Math.Abs(x.Priority - currentPriority)).Min();
            int newIndex = Enumerable.Range(0, OtherList.Count)
                .Where(x => Math.Abs(OtherList[x].Priority - currentPriority) == priorityOffset)
                .First();

            currentIndex = newIndex;
            currentGlobal = !currentGlobal;

            next = Current;
            return true;
        }
    }
}
