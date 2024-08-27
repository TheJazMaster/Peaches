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
using Microsoft.Xna.Framework.Input.Touch;
using System.Globalization;
using TheJazMaster.Peaches.Artifacts;

namespace TheJazMaster.Peaches.Features;
#nullable enable

public class BideManager
{
    private static IModData ModData => ModEntry.Instance.Helper.ModData;

    internal static readonly string RemovesBideKey = "RemovesBide";

    public BideManager()
    {
        ModEntry.Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(AAttack), nameof(AAttack.Begin)),
			transpiler: new HarmonyMethod(GetType(), nameof(AAttack_Begin_Transpiler))
		);
        ModEntry.Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(Card), nameof(Card.GetActionsOverridden)),
			postfix: new HarmonyMethod(GetType(), nameof(Card_GetActionsOverridden_Postfix))
		);
        ModEntry.Instance.Harmony.TryPatch(
		    logger: ModEntry.Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(Card), nameof(Card.GetActualDamage)),
			postfix: new HarmonyMethod(GetType(), nameof(Card_GetActualDamage_Postfix))
		);
    }

    private static IEnumerable<CodeInstruction> AAttack_Begin_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase originalMethod)
    {
        return new SequenceBlockMatcher<CodeInstruction>(instructions)
            .Find(
                ILMatches.LdcI4(1),
                ILMatches.Instruction(OpCodes.Sub),
                ILMatches.Call("Set"),
                ILMatches.Ldarg(0),
                ILMatches.LdcI4(1),
                ILMatches.Stfld("stunEnemy")
            )
			.EncompassUntil(SequenceMatcherPastBoundsDirection.Before, new List<ElementMatch<CodeInstruction>> { ILMatches.Brtrue.GetBranchTarget(out var branchTarget) })
            .PointerMatcher(branchTarget)
			.ExtractLabels(out var extractedLabels)
            .Insert(SequenceMatcherPastBoundsDirection.Before, SequenceMatcherInsertionResultingBounds.IncludingInsertion, new List<CodeInstruction> {
                new CodeInstruction(OpCodes.Ldarg_1).WithLabels(extractedLabels),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldarg_3),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(BideManager), nameof(RemoveBide))),
            })
            .AllElements();
    }

    private static void RemoveBide(G g, State s, Combat c, AAttack attack)
    {
        if (!attack.fromDroneX.HasValue && ModData.TryGetModData<bool>(attack, RemovesBideKey, out var value) && value) {
            Ship ship = attack.targetPlayer ? c.otherShip : s.ship;
            foreach (Artifact item in s.EnumerateAllArtifacts()) {
                if (item is IBideSpenderArtifact artifact)
                    artifact.OnBideSpend(s, c, attack, ship.Get(ModEntry.Instance.BideStatus.Status));
            }
            ship.Set(ModEntry.Instance.BideStatus.Status, 0);
        }
    }

    private static void Card_GetActualDamage_Postfix(State s, ref int __result, int baseDamage, bool targetPlayer = false, Card? card = null)
    {
		Ship enemyShip = (s.route as Combat)?.otherShip!;
		Ship ship = targetPlayer ? enemyShip : s.ship;
        __result += ship.Get(ModEntry.Instance.BideStatus.Status);
    }

    private static void Card_GetActionsOverridden_Postfix(State s, Combat c, Card __instance, ref List<CardAction> __result)
    {
        bool skip = true;
        foreach (CardAction action in __result) {
            if (action is AAttack attack && attack.DoWeHaveCannonsThough(s)) {
                if (attack.disabled) {
                    continue;
                }
                if (skip) {
                    ModData.SetModData(attack, RemovesBideKey, true);
                    skip = false;
                    continue;
                }
                attack.damage -= s.ship.Get(ModEntry.Instance.BideStatus.Status);
            }    
        }
    }
}