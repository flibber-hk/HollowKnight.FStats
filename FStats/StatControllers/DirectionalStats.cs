using FStats.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FStats.StatControllers
{
    public class DirectionalStats : StatController
    {
        // Facing right
        // Dashes
        // Wall jumps
        // Superdashes?
        // fireballs

        public override void Initialize() { }
        public override void Unload() { }

        private IEnumerable<DisplayInfo> GetDisplayInfosBoth()
        {
            IStatCollection coll = GetOwningCollection();

            HeroActionStats has = coll.Get<HeroActionStats>();
            Common common = coll.Get<Common>();

            StringBuilder leftSB = new();
            StringBuilder rightSB = new();

            leftSB.AppendLine(common.GetTimePercentString(common.CountedTime - has.FacingRightTime, "facing left"));
            rightSB.AppendLine(common.GetTimePercentString(has.FacingRightTime, "facing right"));

            leftSB.AppendLine($"{has.DashCountLeft} left dashes");
            rightSB.AppendLine($"{has.DashCountRight} right dashes");

            leftSB.AppendLine($"{has.WallJumpCountLeft} left wall jumps");
            rightSB.AppendLine($"{has.WallJumpCountRight} right wall jumps");

            leftSB.AppendLine($"{has.SlashCountLeft} left slashes");
            rightSB.AppendLine($"{has.SlashCountRight} right slashes");

            yield return new()
            {
                Title = "Directional Stats" + SaveFileCountString(),
                StatColumns = new() { leftSB.ToString(), rightSB.ToString() },
                Priority = BuiltinScreenPriorityValues.DirectionalStats,
            };
        }

        public override IEnumerable<DisplayInfo> GetGlobalDisplayInfos() => GetDisplayInfosBoth();
        public override IEnumerable<DisplayInfo> GetDisplayInfos() => GetDisplayInfosBoth();
    }
}
