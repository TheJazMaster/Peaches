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
using TheJazMaster.Peaches.Artifacts;
using System.ComponentModel;

namespace TheJazMaster.Peaches.Features;
#nullable enable

public class DisarmManager : IStatusLogicHook
{
    private static IKokoroApi KokoroApi => ModEntry.Instance.KokoroApi;

    public DisarmManager()
    {
		ModEntry.Instance.KokoroApi.RegisterStatusLogicHook(this, 0);

        ModEntry.Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(AAttack), nameof(AAttack.DoWeHaveCannonsThough)),
			postfix: new HarmonyMethod(GetType(), nameof(AAttack_DoWeHaveCannonsThough_Postfix))
		);
        ModEntry.Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(AAttack), nameof(AAttack.GetFromX)),
			postfix: new HarmonyMethod(GetType(), nameof(AAttack_GetFromX_Postfix))
		);
        ModEntry.Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(IntentAttack), nameof(IntentAttack.GetOtherRenderStuff)),
			prefix: new HarmonyMethod(GetType(), nameof(IntentAttack_GetOtherRenderStuff_Prefix)),
            postfix: new HarmonyMethod(GetType(), nameof(IntentAttack_GetOtherRenderStuff_Postfix))
		);
        ModEntry.Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(IntentAttack), nameof(IntentAttack.Apply)),
            prefix: new HarmonyMethod(GetType(), nameof(IntentAttack_Apply_Prefix))
		);
        ModEntry.Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(Card), nameof(Card.GetActionsOverridden)),
            postfix: new HarmonyMethod(GetType(), nameof(Card_GetActionsOverridden_Postfix))
		);
    }

    private static void AAttack_DoWeHaveCannonsThough_Postfix(AAttack __instance, ref bool __result, State s)
    {
        Ship? ship = __instance.targetPlayer ? (s.route as Combat)?.otherShip ?? null : s.ship;
        if (ship == null || !__result) return;

        if (ship.Get(ModEntry.Instance.DisarmedStatus.Status) > 0) {
            foreach (Artifact item in s.EnumerateAllArtifacts()) {
                if (item is IDisarmAffectorArtifact artifact)  {
                    if (artifact.IgnoreDisarm(s, s.route as Combat, __instance)) return;
                }     
            }
            __result = false;
        }
    }

    private static void AAttack_GetFromX_Postfix(AAttack __instance, ref int? __result, State s, Combat c)
    {
        Ship ship = __instance.targetPlayer ? c.otherShip : s.ship;
        if (__result.HasValue && ship.Get(ModEntry.Instance.DisarmedStatus.Status) > 0) {
            foreach (Artifact item in s.EnumerateAllArtifacts()) {
                if (item is IDisarmAffectorArtifact artifact)  {
                    if (artifact.IgnoreDisarm(s, c, __instance)) {
                        return;
                    }
                }     
            }

            foreach (Artifact item in s.EnumerateAllArtifacts()) {
                if (item is IOnDisarmArtifact artifact)  {
                    artifact.OnDisarm(s, c, __instance);
                }     
            }
            __result = null;
        }
    }

    public bool HandleStatusTurnAutoStep(State state, Combat combat, StatusTurnTriggerTiming timing, Ship ship, Status status, ref int amount, ref StatusTurnAutoStepSetStrategy setStrategy)
	{
		if (status != ModEntry.Instance.DisarmedStatus.Status)
			return false;
		if (ship.isPlayerShip && timing != StatusTurnTriggerTiming.TurnStart)
			return false;
		if (!ship.isPlayerShip && timing != StatusTurnTriggerTiming.TurnEnd)
			return false;

		if (amount > 0)
			amount = Math.Max(amount - 1, 0);
		return false;
	}

    private static bool IntentAttack_GetOtherRenderStuff_Prefix(State s, Vec v, IntentAttack __instance, ref int __state) {
        if (s.route is Combat c && c.otherShip.Get(ModEntry.Instance.DisarmedStatus.Status) > 0) {
            __state = __instance.damage;
            __instance.damage = -999;
            return false;
        }
        return true;
    }

    private static void IntentAttack_GetOtherRenderStuff_Postfix(State s, Vec v, IntentAttack __instance, int __state) {
        if (s.route is Combat c && c.otherShip.Get(ModEntry.Instance.DisarmedStatus.Status) > 0) {
            __instance.damage = __state;
        }
    }

    private static bool IntentAttack_Apply_Prefix(State s, Combat c, Ship fromShip, int actualX) {
        if (fromShip.Get(ModEntry.Instance.DisarmedStatus.Status) > 0) return false;
        return true;
    }

    private static void Card_GetActionsOverridden_Postfix(State s, Combat c, Card __instance, ref List<CardAction> __result) {
        Deck deck = __instance.GetMeta().deck;
        if (deck != ModEntry.Instance.PeachesDeck.Deck) return;

        foreach(CardAction action in __result) {
            action.whoDidThis = deck;
        }
    }
}