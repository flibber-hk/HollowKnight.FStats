using System.Collections.Generic;
using System.Linq;
using System.Text;
using FStats.Util;
using GlobalEnums;
using Modding;

namespace FStats.StatControllers
{
    public class TransitionStats : StatController
    {
        public Dictionary<GatePosition, int> TransitionsEntered = CollectionUtils.EmptyCounter<GatePosition>();
        public Dictionary<GatePosition, int> TransitionsExited = CollectionUtils.EmptyCounter<GatePosition>();

        public float TransitionTime;

        public static GatePosition NameToDirection(string transition)
        {
            if (transition is null) return GatePosition.unknown;

            if (transition.Contains("left")) return GatePosition.left;
            if (transition.Contains("right")) return GatePosition.right;
            if (transition.Contains("top")) return GatePosition.top;
            if (transition.Contains("bot")) return GatePosition.bottom;

            if (transition.Contains("door") || transition.StartsWith("room")) return GatePosition.door;
            
            // There are a lot of ways it can be a door
            return GatePosition.unknown;
        }

        public override void Initialize()
        {
            On.GameManager.BeginSceneTransition += RecordTransitions;

            ModHooks.HeroUpdateHook += RecordTransitionTime;
        }

        public override void Unload()
        {
            On.GameManager.BeginSceneTransition -= RecordTransitions;

            ModHooks.HeroUpdateHook -= RecordTransitionTime;
        }

        private void RecordTransitions(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            orig(self, info);

            // Assume this is a warp of some sort
            if (info.GetType() != typeof(GameManager.SceneLoadInfo)) return;

            GatePosition source = info.HeroLeaveDirection ?? GatePosition.unknown;
            TransitionsEntered[source]++;

            GatePosition target = NameToDirection(info.EntryGateName);
            TransitionsExited[target]++;
        }

        private void RecordTransitionTime()
        {
            if (HeroController.instance.transitionState == HeroTransitionState.EXITING_SCENE
                || HeroController.instance.transitionState == HeroTransitionState.WAITING_TO_ENTER_LEVEL
                || HeroController.instance.transitionState == HeroTransitionState.ENTERING_SCENE)
            {
                GameManager.instance.IncreaseGameTimer(ref TransitionTime);
            }
        }


        public override IEnumerable<DisplayInfo> GetDisplayInfos()
        {
            string LeftCol = new StringBuilder()
                .AppendLine($"Transitions Entered: {TransitionsEntered.Values.Sum()}")
                .AppendLine()
                .AppendLine($"Left: {TransitionsEntered[GatePosition.left]}")
                .AppendLine($"Right: {TransitionsEntered[GatePosition.right]}")
                .AppendLine($"Top: {TransitionsEntered[GatePosition.top]}")
                .AppendLine($"Bottom: {TransitionsEntered[GatePosition.bottom]}")
                .AppendLine($"Other: {TransitionsEntered[GatePosition.door] + TransitionsEntered[GatePosition.unknown]}")
                .ToString();

            string RightCol = new StringBuilder()
                .AppendLine($"Transitions Exited: {TransitionsExited.Values.Sum()}")
                .AppendLine()
                .AppendLine($"Left: {TransitionsExited[GatePosition.left]}")
                .AppendLine($"Right: {TransitionsExited[GatePosition.right]}")
                .AppendLine($"Top: {TransitionsExited[GatePosition.top]}")
                .AppendLine($"Bottom: {TransitionsExited[GatePosition.bottom]}")
                .AppendLine($"Other: {TransitionsExited[GatePosition.door] + TransitionsExited[GatePosition.unknown]}")
                .ToString();

            yield return new()
            {
                Title = "Transition Stats",
                MainStat = $"Time transitioning: {Common.Instance.GetTimePercentString(TransitionTime)}",
                StatColumns = new() { LeftCol, RightCol },
                Priority = BuiltinScreenPriorityValues.TransitionStats,
            };
        }
    }
}
