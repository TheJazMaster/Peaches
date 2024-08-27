using System;
using System.Collections.Generic;
using System.Linq;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using Nickel;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;

namespace TheJazMaster.Peaches.Features;
#nullable enable

public class CardBrowseFilterManager
{
	static ModEntry Instance => ModEntry.Instance;
    static Harmony Harmony => Instance.Harmony;
    static IModData ModData => Instance.Helper.ModData;
	static readonly IModCards CardsHelper = Instance.Helper.Content.Cards;

    internal const string FilterPriorityKey = "FilterPriority";
    internal const string FilterFastKey = "FilterFast";

    public CardBrowseFilterManager()
    {
        Harmony.TryPatch(
		    logger: Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(ACardSelect), nameof(ACardSelect.BeginWithRoute)),
			transpiler: new HarmonyMethod(GetType(), nameof(ACardSelect_BeginWithRoute_Transpiler))
		);
        Harmony.TryPatch(
		    logger: Instance.Logger,
		    original: AccessTools.DeclaredMethod(typeof(CardBrowse), nameof(CardBrowse.GetCardList)),
			postfix: new HarmonyMethod(GetType(), nameof(CardBrowse_GetCardList_Postfix))
		);
    }

    private static IEnumerable<CodeInstruction> ACardSelect_BeginWithRoute_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase originalMethod)
    {
        return new SequenceBlockMatcher<CodeInstruction>(instructions)
            .Find(
                ILMatches.Newobj(typeof(CardBrowse).GetConstructor([])!),
                ILMatches.Instruction(OpCodes.Dup)
            )
			.Insert(SequenceMatcherPastBoundsDirection.After, SequenceMatcherInsertionResultingBounds.IncludingInsertion, new List<CodeInstruction> {
                new(OpCodes.Dup),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(CardBrowseFilterManager), nameof(CopyDataToCardBrowse))),
            })
            .AllElements();
    }

    private static void CopyDataToCardBrowse(CardBrowse cardBrowse, ACardSelect cardSelect)
    {
        if (ModData.TryGetModData<bool>(cardSelect, FilterFastKey, out var filterFast))
            ModData.SetModData(cardBrowse, FilterFastKey, filterFast);

        if (ModData.TryGetModData<bool>(cardSelect, FilterPriorityKey, out var filterPriority))
            ModData.SetModData(cardBrowse, FilterPriorityKey, filterPriority);
    }


    private static void CardBrowse_GetCardList_Postfix(CardBrowse __instance, ref List<Card> __result, G g)
    {
        bool doesFilterFast = ModData.TryGetModData(__instance, FilterFastKey, out bool filterFast);
        bool doesFilterPriority = ModData.TryGetModData(__instance, FilterPriorityKey, out bool filterPriority);
        Combat combat = g.state.route as Combat ?? DB.fakeCombat;
        if ((doesFilterFast || doesFilterPriority) && __instance.browseSource != CardBrowse.Source.Codex) {
            __result.RemoveAll(delegate(Card c)
            {
                CardData data = c.GetDataWithOverrides(g.state);

                if (doesFilterFast) {
                    if (CardsHelper.IsCardTraitActive(g.state, c, PriorityManager.FastTrait) != filterFast)
                        return true;
                }

                if (doesFilterPriority) {
                    if (CardsHelper.IsCardTraitActive(g.state, c, PriorityManager.PriorityTrait) != filterPriority)
                        return true;
                }

                return false;
            });
        }
    }
}