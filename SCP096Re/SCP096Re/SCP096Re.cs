using Exiled.API.Features;
using HarmonyLib;
using System.Collections.Generic;

using Scp096Events = Exiled.Events.Handlers.Scp096;
using ServerEvents = Exiled.Events.Handlers.Server;

namespace SCP096Re
{
    public sealed class SCP096Re : Plugin<Scp096ReConfig>
    {
        public static SCP096Re Instance { get; } = new SCP096Re();

        public readonly Harmony HarmonyInstance = new Harmony($"VirtualBrightPlayz.{nameof(SCP096Re)}");
        public readonly EventHandler Handler = new EventHandler();

        private SCP096Re() { }

        public override void OnDisabled()
        {
            base.OnDisabled();

            HarmonyInstance.UnpatchAll();

            Scp096Events.AddingTarget -= Handler.AddTargetToScp096;
            Scp096Events.Enraging -= Handler.EnrageScp096;
            Scp096Events.CalmingDown -= Handler.OnScp096CalmingDown;

            ServerEvents.WaitingForPlayers -= Handler.OnWaitingForPlayers;

            Log.Info("SCP-096 Unpatched.");
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            HarmonyInstance.PatchAll();

            Scp096Events.Enraging += Handler.EnrageScp096;
            Scp096Events.AddingTarget += Handler.AddTargetToScp096;
            Scp096Events.CalmingDown += Handler.OnScp096CalmingDown;

            ServerEvents.WaitingForPlayers += Handler.OnWaitingForPlayers;

            Log.Info("SCP-096 Patched.");
        }

        public static bool IsBlockedPlayer(Player player)
        {
            // SCP-035 plugin compatible
            if (player.RankColor == "red" && player.RankName == "SCP-035")
                return true;

            return false;
        }
    }
}
