using Modding;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FStats.StatControllers.ModConditional
{
    /// <summary>
    /// Base class for displays that depend on mod(s) to be installed, and will not generate a screen
    /// without the other mod(s).
    /// Code in <see cref="OnInitialize"/>, <see cref="OnUnload"/> and <see cref="ConditionalGetDisplayInfos"/>
    /// can safely require the mods to be installed.
    /// </summary>
    public abstract class ModConditionalDisplay : StatController
    {
        protected abstract IEnumerable<string> RequiredMods();
        private bool _modsAvailable;

        public virtual void OnInitialize() { }
        public sealed override void Initialize()
        {
            _modsAvailable = true;
            foreach (string mod in RequiredMods())
            {
                if (ModHooks.GetMod(mod) is null)
                {
                    _modsAvailable = false;
                    break;
                }
            }

            if (_modsAvailable) OnInitialize();
        }

        public virtual void OnUnload() { }
        public sealed override void Unload()
        {
            if (_modsAvailable) OnUnload();
        }

        public virtual IEnumerable<DisplayInfo> ConditionalGetDisplayInfos() => Enumerable.Empty<DisplayInfo>();
        public sealed override IEnumerable<DisplayInfo> GetDisplayInfos()
        {
            if (!_modsAvailable)
            {
                return Enumerable.Empty<DisplayInfo>();
            }

            return ConditionalGetDisplayInfos();
        }


        public virtual IEnumerable<DisplayInfo> ConditionalGetGlobalDisplayInfos() => Enumerable.Empty<DisplayInfo>();
        public sealed override IEnumerable<DisplayInfo> GetGlobalDisplayInfos()
        {
            if (!_modsAvailable)
            {
                return Enumerable.Empty<DisplayInfo>();
            }

            return ConditionalGetGlobalDisplayInfos();
        }
    }
}
