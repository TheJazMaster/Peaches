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

namespace TheJazMaster.UnseenEffort.Artifacts.Peaches;

public class PunchingBagArtifact : Artifact, IRegisterableArtifact, IDisarmAffectorArtifact
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Common], helper, package, out _);
	}

	public bool IgnoreDisarm(State s, Combat c, AAttack? attack)
	{
		return attack != null && attack.whoDidThis != ModEntry.Instance.PeachesCharacter.Configuration.Deck;
	}

	public override List<Tooltip> GetExtraTooltips() => StatusMeta.GetTooltips(ModEntry.Instance.DisarmedStatus, 1);
}

[HarmonyPatch]
public class PinaColadaArtifact : Artifact, IRegisterableArtifact
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Common], helper, package, out _);
	}

	public override List<Tooltip> GetExtraTooltips() => [
		.. StatusMeta.GetTooltips(Status.shield, 1),
		.. StatusMeta.GetTooltips(ModEntry.Instance.FakeMissing1Status, 1),
	];

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Card), nameof(Card.MakeAllActionIcons))]
	private static void Card_MakeAllActionIcons_Postfix(G g, State s, Card __instance) {
		CardMeta meta = __instance.GetMeta();
		if (!s.CharacterIsMissing(meta.deck)) return;

		foreach (Artifact item in s.EnumerateAllArtifacts()) {
			if (item is not PinaColadaArtifact) continue;
			var action = new AStatus {
				status = Status.shield,
				statusAmount = 1,
				targetPlayer = true
			};
			int w = Card.RenderAction(g, s, action, dontDraw: true);
			Rect? rect = new Rect(29 - w / 2, 45, 10) + __instance.GetShakeOffset(g);
			g.Push(null, rect);
			Card.RenderAction(g, s, action, dontDraw: false, 0, 0, 0);
			g.Pop();
			break;
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Combat), nameof(Combat.SaveThemFromTheVoid))]
	private static void Combat_SaveThemFromTheVoid_Prefix(State s, Combat __instance, Deck d) {
		foreach (Artifact item in s.EnumerateAllArtifacts()) {
			if (item is not PinaColadaArtifact) continue;
			__instance.QueueImmediate(new AStatus {
				status = Status.shield,
				statusAmount = 1,
				targetPlayer = true,
				artifactPulse = item.Key()
			});
			break;
		}
	}
}

public class LifeSavingsArtifact : Artifact, IRegisterableArtifact
{
	public bool activateNextTurn = false;
	private static Spr activeSprite;
	private static Spr inactiveSprite;

	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Common], helper, package, out _, out activeSprite, out inactiveSprite);
	}

	public override Spr GetSprite() => activateNextTurn ? inactiveSprite : activeSprite;

	public override void OnReceiveArtifact(State state)
	{
		state.ship.baseEnergy++;
	}

	public override void OnRemoveArtifact(State state)
	{
		state.ship.baseEnergy--;
	}

	public override void OnTurnEnd(State state, Combat combat)
	{
		if (combat.energy <= 0)
		{
			activateNextTurn = true;
		}
	}

	public override void OnCombatEnd(State state)
	{
		activateNextTurn = false;
	}

	public override void OnTurnStart(State state, Combat combat)
	{
		if (activateNextTurn)
		{
			activateNextTurn = false;
			combat.Queue(new AEnergy
			{
				changeAmount = -1
			});
			combat.Queue(new AStatus
			{
				status = ModEntry.Instance.DisarmedStatus,
				statusAmount = 1,
				targetPlayer = true,
				artifactPulse = Key()
			});
		}
	}
}

public class FramedPhotographArtifact : Artifact, IRegisterableArtifact, IOnSurviveArtifact
{
	public bool active = true;
	private static Spr activeSprite;
	private static Spr inactiveSprite;

	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Common], helper, package, out _, out activeSprite, out inactiveSprite);
	}

	public override Spr GetSprite() => active ? activeSprite : inactiveSprite;

	public override void OnCombatStart(State state, Combat combat) {
		if (active) {
			combat.Queue(new AStatus {
				status = Status.survive,
				statusAmount = 1,
				targetPlayer = true
			});
		}
	}

	public void OnSurvive(State s, Ship ship)
	{
		if (!ship.isPlayerShip) return;

		active = false;
		if (s.route is Combat c) {
			ship.Add(Status.perfectShield, 1);
			c.QueueImmediate([
				new AStatus {
					status = ModEntry.Instance.FuryStatus,
					statusAmount = 7,
					targetPlayer = true
				}
			]);
		}
	}

	public override List<Tooltip> GetExtraTooltips() => [
		.. StatusMeta.GetTooltips(Status.perfectShield, 1),
		.. StatusMeta.GetTooltips(ModEntry.Instance.FuryStatus, 7)
	];
}

// public class SurveillanceDronesArtifact : Artifact, IRegisterableArtifact, IFurySpenderArtifact
// {
// 	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
// 	{
// 		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Boss], helper, package, out _);
// 	}

// 	public void OnFurySpend(State s, Combat c, AAttack attack, int amount)
// 	{
//         if (!attack.targetPlayer && s.ship.Get(ModEntry.Instance.FuryStatus) > 0) {
// 			s.ship.Add(Status.tempShield, amount);
// 		}
// 	}

// 	public override List<Tooltip> GetExtraTooltips() => [
// 		.. StatusMeta.GetTooltips(ModEntry.Instance.FuryStatus, 1),
// 		.. StatusMeta.GetTooltips(Status.tempShield, 2)
// 	];
// }

[HarmonyPatch]
public class BigGunsArtifact : Artifact, IRegisterableArtifact
{
	public const int threshold = 4;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Boss], helper, package, out _);
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Ship), nameof(Ship.NormalDamage))]
	private static void Ship_NormalDamage_Prefix(State s, Combat c, int incomingDamage, int? maybeWorldGridX, bool piercing, Ship __instance) {
		if (!maybeWorldGridX.HasValue || __instance.isPlayerShip) return;
		Part? part = __instance.GetPartAtWorldX(maybeWorldGridX.Value);
		if (part == null || part.intent == null) return;

		foreach (Artifact item in s.EnumerateAllArtifacts()) {
			if (item is BigGunsArtifact) {
				int damage = __instance.ModifyDamageDueToParts(s, c, incomingDamage, part, piercing);
				if (damage >= 4*threshold) {
					c.QueueImmediate(new AStunShip {
						artifactPulse = item.Key()
					});
				}
				else if (damage >= threshold) {
					c.QueueImmediate(new AStunSmart {
						part = part,
						artifactPulse = item.Key()
					});
					break;
				}
			}
		}
	}

	public override List<Tooltip>? GetExtraTooltips() => [
		new TTGlossary("action.stun"),
		new TTGlossary("action.stunShip")
	];
}

public class MegaphoneArtifact : Artifact, IRegisterableArtifact, ICardDataAffectorArtifact
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Boss], helper, package, out _);
	}

	public override void OnReceiveArtifact(State state)
	{
		state.ship.baseEnergy++;
	}

	public override void OnRemoveArtifact(State state)
	{
		state.ship.baseEnergy--;
	}

	public void AffectCardData(State s, Card card, ref CardData data)
	{
		data.cost = Math.Max(data.cost, 1);
	}
}



// public class PriorityMailArtifact : Artifact, IRegisterableArtifact
// {
// 	public bool active = true;
// 	private static Spr activeSprite;
// 	private static Spr inactiveSprite;

// 	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
// 	{
// 		IRegisterableArtifact.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, [ArtifactPool.Common], helper, package, out _, out activeSprite, out inactiveSprite);
// 	}

// 	public override Spr GetSprite() => active ? activeSprite : inactiveSprite;

// 	public override void OnPlayerPlayCard(int energyCost, Deck deck, Card card, State state, Combat combat, int handPosition, int handCount) {
// 		active = false;
// 	}

// 	public override void OnTurnStart(State state, Combat combat) {
// 		active = true;
// 	}

// 	public override List<Tooltip> GetExtraTooltips() => PriorityManager.FastTrait.Configuration.Tooltips!(DB.fakeState, null).ToList();
// }


// public class ForcedSoftwareUpdateArtifact : Artifact, IRegisterableArtifact, IOnDisarmArtifact
// {
// 	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
// 		helper.Content.Artifacts.RegisterArtifact("ForcedSoftwareUpdate", new() {
// 			ArtifactType = MethodBase.GetCurrentMethod().DeclaringType,
// 			Meta = new() {
// 				owner = ModEntry.Instance.PeachesDeck.Deck,
// 				pools = [ArtifactPool.Common]
// 			},
// 			Sprite = helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("Sprites/Artifacts/ForcedSoftwareUpdate.png")).Sprite,
// 			Name = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "ForcedSoftwareUpdate", "name"]).Localize,
// 			Description = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "ForcedSoftwareUpdate", "description"]).Localize
// 		});
// 	}

// 	public void OnDisarm(State s, Combat c, AAttack attack) {
// 		c.Queue(new AStatus {
// 			status = ModEntry.Instance.FuryStatus,
// 			statusAmount = 1,
// 			targetPlayer = true
// 		});
// 	}

// 	public override List<Tooltip> GetExtraTooltips() => [
// 		.. StatusMeta.GetTooltips(ModEntry.Instance.DisarmedStatus, 1),
// 		.. StatusMeta.GetTooltips(ModEntry.Instance.FuryStatus, 1)
// 	];
// }