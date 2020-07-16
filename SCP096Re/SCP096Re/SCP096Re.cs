using Exiled.API;
using Exiled.API.Features;
using Exiled.Events;
using Exiled;
using HarmonyLib;
using PlayableScps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.Events.EventArgs;

namespace SCP096Re
{
    public class SCP096Re : Plugin<Config>
    {
        public override string Name => "SCP096Re";
        public override string Author => "VirtualBrightPlayz";
        public override Version Version => new Version(1, 0, 0);
        public Harmony hinst;
        public static SCP096Re instance;

        public override void OnDisabled()
        {
            base.OnDisabled();
            hinst.UnpatchAll();
            Exiled.Events.Handlers.Scp096.Enraging -= Events_Scp096EnrageEvent;
            instance = null;
            Log.Info("SCP-096 Unpatched.");
        }

        public override void OnEnabled()
        {
            base.OnEnabled();
            hinst = new Harmony("scp096re");
            hinst.PatchAll();
            Exiled.Events.Handlers.Scp096.Enraging += Events_Scp096EnrageEvent;
            instance = this;
            Log.Info("SCP-096 Patched.");
        }

        private void Events_Scp096EnrageEvent(EnragingEventArgs ev)
        {
            ev.IsAllowed = false;
            if (ev.Scp096.Enraged)
            {
                ev.Scp096.AddReset();
                return;
            }
            ev.Scp096.SetMovementSpeed(12f);
            ev.Scp096.SetJumpHeight(10f);
            ev.Scp096.PlayerState = Scp096PlayerState.Enraged;
            ev.Scp096.EnrageTimeLeft = Config.re096_enrage_time;
        }
    }
}
