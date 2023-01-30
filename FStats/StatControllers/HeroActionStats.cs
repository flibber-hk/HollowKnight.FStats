using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FStats.Util;
using GlobalEnums;
using Modding;

namespace FStats.StatControllers
{
    /// <summary>
    /// Collects stats for both the Hero Actions screen and the Directional Stats screen
    /// </summary>
    public class HeroActionStats : StatController
    {
        public int DashCount;
        public int DashCountLeft;
        public int DashCountRight;
        
        public int JumpCount;

        public int WallJumpCountLeft;
        public int WallJumpCountRight;

        public int DoubleJumpCount;
        public int PogoCount;

        public int SlashCount;
        public int SlashCountLeft;
        public int SlashCountRight;

        public int HardFalls;
        public float FacingRightTime;
        public float WallSlidingTime;
        public float DashingTime;

        public override void Initialize()
        {
            On.HeroController.HeroJump += CountJump;
            On.HeroController.HeroDash += CountDash;
            On.HeroController.DoDoubleJump += CountDoubleJump;
            On.HeroController.DoWallJump += CountWallJump;
            On.HeroController.Attack += CountAttack;

            On.HeroController.Bounce += CountPogo;
            On.HeroController.BounceHigh += CountBigPogo;
            On.HeroController.ShroomBounce += CountShroomPogo;

            On.HeroController.DoHardLanding += CountHardFalls;      // Excludes KP hard fall
            ModHooks.HeroUpdateHook += UpdateTimers;
        }

        private void CountPogo(On.HeroController.orig_Bounce orig, HeroController self)
        {
            if (!self.cState.bouncing && !self.cState.shroomBouncing && !self.controlReqlinquished) PogoCount++;
            orig(self);
        }
        private void CountBigPogo(On.HeroController.orig_BounceHigh orig, HeroController self)
        {
            if (!self.cState.bouncing && !self.controlReqlinquished) PogoCount++;
            orig(self);
        }

        private void CountShroomPogo(On.HeroController.orig_ShroomBounce orig, HeroController self)
        {
            if (!self.cState.shroomBouncing) PogoCount++;
            orig(self);
        }
        private void CountAttack(On.HeroController.orig_Attack orig, HeroController self, AttackDirection direction)
        {
            orig(self, direction); 
            SlashCount++;
            if (direction == AttackDirection.normal)
            {
                if (self.cState.facingRight && !self.cState.wallSliding || self.wallSlidingL)
                {
                    SlashCountRight++;
                }
                else if (!self.cState.facingRight && !self.cState.wallSliding || self.wallSlidingR)
                {
                    SlashCountLeft++;
                }
            }
        }

        private void CountWallJump(On.HeroController.orig_DoWallJump orig, HeroController self)
        {
            if (self.touchingWallL)
            {
                WallJumpCountLeft++;
            }
            else if (self.touchingWallR)
            {
                WallJumpCountRight++;
            }
            orig(self);
        }

        private void CountDoubleJump(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            orig(self); DoubleJumpCount++;
        }

        private void CountDash(On.HeroController.orig_HeroDash orig, HeroController self)
        {
            orig(self);
            DashCount++;

            // Not compatible with skill upgrades, but it's not totally dishonest so I'll allow it I guess
            if (!self.dashingDown)
            {
                if (self.cState.facingRight)
                {
                    DashCountRight++;
                }
                else
                {
                    DashCountLeft++;
                }
            }
        }

        private void CountJump(On.HeroController.orig_HeroJump orig, HeroController self)
        {
            orig(self); JumpCount++;
        }

        private void CountHardFalls(On.HeroController.orig_DoHardLanding orig, HeroController self)
        {
            orig(self); HardFalls++;
        }

        public void UpdateTimers()
        {
            if (HeroController.instance.cState.facingRight)
            {
                GameManager.instance.IncreaseGameTimer(ref FacingRightTime);
            }
            if (HeroController.instance.cState.wallSliding)
            {
                GameManager.instance.IncreaseGameTimer(ref WallSlidingTime);
            }
            if (HeroController.instance.cState.dashing)
            {
                GameManager.instance.IncreaseGameTimer(ref DashingTime);
            }
        }

        public override void Unload()
        {
            On.HeroController.HeroJump -= CountJump;
            On.HeroController.HeroDash -= CountDash;
            On.HeroController.DoDoubleJump -= CountDoubleJump;
            On.HeroController.DoWallJump -= CountWallJump;
            On.HeroController.Attack -= CountAttack;

            On.HeroController.Bounce -= CountPogo;
            On.HeroController.BounceHigh -= CountBigPogo;
            On.HeroController.ShroomBounce -= CountShroomPogo;

            On.HeroController.DoHardLanding -= CountHardFalls;
            ModHooks.HeroUpdateHook -= UpdateTimers;
        }

        private IEnumerable<DisplayInfo> GetDisplayInfosBoth(bool global)
        {
            StringBuilder leftcol = new();
            StringBuilder rightcol = new();

            leftcol.AppendLine($"{DashCount} dashes");
            leftcol.AppendLine($"{JumpCount} jumps");
            leftcol.AppendLine($"{WallJumpCountLeft + WallJumpCountRight} wall jumps");
            leftcol.AppendLine($"{DoubleJumpCount} double jumps");
            leftcol.AppendLine($"{PogoCount} pogo bounces");

            rightcol.AppendLine($"{SlashCount} nail slashes");
            rightcol.AppendLine($"Took {HardFalls} hard falls");

            rightcol.AppendLine($"{WallSlidingTime.PlaytimeHHMMSS()} spent wall clinging");
            rightcol.AppendLine($"{DashingTime.PlaytimeHHMMSS()} spent dashing");

            yield return new()
            {
                Title = $"Hero Actions" + SaveFileCountString(),
                StatColumns = new()
                {
                    leftcol.ToString(),
                    rightcol.ToString()
                },
                Priority = BuiltinScreenPriorityValues.HeroActionStats,
            };
        }

        public override IEnumerable<DisplayInfo> GetGlobalDisplayInfos() => GetDisplayInfosBoth(global: true);
        public override IEnumerable<DisplayInfo> GetDisplayInfos() => GetDisplayInfosBoth(global: false);
    }
}
