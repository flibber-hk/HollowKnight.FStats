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

        public override bool TryGetDisplayInfo(out DisplayInfo info)
        {
            HeroActionStats has = FStatsMod.LS.Get<HeroActionStats>();

            StringBuilder leftSB = new();
            StringBuilder rightSB = new();

            leftSB.AppendLine(Common.Instance.GetTimePercentString(Common.Instance.CountedTime - has.FacingRightTime, "facing left"));
            rightSB.AppendLine(Common.Instance.GetTimePercentString(has.FacingRightTime, "facing right"));

            leftSB.AppendLine($"{has.DashCountLeft} left dashes");
            rightSB.AppendLine($"{has.DashCountRight} right dashes");

            leftSB.AppendLine($"{has.WallJumpCountLeft} left wall jumps");
            rightSB.AppendLine($"{has.WallJumpCountRight} right wall jumps");

            leftSB.AppendLine($"{has.SlashCountLeft} left slashes");
            rightSB.AppendLine($"{has.SlashCountRight} right slashes");

            info = new()
            {
                Title = "Directional Stats",
                StatColumns = new() { leftSB.ToString(), rightSB.ToString() }
            };
            return true;
        }
    }
}
