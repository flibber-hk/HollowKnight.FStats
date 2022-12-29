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
        public enum TransitionType
        {
            Left,
            Right,
            Top,
            Bottom,
            Door,
            Dream,
            Dreamgate,
            Stag,
            Other
        }

        public Dictionary<TransitionType, int> TransitionsEntered = CollectionUtils.EmptyCounter<TransitionType>();
        public Dictionary<TransitionType, int> TransitionsExited = CollectionUtils.EmptyCounter<TransitionType>();

        public float TransitionTime;

        public static TransitionType NameToDirection(string transition)
        {
            if (transition is null) return TransitionType.Other;

            if (transition.Contains("left")) return TransitionType.Left;
            if (transition.Contains("right")) return TransitionType.Right;
            if (transition.Contains("top")) return TransitionType.Top;
            if (transition.Contains("bot")) return TransitionType.Bottom;

            if (transition.Contains("door_stagExit")) return TransitionType.Stag;
            if (transition.Contains("door_dreamReturn")) return TransitionType.Dream;
            if (transition.ToLower().Contains("dreamgate")) return TransitionType.Dreamgate;

            // Not sure
            // if (transition.Contains("door") || transition.StartsWith("room")) return TransitionType.Door;
            
            // There are a lot of ways it can be a door
            return TransitionType.Other;
        }

        public static TransitionType FromGatePosition(GatePosition pos) => pos switch
        {
            GatePosition.right => TransitionType.Right,
            GatePosition.left => TransitionType.Left,
            GatePosition.bottom => TransitionType.Bottom,
            GatePosition.top => TransitionType.Top,
            GatePosition.door => TransitionType.Door,
            _ => TransitionType.Other
        };

        public override void Initialize()
        {
            _recordedLastTransition = null;

            On.GameManager.BeginSceneTransition += RecordTransitions;
            On.HutongGames.PlayMaker.Actions.BeginSceneTransition.OnEnter += NotifyDoorTransitions;

            ModHooks.HeroUpdateHook += RecordTransitionTime;
        }

        public override void Unload()
        {
            On.GameManager.BeginSceneTransition -= RecordTransitions;
            On.HutongGames.PlayMaker.Actions.BeginSceneTransition.OnEnter -= NotifyDoorTransitions;

            ModHooks.HeroUpdateHook -= RecordTransitionTime;
        }

        private TransitionType? _recordedLastTransition = null;

        private void NotifyDoorTransitions(On.HutongGames.PlayMaker.Actions.BeginSceneTransition.orig_OnEnter orig, HutongGames.PlayMaker.Actions.BeginSceneTransition self)
        {
            if (self.Fsm.FsmComponent.FsmName == "Door Control")
            {
                _recordedLastTransition = TransitionType.Door;
            }
            else if (self.Fsm.FsmComponent.gameObject.name.StartsWith("Dream Enter"))
            {
                _recordedLastTransition = TransitionType.Dream;
            }

            orig(self);
        }

        private void RecordTransitions(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            orig(self, info);

            // Assume this is a warp of some sort (e.g. Benchwarp, Debug save state, so we don't need to load it)
            if (info.GetType() != typeof(GameManager.SceneLoadInfo)) return;

            TransitionType source = FromGatePosition(info.HeroLeaveDirection ?? GatePosition.unknown);
            TransitionType target = NameToDirection(info.EntryGateName);

            if (_recordedLastTransition.HasValue)
            {
                source = _recordedLastTransition.Value;
                _recordedLastTransition = null;
            }

            if (target == TransitionType.Stag || target == TransitionType.Dreamgate || target == TransitionType.Dream)
            {
                source = target;
            }
            else if (source == TransitionType.Stag || source == TransitionType.Dreamgate || source == TransitionType.Dream)
            {
                target = source;
            }

            TransitionsEntered[source]++;
            TransitionsExited[target]++;

            FStatsMod.instance.Log($"{source} {target} {info.SceneName} {info.EntryGateName} {info.GetType()}");
            FStatsMod.instance.Log("");
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
            StringBuilder leftCol = new StringBuilder()
                .AppendLine($"Transitions Entered: {TransitionsEntered.Values.Sum()}")
                .AppendLine();
            foreach ((TransitionType t, int count) in TransitionsEntered)
            {
                leftCol.AppendLine($"{t}: {count}");
            }

            StringBuilder rightCol = new StringBuilder()
                .AppendLine($"Transitions Exited: {TransitionsExited.Values.Sum()}")
                .AppendLine();
            foreach ((TransitionType t, int count) in TransitionsExited)
            {
                rightCol.AppendLine($"{t}: {count}");
            }


            yield return new()
            {
                Title = "Transition Stats",
                MainStat = $"Time transitioning: {Common.Instance.GetTimePercentString(TransitionTime)}",
                StatColumns = new() { leftCol.ToString(), rightCol.ToString() },
                Priority = BuiltinScreenPriorityValues.TransitionStats,
            };
        }
    }
}
