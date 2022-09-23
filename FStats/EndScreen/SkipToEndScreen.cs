using UnityEngine;

namespace FStats.EndScreen
{
    /// <summary>
    /// Class containing a utility for skipping directly to the end screen.
    /// This can only be modified through the API - it will not be made available through the FStats global settings.
    /// 
    /// If the <see cref="Enable"/> function has ever been called, then: when entering Room_temple or Room_Final_Boss_Core,
    /// while the key <see cref="EndScreenSkipKeyCode"/> is held, provided that the stat screen should display for that save file,
    /// the game will skip directly to the stat screen.
    /// </summary>
    public static class SkipToEndScreen
    {

        private static bool _skipToEndScreenEnabled = false;

        /// <summary>
        /// If this key is not held when entering black egg or thk's room, will not skip directly to the credits
        /// </summary>
        public static KeyCode EndScreenSkipKeyCode { get; set; } = KeyCode.B;

        /// <summary>
        /// If this function has not been called, will not skip directly to the credits
        /// </summary>
        public static void Enable() => _skipToEndScreenEnabled = true;

        internal static void Hook()
        {
#if DEBUG
            Enable();
#endif
            On.GameManager.BeginSceneTransition += SkipThkFight;
        }

        private static void SkipThkFight(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            if (_skipToEndScreenEnabled
                && (info.SceneName == ItemChanger.SceneNames.Room_Final_Boss_Core || info.SceneName == ItemChanger.SceneNames.Room_temple)
                && Input.GetKey(KeyCode.B)
                && EndScreen.ShouldDisplay)
            {
                info.SceneName = ItemChanger.SceneNames.End_Game_Completion;
            }

            orig(self, info);
        }

    }
}
