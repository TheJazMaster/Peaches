using System.Collections.Generic;
using System.Linq;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using Nickel;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;
using TheJazMaster.UnseenEffort.Artifacts;

namespace TheJazMaster.UnseenEffort.Features;

[HarmonyPatch]
public class FuryManager
{
    private static IModData ModData => ModEntry.Instance.Helper.ModData;

    internal static readonly string RemovesFuryKey = "RemovesFury";

    public FuryManager()
    {
        if (ModEntry.Instance.Helper.ModRegistry.ResolvedMods.ContainsKey("Mezz.TwosCompany"))
            ModEntry.Instance.Harmony.TryPatch(
                logger: ModEntry.Instance.Logger,
                original: AccessTools.AllAssemblies()
                    .First(a => (a.GetName().Name ?? a.GetName().FullName) == "TwosCompany")
                    .GetType("TwosCompany.Actions.AChainLightning")!
                    .GetMethod("Begin", AccessTools.all)!,
                postfix: new HarmonyMethod(GetType(), nameof(AChainLightning_Begin_Postfix))
            );
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(AAttack), nameof(AAttack.Begin))]
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
                new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(FuryManager), nameof(RemoveFury))),
            })
            .AllElements();
    }

    private static void AChainLightning_Begin_Postfix(G g, State s, Combat c, object __instance) {
        RemoveFury(g, s, c, (__instance as AAttack)!);
    }

    private static void RemoveFury(G g, State s, Combat c, AAttack attack)
    {
        if (!attack.fromDroneX.HasValue && ModData.TryGetModData<bool>(attack, RemovesFuryKey, out var value) && value) {
            Ship ship = attack.targetPlayer ? c.otherShip : s.ship;
            foreach (Artifact item in s.EnumerateAllArtifacts()) {
                if (item is IFurySpenderArtifact artifact)
                    artifact.OnFurySpend(s, c, attack, ship.Get(ModEntry.Instance.FuryStatus));
            }
            ship.Set(ModEntry.Instance.FuryStatus, 0);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Card), nameof(Card.GetActualDamage))]
    private static void Card_GetActualDamage_Postfix(State s, ref int __result, int baseDamage, bool targetPlayer = false, Card? card = null)
    {
        if (card != null) {
            Ship enemyShip = (s.route as Combat)?.otherShip!;
            Ship ship = targetPlayer ? enemyShip : s.ship;
            __result += ship.Get(ModEntry.Instance.FuryStatus);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Card), nameof(Card.GetActionsOverridden))]
    private static void Card_GetActionsOverridden_Postfix(State s, Combat c, Card __instance, ref List<CardAction> __result)
    {
        bool skip = true;
        foreach (CardAction action in __result) {
            if (action is AAttack attack && attack.DoWeHaveCannonsThough(s)) {
                if (attack.disabled) {
                    continue;
                }
                if (skip) {
                    ModData.SetModData(attack, RemovesFuryKey, true);
                    skip = false;
                    continue;
                }
                attack.damage -= s.ship.Get(ModEntry.Instance.FuryStatus);
            }    
        }
    }
}