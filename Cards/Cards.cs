using Nickel;
using TheJazMaster.Peaches.Actions;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using static TheJazMaster.Peaches.Features.PriorityManager;
using TheJazMaster.Peaches.Features;

namespace TheJazMaster.Peaches.Cards;


internal sealed class HitEmCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("HitEm", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.common,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "HitEm", "name"]).Localize,
			Art = StableSpr.cards_DrakeCannon
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		retain = upgrade == Upgrade.B
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.A => [
			new AAttack
			{
				damage = GetDmg(s, 4)
			},
			new AStatus
			{
				status = ModEntry.Instance.BideStatus.Status,
				statusAmount = 1,
				targetPlayer = true
			}
		],
		_ => [
			new AAttack
			{
				damage = GetDmg(s, 3)
			},
			new AStatus
			{
				status = ModEntry.Instance.BideStatus.Status,
				statusAmount = 1,
				targetPlayer = true
			}
		]
	};
}


internal sealed class WhereItHurtsCard : Card, IPeachesCard, IHasCustomCardTraits
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("WhereItHurts", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.common,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "WhereItHurts", "name"]).Localize,
			Art = StableSpr.cards_DrakeTech
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		description = upgrade == Upgrade.B ? ModEntry.Instance.Localizations.Localize(["card", "WhereItHurts", "description", "B"]) :
			ModEntry.Instance.Localizations.Localize(["card", "WhereItHurts", "description", upgrade.ToString(), HasRuinedPriority(state, this) ? "noPriority" : "priority"])
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => upgrade == Upgrade.B ? [] : new HashSet<ICardTraitEntry> { PriorityTrait };

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.A => PrioritySet(s, this, [
			new ARandomBrittle
			{
				single = true
			},
			new AStatus
			{
				status = ModEntry.Instance.BideStatus.Status,
				statusAmount = 1,
				targetPlayer = true
			}
		], [
			new ARandomBrittle
			{
				single = true,
				weakInstead = true
			},
			new AStatus
			{
				status = ModEntry.Instance.BideStatus.Status,
				statusAmount = 1,
				targetPlayer = true
			}
		]),
		Upgrade.B => [
			new ARandomBrittle
			{
				single = true,
				hidden = true,
				count = 2
			}
		],
		_ => PrioritySet(s, this, [
			new ARandomBrittle
			{
				single = true
			}
		], [
			new ARandomBrittle
			{
				single = true,
				weakInstead = true
			}
		])
	};
}


internal sealed class SwerveCard : Card, IPeachesCard, IHasCustomCardTraits
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("Swerve", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.common,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "Swerve", "name"]).Localize
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		flippable = upgrade == Upgrade.A,
		art = flipped ? StableSpr.cards_ScootRight : StableSpr.cards_ScootLeft
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { PriorityTrait };

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.B => PrioritySet(s, this, [
			new AMove
			{
				dir = -3,
				targetPlayer = true,
			},
			new AStatus
			{
				status = ModEntry.Instance.BideStatus.Status,
				statusAmount = 1,
				targetPlayer = true
			}
		], [
			new AMove
			{
				dir = -1,
				targetPlayer = true,
			}
		]),
		_ => PrioritySet(s, this, [
			new AMove
			{
				dir = -3,
				targetPlayer = true,
			}
		], [
			new AMove
			{
				dir = -1,
				targetPlayer = true,
			}
		]),
	};
}


internal sealed class SimmerCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("Simmer", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.common,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "Simmer", "name"]).Localize,
			Art = StableSpr.cards_Heat
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		exhaust = upgrade != Upgrade.B
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AStatus
		{
			status = ModEntry.Instance.BideStatus.Status,
			statusAmount = upgrade == Upgrade.A ? 2 : 1,
			targetPlayer = true
		},
		new AStatus
		{
			status = Status.energyNextTurn,
			statusAmount = 1,
			targetPlayer = true
		}
	];
}


internal sealed class AngerManagementCard : Card, IPeachesCard, IHasCustomCardTraits
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("AngerManagement", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.common,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "AngerManagement", "name"]).Localize,
			Art = StableSpr.cards_AggressiveArmoring
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 1
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { PriorityTrait };

	public override List<CardAction> GetActions(State s, Combat c) => PrioritySet(s, this, [
		new AStatus
		{
			status = Status.shield,
			statusAmount = upgrade == Upgrade.B ? 3 : 2,
			targetPlayer = true
		},
		new ADrawCard
		{
			count = upgrade == Upgrade.None ? 2 : 3
		},
		new AStatus
		{
			status = ModEntry.Instance.DisarmedStatus.Status,
			statusAmount = upgrade == Upgrade. B ? 2 : 1,
			targetPlayer = true
		}
	], [
		new AStatus
		{
			status = Status.shield,
			statusAmount = 1,
			targetPlayer = true
		}
	]);
}


internal sealed class FinisherCard : Card, IPeachesCard, IHasCustomCardTraits
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("Finisher", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.common,
				upgradesTo = [Upgrade.A, Upgrade.B],
				dontOffer = true
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "Finisher", "name"]).Localize,
			Art = StableSpr.cards_Cannon
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 2
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => upgrade == Upgrade.B ? new HashSet<ICardTraitEntry> { PriorityTrait } : [];

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.B => PrioritySet(s, this, [
			new AAttack
			{
				damage = GetDmg(s, 8)
			},
			new AEndTurn()
		], [
			new AAttack
			{
				damage = GetDmg(s, 4)
			},
			new AEndTurn()
		]),
		_ => [
			new AAttack
			{
				damage = GetDmg(s, upgrade == Upgrade.A ? 6 : 4)
			},
			new AEndTurn()
		]
	};
}


internal sealed class CatharsisCard : Card, IPeachesCard, IHasCustomCardTraits
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("Catharsis", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.common,
				upgradesTo = [Upgrade.A, Upgrade.B],
				dontOffer = true
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "Catharsis", "name"]).Localize,
			Art = StableSpr.cards_Cannon
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 1
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { PriorityTrait };

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.B => PrioritySet(s, this, [
			new AStatus {
				status = ModEntry.Instance.DisarmedStatus.Status,
				statusAmount = 3,
				targetPlayer = true
			},
			new AStatus {
				status = ModEntry.Instance.BideStatus.Status,
				statusAmount = 4,
				targetPlayer = true
			}
		], [
			new AStatus {
				status = ModEntry.Instance.DisarmedStatus.Status,
				statusAmount = -2,
				targetPlayer = true
			}
		]),
		_ => PrioritySet(s, this, [
			new AStatus {
				status = ModEntry.Instance.DisarmedStatus.Status,
				statusAmount = 2,
				targetPlayer = true
			},
			new AStatus {
				status = ModEntry.Instance.BideStatus.Status,
				statusAmount = upgrade == Upgrade.A ? 3 : 2,
				targetPlayer = true
			}
		], [
			new AStatus {
				status = ModEntry.Instance.DisarmedStatus.Status,
				statusAmount = -2,
				targetPlayer = true
			}
		])
	};
}


internal sealed class SurpriseShotCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("SurpriseShot", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.common,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "SurpriseShot", "name"]).Localize,
			Art = StableSpr.cards_Cannon
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		retain = upgrade == Upgrade.B
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.A => [
			new AStatus
			{
				status = ModEntry.Instance.DisarmedStatus.Status,
				mode = AStatusMode.Set,
				statusAmount = 0,
				targetPlayer = true
			},
			new AAttack
			{
				damage = GetDmg(s, 1)
			}
		],
		_ => [
			new AStatus	
			{
				status = ModEntry.Instance.DisarmedStatus.Status,
				statusAmount = -2,
				targetPlayer = true,
			},
			new AAttack
			{
				damage = GetDmg(s, 1)
			}
		]
	};
}


internal sealed class AngerCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("Anger", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.common,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "Anger", "name"]).Localize,
			Art = StableSpr.cards_AggressiveArmoring
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		recycle = upgrade == Upgrade.B
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AStatus
		{
			status = ModEntry.Instance.BideStatus.Status,
			statusAmount = 1,
			targetPlayer = true
		},
		new AStatus
		{
			status = Status.tempShield,
			statusAmount = upgrade == Upgrade.A ? 2 : 1,
			targetPlayer = true
		},
		new AStatus
		{
			status = ModEntry.Instance.DisarmedStatus.Status,
			statusAmount = 1,
			targetPlayer = true
		}
	];
}


internal sealed class HellsFuryCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("HellsFury", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.common,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "HellsFury", "name"]).Localize,
			Art = StableSpr.cards_Heat
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		retain = true,
		flippable = true,
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.B => [
			new AMove
			{
				dir = -3,
				targetPlayer = true
			},
			new AStatus
			{
				status = ModEntry.Instance.BideStatus.Status,
				statusAmount = 1,
				targetPlayer = true
			}
		],
		_ => [
			new AMove
			{
				dir = -1,
				targetPlayer = true
			},
			new AStatus
			{
				status = ModEntry.Instance.BideStatus.Status,
				statusAmount = upgrade == Upgrade.A ? 2 : 1,
				targetPlayer = true
			}
		]
	};
}



// UNCOMMONS

internal sealed class OvertimeCard : Card, IPeachesCard, IHasCustomCardTraits
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("Overtime", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.uncommon,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "Overtime", "name"]).Localize,
			Art = StableSpr.cards_Terminal
		});
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 0 : 1,
		exhaust = upgrade == Upgrade.B,
		description = ModEntry.Instance.Localizations.Localize(["card", "Overtime", "description", upgrade.ToString(), HasRuinedPriority(state, this) ? "noPriority" : "priority"]),
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { PriorityTrait };

	public override List<CardAction> GetActions(State s, Combat c)
	{
		var drawAction1 = new ADrawCard();
		var drawAction2 = new ADrawCard();

		return PrioritySet(s, this, [
			new ADiscardAttacks
			{
				drawAction = drawAction1,
				flat = upgrade == Upgrade.A ? 3 : 2
			},
			drawAction1
		], [
			new ADiscardAttacks
			{
				drawAction = drawAction2,
				flat = upgrade == Upgrade.A ? 2 : 1
			},
			drawAction2
		]);
	}
}


internal sealed class BottleUpCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("BottleUp", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.uncommon,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "BottleUp", "name"]).Localize,
			Art = ModEntry.Instance.BottleUpArt,
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 0,
		exhaust = upgrade != Upgrade.B,
		description = ModEntry.Instance.Localizations.Localize(["card", "BottleUp", "description", upgrade.ToString()]),
	};

	public override List<CardAction> GetActions(State s, Combat c) {
		int bideAmt = s.ship.Get(ModEntry.Instance.BideStatus.Status);

		return [
			new AStatus
			{
				status = ModEntry.Instance.BideStatus.Status,
				statusAmount = 0,
				mode = AStatusMode.Set,
				targetPlayer = true
			},
			new AAddCard
			{
				card = new BreakTheBottleCard {
					amount = bideAmt,
					discount = upgrade == Upgrade.A ? -1 : 0,
					upgrade = upgrade == Upgrade.B ? Upgrade.B : Upgrade.None
				},
				showCardTraitTooltips = true
			}
		];
	}
}


internal sealed class SmashControlsCard : Card, IPeachesCard, IHasCustomCardTraits
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("SmashControls", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.uncommon,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "SmashControls", "name"]).Localize,
			Art = StableSpr.cards_CorruptedCore
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		exhaust = true
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { PriorityTrait };

	public override List<CardAction> GetActions(State s, Combat c) => PrioritySet(s, this, [
		new AEnergy
		{
			changeAmount = 2
		},
		new AStatus
		{
			status = Status.energyNextTurn,
			statusAmount = upgrade == Upgrade.A ? 2 : 1,
			targetPlayer = true
		},
		new AStatus
		{
			status = ModEntry.Instance.DisarmedStatus.Status,
			statusAmount = 1,
			targetPlayer = true
		}
	], [
		new AEnergy
		{
			changeAmount = upgrade == Upgrade.B ? 2 : 1
		}
	]);
}


internal sealed class YouCantHideCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("YouCantHide", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.uncommon,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "YouCantHide", "name"]).Localize,
			Art = StableSpr.cards_WeakenHull
		});
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.A ? 1 : 2,
		exhaust = true
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.B => [
			new AStatus
			{
				status = Status.reflexiveCoating,
				statusAmount = 99,
				targetPlayer = false
			},
			new AAttack
			{
				damage = GetDmg(s, 4),
				piercing = true,
				brittle = true
			}
		],
		_ => [
			new AStatus
			{
				status = Status.reflexiveCoating,
				statusAmount = 99,
				targetPlayer = false
			},
			new AAttack
			{
				damage = GetDmg(s, 1),
				piercing = true,
				brittle = true
			}
		]
	};
}


internal sealed class LeaveAMessageCard : Card, IPeachesCard, IHasCustomCardTraits
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("LeaveAMessage", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.uncommon,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "LeaveAMessage", "name"]).Localize,
			Art = StableSpr.cards_Desktop
		});
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.A ? 1 : 2,
		retain = upgrade == Upgrade.B,
		exhaust = true
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { PriorityTrait };

	public override List<CardAction> GetActions(State s, Combat c) => PrioritySet(s, this, [
		new AStatus
		{
			status = ModEntry.Instance.DisarmedStatus.Status,
			statusAmount = 1
		},
		new AStatus
		{
			status = ModEntry.Instance.DisarmedStatus.Status,
			statusAmount = 1,
			targetPlayer = true
		}
	], [
		new AStatus
		{
			status = Status.tempShield,
			statusAmount = 1,
			targetPlayer = true
		}
	]);
}


internal sealed class LetMeAtEmCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("LetMeAtEm", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.uncommon,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "LetMeAtEm", "name"]).Localize,
			Art = StableSpr.cards_WeakenHull
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		exhaust = true
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.B => [
			new AAttack
			{
				damage = GetDmg(s, 2),
				piercing = true,
				brittle = true
			}
		],
		_ => [
			new AAttack
			{
				damage = GetDmg(s, upgrade == Upgrade.A ? 5 : 2),
				piercing = true,
				brittle = true
			},
			new AStatus
			{
				status = ModEntry.Instance.PeachesCharacter.MissingStatus.Status,
				statusAmount = 1,
				targetPlayer = true
			}
		]
	};
}


internal sealed class ViolentDaydreamsCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("ViolentDaydreams", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.uncommon,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "ViolentDaydreams", "name"]).Localize,
			Art = StableSpr.cards_WeakenHull
		});
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.A ? 0 : 1,
		exhaust = upgrade == Upgrade.B,
		description = ModEntry.Instance.Localizations.Localize(["card", "ViolentDaydreams", "description", upgrade.ToString()]),
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.B => [
			new ADrawAllAttacks()
		],
		_ => [
			new ADrawAllAttacks(),
			new AStatus
			{
				status = ModEntry.Instance.DisarmedStatus.Status,
				statusAmount = 1,
				targetPlayer = true
			}
		]
	};
}


// RARES

internal sealed class FuckOffCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("FuckOff", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.rare,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "FuckOff", "name"]).Localize,
			Art = StableSpr.cards_Zoom
		});
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade switch {
			Upgrade.A => 3,
			_ => 4
		},
		exhaust = true
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.B => [
			new AAttack
			{
				damage = GetDmg(s, 2),
				moveEnemy = 7
			},
			new AStatus
			{
				status = Status.lockdown,
				statusAmount = 1,
				targetPlayer = false
			}
		],
		_ => [
			new AAttack
			{
				damage = GetDmg(s, 2),
				moveEnemy = 7
			},
			new AStatus
			{
				status = Status.lockdown,
				statusAmount = 1,
				targetPlayer = false
			},
			new AStatus
			{
				status = Status.drawLessNextTurn,
				statusAmount = 2,
				targetPlayer = true
			}
		]
	};
}


internal sealed class HardWorkerCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("HardWorker", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.rare,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "HardWorker", "name"]).Localize,
			Art = StableSpr.cards_StunCharge
		});
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 3 : 2,
		exhaust = true
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.A => [
			new AStatus
			{
				status = ModEntry.Instance.HardWorkerStatus.Status,
				statusAmount = 1,
				targetPlayer = true
			},
			new AStatus
			{
				status = Status.drawNextTurn,
				statusAmount = 2,
				targetPlayer = true
			}
		],
		Upgrade.B => [
			new AStatus
			{
				status = ModEntry.Instance.HardWorkerStatus.Status,
				statusAmount = 2,
				targetPlayer = true
			}
		],
		_ => [
			new AStatus
			{
				status = ModEntry.Instance.HardWorkerStatus.Status,
				statusAmount = 1,
				targetPlayer = true
			}
		]
	};
}


internal sealed class TheCalmCard : Card, IPeachesCard, IHasCustomCardTraits
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("TheCalm", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.rare,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "TheCalm", "name"]).Localize,
			Art = StableSpr.cards_TrashFumes
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		exhaust = true,
		description = ModEntry.Instance.Localizations.Localize(["card", "TheCalm", "description", upgrade.ToString(), HasRuinedPriority(state, this) ? "noPriority" : "priority"]),
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { PriorityTrait };

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AAddCard
		{
			card = new TheStormCard {
				upgrade = upgrade == Upgrade.A ? Upgrade.A : Upgrade.None
			},
			destination = CardDestination.Discard,
		},
		new AStatus
		{
			status = ModEntry.Instance.DisarmedStatus.Status,
			statusAmount = (HasRuinedPriority(s, this) ? 5 : 3) - (upgrade == Upgrade.B ? 1 : 0),
			targetPlayer = true
		}
	];
}


internal sealed class TheStormCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("TheStorm", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.rare,
				upgradesTo = [Upgrade.A, Upgrade.B],
				dontOffer = true
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "TheStorm", "name"]).Localize,
			Art = StableSpr.cards_Ace
		});
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		temporary = true
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.B => [
			new AStatus
			{
				status = Status.stunCharge,
				statusAmount = 9
			},
			new AAttack
			{
				damage = GetDmg(s, 2),
				brittle = true
			},
			new AMove
			{
				dir = -1,
				targetPlayer = true
			},
			new AAttack
			{
				damage = GetDmg(s, 2),
				brittle = true
			},
			new AStatus
			{
				status = ModEntry.Instance.BideStatus.Status,
				statusAmount = 3,
				targetPlayer = true
			},
		],
		_ => [
			new AAttack
			{
				damage = GetDmg(s, upgrade == Upgrade.A ? 4 : 2),
				brittle = true
			},
			new AMove
			{
				dir = -1,
				targetPlayer = true
			},
			new AAttack
			{
				damage = GetDmg(s, upgrade == Upgrade.A ? 4 : 2),
				brittle = true
			},
			new AStatus
			{
				status = ModEntry.Instance.BideStatus.Status,
				statusAmount = 3,
				targetPlayer = true
			},
		]
	};
}


internal sealed class YoureFiredCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("YoureFired", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.rare,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "YoureFired", "name"]).Localize
		});
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.A ? 2: 3,
		exhaust = true
	};

	internal static int CountNonPeachesCards(Combat c)
	{
		State s = MG.inst.g.state;
		List<Card> list = new (s.deck);
		if (c != null) {
			list = [.. list, .. c.discard, .. c.exhausted, .. c.hand];
		}

		return s.characters.Where(character => character.deckType != ModEntry.Instance.PeachesDeck.Deck).ToList().
			Select(character => list.Where(card => card.GetMeta().deck == character.deckType && !card.GetDataWithOverrides(s).temporary).ToList()).ToList().Select(cards => cards.Count).Sum();
	}

	private static List<Status> GetMissingStatuses()
	{
		State s = MG.inst.g.state;
		if (s.IsOutsideRun()) {
			return [ModEntry.Instance.FakeMissing1Status.Status, ModEntry.Instance.FakeMissing2Status.Status];
		}
		List<Status> result = [];
		foreach(Character character in s.characters) {
			if (character.deckType != null && character.deckType != ModEntry.Instance.PeachesCharacter.Configuration.Deck)
				result.Add(StatusMeta.deckToMissingStatus[character.deckType.Value]);
		}
		return result;
	}

	public override List<CardAction> GetActions(State fakeState, Combat c)
	{
		State s = MG.inst.g.state;
		List<CardAction> result = [];

		int x = CountNonPeachesCards(c);// / s.characters.Select((Character c) => c.deckType != ModEntry.Instance.PeachesDeck.Deck).ToList().Count;
		result.Add(new AVariableHintCards
		{
			value = x
		});

		result.Add(new AAttack {
			damage = GetDmg(s, x),
			xHint = 1
		});

		foreach(Status missingStatus in GetMissingStatuses()) {
			result.Add(new AStatus {
				status = missingStatus,
				statusAmount = upgrade == Upgrade.B ? 1 : 2,
				targetPlayer = true
			});
		}

		return result;
	}
}


internal sealed class EruptionCard : Card, IPeachesCard
{
	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("Eruption", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.rare,
				upgradesTo = [Upgrade.A, Upgrade.B]
			},
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "Eruption", "name"]).Localize
		});
	}

	public int GetDamageMultiplier() => upgrade switch
	{
		Upgrade.B => 3,
		_ => 2
	};

	public override CardData GetData(State state) => new() {
		cost = upgrade switch {
			Upgrade.A => 1,
			Upgrade.B => 3,
			_ => 2
		},
		description = ModEntry.Instance.Localizations.Localize(["card", "Eruption", "description", upgrade.ToString()], new { Damage = GetDmg(state, 0) * GetDamageMultiplier() }),
		exhaust = upgrade == Upgrade.B
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		_ => [
			new AAttack {
				damage = GetDamageMultiplier() * GetDmg(s, 0)
			}
		]
	};
}


internal sealed class BreakTheBottleCard : Card, IPeachesCard
{
	public int amount;

	public static void Register(IModHelper helper) {
		helper.Content.Cards.RegisterCard("BreakTheBottle", new()
		{
			CardType = MethodBase.GetCurrentMethod()!.DeclaringType!,
			Meta = new()
			{
				deck = ModEntry.Instance.PeachesDeck.Deck,
				rarity = Rarity.uncommon,
				upgradesTo = [Upgrade.A, Upgrade.B],
				dontOffer = true
			},
			Art = ModEntry.Instance.BottleUpArt,
			Name = ModEntry.Instance.AnyLocalizations.Bind(["card", "BreakTheBottle", "name"]).Localize
		});
	}

	int GetMultiplier() {
		return upgrade == Upgrade.A ? 2 : 1;
	}

	int GetAmount() {
		return GetMultiplier() * amount;
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		// description = ModEntry.Instance.Localizations.Localize(["card", "BreakTheBottle", "description", upgrade.ToString()], new { Amount = GetAmount() }),
		singleUse = true,
		temporary = true,
		retain = upgrade == Upgrade.B
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AStatus {
			status = ModEntry.Instance.DisarmedStatus.Status,
			statusAmount = -1,
			targetPlayer = true
		},
		new AStatus {
			status = ModEntry.Instance.BideStatus.Status,
			xHint = GetMultiplier(),
			statusAmount = GetAmount(),
			targetPlayer = true
		}
	];
}