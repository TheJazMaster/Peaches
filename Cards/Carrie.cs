using Nickel;
using TheJazMaster.UnseenEffort.Actions;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using TheJazMaster.UnseenEffort.Features;
using Nanoray.PluginManager;
using HarmonyLib;
using daisyowl.text;
using System;

namespace TheJazMaster.UnseenEffort.Cards.Carrie;


internal sealed class ClockInCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		singleUse = true,
		temporary = true
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => upgrade == Upgrade.A ? new HashSet<ICardTraitEntry>() : [
		TiringManager.TiringTrait
	];

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.B => [
			new AAttack {
				damage = GetDmg(s, 4),
				stunEnemy = true
			},
			new AHullMax {
				amount = 2,
				targetPlayer = true
			}
		],
		_ => [
			new AAttack {
				damage = GetDmg(s, upgrade == Upgrade.A ? 8 : 4),
				stunEnemy = true
			}
		]
	};
}

internal sealed class ScroungeCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.A ? 0 : 1,
		exhaust = upgrade != Upgrade.B,
		singleUse = upgrade == Upgrade.B
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AStatus {
			status = ModEntry.Instance.CardFindStatus,
			statusAmount = upgrade == Upgrade.B ? 4 : 1,
			targetPlayer = true
		}
	];

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { MundaneManager.MundaneTrait };
}

internal sealed class SafetyMeasuresCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		singleUse = true,
		temporary = true
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => upgrade == Upgrade.A ? new HashSet<ICardTraitEntry>() : [
		TiringManager.TiringTrait
	];

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AStatus {
			status = upgrade == Upgrade.B ? Status.perfectShield : Status.shield,
			statusAmount = upgrade == Upgrade.A ? 4 : 2,
			targetPlayer = true
		},
		new AStatus {
			status = Status.evade,
			statusAmount = upgrade == Upgrade.A ? 4 : 2,
			targetPlayer = true
		}
	];
}

internal sealed class SmokeBreakCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		singleUse = upgrade != Upgrade.B,
		exhaust = upgrade == Upgrade.B,
		temporary = upgrade != Upgrade.B,
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => upgrade == Upgrade.A ? new HashSet<ICardTraitEntry>() : [
		TiringManager.TiringTrait
	];

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AStunShip()
	];
}

internal sealed class PackageCard : Card, IRegisterableCard
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out CardName);
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		singleUse = true,
		retain = true,
		description = ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "description", upgrade.ToString()])
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.A => [
			new AMultiCardSelect {
				browseSource = CardBrowse.Source.Hand,
				browseAction = new APackageCards(),
				omitFromTooltips = true,
				maxSelected = 3,
				filterUUID = uuid
			}
		],
		_ => [
			new ACardSelect {
				browseSource = CardBrowse.Source.Hand,
				browseAction = upgrade == Upgrade.B ? new ARemoveSelectedCard() : new APackageCards(),
				omitFromTooltips = true,
				filterUUID = uuid
			}
		]
	};
}

internal sealed class InventoryLogsCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out CardName);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 2 : 0,
		singleUse = true,
		temporary = true,
		description = ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "description", upgrade.ToString()])
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => upgrade == Upgrade.A ? new HashSet<ICardTraitEntry>() : [
		TiringManager.TiringTrait
	];

    public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
    {
        Upgrade.A => [
			new ACardSelect
			{
				browseAction = new ChooseCardToPutInHand(),
				browseSource = CardBrowse.Source.DrawPile,
				filterUUID = uuid,
				omitFromTooltips = true
			},
			new ACardSelect
			{
				browseAction = new ChooseCardToPutInHand(),
				browseSource = CardBrowse.Source.DrawPile,
				filterUUID = uuid,
				omitFromTooltips = true
			},
			new ACardSelect
			{
				browseAction = new ChooseCardToPutInHand(),
				browseSource = CardBrowse.Source.DrawPile,
				filterUUID = uuid,
				omitFromTooltips = true
			}
		],
        Upgrade.B => [
			new ACardSelect {
				browseAction = new CardSelectAddBuoyantForever(),
				browseSource = CardBrowse.Source.Hand,
				omitFromTooltips = true,
				filterBuoyant = false
			}
		],
        _ => [
			new ACardSelect
			{
				browseAction = new ChooseCardToPutInHand(),
				browseSource = CardBrowse.Source.DrawPile,
				filterUUID = uuid,
				omitFromTooltips = true
			},
			new ACardSelect
			{
				browseAction = new ChooseCardToPutInHand(),
				browseSource = CardBrowse.Source.DrawPile,
				filterUUID = uuid,
				omitFromTooltips = true
			}
		]
    };
}

internal sealed class FixerUpperCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 0,
		singleUse = true,
		temporary = true,
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> {
		TiringManager.TiringTrait
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.B => [
			new AShieldMax {
				amount = 1,
				targetPlayer = true
			}
		],
		Upgrade.A => [
			new AHullMax {
				amount = 2,
				targetPlayer = true,
				canRunAfterKill = true
			},
			new AHeal {
				healAmount = 2,
				targetPlayer = true,
				canRunAfterKill = true
			}
		],
		_ => [
			new AHeal {
				healAmount = 2,
				targetPlayer = true,
				canRunAfterKill = true
			}
		]
	};
}

internal sealed class UnboxingCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out CardName);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 1 : 0,
		singleUse = true,
		temporary = true,
		description = upgrade == Upgrade.B ? null : ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "description", upgrade.ToString()], new { Amount = GetAmount() })
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => upgrade == Upgrade.B ? new HashSet<ICardTraitEntry> {
		MundaneManager.MundaneTrait
	} : [];

	public int GetAmount() => upgrade == Upgrade.A ? 6 : 1;

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.B => [
			new AStatus {
				status = ModEntry.Instance.ArtifactFindStatus,
				statusAmount = 1,
				targetPlayer = true
			}
		],
		_ => [
			new ACardOffering {
				amount = GetAmount(),
				overrideUpgradeChances = true,
				inCombat = true,
				canSkip = true,
				omitFromTooltips = true
			}
		]
	};
}

internal sealed class RepurposeCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		singleUse = true,
	};

	public static int GetAmount() => 4;

	public override List<CardAction> GetActions(State s, Combat c) => [
		upgrade == Upgrade.A ? new AHurt {
			hurtAmount = 4,
			targetPlayer = true
		} : new AHullMax {
			amount = upgrade switch {
				Upgrade.B => -5,
				_ => -4
			},
			targetPlayer = true
		},
		new AStatus {
			status = ModEntry.Instance.CardFindStatus,
			statusAmount = upgrade == Upgrade.B ? 2 : 1,
			targetPlayer = true
		},
		new AStatus {
			status = ModEntry.Instance.ArtifactFindStatus,
			statusAmount = upgrade == Upgrade.B ? 2 : 1,
			targetPlayer = true
		}
	];

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { MundaneManager.MundaneTrait };
}

internal sealed class MadeToLastCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out CardName);
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		singleUse = upgrade != Upgrade.B,
		exhaust = upgrade == Upgrade.B,
		description = ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "description", upgrade.ToString()])
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.A => [
			new ARemoveTempFromDeck {
				omitFromTooltips = true
			}
		],
		_ => [
			new ACardSelect {
				browseAction = new ARemoveTemp(),
				browseSource = CardBrowse.Source.Deck,
				filterUUID = uuid,
				omitFromTooltips = true,
				filterTemporary = true
			}
		]
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { MundaneManager.MundaneTrait };
}

internal sealed class TearDownCard : Card, IRegisterableCard
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out CardName);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 0 : 1,
		singleUse = upgrade != Upgrade.B,
		exhaust = upgrade == Upgrade.B,
		description = ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "description", upgrade.ToString()])
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.B => [
			new ACardSelect {
				browseAction = new AUpgrade(),
				browseSource = CardBrowse.Source.Hand,
				filterUUID = uuid,
				omitFromTooltips = true,
				filterUpgrade = Upgrade.None
			}
		],
		_ => [
			new ACardSelect {
				browseAction = new AUnupgradeAndReimburse {
					reimburseAmount = upgrade == Upgrade.A ? 2 : 1
				},
				browseSource = CardBrowse.Source.Hand,
				filterUUID = uuid,
				omitFromTooltips = true,
			}.ApplyModData(CardBrowseFilterManager.FilterNoUpgrade, Upgrade.None)
		]
	};
}

internal sealed class ReimburseCard : Card, IRegisterableCard
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out CardName, true);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.A ? 3 : 1,
		singleUse = true,
		temporary = true,
		retain = true,
		description = ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "description", upgrade.ToString()])
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.A => [
			new AUpgradeHand()
		],
		Upgrade.B => [
			new ACardSelect {
				browseAction = new CardSelectDuplicate(),
				browseSource = CardBrowse.Source.Hand,
				filterUUID = uuid,
				omitFromTooltips = true
			}
		],
		_ => [
			new ACardSelect {
				browseAction = new AUpgrade(),
				browseSource = CardBrowse.Source.Hand,
				filterUUID = uuid,
				omitFromTooltips = true,
				filterUpgrade = Upgrade.None
			}
		]
	};
}

internal sealed class DetourCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 2 : 1,
		temporary = upgrade == Upgrade.A,
		exhaust = upgrade == Upgrade.B,
		singleUse = upgrade != Upgrade.B
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AStatus {
			status = upgrade == Upgrade.A ? ModEntry.Instance.DetourPlusStatus : ModEntry.Instance.DetourStatus,
			statusAmount = 1,
			targetPlayer = true
		}
	];

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { MundaneManager.MundaneTrait };
}

[HarmonyPatch]
internal sealed class TradeCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out CardName);
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		temporary = upgrade != Upgrade.B,
		exhaust = upgrade == Upgrade.B,
		singleUse = upgrade != Upgrade.B,
		description = ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "description", upgrade.ToString()]),
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new Action {
			cardToRefund = this,
			actionAfter = upgrade == Upgrade.B ? new AStatus {
				status = ModEntry.Instance.ArtifactFindStatus,
				statusAmount = 1,
				targetPlayer = true
			} : new AAddFairTrade {
				uses = upgrade == Upgrade.A ? 2 : 1,
			}
		}
	];

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { MundaneManager.MundaneTrait };

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Combat), nameof(Combat.IsVisible))]
	private static void Combat_IsVisible_Postfix(Combat __instance, ref bool __result)
	{
		if (__instance.routeOverride is ActionRoute)
			__result = true;
	}

	private sealed class Action : CardAction
	{
        public Card? cardToRefund;
        public required CardAction actionAfter;

		public override Route? BeginWithRoute(G g, State s, Combat c)
			=> new ActionRoute {
				cardToRefund = cardToRefund,
				actionAfter = actionAfter
			};
	}

	private sealed class ActionRoute : Route, OnMouseDown
	{
        public required Card? cardToRefund;
        public required CardAction actionAfter;
		private static readonly UK CancelExecutionUK = ModEntry.Instance.Helper.Utilities.ObtainEnumCase<UK>();
		private static readonly UK ArtifactUK = ModEntry.Instance.Helper.Utilities.ObtainEnumCase<UK>();

		public override bool GetShowOverworldPanels()
			=> true;

		public override bool CanBePeeked()
			=> false;

		public override void Render(G g)
		{
			base.Render(g);

			if (g.state.route is not Combat combat)
			{
				g.CloseRoute(this);
				return;
			}

			Draw.Rect(0, 0, MG.inst.PIX_W, MG.inst.PIX_H, Colors.black.fadeAlpha(0.5));

			var keyPrefix = $"{typeof(ModEntry).Namespace!}::{nameof(TradeCard)}";
            bool success = false;
            foreach (Artifact artifact in g.state.EnumerateAllArtifacts())
			{
				if (artifact.GetMeta().unremovable)
					continue;
                if (g.boxes.FirstOrDefault(b => b.key is { } key && key.k == StableUK.artifact && key.v == 0 && key.str == artifact.Key()) is not { } realBox)
					continue;

                success = true;

                artifact.Render(g, realBox.rect.xy);

				Box box = g.Push(new UIKey(ArtifactUK, 0, artifact.Key()), new Rect(realBox.rect.x, realBox.rect.y, realBox.rect.w, realBox.rect.h));

				box.onMouseDown = this;
				if (box.IsHover())
				{
					if (!Input.gamepadIsActiveInput)
						MouseUtil.DrawGamepadCursor(box);
					artifact.glowTimer = 0.5;
				}

				g.Pop();
			}

			if (!success) {
                Draw.Text(ModEntry.Instance.Localizations.Localize(["card", "Carrie", "Trade", "ui", "noValid"]), MG.inst.PIX_W / 2, MG.inst.PIX_H / 2 - 26, color: Colors.textMain, align: TAlign.Center);
            }

			SharedArt.ButtonText(
				g,
				new Vec(MG.inst.PIX_W - 69, MG.inst.PIX_H - 31),
				CancelExecutionUK,
				ModEntry.Instance.Localizations.Localize(["card", "Carrie", "Trade", "ui", "cancel"]),
				onMouseDown: this
			);
		}

		public void OnMouseDown(G g, Box b)
		{
			if (b.key == null) return;
			if (b.key == CancelExecutionUK) {
				if (cardToRefund != null) {
                    g.state.RemoveCardFromWhereverItIs(cardToRefund.uuid);
					if (g.state.route is Combat c) c.SendCardToHand(g.state, cardToRefund);
                }
				g.CloseRoute(this);
			}
			else if (b.key.Value.k == ArtifactUK) {
				var artifact = g.state.EnumerateAllArtifacts().FirstOrDefault(artifact => artifact.Key() == b.key.Value.str);
				if (artifact != null && g.state.route is Combat c) {
					c.QueueImmediate([
						new ALoseArtifact {
							artifactType = artifact.Key()
						},
						actionAfter
					]);
				}
				g.CloseRoute(this);
			}
		}
	}
}

internal sealed class PushThroughCard : Card, IRegisterableCard
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out CardName);
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		exhaust = upgrade != Upgrade.B,
		singleUse = upgrade == Upgrade.B,
		description = upgrade == Upgrade.B ? null : ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, state == DB.fakeState ? "descriptionAlt" : "description"], new {
			Damage = GetDmg(state, GetBaseDmg(state)),
			Reduction = GetReduction()
		})
	};

	public static int GetZonesCleared(State s) => ModEntry.Instance.Helper.ModData.GetModDataOrDefault(s, TiringManager.TiringCostKey, 0);

    public int GetReduction() => upgrade switch {
        Upgrade.A => 6,
        Upgrade.B => 0,
        _ => 4
    };

    public int GetBaseDmg(State s) => upgrade switch {
		Upgrade.A => 13,
		Upgrade.B => 9,
		_ => 9
	} - GetReduction()*GetZonesCleared(s);

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AAttack {
			damage = GetDmg(s, GetBaseDmg(s))
		}
	];
}

internal sealed class FullEffortCard : Card, IRegisterableCard
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out CardName, true);
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		singleUse = upgrade != Upgrade.B,
		retain = true,
		description = ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "description", upgrade.ToString()])
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.A => [
			new AUpgradeHand {
				tempOnly = true,
				fromEverywhere = true
			}
		],
		_ => [
			new ACardSelect {
				browseAction = new AUpgrade(),
				browseSource = CardBrowse.Source.Hand,
				filterUUID = uuid,
				omitFromTooltips = true,
				filterUpgrade = Upgrade.None,
				filterTemporary = true
			}
		]
	};
}

internal sealed class UnshakeableCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		exhaust = upgrade != Upgrade.B,
	};

	public static int? CountSingleUse(State s, Combat c) => c == DB.fakeCombat ? null :
		s.deck.Concat(c.hand).Concat(c.exhausted).Concat(c.discard).Count(card => ModEntry.Instance.Helper.Content.Cards.IsCardTraitActive(s, card, ModEntry.Instance.Helper.Content.Cards.SingleUseCardTrait));

	public override List<CardAction> GetActions(State s, Combat c) {
		int? count = CountSingleUse(s, c);
		List<CardAction> ret = [
			new AVariableHintSingleUse {
				value = count
			},
			new AStatus {
				status = Status.tempShield,
				statusAmount = count ?? 1,
				targetPlayer = true,
				xHint = 1
			}
		];
		if (upgrade == Upgrade.A) ret.Add(new AStatus {
			status = Status.evade,
			statusAmount = 1,
			targetPlayer = true,
		});
		return ret;
	}
}

internal sealed class SpecialDeliveryCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.rare, helper, package, out CardName);
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		singleUse = true,
		description = ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "description", upgrade.ToString()])
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.A => [
			new AStatus {
				status = ModEntry.Instance.ArtifactFindStatus,
				statusAmount = 1,
				targetPlayer = true
			}
		],
		_ => [
			new AStatus {
				status = ModEntry.Instance.ArtifactFindStatus,
				statusAmount = upgrade == Upgrade.B ? 2 : 1,
				targetPlayer = true
			},
			new AAddCard {
				card = new DeliveryBoxCard(),
				amount = upgrade == Upgrade.B ? 2 : 1,
				destination = CardDestination.Hand
			}
		]
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { MundaneManager.MundaneTrait };
}

internal sealed class DeliveryBoxCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, Deck.trash, charname, Rarity.rare, helper, package, out _, true);
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		exhaust = true,
		description = Loc.T("card.GenesisCanister.desc", "<c=textFaint>Seems like a wasteful amount of packaging, to be honest...")
	};

	public override List<CardAction> GetActions(State s, Combat c) => [];
}

internal sealed class HaulAssCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.rare, helper, package, out CardName);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 4 : 3,
		singleUse = upgrade != Upgrade.B,
		exhaust = upgrade == Upgrade.B,
		retain = true,
		description = ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "description", upgrade.ToString()])
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AFlee {
			targetPlayer = true,
			skipRewards = upgrade != Upgrade.A,
			keepTemp = true
		}
	];

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { MundaneManager.MundaneTrait };
}

internal sealed class ClockOutCard : Card, IRegisterableCard
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.rare, helper, package, out CardName);
	}

	public override CardData GetData(State state) => new() {
		cost = 3,
		singleUse = upgrade != Upgrade.B,
		description = ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "description", upgrade.ToString()], new {
			Amount = upgrade == Upgrade.A ? 6 : 3
		})
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new ADummyAction {
			timer = 2,
			dialogueSelector = ".clockOut"
		},
		new AStatus {
			status = StatusMeta.deckToMissingStatus[ModEntry.Instance.CarrieDeck],
			statusAmount = 9,
			mode = AStatusMode.Set,
			targetPlayer = true,
			omitFromTooltips = true
		},
		new Action {
			choiceAmount = upgrade == Upgrade.A ? 6 : 3,
			upgradedStarters = upgrade == Upgrade.B
		},
		new AStatus {
			status = StatusMeta.deckToMissingStatus[ModEntry.Instance.CarrieDeck],
			statusAmount = 0,
			mode = AStatusMode.Set,
			targetPlayer = true,
			omitFromTooltips = true
		},
	];

	private sealed class Action : CardAction
	{
		public required int choiceAmount;
		public bool upgradedStarters;

		public override Route? BeginWithRoute(G g, State s, Combat c) {
			List<Deck> choices = s.storyVars.GetUnlockedChars().Shuffle(s.rngActions).Except(
				s.characters.Where(character => character.deckType != null).Select(character => character.deckType!.Value)
			).ToList();
			if (ModEntry.Instance.EssentialsApi != null) choices.RemoveAll(ModEntry.Instance.EssentialsApi.IsBlacklistedExeOffering);
			if (ModEntry.Instance.MoreDifficultiesApi != null) choices.RemoveAll(deck => ModEntry.Instance.MoreDifficultiesApi.IsBanned(s, deck));
			choices = choices.Take(choiceAmount).ToList();

			return new ActionRoute {
				choices = choices,
				upgradedStarters = upgradedStarters
			};
		}
	}

	private sealed class ActionRoute : Route, OnMouseDown
	{
		public required List<Deck> choices;
		public bool upgradedStarters;

		public override bool GetShowOverworldPanels()
			=> true;

		public override bool CanBePeeked()
			=> true;

		public override void Render(G g)
		{
			base.Render(g);
			State s = g.state;

			if (s.route is not Combat combat)
			{
				g.CloseRoute(this);
				return;
			}

			SharedArt.DrawCore(g);
			Draw.Text(ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "choiceText"]), G.screenSize.x / 2, 47, DB.pinch, Colors.textMain, align: TAlign.Center);
			
			for (int i = 0; i < choices.Count; i++)
			{
				Deck deck = choices[i];
				Character character = new() {
					type = deck.Key(),
					deckType = deck
				};
				character.Render(g, (int)(208.0 + 64.0 * (i - (choices.Count - 1) / 2.0)), 104, onMouseDown: this);
			}
		}

		public void OnMouseDown(G g, Box b)
		{
			if (b.key == null) return;
			
			else if (b.key.Value.k == StableUK.character) {
				if (g.state.route is Combat c) {
					Deck deck = (Deck)b.key.Value.v;
					List<CardAction> actions = [
						new ARemoveCharacter {
							deck = ModEntry.Instance.CarrieDeck,
							keepOldArtifacts = true,
							keepOldCards = true
						},
						new AAddCharacter {
							deck = deck,
							timer = 0
						}
					];
					StarterDeck starters = (ModEntry.Instance.MoreDifficultiesApi != null && ModEntry.Instance.MoreDifficultiesApi.HasAltStarters(deck) && g.state.rngActions.NextInt() % 2 == 0) ?
						ModEntry.Instance.MoreDifficultiesApi.GetAltStarters(deck)! : StarterDeck.starterSets[deck];
					actions.AddRange(starters.cards.Select(card => {
						Card newCard = card.CopyWithNewId();
						if (upgradedStarters) {
							var upgradesTo = newCard.GetMeta().upgradesTo;
							newCard.upgrade = upgradesTo[g.state.rngActions.NextInt() % upgradesTo.Length];
						}
						return new AAddCard {
							card = newCard,
							timer = 0,
							destination = CardDestination.Deck,
							callItTheDeckNotTheDrawPile = true
						};
					}));
					actions.AddRange(starters.artifacts.Select(artifact => new AAddArtifact {
						artifact = Mutil.DeepCopy(artifact),
						timer = 0,
					}));
					actions.Add(new ADummyAction {
						dialogueSelector = deck.Key() + "_clock_in"
					});
					c.QueueImmediate(actions);
				}
				g.CloseRoute(this);
			}
		}
	}
}

internal sealed class RetrofitCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	private static string CardName = null!;
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.rare, helper, package, out CardName);
	}

	public override CardData GetData(State state) => new() {
		cost = 3,
		singleUse = true,
		description = ModEntry.Instance.Localizations.Localize(["card", "Carrie", CardName, "description", upgrade.ToString()])
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.A => [
			new ARetrofit {
				drawChange = 1
			}
		],
		Upgrade.B => [
			new ARetrofit {
				energyChange = 1,
				drawChange = -1
			}
		],
		_ => [
			new AHullMax {
				amount = 1,
				targetPlayer = true
			}
		]
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { MundaneManager.MundaneTrait };
}

internal sealed class BestOfTheBestCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package) {
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.rare, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.A ? 2 : 3,
		exhaust = upgrade != Upgrade.B,
		singleUse = upgrade == Upgrade.B
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AStatus {
			status = ModEntry.Instance.CardFindStatus,
			statusAmount = upgrade == Upgrade.B ? 4 : 3,
			targetPlayer = true
		},
		new AEndTurn()
	];

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { TiringManager.TiringTrait, MundaneManager.MundaneTrait };
}