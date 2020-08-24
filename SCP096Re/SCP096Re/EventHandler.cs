using Exiled.Events.EventArgs;
using HarmonyLib;
using Hints;
using PlayableScps;

namespace SCP096Re
{
    public sealed class EventHandler
    {
        public void AddTargetToScp096(AddingTargetEventArgs ev)
        {
            // Exiled tutorials trigger feature
            if (!ev.IsAllowed)
                return;

            ev.EnrageTimeToAdd = SCP096Re.Instance.Config.re096_target_enrage_add;
            ev.AhpToAdd = SCP096Re.Instance.Config.re096_shield_per_target;

            ev.Target.ReferenceHub.hints.Show(new TextHint(SCP096Re.Instance.Config.re096_target_hint, new HintParameter[]
            {
                new StringHintParameter("")
            }, HintEffectPresets.FadeInAndOut(0.25f, 1f, 0f), 5f));
        }

        public void EnrageScp096(EnragingEventArgs ev)
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
            ev.Scp096.EnrageTimeLeft = SCP096Re.Instance.Config.re096_enrage_time; // Scp096Re
        }

        public void OnScp096CalmingDown(CalmingDownEventArgs ev)
        {
            ev.IsAllowed = false;

            ev.Scp096.EndCharge();
            ev.Scp096.SetMovementSpeed(0f);
            ev.Scp096.SetJumpHeight(4f);
            ev.Scp096.ResetShield();
            ev.Scp096.PlayerState = Scp096PlayerState.Calming;
            ev.Scp096._calmingTime = SCP096Re.Instance.Config.re096_calm_time; // Scp096Re
            ev.Scp096._targets.Clear();
        }

        bool patched = false;
        public void OnWaitingForPlayers()
        {
            if (patched)
                return;

            SCP096Re.Instance.HarmonyInstance.Unpatch(AccessTools.Method(typeof(PlayableScps.Scp096), nameof(PlayableScps.Scp096.ParseVisionInformation)), HarmonyPatchType.Prefix, "cyanox.serpentshand");
            patched = true;
        }
    }
}
