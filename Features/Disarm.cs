using System;
using System.Collections.Generic;
using System.Linq;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using System.Threading.Tasks;
using Nickel;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;
using Microsoft.Extensions.Logging;
using TheJazMaster.UnseenEffort.Artifacts;
using System.ComponentModel;
using Shockah.Kokoro;

namespace TheJazMaster.UnseenEffort.Features;

[HarmonyPatch]
public class DisarmManager : IKokoroApi.IV2.IStatusLogicApi.IHook
{
    private static IKokoroApi.IV2 KokoroApi => ModEntry.Instance.KokoroApi;

    public DisarmManager()
    {
		KokoroApi.StatusLogic.RegisterHook(this, 0);

        if (ModEntry.Instance.Helper.ModRegistry.ResolvedMods.ContainsKey("Mezz.TwosCompany")) {
            ModEntry.Instance.Harmony.TryPatch(
                logger: ModEntry.Instance.Logger,
                original: AccessTools.AllAssemblies()
                    .First(a => (a.GetName().Name ?? a.GetName().FullName) == "TwosCompany")
                    .GetType("TwosCompany.Actions.AChainLightning")!
                    .GetMethod("DoWeHaveCannonsThough", AccessTools.all)!,
                postfix: new HarmonyMethod(GetType(), nameof(AChainLightning_DoWeHaveCannonsThough_Postfix))
            );
            ModEntry.Instance.Harmony.TryPatch(
                logger: ModEntry.Instance.Logger,
                original: AccessTools.AllAssemblies()
                    .First(a => (a.GetName().Name ?? a.GetName().FullName) == "TwosCompany")
                    .GetType("TwosCompany.Actions.AChainLightning")!
                    .GetMethod("GetFromX", AccessTools.all)!,
                postfix: new HarmonyMethod(GetType(), nameof(AChainLightning_GetFromX_Postfix))
            );
        }
    }

    private static bool GetsDisarmed(State s, Ship ship, int damage, AAttack? attackAction = null) {
        if (ship.Get(ModEntry.Instance.DisarmedStatus) == 0 || damage > ship.Get(ModEntry.Instance.DisarmedStatus) || s.route is not Combat c) return false;
        if (ship.isPlayerShip) foreach (Artifact item in s.EnumerateAllArtifacts()) {
            if (item is IDisarmAffectorArtifact artifact)  {
                if (artifact.IgnoreDisarm(s, c, attackAction)) return false;
            }     
        }
        return true;
    }

    private static void AChainLightning_DoWeHaveCannonsThough_Postfix(object __instance, ref bool __result, State s)
    {
        var attack = (__instance as AAttack)!;
        Ship? ship = attack.targetPlayer ? (s.route as Combat)?.otherShip ?? null : s.ship;
        if (ship == null || !__result) return;
        __result = !GetsDisarmed(s, ship, attack.damage, attack);
    }
    private static void AChainLightning_GetFromX_Postfix(object __instance, ref int? __result, State s, Combat c)
    {
        var attack = (__instance as AAttack)!;
        Ship ship = attack.targetPlayer ? c.otherShip : s.ship;
        if (__result.HasValue && GetsDisarmed(s, ship, attack.damage, attack)) {
            foreach (Artifact item in s.EnumerateAllArtifacts()) {
                if (item is IOnDisarmArtifact artifact)  {
                    artifact.OnDisarm(s, c, attack);
                }     
            }
            __result = null;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AAttack), nameof(AAttack.DoWeHaveCannonsThough))]
    private static void AAttack_DoWeHaveCannonsThough_Postfix(AAttack __instance, ref bool __result, State s)
    {
        Ship? ship = __instance.targetPlayer ? (s.route as Combat)?.otherShip ?? null : s.ship;
        if (ship == null || !__result) return;
        __result = !GetsDisarmed(s, ship, __instance.damage, __instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AAttack), nameof(AAttack.GetFromX))]
    private static void AAttack_GetFromX_Postfix(AAttack __instance, ref int? __result, State s, Combat c)
    {
        Ship ship = __instance.targetPlayer ? c.otherShip : s.ship;
        if (__result.HasValue && GetsDisarmed(s, ship, __instance.damage, __instance)) {
            foreach (Artifact item in s.EnumerateAllArtifacts()) {
                if (item is IOnDisarmArtifact artifact)  {
                    artifact.OnDisarm(s, c, __instance);
                }     
            }
            __result = null;
        }
    }

    public bool HandleStatusTurnAutoStep(IKokoroApi.IV2.IStatusLogicApi.IHook.IHandleStatusTurnAutoStepArgs args)
	{
        Ship ship = args.Ship; IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming timing = args.Timing;
		if (args.Status != ModEntry.Instance.DisarmedStatus)
			return false;
		if (ship.isPlayerShip && timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnStart)
			return false;
		if (!ship.isPlayerShip && timing != IKokoroApi.IV2.IStatusLogicApi.StatusTurnTriggerTiming.TurnEnd)
			return false;

		if (args.Amount > 0)
			args.Amount /= 2;
		return false;
	}

    [HarmonyPrefix]
    [HarmonyPatch(typeof(IntentAttack), nameof(Intent.GetOtherRenderStuff))]
    private static bool IntentAttack_GetOtherRenderStuff_Prefix(State s, Vec v, IntentAttack __instance, ref int __state) {
        if (s.route is Combat c && GetsDisarmed(s, c.otherShip, Card.GetActualDamage(s, __instance.damage, targetPlayer: true))) {
            __state = __instance.damage;
            __instance.damage = -999;
            return false;
        }
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(IntentAttack), nameof(Intent.GetOtherRenderStuff))]
    private static void IntentAttack_GetOtherRenderStuff_Postfix(State s, Vec v, IntentAttack __instance, int __state) {
        if (s.route is Combat c && GetsDisarmed(s, c.otherShip, Card.GetActualDamage(s, __instance.damage, targetPlayer: true))) {
            __instance.damage = __state;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(IntentAttack), nameof(Intent.Apply))]
    private static bool IntentAttack_Apply_Prefix(State s, Combat c, Ship fromShip, int actualX, IntentAttack __instance) {
        if (GetsDisarmed(s, fromShip, Card.GetActualDamage(s, __instance.damage, targetPlayer: true))) return false;
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Card), nameof(Card.GetActionsOverridden))]
    private static void Card_GetActionsOverridden_Postfix(State s, Combat c, Card __instance, ref List<CardAction> __result) {
        Deck deck = __instance.GetMeta().deck;
        if (deck != ModEntry.Instance.PeachesDeck) return;

        foreach(CardAction action in __result) {
            action.whoDidThis = deck;
        }
    }
}