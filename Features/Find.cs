using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using Nickel;
using TheJazMaster.UnseenEffort.Actions;

namespace TheJazMaster.UnseenEffort.Features;

[HarmonyPatch]
public class FindManager
{
    private static IModData ModData => ModEntry.Instance.Helper.ModData;

    internal static readonly string RewardSequenceKey = "CardRewardSequence";

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(Combat), nameof(Combat.PlayerWon))]
    private static IEnumerable<CodeInstruction> Combat_PlayerWon_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase originalMethod)
    {
        Label label = il.DefineLabel();
        return new SequenceBlockMatcher<CodeInstruction>(instructions)
            .Find(
                ILMatches.AnyLdloc.Anchor(out var anchor).ExtractLabels(out var labels),
                ILMatches.Ldfld("s"),
                ILMatches.Ldfld("rewardsQueue"),
                ILMatches.Newobj(typeof(ADestroyAllTempCards).GetConstructor([])!),
                ILMatches.Call("Queue")
            )
            .PointerMatcher(SequenceMatcherRelativeElement.Last)
            .Advance(1)
            .ExtractLabels(out var lbls)
            .Insert(SequenceMatcherPastBoundsDirection.Before, SequenceMatcherInsertionResultingBounds.IncludingInsertion, [
                new CodeInstruction(OpCodes.Ldarg_1).WithLabels(label).WithLabels(lbls),
                new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(FindManager), nameof(GetExtraCardRewards))),
            ])
            .Anchors()
            .PointerMatcher(anchor)
			.Insert(SequenceMatcherPastBoundsDirection.Before, SequenceMatcherInsertionResultingBounds.IncludingInsertion, [
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(labels),
                new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(FleeManager), nameof(FleeManager.ShouldKeepTempCards))),
                new(OpCodes.Brtrue, label)
            ])
            .AllElements();
    }
    
    public static void GetExtraCardRewards(G g) {
        {
            int i = 0;
            int total = g.state.ship.Get(ModEntry.Instance.CardFindStatus);
            int remaining = total;
            while (remaining > 0)
            {
                remaining--;
                i++;
                g.state.rewardsQueue.Queue(new ASequencedCardOffering
                {
                    amount = 3,
                    battleType = BattleType.Normal,
                    index = i,
                    total = total
                });
            }
        }
        {
            int i = 0;
            int total = g.state.ship.Get(ModEntry.Instance.ArtifactFindStatus);
            int remaining = total;
            while (remaining > 0)
            {
                remaining--;
                g.state.rewardsQueue.Queue(new ASequencedArtifactOffering
                {
                    amount = 2,
                    limitPools = [ArtifactPool.Common],
                    index = i,
                    total = total
                });
            }
        }
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(CardReward), nameof(CardReward.Render))]
    private static IEnumerable<CodeInstruction> CardReward_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase originalMethod) {
        return new SequenceBlockMatcher<CodeInstruction>(instructions)
            .Find(
                ILMatches.Ldstr("PICK A CARD"),
                ILMatches.Call("T")
            )
            .Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.JustInsertion, [
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(FindManager), nameof(AddCardSequenceText)))
            ])
            .AllElements();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(ArtifactReward), nameof(ArtifactReward.Render))]
    private static IEnumerable<CodeInstruction> ArtifactReward_Render_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase originalMethod) {
        return new SequenceBlockMatcher<CodeInstruction>(instructions)
            .Find(
                ILMatches.Ldstr("PICK AN ARTIFACT"),
                ILMatches.Call("T")
            )
            .Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.JustInsertion, [
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(FindManager), nameof(AddArtifactSequenceText)))
            ])
            .AllElements();
    }

    private static string AddCardSequenceText(string text, CardReward cardReward) {
        if (!ModData.TryGetModData(cardReward, RewardSequenceKey, out SequenceData data)) return text;

        string str = ModEntry.Instance.Localizations.Localize(["action", "sequencedReward", "card"]);
        if (data.Total > 1)
            return str + ModEntry.Instance.Localizations.Localize(["action", "sequencedReward", "sequence"], new {
                data.Index,
                data.Total
            });
        return str;
    }

    private static string AddArtifactSequenceText(string text, ArtifactReward artifactReward) {
        if (!ModData.TryGetModData(artifactReward, RewardSequenceKey, out SequenceData data)) return text;

        string str = ModEntry.Instance.Localizations.Localize(["action", "sequencedReward", "artifact"]);
        if (data.Total > 1)
            return str + ModEntry.Instance.Localizations.Localize(["action", "sequencedReward", "sequence"], new {
                data.Index,
                data.Total
            });
        return str;
    }

    public struct SequenceData {
        public int Index;
        public int Total;
    }
}