using HarmonyLib;
using Hints;
using Mirror;
using PlayableScps;
using PlayableScps.Messages;
using System.Collections.Generic;
using UnityEngine;

namespace SCP096Re
{
    [HarmonyPatch(typeof(Scp096), nameof(Scp096.Charge))]
    public class Scp096PatchCharge
    {
        public static bool Prefix(Scp096 __instance)
        {
            if (!__instance.CanCharge)
            {
                return false;
            }
            __instance.SetMovementSpeed(25f);
            __instance._chargeTimeRemaining = SCP096Re.instance.Config.re096_charge_time;
            __instance._chargeCooldown = SCP096Re.instance.Config.re096_charge_cooldown;
            __instance.PlayerState = Scp096PlayerState.Charging;
            __instance.Hub.fpc.NetworkmovementOverride = new Vector2(1f, 0f);
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), "get_MaxShield")]
    public class Scp096MaxShieldPatch
    {
        public static bool Prefix(Scp096 __instance, ref float __result)
        {
            __result = SCP096Re.instance.Config.re096_max_shield;
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.OnDamage))]
    public class Scp096PatchOnDamage
    {
        public static bool Prefix(Scp096 __instance, PlayerStats.HitInfo info)
        {
            if (info.GetDamageType().isWeapon && SCP096Re.instance.Config.re096_damage_add_target)
            {
                GameObject playerObject = info.GetPlayerObject();
                if (playerObject != null && __instance.CanEnrage)
                {
                    __instance.AddTarget(playerObject);
                    __instance.Windup(false);
                }
            }
            __instance.TimeUntilShieldRecharge = 10f;
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.AddTarget))]
    public class Scp096PatchAddTarget
    {
        public static bool Prefix(Scp096 __instance, GameObject target)
        {
            ReferenceHub hub = ReferenceHub.GetHub(target);
            if (!__instance.CanReceiveTargets || hub == null || __instance._targets.Contains(hub))
            {
                return false;
            }
            if (!__instance._targets.IsEmpty<ReferenceHub>())
            {
                __instance.EnrageTimeLeft += SCP096Re.instance.Config.re096_target_enrage_add;
            }
            hub.hints.Show(new TextHint(SCP096Re.instance.Config.re096_target_hint, new HintParameter[]
                {
                    new StringHintParameter("")
                }, HintEffectPresets.FadeInAndOut(0.25f, 1f, 0f), 5f));
            __instance._targets.Add(hub);
            __instance.AdjustShield(SCP096Re.instance.Config.re096_shield_per_target);
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.ChargePlayer))]
    public class Scp096PatchChargePlayer
    {
        public static bool Prefix(Scp096 __instance, ReferenceHub player)
        {
            if (player.characterClassManager.IsAnyScp())
            {
                return false;
            }
            bool flag = __instance._targets.Contains(player);
            if (__instance.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(flag ? 9696f : 35f, player.LoggedNameFromRefHub(), DamageTypes.Scp096, __instance.Hub.queryProcessor.PlayerId), player.gameObject))
            {
                __instance._targets.Remove(player);
                NetworkServer.SendToClientOfPlayer<Scp096HitmarkerMessage>(__instance.Hub.characterClassManager.netIdentity, new Scp096HitmarkerMessage(1.35f));
                NetworkServer.SendToAll<Scp096OnKillMessage>(default(Scp096OnKillMessage), 0);
            }
            if (flag && !SCP096Re.instance.Config.re096_charge_targets_only)
            {
                //if (Plugin.Config.GetBool("096_charge_stop_by_player", false))
                __instance.EndChargeNextFrame();
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.Attack))]
    public class Scp096PatchAttack
    {
        public static bool Prefix(Scp096 __instance)
        {
            if (!__instance.CanAttack)
            {
                return false;
            }
            __instance._leftAttack = !__instance._leftAttack;
            __instance._attacking = true;
            __instance.PlayerState = Scp096PlayerState.Attacking;
            Transform playerCameraReference = __instance.Hub.PlayerCameraReference;
            Collider[] array = Physics.OverlapSphere(playerCameraReference.position + playerCameraReference.forward * 1.25f, SCP096Re.instance.Config.re096_attack_radius, LayerMask.GetMask(new string[]
            {
                "PlyCenter",
                "Door",
                "Glass"
            }));
            HashSet<GameObject> hashSet = new HashSet<GameObject>();
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
                    if (!(componentInParent3 == null) && !(componentInParent3 == __instance.Hub) && hashSet.Add(componentInParent3.gameObject) && !Physics.Linecast(__instance.Hub.transform.position, componentInParent3.transform.position, LayerMask.GetMask(new string[]
                {
                    "Default",
                    "Door",
                    "Glass"
                })))
                    {
                        num = 1.2f;
                        if ((__instance._targets.Contains(componentInParent3) && SCP096Re.instance.Config.re096_hurt_targets_only) || !SCP096Re.instance.Config.re096_hurt_targets_only)
                        {
                            if (__instance.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(9696f, __instance.Hub.LoggedNameFromRefHub(), DamageTypes.Scp096, componentInParent3.queryProcessor.PlayerId), componentInParent3.gameObject))
                            {
                                num = 1.35f;
                                __instance._targets.Remove(componentInParent3);
                                NetworkServer.SendToAll<Scp096OnKillMessage>(default(Scp096OnKillMessage), 0);
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
    public class Scp096PatchEndAttack
    {
        public static bool Prefix(Scp096 __instance)
        {
            __instance.PlayerState = Scp096PlayerState.Enraged;
            __instance._attackCooldown = SCP096Re.instance.Config.re096_attack_cooldown;
            __instance._attacking = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.EndEnrage))]
    public class Scp096PatchEndEnrage
    {
        public static bool Prefix(Scp096 __instance)
        {
            __instance.EndCharge();
            __instance.SetMovementSpeed(0f);
            __instance.SetJumpHeight(4f);
            __instance.ResetShield();
            __instance.PlayerState = Scp096PlayerState.Calming;
            __instance._calmingTime = SCP096Re.instance.Config.re096_calm_time;
            __instance._targets.Clear();
            return false;
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.Windup))]
    public class Scp096PatchWindup
    {
        public static bool Prefix(Scp096 __instance, bool force)
        {
            if (!force && (__instance.IsPreWindup || !__instance.CanEnrage))
            {
                return false;
            }
            __instance.SetMovementSpeed(0f);
            __instance.SetJumpHeight(4f);
            __instance.PlayerState = Scp096PlayerState.Enraging;
            __instance._enrageWindupRemaining = SCP096Re.instance.Config.re096_enrage_windup_time;
            return false;
        }
    }

    /*[HarmonyPatch(typeof(Scp096), nameof(Scp096.GetVisionInformation))]
    public class Scp096PatchGetVision
    {
        public static bool Prefix(Scp096 __instance, GameObject source, ref VisionInformation __result)
        {

            VisionInformation visionInformation = new VisionInformation
            {
                Source = source,
                Target = __instance.Hub.gameObject,
                RaycastHit = false,
                Looking = false
            };
            if (source == __instance.Hub.gameObject)
            {
                __result = visionInformation;
                return false;
            }
            Transform playerCameraReference = ReferenceHub.GetHub(source).PlayerCameraReference;
            Vector3 position = __instance.Hub.PlayerCameraReference.position;
            Vector3 position2 = playerCameraReference.position;
            visionInformation.Distance = Vector3.Distance(position2, position);
            float num = -Vector3.Dot((position2 - position).normalized, playerCameraReference.forward);
            float num2 = -Vector3.Dot((position - position2).normalized, __instance.Hub.PlayerCameraReference.forward);
            visionInformation.DotProduct = num;
            visionInformation.Looking = true;
            if (num < 0.64f || num2 < 0.1f)
            {
                visionInformation.Looking = false;
                __result = visionInformation;
                return false;
            }
            if (visionInformation.Distance > ((__instance.Hub.transform.position.y > 980f) ? 60f : 30f))
            {
                __result = visionInformation;
                return false;
            }
            RaycastHit raycastResult;
            if (!Physics.Raycast(visionInformation.Source.transform.position, (visionInformation.Target.transform.position - visionInformation.Source.transform.position).normalized, ref raycastResult, 60f, Scp096.VisionLayerMask))
            {
                __result = visionInformation;
                return false;
            }
            visionInformation.RaycastHit = true;
            visionInformation.RaycastResult = raycastResult;
            __result = visionInformation;
            return false;
        }
    }*/
}
