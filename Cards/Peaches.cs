using Nickel;
using TheJazMaster.UnseenEffort.Actions;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using static TheJazMaster.UnseenEffort.Features.PriorityManager;
using TheJazMaster.UnseenEffort.Features;
using Nanoray.PluginManager;

namespace TheJazMaster.UnseenEffort.Cards.Peaches;


internal sealed class HitEmCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		retain = upgrade == Upgrade.B
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AAttack
		{
			damage = GetDmg(s, upgrade == Upgrade.A ? 4 : 3)
		},
		new AStatus
		{
			status = ModEntry.Instance.FuryStatus,
			statusAmount = upgrade == Upgrade.A ? 2 : 1,
			targetPlayer = true
		}
	];
}


internal sealed class WhereItHurtsCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	private int GetEvadeAmount() => upgrade switch {
		Upgrade.A => 2,
		_ => 1
	};

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 1 : 2,
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new ARandomBrittle
		{
			single = true,
			hidden = upgrade == Upgrade.B,
			count = 1
		},
		new AStatus
		{
			status = Status.evade,
			statusAmount = upgrade == Upgrade.A ? 2 : 1,
			targetPlayer = true
		}
	];
}


internal sealed class ResentmentCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 1
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { PriorityTrait };

	public override List<CardAction> GetActions(State s, Combat c) => PrioritySet(s, this,
		new AStatus
		{
			status = Status.tempShield,
			statusAmount = upgrade == Upgrade.B ? 2 : 1,
			targetPlayer = true
		}, [
		new AStatus
		{
			status = Status.shield,
			statusAmount = 2,
			targetPlayer = true
		},
		new AStatus
		{
			status = ModEntry.Instance.DisarmedStatus,
			statusAmount = upgrade == Upgrade.A ? 1 : 2,
			targetPlayer = true
		}
	]);
}


internal sealed class PlatonicShotCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 1
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.B => [
			new AVariableHint {
				status = ModEntry.Instance.DisarmedStatus,
			},
			new AAttack
			{
				damage = GetDmg(s, s.ship.Get(ModEntry.Instance.DisarmedStatus)),
				xHint = 1
			}
		],
		_ => [
			new AAttack
			{
				damage = GetDmg(s, 0)
			},
			new AStatus	
			{
				status = ModEntry.Instance.FuryStatus,
				statusAmount = upgrade == Upgrade.A ? 3 : 2,
				targetPlayer = true,
			}
		]
	};
}


internal sealed class VeerCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		flippable = upgrade == Upgrade.A,
		art = flipped ? StableSpr.cards_ScootRight : StableSpr.cards_ScootLeft
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { PriorityTrait };

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.B => PrioritySet(s, this, 
			new AStatus
			{
				status = Status.evade,
				statusAmount = 1,
				targetPlayer = true
			}, [
			new AMove
			{
				dir = -2,
				targetPlayer = true,
			},
			new AStatus
			{
				status = ModEntry.Instance.FuryStatus,
				statusAmount = 1,
				targetPlayer = true
			}
		]),
		_ => PrioritySet(s, this, 
			new AStatus
			{
				status = Status.evade,
				statusAmount = 1,
				targetPlayer = true
			}, [
			new AMove
			{
				dir = -2,
				targetPlayer = true,
			}
		]),
	};
}


internal sealed class SimmerCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		exhaust = upgrade != Upgrade.B
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AStatus
		{
			status = ModEntry.Instance.FuryStatus,
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


internal sealed class FinisherCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 2
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => upgrade == Upgrade.B ? new HashSet<ICardTraitEntry> { PriorityTrait } : [];

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.B => PrioritySet(s, this,
			new AAttack
			{
				damage = GetDmg(s, 8)
			}, [
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


internal sealed class IreCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 2 : 1
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AStatus
		{
			status = ModEntry.Instance.FuryStatus,
			statusAmount = upgrade == Upgrade.B ? 2 : 1,
			targetPlayer = true
		},
		new AStatus
		{
			status = Status.shield,
			statusAmount = upgrade == Upgrade.None ? 1 : 2,
			targetPlayer = true
		}
	];
}


internal sealed class SnapCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.common, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		retain = true,
		flippable = upgrade == Upgrade.A,
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch
	{
		Upgrade.B => [
			new AMove
			{
				dir = 2,
				targetPlayer = true
			},
			new AStatus
			{
				status = ModEntry.Instance.FuryStatus,
				statusAmount = 2,
				targetPlayer = true
			}
		],
		_ => [
			new AMove
			{
				dir = 2,
				targetPlayer = true
			},
			new AAttack
			{
				damage = GetDmg(s, 2)
			}
		]
	};
}



// UNCOMMONS

internal sealed class OverworkCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out _);
	}

	private static List<Status> GetMissingStatuses()
	{
		State s = MG.inst.g.state;
		if (s.IsOutsideRun()) {
			return [ModEntry.Instance.FakeMissing1Status, ModEntry.Instance.FakeMissing2Status];
		}
		List<Status> result = [];
		foreach(Character character in s.characters) {
			if (character.deckType != null && character.deckType != ModEntry.Instance.PeachesCharacter.Configuration.Deck)
				result.Add(StatusMeta.deckToMissingStatus[character.deckType.Value]);
		}
		return result;
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		exhaust = upgrade != Upgrade.B,
	};

	public override List<CardAction> GetActions(State s, Combat c)
	{	
		if (upgrade == Upgrade.B) {
			return [
				new AStatus {
					status = Status.evade,
					statusAmount = 5,
					targetPlayer = true
				},
				new AStatus {
					status = StatusMeta.deckToMissingStatus[ModEntry.Instance.PeachesDeck],
					statusAmount = 2,
					targetPlayer = true
				}
			];
		}

		List<CardAction> result = [
			new AStatus {
				status = Status.evade,
				statusAmount = upgrade == Upgrade.A ? 7 : 5,
				targetPlayer = true
			}
		];
		foreach(Status missingStatus in GetMissingStatuses()) {
			result.Add(new AStatus {
				status = missingStatus,
				statusAmount = 1,
				targetPlayer = true
			});
		}
		return result;
	}
}


internal sealed class BottleUpCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.B ? 0 : 1,
		exhaust = true,
		description = ModEntry.Instance.Localizations.Localize(["card", "Peaches", "BottleUp", "description", upgrade.ToString()]),
	};

	public override List<CardAction> GetActions(State s, Combat c) {
		int furyAmt = s.ship.Get(ModEntry.Instance.FuryStatus);

		return [
			new AVariableHint {
				status = ModEntry.Instance.FuryStatus
			},	
			new AStatus
			{
				status = ModEntry.Instance.FuryStatus,
				statusAmount = 0,
				mode = AStatusMode.Set,
				targetPlayer = true
			},
			new AAddCard
			{
				card = new BreakTheBottleCard {
					amount = furyAmt,
					upgrade = upgrade
				},
				showCardTraitTooltips = true
			}
		];
	}
}


internal sealed class SmashControlsCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		exhaust = true
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { PriorityTrait };

	public override List<CardAction> GetActions(State s, Combat c) => PrioritySet(s, this, 
		new AStatus
		{
			status = Status.energyNextTurn,
			statusAmount = upgrade == Upgrade.None ? 1 : 2,
			targetPlayer = true
		},
	[
		new AEnergy
		{
			changeAmount = upgrade == Upgrade.B ? 4 : 3
		},
		new AStatus
		{
			status = ModEntry.Instance.DisarmedStatus,
			statusAmount = upgrade == Upgrade.B ? 5 : 3,
			targetPlayer = true
		}
	]);
}


internal sealed class YouCantHideCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out _);
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
				damage = GetDmg(s, 2),
				piercing = true,
				brittle = true
			}
		]
	};
}


internal sealed class TriflingMattersCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade switch {
			Upgrade.A => 1,
			_ => 2
		},
		exhaust = upgrade == Upgrade.B
	};

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => upgrade == Upgrade.A ? [] : new HashSet<ICardTraitEntry> { PriorityTrait };

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.A => [
			new AStatus {
				status = ModEntry.Instance.DisarmedStatus,
				statusAmount = 1,
				mode = AStatusMode.Set
			}
		],
		_ => PrioritySet(s, this, 
			new AEnergy {
				changeAmount = 1
		}, [
			new AStatus
			{
				status = ModEntry.Instance.DisarmedStatus,
				statusAmount = upgrade == Upgrade.B ? 2 : 1,
				mode = AStatusMode.Set
			},
			new AStatus
			{
				status = ModEntry.Instance.DisarmedStatus,
				statusAmount = 2,
				targetPlayer = true
			}
		])
	};
}


internal sealed class LetMeAtEmCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		exhaust = true
	};

	public override List<CardAction> GetActions(State s, Combat c) => upgrade switch {
		Upgrade.B => [
			new AAttack
			{
				damage = GetDmg(s, 3),
				piercing = true,
				brittle = true
			},
			new AStatus
			{
				status = ModEntry.Instance.PeachesCharacter.MissingStatus.Status,
				statusAmount = 1,
				targetPlayer = true
			}
		],
		_ => [
			new AAttack
			{
				damage = GetDmg(s, upgrade == Upgrade.A ? 6 : 3),
				piercing = true,
				brittle = true
			},
			new AStatus
			{
				status = ModEntry.Instance.PeachesCharacter.MissingStatus.Status,
				statusAmount = 2,
				targetPlayer = true
			}
		]
	};
}


internal sealed class ViolentDaydreamsCard : Card, IRegisterableCard, IHasCustomCardTraits
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.uncommon, helper, package, out _);
	}

	public IReadOnlySet<ICardTraitEntry> GetInnateTraits(State state) => new HashSet<ICardTraitEntry> { PriorityTrait };

	public override CardData GetData(State state) => new() {
		cost = 1
	};

	public override List<CardAction> GetActions(State s, Combat c) => PrioritySet(s, this, 
		upgrade == Upgrade.B ? new AStatus {
			status = Status.drawNextTurn,
			statusAmount = 2,
			targetPlayer = true
		} : new ADrawCard {
			count = 1
		},
	[	
		new ADrawCard {
			count = upgrade switch {
				Upgrade.B => 4,
				_ => 3
			}
		},
		new AStatus
		{
			status = ModEntry.Instance.DisarmedStatus,
			statusAmount = upgrade == Upgrade.B ? 4 : 2,
			targetPlayer = true
		}
	]);
}


// RARES

internal sealed class FuckOffCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.rare, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade switch {
			Upgrade.A => 3,
			_ => 4
		},
		exhaust = true
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AAttack
		{
			damage = GetDmg(s, upgrade == Upgrade.B ? 5 : 2),
			moveEnemy = upgrade == Upgrade.B ? 9 : 7
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
	];
}


internal sealed class HardWorkerCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.rare, helper, package, out _);
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
				status = ModEntry.Instance.HardWorkerStatus,
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
				status = ModEntry.Instance.HardWorkerStatus,
				statusAmount = 2,
				targetPlayer = true
			}
		],
		_ => [
			new AStatus
			{
				status = ModEntry.Instance.HardWorkerStatus,
				statusAmount = 1,
				targetPlayer = true
			}
		]
	};
}


internal sealed class TheCalmCard : Card, IRegisterableCard
{
	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.rare, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		exhaust = true,
		description = ModEntry.Instance.Localizations.Localize(["card", "Peaches", "TheCalm", "description", upgrade.ToString()]),
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AVariableHint {
			status = ModEntry.Instance.FuryStatus
		},
		new AAddCard
		{
			card = new TheStormCard {
				amount = s.ship.Get(ModEntry.Instance.DisarmedStatus),
				upgrade = upgrade == Upgrade.A ? Upgrade.A : Upgrade.None,
			},
			destination = CardDestination.Deck,
		}
	];
}


internal sealed class TheStormCard : Card, IRegisterableCard
{
	public int amount = 0;

	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.rare, helper, package, out _, true);
	}

	public override CardData GetData(State state) => new() {
		cost = 2,
		temporary = true
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AAttack
		{
			damage = GetDmg(s, amount),
			xHint = 1,
			brittle = upgrade == Upgrade.B
		},
		new AStatus
		{
			status = Status.evade,
			statusAmount = upgrade == Upgrade.A ? 2 : 1,
			targetPlayer = true
		},
	];
}


internal sealed class YoureFiredCard : Card, IRegisterableCard
{

	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.rare, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.A ? 2: 3,
		exhaust = true
	};

	internal static int CountNonPeachesCards(Combat? c)
	{
		State s = MG.inst.g.state;
		List<Card> list = new (s.deck);
		if (c != null) {
			list = [.. list, .. c.discard, .. c.exhausted, .. c.hand];
		}

		return s.characters.Where(character => character.deckType != ModEntry.Instance.PeachesDeck).ToList().
			Select(character => list.Where(card => card.GetMeta().deck == character.deckType && !card.GetDataWithOverrides(s).temporary).ToList()).ToList().Select(cards => cards.Count).Sum();
	}

	private static List<Status> GetMissingStatuses()
	{
		State s = MG.inst.g.state;
		if (s.IsOutsideRun()) {
			return [ModEntry.Instance.FakeMissing1Status, ModEntry.Instance.FakeMissing2Status];
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
				statusAmount = upgrade == Upgrade.B ? 1 : 3,
				targetPlayer = true
			});
		}

		return result;
	}
}


internal sealed class PassiveAggressionCard : Card, IRegisterableCard
{

	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.rare, helper, package, out _);
	}

	public override CardData GetData(State state) => new() {
		cost = 1,
		infinite = upgrade == Upgrade.B
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AStatus {
			status = ModEntry.Instance.FuryStatus,
			statusAmount = upgrade == Upgrade.A ? 4 : 3,
			targetPlayer = true
		},
		new AStatus {
			status = ModEntry.Instance.DisarmedStatus,
			statusAmount = 4,
			targetPlayer = true
		}
	];
}


internal sealed class BreakTheBottleCard : Card, IRegisterableCard
{
	public int amount;

	public static void Register(Deck deck, string charname, IModHelper helper, IPluginPackage<IModManifest> package)
	{
		IRegisterableCard.Register(MethodBase.GetCurrentMethod()!.DeclaringType!, deck, charname, Rarity.rare, helper, package, out _, true);
	}

	int GetMultiplier() => upgrade switch {
		Upgrade.B => 1,
		_ => 2
	};

	int GetAmount() {
		return GetMultiplier() * amount;
	}

	public override CardData GetData(State state) => new() {
		cost = upgrade == Upgrade.A ? 0 : 1,
		singleUse = upgrade != Upgrade.B,
		temporary = true,
	};

	public override List<CardAction> GetActions(State s, Combat c) => [
		new AStatus
		{
			status = ModEntry.Instance.FuryStatus,
			statusAmount = GetAmount(),
			targetPlayer = true,
			xHint = GetMultiplier()
		}
	];
}