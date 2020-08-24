using Exiled.Events.EventArgs;
using Hints;
using PlayableScps;

namespace SCP096Re
{
    public class EventHandler
    {
        public void AddTargetToScp096(AddingTargetEventArgs ev)
        {
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
            ev.Scp096.EnrageTimeLeft = SCP096Re.Instance.Config.re096_enrage_time;
        }
    }
}
