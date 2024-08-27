using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Nickel;
using TheJazMaster.Peaches.Actions;
using TheJazMaster.Peaches.Features;

namespace TheJazMaster.Peaches.Artifacts;

public class PunchingBag : Artifact, IPeachesArtifact, IDisarmAffectorArtifact
{
	public static void Register(IModHelper helper) {
		helper.Content.Artifacts.RegisterArtifact("PunchingBag", new() {
			ArtifactType = MethodBase.GetCurrentMethod().DeclaringType,
			Meta = new() {
				owner = ModEntry.Instance.PeachesDeck.Deck,
				pools = [ArtifactPool.Common]
			},
			Sprite = helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("Sprites/Artifacts/PunchingBag.png")).Sprite,
			Name = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "PunchingBag", "name"]).Localize,
			Description = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "PunchingBag", "description"]).Localize
		});
	}

	public bool IgnoreDisarm(State s, Combat c, AAttack attack)
	{
		return attack.whoDidThis != ModEntry.Instance.PeachesDeck.Deck;
	}

	public override List<Tooltip> GetExtraTooltips() => StatusMeta.GetTooltips(ModEntry.Instance.DisarmedStatus.Status, 1);
}

public class ForcedSoftwareUpdate : Artifact, IPeachesArtifact, IOnDisarmArtifact
{
	public static void Register(IModHelper helper) {
		helper.Content.Artifacts.RegisterArtifact("ForcedSoftwareUpdate", new() {
			ArtifactType = MethodBase.GetCurrentMethod().DeclaringType,
			Meta = new() {
				owner = ModEntry.Instance.PeachesDeck.Deck,
				pools = [ArtifactPool.Common]
			},
			Sprite = helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("Sprites/Artifacts/ForcedSoftwareUpdate.png")).Sprite,
			Name = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "ForcedSoftwareUpdate", "name"]).Localize,
			Description = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "ForcedSoftwareUpdate", "description"]).Localize
		});
	}

	public void OnDisarm(State s, Combat c, AAttack attack) {
		c.Queue(new AStatus {
			status = ModEntry.Instance.BideStatus.Status,
			statusAmount = 1,
			targetPlayer = true
		});
	}

	public override List<Tooltip> GetExtraTooltips() => [
		.. StatusMeta.GetTooltips(ModEntry.Instance.DisarmedStatus.Status, 1),
		.. StatusMeta.GetTooltips(ModEntry.Instance.BideStatus.Status, 1)
	];
}

public class FramedPhotograph : Artifact, IPeachesArtifact, IOnSurviveArtifact
{
	bool active = true;
	static Spr ActiveSprite;
	static Spr InactiveSprite;

	public static void Register(IModHelper helper) {
		ActiveSprite = helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("Sprites/Artifacts/FramedPhotograph.png")).Sprite;
		InactiveSprite = helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("Sprites/Artifacts/FramedPhotographInactive.png")).Sprite;
		helper.Content.Artifacts.RegisterArtifact("FramedPhotograph", new() {
			ArtifactType = MethodBase.GetCurrentMethod().DeclaringType,
			Meta = new() {
				owner = ModEntry.Instance.PeachesDeck.Deck,
				pools = [ArtifactPool.Common]
			},
			Sprite = ActiveSprite,
			Name = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "FramedPhotograph", "name"]).Localize,
			Description = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "FramedPhotograph", "description"]).Localize
		});
	}

	public override Spr GetSprite() => active ? ActiveSprite : InactiveSprite;

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
		active = false;
		ship.hullMax = 1;
		if (s.route is Combat c) {
			ship.Add(Status.perfectShield, 1);
			c.QueueImmediate([
				new AStatus {
					status = ModEntry.Instance.BideStatus.Status,
					statusAmount = 7,
					targetPlayer = true
				}
			]);
		}
	}

	public override List<Tooltip> GetExtraTooltips() => [
		.. StatusMeta.GetTooltips(Status.perfectShield, 1),
		.. StatusMeta.GetTooltips(ModEntry.Instance.BideStatus.Status, 7)
	];
}

public class PriorityMail : Artifact, IPeachesArtifact
{
	public static void Register(IModHelper helper) {
		helper.Content.Artifacts.RegisterArtifact("PriorityMail", new() {
			ArtifactType = MethodBase.GetCurrentMethod().DeclaringType,
			Meta = new() {
				owner = ModEntry.Instance.PeachesDeck.Deck,
				pools = [ArtifactPool.Common]
			},
			Sprite = helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("Sprites/Artifacts/PriorityMail.png")).Sprite,
			Name = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "PriorityMail", "name"]).Localize,
			Description = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "PriorityMail", "description"]).Localize
		});
	}

	public override void OnReceiveArtifact(State state)
	{
		for (int i = 0; i < 2; i++) {
			state.GetCurrentQueue().QueueImmediate(new ACardSelect {
				browseAction = new AMakeFast(),
				browseSource = CardBrowse.Source.Deck,
				filterMinCost = 1
			}.ApplyModData(CardBrowseFilterManager.FilterFastKey, false).ApplyModData(CardBrowseFilterManager.FilterPriorityKey, false));
		}
	}

	public override List<Tooltip> GetExtraTooltips() => PriorityManager.PriorityTrait.Configuration.Tooltips(DB.fakeState, null).ToList();
}

public class SurveillanceDrones : Artifact, IPeachesArtifact, IBideSpenderArtifact
{
	public static void Register(IModHelper helper) {
		helper.Content.Artifacts.RegisterArtifact("SurveillanceDrones", new() {
			ArtifactType = MethodBase.GetCurrentMethod().DeclaringType,
			Meta = new() {
				owner = ModEntry.Instance.PeachesDeck.Deck,
				pools = [ArtifactPool.Boss]
			},
			Sprite = helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("Sprites/Artifacts/SurveilanceDrones.png")).Sprite,
			Name = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "SurveillanceDrones", "name"]).Localize,
			Description = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "SurveillanceDrones", "description"]).Localize
		});
	}

	public void OnBideSpend(State s, Combat c, AAttack attack, int amount)
	{
        if (!attack.targetPlayer && s.ship.Get(ModEntry.Instance.BideStatus.Status) > 0) {
			s.ship.Add(Status.tempShield, amount);
		}
	}

	public override List<Tooltip> GetExtraTooltips() => [
		.. StatusMeta.GetTooltips(ModEntry.Instance.BideStatus.Status, 1),
		.. StatusMeta.GetTooltips(Status.tempShield, 2)
	];
}

public class BigGuns : Artifact, IPeachesArtifact, ICardDataAffectorArtifact
{
	public static void Register(IModHelper helper) {
		helper.Content.Artifacts.RegisterArtifact("BigGuns", new() {
			ArtifactType = MethodBase.GetCurrentMethod().DeclaringType,
			Meta = new() {
				owner = ModEntry.Instance.PeachesDeck.Deck,
				pools = [ArtifactPool.Boss]
			},
			Sprite = helper.Content.Sprites.RegisterSprite(ModEntry.Instance.Package.PackageRoot.GetRelativeFile("Sprites/Artifacts/BigGuns.png")).Sprite,
			Name = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "BigGuns", "name"]).Localize,
			Description = ModEntry.Instance.AnyLocalizations.Bind(["artifact", "BigGuns", "description"]).Localize
		});
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