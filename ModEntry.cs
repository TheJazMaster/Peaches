using FMOD;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework.Graphics;
using Nanoray.PluginManager;
using Nickel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TheJazMaster.Peaches.Artifacts;
using TheJazMaster.Peaches.Cards;
using TheJazMaster.Peaches.Features;
using MGColor = Microsoft.Xna.Framework.Color;

namespace TheJazMaster.Peaches;

public sealed class ModEntry : SimpleMod {
    internal static ModEntry Instance { get; private set; } = null;

    internal Harmony Harmony { get; }
	internal IKokoroApi KokoroApi { get; }
	internal IMoreDifficultiesApi MoreDifficultiesApi { get; }
	internal SingleDamModManager SingleBrittleManager { get; }
	internal SingleDamModManager SingleWeakManager { get; }
	internal ILocalizationProvider<IReadOnlyList<string>> AnyLocalizations { get; }
	internal ILocaleBoundNonNullLocalizationProvider<IReadOnlyList<string>> Localizations { get; }

    internal IPlayableCharacterEntryV2 PeachesCharacter { get; }

    internal IDeckEntry PeachesDeck { get; }

	internal IStatusEntry BideStatus { get; }
	internal IStatusEntry HardWorkerStatus { get; }
    internal IStatusEntry DisarmedStatus { get; }
    internal IStatusEntry SimmerStatus { get; }

    internal IStatusEntry FakeMissing1Status { get; }
    internal IStatusEntry FakeMissing2Status { get; }

    internal ISpriteEntry PeachesPortrait { get; }
    internal ISpriteEntry PeachesPortraitMini { get; }
    internal ISpriteEntry PeachesFrame { get; }
    internal ISpriteEntry PeachesCardBorder { get; }

    internal ISpriteEntry WorkforceIcon { get; }
    internal ISpriteEntry DrawAttacksIcon { get; }

    internal Spr BottleUpArt { get; }

    internal static IReadOnlyList<Type> StarterCardTypes { get; } = [
		typeof(HitEmCard),
		typeof(WhereItHurtsCard),
	];

	internal static IReadOnlyList<Type> CommonCardTypes { get; } = [
		typeof(SwerveCard),
		typeof(SimmerCard),
		typeof(CatharsisCard),
		typeof(AngerManagementCard),
		typeof(SurpriseShotCard),
		typeof(AngerCard),
        typeof(HellsFuryCard),
	];

	internal static IReadOnlyList<Type> UncommonCardTypes { get; } = [
		typeof(OvertimeCard),
		typeof(BottleUpCard),
		typeof(ViolentDaydreamsCard),
		typeof(SmashControlsCard),
		typeof(YouCantHideCard),
		typeof(LeaveAMessageCard),
		typeof(LetMeAtEmCard),
	];

	internal static IReadOnlyList<Type> RareCardTypes { get; } = [
		typeof(FuckOffCard),
		typeof(HardWorkerCard),
		typeof(TheCalmCard),
		typeof(YoureFiredCard),
		typeof(EruptionCard),
	];

	internal static IReadOnlyList<Type> SecretCardTypes { get; } = [
		typeof(TheStormCard),
		typeof(BreakTheBottleCard),
	];

    internal static IEnumerable<Type> AllCardTypes
		=> StarterCardTypes
			.Concat(CommonCardTypes)
			.Concat(UncommonCardTypes)
			.Concat(RareCardTypes)
			.Concat(SecretCardTypes);

    internal static IReadOnlyList<Type> CommonArtifacts { get; } = [
		typeof(FramedPhotograph),
		typeof(ForcedSoftwareUpdate),
		typeof(PunchingBag),
		typeof(PriorityMail),
	];

	internal static IReadOnlyList<Type> BossArtifacts { get; } = [
		typeof(BigGuns),
		typeof(SurveillanceDrones),
	];

	internal static IEnumerable<Type> AllArtifactTypes
		=> CommonArtifacts.Concat(BossArtifacts);


    public ModEntry(IPluginPackage<IModManifest> package, IModHelper helper, ILogger logger) : base(package, helper, logger)
	{
		Instance = this;
		Harmony = new(package.Manifest.UniqueName);
		KokoroApi = helper.ModRegistry.GetApi<IKokoroApi>("Shockah.Kokoro")!;

		DynamicWidthCardAction.ApplyPatches(Harmony);
		_ = new CardBrowseFilterManager();
		_ = new BideManager();
		SingleBrittleManager = new SingleDamModManager(PDamMod.brittle);
		SingleWeakManager = new SingleDamModManager(PDamMod.weak);
		_ = new DisarmManager();
		_ = new HardWorkerManager();
		_ = new ArtifactInterfacesManager();
		_ = new PriorityManager();

		AnyLocalizations = new JsonLocalizationProvider(
			tokenExtractor: new SimpleLocalizationTokenExtractor(),
			localeStreamFunction: locale => package.PackageRoot.GetRelativeFile($"I18n/{locale}.json").OpenRead()
		);
		Localizations = new MissingPlaceholderLocalizationProvider<IReadOnlyList<string>>(
			new CurrentLocaleOrEnglishLocalizationProvider<IReadOnlyList<string>>(AnyLocalizations)
		);


        PeachesPortrait = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/PeachesPortrait.png"));
        PeachesPortraitMini = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/PeachesPortraitMini.png"));
		PeachesFrame = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/PeachesFrame.png"));
        PeachesCardBorder = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/PeachesCardBorder.png"));


        BideStatus = helper.Content.Statuses.RegisterStatus("Bide", new()
		{
			Definition = new()
			{
				icon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/Bide.png")).Sprite,
				color = new("FF2D2D"),
				isGood = true
			},
			Name = AnyLocalizations.Bind(["status", "Bide", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "Bide", "description"]).Localize
		});

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
		});

        DisarmedStatus = helper.Content.Statuses.RegisterStatus("Disarmed", new()
		{
			Definition = new()
			{
				icon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/Disarmed.png")).Sprite,
				color = new("302060")
			},
			Name = AnyLocalizations.Bind(["status", "Disarmed", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "Disarmed", "description"]).Localize
		});

        FakeMissing1Status = helper.Content.Statuses.RegisterStatus("FakeStatus1", new()
		{
			Definition = new()
			{
				icon = StableSpr.icons_missingCat,
				color = new("ffffff")
			},
			Name = AnyLocalizations.Bind(["status", "FakeMissing1", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "FakeMissing1", "description"]).Localize
		});
        FakeMissing2Status = helper.Content.Statuses.RegisterStatus("FakeStatus2", new()
		{
			Definition = new()
			{
				icon = StableSpr.icons_missingCat,
				color = new("ffffff")
			},
			Name = AnyLocalizations.Bind(["status", "FakeMissing2", "name"]).Localize,
			Description = AnyLocalizations.Bind(["status", "FakeMissing2", "description"]).Localize
		});

		BottleUpArt = helper.Content.Sprites.RegisterSprite(Instance.Package.PackageRoot.GetRelativeFile("Sprites/Cards/BottleUp.png")).Sprite;


		Spr WorkforceBlankIcon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/Workforce.png")).Sprite;
		HashSet<Deck> lastCrew = [];
		Texture2D cachedTexture = null;
		WorkforceIcon = helper.Content.Sprites.RegisterDynamicSprite(delegate {
			var characterSet = MG.inst.g.state.characters.ToHashSet().Select(c => (Deck)c.deckType).ToHashSet();
			characterSet.Remove(PeachesDeck.Deck);
			if (lastCrew.SetEquals(characterSet)) return cachedTexture;


			lastCrew = characterSet;
			var colors = characterSet.Select(deck => DB.decks[deck].color).ToList();
			if (colors.Count == 0) {
				colors = [DB.decks[Deck.dizzy].color, DB.decks[Deck.riggs].color];
			}
			var texture = SpriteLoader.Get(WorkforceBlankIcon);
			var data = new MGColor[texture.Width * texture.Height];
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
			cachedTexture = outTexture;
			return outTexture;
		});
		DrawAttacksIcon = helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile("Sprites/icons/DrawAttacks.png"));


		PeachesDeck = helper.Content.Decks.RegisterDeck("Peaches", new()
		{
			Definition = new() { color = new Color("DEA0A0"), titleColor = Colors.black },
			DefaultCardArt = StableSpr.cards_colorless,
			BorderSprite = PeachesCardBorder.Sprite,
			Name = AnyLocalizations.Bind(["character", "name"]).Localize
		});

        foreach (var cardType in AllCardTypes)
			AccessTools.DeclaredMethod(cardType, nameof(IPeachesCard.Register))?.Invoke(null, [helper]);
		foreach (var artifactType in AllArtifactTypes)
			AccessTools.DeclaredMethod(artifactType, nameof(IPeachesCard.Register))?.Invoke(null, [helper]);

			helper.Content.Cards.OnGetFinalDynamicCardTraitOverrides += (card, data) => {
			State state = data.State;
			if (state.route is Combat combat) {
				foreach (Artifact item in data.State.EnumerateAllArtifacts()) {
					if (item is PriorityMail artifact && artifact.active && !data.TraitStates[PriorityManager.PriorityTrait].IsActive) {
						data.SetOverride(PriorityManager.FastTrait, true);
					}
				}
			}
		};

		MoreDifficultiesApi?.RegisterAltStarters(PeachesDeck.Deck, new() {
			cards = [
				new AngerManagementCard(),
				new SurpriseShotCard()
			]
		});

        PeachesCharacter = helper.Content.Characters.V2.RegisterPlayableCharacter("Peaches", new()
		{
			Deck = PeachesDeck.Deck,
			Description = AnyLocalizations.Bind(["character", "description"]).Localize,
			BorderSprite = PeachesFrame.Sprite,
			Starters = new StarterDeck {
				cards = [
					new HitEmCard(),
					new WhereItHurtsCard()
				]
			},
			NeutralAnimation = new()
			{
				CharacterType = PeachesDeck.Deck.Key(),
				LoopTag = "neutral",
				Frames = [
					PeachesPortrait.Sprite
				]
			},
			MiniAnimation = new()
			{
				CharacterType = PeachesDeck.Deck.Key(),
				LoopTag = "mini",
				Frames = [
					PeachesPortraitMini.Sprite
				]
			}
		});

		// helper.Content.Characters.RegisterCharacterAnimation("GameOver", new()
		// {
		// 	Deck = PeachesDeck.Deck,
		// 	LoopTag = "gameover",
		// 	Frames = Enumerable.Range(0, 1)
		// 		.Select(i => helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile($"Sprites/character/GameOver/{i}.png")).Sprite)
		// 		.ToList()
		// });
		// helper.Content.Characters.RegisterCharacterAnimation("Squint", new()
		// {
		// 	Deck = PeachesDeck.Deck,
		// 	LoopTag = "squint",
		// 	Frames = Enumerable.Range(0, 5)
		// 		.Select(i => helper.Content.Sprites.RegisterSprite(package.PackageRoot.GetRelativeFile($"Sprites/character/Squint/{i}.png")).Sprite)
		// 		.ToList()
		// });
    }
}