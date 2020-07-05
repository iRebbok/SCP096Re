using EXILED;
using Harmony;
using PlayableScps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCP096Re
{
    public class SCP096Re : Plugin
    {
        public override string getName => "SCP096Re";
        public HarmonyInstance hinst;

        public override void OnDisable()
        {
            hinst.UnpatchAll();
            Events.Scp096EnrageEvent -= Events_Scp096EnrageEvent;
            Log.Info("SCP-096 Unpatched.");
        }

        public override void OnEnable()
        {
            hinst = HarmonyInstance.Create("scp096re");
            hinst.PatchAll();
            Events.Scp096EnrageEvent += Events_Scp096EnrageEvent;
            Log.Info("SCP-096 Patched.");
        }

        private void Events_Scp096EnrageEvent(ref Scp096EnrageEvent ev)
        {
            ev.Allow = false;
            if (ev.Script.Enraged)
            {
                ev.Script.AddReset();
                return;
            }
            ev.Script.SetMovementSpeed(12f);
            ev.Script.SetJumpHeight(10f);
            ev.Script.PlayerState = Scp096PlayerState.Enraged;
            ev.Script.EnrageTimeLeft = Plugin.Config.GetFloat("096_enrage_time", 15f);
        }

        public override void OnReload()
        {

        }
    }
}
