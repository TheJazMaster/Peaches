using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FSPRO;
using HarmonyLib;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Audio;
using Nanoray.PluginManager;
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


	[HarmonyPrefix]
	[HarmonyPatch(typeof(CardReward), nameof(CardReward.GetUpgrade))]
	private static void CardReward_GetUpgrade_Prefix(State s, Rand rng, MapBase zone, Card card, ref double oddsMultiplier, bool? overrideUpgradeChances = null) {
		if (s.EnumerateAllArtifacts().OfType<MultiToolArtifact>().Any()) oddsMultiplier *= 2;
	}
}

[HarmonyPatch]
public class SlowAndSteadyArtifact : Artifact, IRegisterableArtifact
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Common], helper, package, out _);
	}

	public override void OnPlayerPlayCard(int energyCost, Deck deck, Card card, State state, Combat combat, int handPosition, int handCount)
	{
		if (deck == Deck.trash) combat.Queue(new ADrawCard {
			count = 1,
			artifactPulse = Key()
		});
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AStatus), nameof(AStatus.Begin))]
	private static bool AStatus_Begin_Prefix(AStatus __instance, G g, State s, Combat c) {
		if (s.EnumerateAllArtifacts().OfType<MultiToolArtifact>().FirstOrDefault() is not { } artifact) return true;
		
		if (__instance.targetPlayer || !(__instance.status == Status.shield || __instance.status == Status.powerdrive)) return true;
		if (__instance.mode switch {
			AStatusMode.Set => __instance.statusAmount > c.otherShip.Get(__instance.status),
			_ => __instance.statusAmount > 0,
		}) return true;

		artifact.Pulse();
		Audio.Play(Event.Status_PowerDown);
		return false;
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
		}.ApplyModData(CardBrowseFilterManager.FilterSingleUse, true));
	}

	public override List<Tooltip>? GetExtraTooltips() => [
		new TTGlossary("cardtrait.singleuse")
	];
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
			List<CardAction> actions = [
				.. packagedCards.Select(card => new AAddCard {
					card = card,
					destination = CardDestination.Hand,
					artifactPulse = Key()
				}),
				new ALoseArtifact {
					artifactType = Key()
				}
			];
		}
	}

	public override List<Tooltip>? GetExtraTooltips() => packagedCards.Select(card => new TTCard {
		card = card
	}).ToList<Tooltip>();
}

public class FairTradeArtifact : Artifact, IRegisterableArtifact
{
	public int uses = 1;

	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [], helper, package, out _);
	}
}