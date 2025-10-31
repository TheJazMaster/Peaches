using FMOD;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using Nanoray.PluginManager;
using Nickel;
using Nickel.Essentials;
using Shockah.Kokoro;
using System;
using System.Collections.Generic;
using System.Linq;
using TheJazMaster.UnseenEffort.Artifacts.Carrie;
using TheJazMaster.UnseenEffort.Artifacts.Peaches;
using TheJazMaster.UnseenEffort.Cards.Carrie;
using TheJazMaster.UnseenEffort.Cards.Peaches;
using TheJazMaster.UnseenEffort.Dialogue.Carrie;
using TheJazMaster.UnseenEffort.Features;
using MGColor = Microsoft.Xna.Framework.Color;

namespace TheJazMaster.UnseenEffort;

public sealed class ModEntry : SimpleMod {
    internal static ModEntry Instance { get; private set; } = null!;

    internal Harmony Harmony { get; }
	internal IEssentialsApi? EssentialsApi { get; }
	internal IKokoroApi.IV2 KokoroApi { get; }
	internal IMoreDifficultiesApi? MoreDifficultiesApi { get; }
    internal LocalDB LocalDB { get; private set; } = null!;

	internal SingleDamModManager SingleBrittleManager { get; }
	internal SingleDamModManager SingleWeakManager { get; }
	internal ILocalizationProvider<IReadOnlyList<string>> AnyLocalizations { get; }
	internal ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Localizations { get; }


	// PEACHES DEFINITIONS
    internal IPlayableCharacterEntryV2 PeachesCharacter { get; }
	internal Deck PeachesDeck { get; }

	internal Status FuryStatus { get; }
	internal Status HardWorkerStatus { get; }
    internal Status DisarmedStatus { get; }

    internal Status FakeMissing1Status { get; }
    internal Status FakeMissing2Status { get; }

    internal Spr WorkforceIcon { get; }
    internal Spr DrawAttacksIcon { get; }
	internal Spr SecretBrittleIcon { get; }
	internal Spr SingleBrittleIcon { get; }
	internal Spr SecretSingleBrittleIcon { get; }

	internal static IReadOnlyList<Type> PeachesCards { get; } = [
		typeof(HitEmCard),
		typeof(WhereItHurtsCard),
		typeof(ResentmentCard),
		typeof(PlatonicShotCard),
		typeof(VeerCard),
		typeof(SimmerCard),
		typeof(FinisherCard),
		typeof(IreCard),
        typeof(SnapCard),

		typeof(OverworkCard),
		typeof(BottleUpCard),
		typeof(ViolentDaydreamsCard),
		typeof(SmashControlsCard),
		typeof(YouCantHideCard),
		typeof(TriflingMattersCard),
		typeof(LetMeAtEmCard),
	
		typeof(FuckOffCard),
		typeof(HardWorkerCard),
		typeof(TheCalmCard),
		typeof(YoureFiredCard),
		typeof(PassiveAggressionCard),
	
		typeof(TheStormCard),
		typeof(BreakTheBottleCard),
	];

    internal static IReadOnlyList<Type> PeachesArtifacts { get; } = [
		typeof(FramedPhotographArtifact),
		typeof(PinaColadaArtifact),
		typeof(LifeSavingsArtifact),
		typeof(PunchingBagArtifact),
	
		typeof(BigGunsArtifact),
		typeof(MegaphoneArtifact),
	];


	// CARRIE DEFINITIONS
    internal IPlayableCharacterEntryV2 CarrieCharacter { get; }
	internal Deck CarrieDeck { get; }

	internal Status CardFindStatus { get; }
	internal Status ArtifactFindStatus { get; }
    internal Status DetourStatus { get; }
	internal Status DetourPlusStatus { get; }

	internal static IReadOnlyList<Type> CarrieCards { get; } = [
		typeof(ClockInCard),
		typeof(ScroungeCard),
		typeof(SafetyMeasuresCard),
		typeof(SmokeBreakCard),
		typeof(PackageCard),
		typeof(InventoryLogsCard),
		typeof(FixerUpperCard),
        typeof(UnboxingCard),
		typeof(RepurposeCard),

		typeof(MadeToLastCard),
		typeof(TearDownCard),
		typeof(DetourCard),
		typeof(TradeCard),
		typeof(PushThroughCard),
		typeof(FullEffortCard),
		typeof(UnshakeableCard),
	
		typeof(SpecialDeliveryCard),
		typeof(HaulAssCard),
		typeof(ClockOutCard),
		typeof(RetrofitCard),
		typeof(BestOfTheBestCard),
	
		typeof(ReimburseCard),
		typeof(DeliveryBoxCard),
	];

    internal static IReadOnlyList<Type> CarrieArtifacts { get; } = [
		typeof(WatchArtifact),
		typeof(CatalogueArtifact),
		typeof(MultiToolArtifact),
		typeof(ElbowGreaseArtifact),
			
		typeof(ForkliftArtifact),

		typeof(PackageArtifact),
		typeof(RetrofittedPartsArtifact),
		typeof(FairTradeArtifact),
	];

    public ModEntry(IPluginPackage<IModManifest> package, IModHelper helper, ILogger logger) : base(package, helper, logger)
	{
		Instance = this;
		Harmony = new(package.Manifest.UniqueName);
		EssentialsApi = helper.ModRegistry.GetApi<IEssentialsApi>("Nickel.Essentials");
		KokoroApi = helper.ModRegistry.GetApi<IKokoroApi>("Shockah.Kokoro")!.V2;
		MoreDifficultiesApi = helper.ModRegistry.GetApi<IMoreDifficultiesApi>("TheJazMaster.MoreDifficulties");

		AnyLocalizations = new JsonLocalizationProvider(
			tokenExtractor: new SimpleLocalizationTokenExtractor(),
			localeStreamFunction: locale => package.PackageRoot.GetRelativeFile($"I18n/{locale}.json").OpenRead()
		);
		Localizations = new MissingPlaceholderLocalizationProvider<IReadOnlyList<string>>(
			new CurrentLocaleOrEnglishLocalizationProvider<IReadOnlyList<string>>(AnyLocalizations)
		);

		// PEACHES REGISTRATION
		{
		_ = new FuryManager();
		SingleBrittleManager = new SingleDamModManager(PDamMod.brittle);
		SingleWeakManager = new SingleDamModManager(PDamMod.weak);
		_ = new DisarmManager();
		_ = new HardWorkerManager();
		_ = new PriorityManager();

		PeachesCharacter = RegisterCharacter("Peaches", new Color("DEA0A0"), PeachesCards, PeachesArtifacts,
			new StarterDeck {
				cards = [
					new HitEmCard(),
					new WhereItHurtsCard()
				]
			},
			new StarterDeck {
				cards = [
					new ResentmentCard(),
					new PlatonicShotCard()
				]
			}
		);
		PeachesDeck = PeachesCharacter.Configuration.Deck;
		RegisterAnimation(PeachesDeck, "Peaches", "squint");

        FuryStatus = helper.Content.Statuses.RegisterStatus("Fury", new()
		{
			Definition = new()
			{
				icon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/Fury.png")).Sprite,
				color = new("FF2D2D"),
				isGood = true
			},	
			Name = AnyLocalizations.Bind(["status", "Fury", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "Fury", "description"]).Localize
		}).Status;

        HardWorkerStatus = helper.Content.Statuses.RegisterStatus("HardWorker", new()
		{
			Definition = new()
			{
				icon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/HardWorker.png")).Sprite,
				color = new("DEA0A0"),
				isGood = true
			},
			Name = AnyLocalizations.Bind(["status", "HardWorker", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "HardWorker", "description"]).Localize
		}).Status;

        DisarmedStatus = helper.Content.Statuses.RegisterStatus("Disarmed", new()
		{
			Definition = new()
			{
				icon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/Disarmed.png")).Sprite,
				color = new("302060")
			},
			Name = AnyLocalizations.Bind(["status", "Disarmed", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "Disarmed", "description"]).Localize
		}).Status;

        FakeMissing1Status = helper.Content.Statuses.RegisterStatus("FakeStatus1", new()
		{
			Definition = new()
			{
				icon = StableSpr.icons_missingCat,
				color = new("ffffff")
			},
			Name = AnyLocalizations.Bind(["status", "FakeMissing1", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "FakeMissing1", "description"]).Localize
		}).Status;
        FakeMissing2Status = helper.Content.Statuses.RegisterStatus("FakeStatus2", new()
		{
			Definition = new()
			{
				icon = StableSpr.icons_missingCat,
				color = new("ffffff")
			},
			Name = AnyLocalizations.Bind(["status", "FakeMissing2", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "FakeMissing2", "description"]).Localize
		}).Status;


		// helper.Content.Cards.OnGetFinalDynamicCardTraitOverrides += (card, data) => {
		// 	State state = data.State;
		// 	if (state.route is Combat combat) {
		// 		foreach (Artifact item in data.State.EnumerateAllArtifacts()) {
		// 			if (item is PriorityMail artifact && artifact.active && !data.TraitStates[PriorityManager.PriorityTrait].IsActive) {
		// 				data.SetOverride(PriorityManager.FastTrait, true);
		// 			}
		// 		}
		// 	}
		// };

		Spr WorkforceBlankIcon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/Workforce.png")).Sprite;
		HashSet<Deck> lastCrew = [];
		Texture2D cachedTexture = SpriteLoader.Get(WorkforceBlankIcon)!;
		WorkforceIcon = helper.Content.Sprites.RegisterDynamicSprite(delegate {
			var characterSet = MG.inst.g.state.characters.ToHashSet().Select(c => (Deck)c.deckType!).ToHashSet();
			characterSet.Remove(PeachesDeck);
			if (lastCrew.SetEquals(characterSet)) return cachedTexture;


			lastCrew = characterSet;
			var colors = characterSet.Select(deck => DB.decks[deck].color).ToList();
			if (colors.Count == 0) {
				colors = [DB.decks[Deck.dizzy].color, DB.decks[Deck.riggs].color];
			}
			var texture = SpriteLoader.Get(WorkforceBlankIcon);
			var data = new MGColor[texture!.Width * texture.Height];
			int count = colors.Count; int textureWidth = 5;
			texture.GetData(data);

			for (var i = 0; i < data.Length; i++) {
				int x = Math.Clamp(i % texture.Width, 2, 6) - 2;
				int y = i / texture.Width;
				Color color;
				if (y < texture.Height / 2) {
					color = colors[x * count / textureWidth];
				} else {
					color = colors[Math.Min(count - 1, (x+1) * count / textureWidth)];
				}
				data[i] = new MGColor(
					(float)(data[i].R / 255 * color.r),
					(float)(data[i].G / 255 * color.g),
					(float)(data[i].B / 255 * color.b),
					(float)(data[i].A / 255 * color.a)
				);
			}

			var outTexture = new Texture2D(MG.inst.GraphicsDevice, texture.Width, texture.Height);
			outTexture.SetData(data);
			
			texture.Dispose();
			cachedTexture.Dispose();
			
			cachedTexture = outTexture;
			return outTexture;
		}).Sprite;

		DrawAttacksIcon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/DrawAttacks.png")).Sprite;
		// BrittleIcon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/DrawAttacks.png"));
		SecretBrittleIcon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/SecretBrittle.png")).Sprite;
		SingleBrittleIcon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/SingleBrittle.png")).Sprite;
		SecretSingleBrittleIcon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/SecretSingleBrittle.png")).Sprite;
		}

		// CARRIE REGISTRATION
		{
		_ = new TiringManager();
		_ = new MundaneManager();
        _ = new FindManager();
        _ = new DetourManager();

        CarrieCharacter = RegisterCharacter("Carrie", new Color("464764"), CarrieCards, CarrieArtifacts,
			new StarterDeck {
				cards = [
					new ClockInCard(),
					new ScroungeCard()
				]
			},
			new StarterDeck {
				cards = [
					new SpecialDeliveryCard()
				],
				artifacts = [
					new WatchArtifact()
				]
			}
		);
		CarrieDeck = CarrieCharacter.Configuration.Deck;
		RegisterAnimation(CarrieDeck, "Carrie", "squint");

        CardFindStatus = helper.Content.Statuses.RegisterStatus("CardFind", new()
		{
			Definition = new()
			{
				icon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/CardFind.png")).Sprite,
				color = new("50687c"),
				isGood = true
			},	
			Name = AnyLocalizations.Bind(["status", "CardFind", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "CardFind", "description"]).Localize
		}).Status;

        ArtifactFindStatus = helper.Content.Statuses.RegisterStatus("ArtifactFind", new()
		{
			Definition = new()
			{
				icon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/ArtifactFind.png")).Sprite,
				color = new("ffb300"),
				isGood = true
			},
			Name = AnyLocalizations.Bind(["status", "ArtifactFind", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "ArtifactFind", "description"]).Localize
		}).Status;

        DetourStatus = helper.Content.Statuses.RegisterStatus("Detour", new()
		{
			Definition = new()
			{
				icon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/Detour.png")).Sprite,
				color = new("ffb300"),
				isGood = true
			},
			Name = AnyLocalizations.Bind(["status", "Detour", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "Detour", "description"]).Localize
		}).Status;

        DetourPlusStatus = helper.Content.Statuses.RegisterStatus("DetourPlus", new()
		{
			Definition = new()
			{
				icon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/DetourPlus.png")).Sprite,
				color = new("ffb300"),
				isGood = true
			},
			Name = AnyLocalizations.Bind(["status", "DetourPlus", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "DetourPlus", "description"]).Localize
		}).Status;
		}

		EventDialogue.Initialize(package, helper);

        helper.Events.OnModLoadPhaseFinished += (_, phase) =>
        {
            if (phase == ModLoadPhase.AfterDbInit)
            {
            	LocalDB = new(helper, package);

                EventDialogue.MakeFairTradeNodes();
            }
        };
		helper.Events.OnLoadStringsForLocale += (_, thing) =>
        {
            foreach (KeyValuePair<string, string> entry in LocalDB.GetLocalizationResults())
            {
                thing.Localizations[entry.Key] = entry.Value;
            }
            EventDialogue.AddFairTradeNodeLines(thing);
        };

		Harmony.PatchAll();

    }


	public IPlayableCharacterEntryV2 RegisterCharacter(string name, Color color, IReadOnlyList<Type> cardTypes, IReadOnlyList<Type> artifactTypes, StarterDeck starters, StarterDeck altStarters) {
		var borderSprite = Helper.Content.Sprites.RegisterSprite(Package.PackageRoot.GetRelativeFile($"Sprites/Characters/{name}/{name}CardBorder.png")).Sprite;
		var frameSprite = Helper.Content.Sprites.RegisterSprite(Package.PackageRoot.GetRelativeFile($"Sprites/Characters/{name}/{name}Frame.png")).Sprite;

		var deck = Helper.Content.Decks.RegisterDeck(name, new()
		{
			Definition = new() {
				color = color,
				titleColor = Colors.black
			},
			DefaultCardArt = StableSpr.cards_colorless,
			BorderSprite = borderSprite,
			Name = AnyLocalizations.Bind(["character", name, "name"]).Localize
		}).Deck;

        foreach (var cardType in cardTypes)
			AccessTools.DeclaredMethod(cardType, nameof(IRegisterableCard.Register))?.Invoke(null, [deck, name, Helper, Package]);
		foreach (var artifactType in artifactTypes)
			AccessTools.DeclaredMethod(artifactType, nameof(IRegisterableCard.Register))?.Invoke(null, [deck, name, Helper, Package]);

		MoreDifficultiesApi?.RegisterAltStarters(deck, altStarters);

        return Helper.Content.Characters.V2.RegisterPlayableCharacter(name, new()
		{
			Deck = deck,
			Description = AnyLocalizations.Bind(["character", name, "description"]).Localize,
			BorderSprite = frameSprite,
			Starters = starters,
			NeutralAnimation = RegisterAnimation(deck, name, "neutral"),
			MiniAnimation = RegisterAnimation(deck, name, "mini"),
		});
	}

	private CharacterAnimationConfigurationV2 RegisterAnimation(Deck deck, string charname, string name) =>
		Helper.Content.Characters.V2.RegisterCharacterAnimation(charname + "_" + name, new()
		{
			CharacterType = deck.Key(),
			LoopTag = name,
			Frames = Enumerable.Range(1, 10)
				.Select(i => Package.PackageRoot.GetRelativeFile($"Sprites/Characters/{charname}/{name}/{charname}_{name}_{i}.png"))
				.TakeWhile(f => f.Exists)
				.Select(f => Helper.Content.Sprites.RegisterSprite(f).Sprite)
				.ToList()
		}).Configuration;
}