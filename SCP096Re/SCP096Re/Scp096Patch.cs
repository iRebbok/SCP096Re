using Exiled.API.Features;
using HarmonyLib;
using Mirror;
using NorthwoodLib.Pools;
using PlayableScps;
using PlayableScps.Messages;
using System.Collections.Generic;
using UnityEngine;

using Scp096 = PlayableScps.Scp096;

namespace SCP096Re
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.Charge))]
    public static class Scp096PatchCharge
    {
        public static bool Prefix(Scp096 __instance)
        {
            if (!__instance.CanCharge)
                return false;

            __instance.SetMovementSpeed(25f);
            __instance._chargeTimeRemaining = SCP096Re.Instance.Config.re096_charge_time;
            __instance._chargeCooldown = SCP096Re.Instance.Config.re096_charge_cooldown;
            __instance.PlayerState = Scp096PlayerState.Charging;
            __instance.Hub.fpc.NetworkmovementOverride = new Vector2(1f, 0f);
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.MaxShield), MethodType.Getter)]
    public static class Scp096MaxShieldPatch
    {
        public static bool Prefix(Scp096 __instance, ref float __result)
        {
            __result = SCP096Re.Instance.Config.re096_max_shield;
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.OnDamage))]
    public static class Scp096PatchOnDamage
    {
        public static bool Prefix(Scp096 __instance, PlayerStats.HitInfo info)
        {
            if (info.GetDamageType().isWeapon && SCP096Re.Instance.Config.re096_damage_add_target)
            {
                GameObject playerObject = info.GetPlayerObject();
                if (playerObject != null && __instance.CanEnrage)
                {
                    __instance.AddTarget(playerObject);
                    __instance.Windup();
                }
            }
            __instance.TimeUntilShieldRecharge = 10f;
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.ChargePlayer))]
    public static class Scp096PatchChargePlayer
    {
        public static bool Prefix(Scp096 __instance, ReferenceHub player)
        {
            if (player.characterClassManager.IsAnyScp())
                return false;

            if (Physics.Linecast(__instance.Hub.transform.position, player.transform.position, LayerMask.GetMask("Default", "Door", "Glass")))
                return false;

            if (SCP096Re.IsBlockedPlayer(Player.Get(player)) || SerpentsHand.EventHandlers.shPlayers.Contains(player.playerId))
                return false;

            bool flag = __instance._targets.Contains(player);

            if (!flag && SCP096Re.Instance.Config.re096_charge_targets_only)
                return false;

            if (__instance.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(flag ? 9696f : 35f, player.LoggedNameFromRefHub(), DamageTypes.Scp096, __instance.Hub.queryProcessor.PlayerId), player.gameObject, false))
            {
                __instance._targets.Remove(player);
                NetworkServer.SendToClientOfPlayer<Scp096HitmarkerMessage>(__instance.Hub.characterClassManager.netIdentity, new Scp096HitmarkerMessage(1.35f));
                NetworkServer.SendToAll<Scp096OnKillMessage>(default(Scp096OnKillMessage), 0);
            }
            if (flag)
            {
                __instance.EndChargeNextFrame();
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.Attack))]
    public static class Scp096PatchAttack
    {
        public static bool Prefix(Scp096 __instance)
        {
            if (!__instance.CanAttack)
                return false;

            __instance._leftAttack = !__instance._leftAttack;
            __instance._attacking = true;
            __instance.PlayerState = Scp096PlayerState.Attacking;
            Transform playerCameraReference = __instance.Hub.PlayerCameraReference;
            Collider[] array = Physics.OverlapSphere(playerCameraReference.position + (playerCameraReference.forward * 1.25f), SCP096Re.Instance.Config.re096_attack_radius, LayerMask.GetMask("PlyCenter", "Door", "Glass"));

            HashSet<GameObject> hashSet = HashSetPool<GameObject>.Shared.Rent(); /* new HashSet<GameObject>(); */
            float num = 0f;
            foreach (Collider collider in array)
            {
                Door componentInParent = collider.GetComponentInParent<Door>();
                if (componentInParent != null)
                {
                    componentInParent.DestroyDoor(true);
                    if (componentInParent.destroyed && num < 0.5f)
                    {
                        num = 1f;
                    }
                }
                else
                {
                    BreakableWindow componentInParent2 = collider.GetComponentInParent<BreakableWindow>();
                    if (componentInParent2 != null && num < 0.25f)
                    {
                        componentInParent2.ServerDamageWindow(500f);
                        num = 0.5f;
                        break;
                    }

                    ReferenceHub componentInParent3 = collider.GetComponentInParent<ReferenceHub>();
                    if (!(componentInParent3 == null) && !(componentInParent3 == __instance.Hub) && hashSet.Add(componentInParent3.gameObject) && !Physics.Linecast(__instance.Hub.transform.position, componentInParent3.transform.position, LayerMask.GetMask("Default", "Door", "Glass")))
                    {
                        num = 1.2f;
                        //>Scp096Re
                        if (
                            (__instance._targets.Contains(componentInParent3) && SCP096Re.Instance.Config.re096_hurt_targets_only)
                            || !SCP096Re.Instance.Config.re096_hurt_targets_only
                            )
                        //<Scp096Re
                        {
                            if (__instance.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(9696f, __instance.Hub.LoggedNameFromRefHub(), DamageTypes.Scp096, componentInParent3.queryProcessor.PlayerId), componentInParent3.gameObject))
                            {
                                num = 1.35f;
                                __instance._targets.Remove(componentInParent3);
                                NetworkServer.SendToAll<Scp096OnKillMessage>(default(Scp096OnKillMessage));
                            }
                        }
                    }
                }
            }
            if (num > 0f)
            {
                NetworkServer.SendToClientOfPlayer<Scp096HitmarkerMessage>(__instance.Hub.characterClassManager.netIdentity, new Scp096HitmarkerMessage(num));
            }
            __instance._attackDuration = 0.5f;
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.EndAttack))]
    public static class Scp096PatchEndAttack
    {
        public static bool Prefix(Scp096 __instance)
        {
            __instance.PlayerState = Scp096PlayerState.Enraged;
            __instance._attackCooldown = SCP096Re.Instance.Config.re096_attack_cooldown;
            __instance._attacking = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.Windup))]
    public static class Scp096PatchWindup
    {
        public static bool Prefix(Scp096 __instance, bool force)
        {
            if (!force && (__instance.IsPreWindup || !__instance.CanEnrage))
                return false;

            Scp096.takenDoors.Remove(__instance.Hub.gameObject);
            __instance.SetMovementSpeed(0f);
            __instance.SetJumpHeight(4f);
            __instance.PlayerState = Scp096PlayerState.Enraging;
            __instance._enrageWindupRemaining = SCP096Re.Instance.Config.re096_enrage_windup_time;
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.ParseVisionInformation))]
    [HarmonyPriority(Priority.HigherThanNormal)]
    public static class Scp096PatchParseVisionInformation
    {
        public static bool Prefix(Scp096 __instance, VisionInformation info)
        {
            if (info.Looking && info.RaycastHit && info.RaycastResult.transform.gameObject.TryGetComponent(out PlayableScpsController component) && component.CurrentScp != null && component.CurrentScp == __instance)
            {
                //>Scp096Re
                var player = Player.Get(info.Source);

                // scp035 compatibility
                if (SCP096Re.IsBlockedPlayer(player))
                    return false;

                if (SerpentsHand.EventHandlers.shPlayers.Contains(player.Id) && SerpentsHand.SerpentsHand.instance?.Config.CanTrigger096 == false)
                    return false;
                //<Scp096Re

                float delay = (1f - info.DotProduct) / 0.25f * (Vector3.Distance(info.Source.transform.position, info.Target.transform.position) * 0.1f);
                if (!__instance.Calming)
                {
                    __instance.AddTarget(info.Source);
                }
                if (__instance.CanEnrage && info.Source != null)
                {
                    __instance.PreWindup(delay);
                }
            }

            return false;
        }
    }
}
