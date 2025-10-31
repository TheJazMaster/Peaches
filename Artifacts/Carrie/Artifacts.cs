using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using FSPRO;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nanoray.Shrike;
using Nanoray.Shrike.Harmony;
using Nickel;
using TheJazMaster.UnseenEffort.Actions;
using TheJazMaster.UnseenEffort.Features;

namespace TheJazMaster.UnseenEffort.Artifacts.Carrie;

[HarmonyPatch]
public class CatalogueArtifact : Artifact, IRegisterableArtifact
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Common], helper, package, out _);
	}

	public override int ModifyCardRewardCount(State state, bool isEvent, bool inCombat)
	{
		return 1;
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(ArtifactReward), nameof(ArtifactReward.GetOffering))]
	private static void ArtifactReward_GetOffering_Prefix(State s, ref int count, Deck? limitDeck = null, List<ArtifactPool>? limitPools = null, Rand? rngOverride = null) {
		if (s.EnumerateAllArtifacts().OfType<CatalogueArtifact>().Any()) count++;
	}
}

[HarmonyPatch]
public class MultiToolArtifact : Artifact, IRegisterableArtifact
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Common], helper, package, out _);
	}


    [HarmonyTranspiler]
    [HarmonyPatch(typeof(CardReward), nameof(CardReward.GetUpgrade))]
    private static IEnumerable<CodeInstruction> CardReward_GetUpgrade_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il, MethodBase originalMethod)
    {
        return new SequenceBlockMatcher<CodeInstruction>(instructions)
            .Find(
                ILMatches.Ldarg(2),
                ILMatches.Call("GetUpgradeChance"),
                ILMatches.AnyLdarg,
                ILMatches.Instruction(OpCodes.Mul),
                ILMatches.Stloc<double>(originalMethod)
            )
            .PointerMatcher(SequenceMatcherRelativeElement.Last)
            .Insert(SequenceMatcherPastBoundsDirection.Before, SequenceMatcherInsertionResultingBounds.IncludingInsertion, [
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.DeclaredMethod(typeof(MultiToolArtifact), nameof(GetFlatModifier))),
                new(OpCodes.Add),
            ])
            .AllElements();
    }

    private static double GetFlatModifier(State s) => s.EnumerateAllArtifacts().OfType<MultiToolArtifact>().Any() ? 0.25 : 0;
}

public class WatchArtifact : Artifact, IRegisterableArtifact
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Common], helper, package, out _);
	}

	public override void OnCombatEnd(State state)
	{
		state.rewardsQueue.QueueImmediate(new ACardSelect
		{
			browseAction = new IntermediateAction(),
			browseSource = CardBrowse.Source.Deck,
			filterTemporary = true,
			allowCloseOverride = true
		});
	}

    class IntermediateAction : CardAction {
        public override void Begin(G g, State s, Combat c)
        {
			if (selectedCard == null) return;

            s.RemoveCardFromWhereverItIs(selectedCard.uuid);
            s.rewardsQueue.Queue(new AAddCard
            {
                card = selectedCard
            });
        }
	}
}

public class ForkliftArtifact : Artifact, IRegisterableArtifact
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Boss], helper, package, out _);
	}

	public override void OnPlayerPlayCard(int energyCost, Deck deck, Card card, State state, Combat combat, int handPosition, int handCount)
	{
		if (ModEntry.Instance.Helper.Content.Cards.IsCardTraitActive(state, card, ModEntry.Instance.Helper.Content.Cards.SingleUseCardTrait) && card.upgrade != Upgrade.None) {
			Card newCard = card.CopyWithNewId();
			newCard.upgrade = Upgrade.None;
			combat.Queue(new AAddCard {
				card = newCard,
				destination = CardDestination.Discard,
				canRunAfterKill = true
			});
		}
	}

	public override void OnReceiveArtifact(State state)
	{
		state.GetCurrentQueue().QueueImmediate(new ACardSelect {
			browseAction = new AUpgrade(),
			browseSource = CardBrowse.Source.Deck,
			filterUpgrade = Upgrade.None
		}.ApplyModData(CardBrowseFilterManager.FilterSingleUse, true));
	}

	public override List<Tooltip>? GetExtraTooltips() => [
		new TTGlossary("cardtrait.singleuse")
	];
}

public class ElbowGreaseArtifact : Artifact, IRegisterableArtifact
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Common], helper, package, out _);
	}

	public override void OnReceiveArtifact(State state)
	{
        HashSet<Card> list = [.. state.deck];
		if (state.route is Combat c) {
            list.UnionWith(c.discard.Concat(c.hand).Concat(c.exhausted));
        }

		state.GetCurrentQueue().QueueImmediate(
			new AUpgradeCardSelectLimited {
				comparisonList = list
			}
		);
        state.GetCurrentQueue().QueueImmediate(
            new ARemoveCard {
                allowCancel = true
            }
        );
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(CardBrowse), nameof(CardBrowse.GetCardList))]
	private static void CardBrowse_GetCardList_Postfix(G g, CardBrowse __instance, List<Card> __result) {
		if (ModEntry.Instance.Helper.ModData.TryGetModData(__instance, AUpgradeCardSelectLimited.LimitDeckKey, out Deck data)) {
            __result.RemoveAll(card => card.GetMeta().deck != data);
        }
	}
}

public class PackageArtifact : Artifact, IRegisterableArtifact
{
	public List<Card> packagedCards = [];

	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [], helper, package, out _);
	}

	public override void OnCombatStart(State state, Combat combat)
	{
        if (state.map.markers[state.map.currentLocation].contents is MapBattle mapBattle && mapBattle.battleType == BattleType.Boss) {
            combat.Queue([
                .. packagedCards.Select(card => new AAddCard {
                    card = card,
                    destination = CardDestination.Hand,
                    timer = 1.5 / Math.Max(1, packagedCards.Count),
                    artifactPulse = Key()
                }),
                new ALoseArtifact {
                    artifactType = Key()
                }
            ]);
        }
    }

	public override List<Tooltip>? GetExtraTooltips() => [
		.. packagedCards.Select(card => new TTText {
			text = card.GetFullDisplayName()
		}),
		new TTDivider(),
		.. packagedCards.Select(card => new TTCard {
			card = card
		})
	];
}

public class RetrofittedPartsArtifact : Artifact, IRegisterableArtifact
{
    public int plusEnergy;
    public int plusDraw;

    public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [], helper, package, out _);
	}

    public override void OnRemoveArtifact(State state)
    {
        state.ship.baseDraw -= plusDraw;
		state.ship.baseEnergy -= plusEnergy;
    }

    public override void OnReceiveArtifact(State state)
    {
        state.ship.baseDraw += plusDraw;
		state.ship.baseEnergy += plusEnergy;
    }

	public override List<Tooltip>? GetExtraTooltips() => [
		new TTText {
			text = ModEntry.Instance.Localizations.Localize(["artifact", "Carrie", "RetrofittedParts", "draw"], new { Num = plusDraw })
		},
		new TTDivider(),
		new TTText {
			text = ModEntry.Instance.Localizations.Localize(["artifact", "Carrie", "RetrofittedParts", "energy"], new { Num = plusEnergy })
		}
	];
}

public class FairTradeArtifact : Artifact, IRegisterableArtifact
{
	public int uses = 1;

	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [], helper, package, out _);
	}

    public override int? GetDisplayNumber(State s) => uses;
}