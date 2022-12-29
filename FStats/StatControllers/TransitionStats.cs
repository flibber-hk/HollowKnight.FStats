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
        private readonly Modding.ILogger _logger = new SimpleLogger("FStats.TransitionStats");

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
            Elevator,
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
            if (transition.ToLower().StartsWith("elev")) return TransitionType.Elevator;

            // May lead to false positives, but is unlikely
            if ((transition.Contains("door") || transition.StartsWith("room"))
                && !transition.ToLower().Contains("dream"))
            {
                return TransitionType.Door;
            }
            
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
            PlayMakerFSM fsm = self.Fsm.FsmComponent;
            string goName = fsm.gameObject.name;
            string fsmName = fsm.FsmName;

            if (fsmName == "Door Control")
            {
                _recordedLastTransition = TransitionType.Door;
            }
            else if (goName.StartsWith("Dream Enter"))
            {
                _recordedLastTransition = TransitionType.Dream;
            }
            else if (goName == "Door" && fsmName == "Great Door" && fsm.gameObject.scene.name == ItemChanger.SceneNames.Tutorial_01)
            {
                _recordedLastTransition = TransitionType.Right;
            }
            else if (fsmName == "Lift Move")
            {
                _recordedLastTransition = TransitionType.Elevator;
            }

            else
            {
                _logger.LogWarn($"Unrecognized transition: "
                    + $"{self.Fsm.FsmComponent.gameObject.name} - {self.Fsm.FsmComponent.FsmName} @ {self.State.Name}");
            }

            // orig(self) calls GM.BeginSceneTransition, so we need to execute beforehand
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

            if (target == TransitionType.Stag || target == TransitionType.Dreamgate || target == TransitionType.Dream || target == TransitionType.Elevator)
            {
                source = target;
            }
            else if (source == TransitionType.Stag || source == TransitionType.Dreamgate || source == TransitionType.Dream || target == TransitionType.Elevator)
            {
                target = source;
            }

            TransitionsEntered[source]++;
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
            bool FilterTransitionType(TransitionType t)
            {
                if (t == TransitionType.Left || t == TransitionType.Right || t == TransitionType.Top || t == TransitionType.Bottom) return true;
                if (TransitionsEntered[t] > 0 || TransitionsExited[t] > 0) return true;

                return false;
            }

            StringBuilder leftCol = new StringBuilder()
                .AppendLine($"Transitions Entered: {TransitionsEntered.Values.Sum()}");
            foreach ((TransitionType t, int count) in TransitionsEntered.Where(kvp => FilterTransitionType(kvp.Key)))
            {
                leftCol.AppendLine($"{t}: {count}");
            }

            StringBuilder rightCol = new StringBuilder()
                .AppendLine($"Transitions Exited: {TransitionsExited.Values.Sum()}");
            foreach ((TransitionType t, int count) in TransitionsExited.Where(kvp => FilterTransitionType(kvp.Key)))
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
