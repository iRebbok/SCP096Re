using Exiled.API.Interfaces;

namespace SCP096Re
{
    public sealed class Scp096ReConfig : IConfig
    {
        public bool IsEnabled { get; set; } = true;

        public float re096_enrage_time { get; set; } = 15f;
        public bool re096_damage_add_target { get; set; } = true;
        public float re096_max_shield { get; set; } = 250f;
        public float re096_charge_time { get; set; } = 0.8f;
        public float re096_charge_cooldown { get; set; } = 6f;
        public float re096_target_enrage_add { get; set; } = 3f;
        public string re096_target_hint { get; set; } = "<i><color=red>Oh shit! I saw it's face!</color></i>";
        public int re096_shield_per_target { get; set; } = 250;
        public bool re096_charge_targets_only { get; set; } = false;
        public bool re096_hurt_targets_only { get; set; } = false;
        public float re096_attack_radius { get; set; } = 1.75f;
        public float re096_attack_cooldown { get; set; } = 0.1f;
        public float re096_calm_time { get; set; } = 6f;
        public float re096_enrage_windup_time { get; set; } = 6f;
    }
}
