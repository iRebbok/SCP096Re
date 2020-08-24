using Exiled.API.Features;
using HarmonyLib;

namespace SCP096Re
{
    public sealed class SCP096Re : Plugin<Scp096ReConfig>
    {
        public override string Author => "VirtualBrightPlayz";

        public static SCP096Re Instance { get; } = new SCP096Re();

        public readonly Harmony HarmonyInstance = new Harmony($"VirtualBrightPlayz.{nameof(SCP096Re)}");
        public readonly EventHandler Handler = new EventHandler();

        private SCP096Re() { }

        public override void OnDisabled()
        {
            base.OnDisabled();

            HarmonyInstance.UnpatchAll();

            Exiled.Events.Handlers.Scp096.AddingTarget -= Handler.AddTargetToScp096;
            Exiled.Events.Handlers.Scp096.Enraging -= Handler.EnrageScp096;

            Log.Info("SCP-096 Unpatched.");
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            HarmonyInstance.PatchAll();

            Exiled.Events.Handlers.Scp096.Enraging += Handler.EnrageScp096;
            Exiled.Events.Handlers.Scp096.AddingTarget += Handler.AddTargetToScp096;

            Log.Info("SCP-096 Patched.");
        }
    }
}
