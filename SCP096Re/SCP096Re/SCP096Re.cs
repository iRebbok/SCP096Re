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
        public Harmony HarmonyInstance;
        public EventHandler Handler;
        public static SCP096Re Instance;

        public override void OnDisabled()
        {
            HarmonyInstance.UnpatchAll();
            Exiled.Events.Handlers.Scp096.AddingTarget -= Handler.AddTargetToScp096;
            Exiled.Events.Handlers.Scp096.Enraging -= Handler.EnrageScp096;
            Handler = null;
            Instance = null;
            Log.Info("SCP-096 Unpatched.");
        }

        public override void OnEnabled()
        {
            HarmonyInstance = new Harmony("scp096re");
            HarmonyInstance.PatchAll();
            Instance = this;
            Handler = new EventHandler();
            Exiled.Events.Handlers.Scp096.Enraging += Handler.EnrageScp096;
            Exiled.Events.Handlers.Scp096.AddingTarget += Handler.AddTargetToScp096;
            Log.Info("SCP-096 Patched.");
        }
    }
}
