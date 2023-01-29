using System;
using UnityEngine;

namespace FStats.EndScreen
{
    public class StatScreenCycler : MonoBehaviour
    {
        public NavigationManager NavigationManager { get; set; }
        public EndScreenObjectHolder ObjectHolder { get; set; }

        public Action OnStart;

        void Start() => OnStart?.Invoke();

        void Update()
        {
            NavigationDirection dir = NavigationDirection.None;

            if (InputHandler.Instance.inputActions.left.WasPressed)
            {
                dir = NavigationDirection.Left;
            }
            else if (InputHandler.Instance.inputActions.right.WasPressed)
            {
                dir = NavigationDirection.Right;
            }
            else if (InputHandler.Instance.inputActions.up.WasPressed)
            {
                dir = NavigationDirection.Up;
            }
            else if (InputHandler.Instance.inputActions.down.WasPressed)
            {
                dir = NavigationDirection.Down;
            }

            if (dir != NavigationDirection.None && NavigationManager.TryMove(dir, out DisplayInfo next))
            {
                ObjectHolder.Display(next);
            }
        }
    }
}
