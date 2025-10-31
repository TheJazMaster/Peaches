// using Nanoray.PluginManager;
// using System.Collections.Generic;
// using TheJazMaster.Bucket.Patches;

// namespace TheJazMaster.UnseenEffort.Dialogue;

// internal class CombatDialogue : BaseDialogue
// {
// 	internal void Inject()
// 	{
// 		var nodePresets = new Dictionary<string, StoryNode> {
// 			{"TookDamage", new StoryNode {
// 				enemyShotJustHit = true,
// 				minDamageDealtToPlayerThisTurn = 1,
// 				maxDamageDealtToPlayerThisTurn = 1
// 			}},
// 			{"TookNonHullDamage", new StoryNode {
// 				enemyShotJustHit = true,
// 				maxDamageDealtToPlayerThisTurn = 0
// 			}},
// 			{"BucketDealtDamage", new StoryNode {
// 				whoDidThat = ModEntry.Instance.BucketDeck.Deck,
// 				playerShotJustHit = true,
// 				minDamageDealtToEnemyThisAction = 1,
// 				oncePerCombatTags = [
// 					"BucketShotThatGuy"
// 				],
// 			}},
// 			{"PeriDealtDamage", new StoryNode {
// 				whoDidThat = Deck.peri,
// 				playerShotJustHit = true,
// 				minDamageDealtToEnemyThisAction = 1,
// 				oncePerCombatTags = [
// 					"BucketShotThatGuy"
// 				],
// 			}},
// 			{"DealtBigDamage", new StoryNode {
// 				playerShotJustHit = true,
// 				minDamageDealtToEnemyThisTurn = 10
// 			}},
// 			{"Missed", new StoryNode {
// 				playerShotJustMissed = true,
// 				oncePerCombat = true,
// 				doesNotHaveArtifacts = [
// 					"Recalibrator",
// 					"GrazerBeam"
// 				]
// 			}},
// 			{"AboutToDie", new StoryNode {
// 				enemyShotJustHit = true,
// 				maxHull = 2,
// 				oncePerCombatTags = [
// 					"aboutToDie"
// 				],
// 				oncePerRun = true
// 			}},
// 			{"HitArmor", new StoryNode {
// 				playerShotJustHit = true,
// 				minDamageBlockedByEnemyArmorThisTurn = 1,
// 				oncePerCombat = true,
// 				oncePerRun = true,
// 			}},
// 			{"UsedArmor", new StoryNode {
// 				enemyShotJustHit = true,
// 				minDamageBlockedByPlayerArmorThisTurn = 1,
// 				oncePerCombatTags = [
// 					"WowArmorISPrettyCoolHuh"
// 				],
// 				oncePerRun = true,
// 			}},
// 			{"EmptyHand", new StoryNode {
// 				handEmpty = true,
// 				oncePerCombatTags = [
// 					"handEmpty"
// 				],
// 			}},
// 			{"EmptyHandExcessEnergy", new StoryNode {
// 				handEmpty = true,
// 				minEnergy = 1,
// 			}},
// 			{"TrashHand", new StoryNode {
// 				handFullOfTrash = true,
// 				oncePerCombatTags = [
// 					"handOnlyHasTrashCards"
// 				],
// 				oncePerRun = true,
// 			}},
// 			{"UnplayableHand", new StoryNode {
// 				handFullOfUnplayableCards = true,
// 				oncePerCombatTags = [
// 					"handFullOfUnplayableCards"
// 				],
// 				oncePerRun = true,
// 			}},
// 			{"NoOverlap", new StoryNode {
// 				priority = true,
// 				shipsDontOverlapAtAll = true,
// 				oncePerCombatTags = [
// 					"NoOverlapBetweenShips"
// 				],
// 				oncePerRun = true,
// 				nonePresent = [
// 					"crab",
// 					"scrap"
// 				],
// 			}},
// 			{"NoOverlapButSeeker", new StoryNode {
// 				priority = true,
// 				shipsDontOverlapAtAll = true,
// 				oncePerCombatTags = [
// 					"NoOverlapBetweenShipsSeeker"
// 				],
// 				anyDronesHostile = [
// 					"missile_seeker"
// 				],
// 				oncePerRun = true,
// 				nonePresent = [
// 					"crab"
// 				],
// 			}},
// 			{"EvilRiggsGetsTrolled", new StoryNode {
// 				allPresent = [
// 					"pirateBoss",
// 					TranslateChar("Bucket")
// 				],
// 				enemyIntent = "hardRiggsGetsMad",
// 				priority = true,
// 				oncePerCombatTags = [
// 					"RiggsBossGivesYouOneTurnToGetBackHere"
// 				]
// 			}},
// 			{"LongFight", new StoryNode {
// 				minTurnsThisCombat = 9,
// 				oncePerCombatTags = [
// 					"manyTurns"
// 				],
// 				oncePerRun = true,
// 				turnStart = true,
// 			}},
// 			{"GoingToOverheat", new StoryNode {
// 				goingToOverheat = true,
// 				oncePerCombatTags = [
// 					"OverheatGeneric"
// 				],
// 			}},
// 			{"Tiderunner", new StoryNode {
// 				turnStart = true,
// 				  maxTurnsThisCombat = 1,
// 				oncePerCombatTags = [
// 					"Tiderunner"
// 				],
// 				oncePerRun = true,
// 				hasArtifacts = [
// 					"Tiderunner"
// 				],
// 			}},
// 			{"GotAutopilot", new StoryNode {
// 				lastTurnPlayerStatuses = [
// 					Status.autopilot
// 				],
// 				oncePerCombatTags = [
// 					"gotAutopilot"
// 				],
// 			}},
// 			{"CreatingTrash", new StoryNode {
// 				whoDidThat = ModEntry.Instance.BucketDeck.Deck,
// 				oncePerCombatTags = [
// 					"CreatingTrash"
// 				]
// 			}.ApplyModData(StoryVarsPatches.CreatedTrashKey, true)},
// 			{"Wizbo", new StoryNode {
// 				turnStart = true,
// 				allPresent = [
// 					TranslateChar("Wizbo"),
// 					TranslateChar("Bucket")
// 				],
// 				enemyIntent = "wizardMagic",
// 			}},
// 			{"Jumbo", new StoryNode {
// 				turnStart = true,
// 				allPresent = [
// 					TranslateChar("Jumbo"),
// 					TranslateChar("Bucket")
// 				],
// 				enemyIntent = "wideBigAttack",
// 				oncePerCombatTags = [
// 					"MinerGonnaSmackYa4X"
// 				]
// 			}},
// 			{"George", new StoryNode {
// 				turnStart = true,
// 				allPresent = [
// 					TranslateChar("George"),
// 					TranslateChar("Bucket")
// 				],
// 				maxTurnsThisCombat = 1,
// 				oncePerCombatTags = [
// 					"OldSpikeNewName"
// 				]
// 			}},
// 			{"PlayingSports", new StoryNode {
// 				allPresent = [
// 					TranslateChar("Sasha"),
// 					TranslateChar("Bucket")
// 				],
// 				oncePerCombat = true,
// 				playerJustShotASoccerBall = true
// 			}},
// 			{"DrawThree", new StoryNode {
// 				oncePerRun = true,
// 				lookup = [
// 					"DrawThree"
// 				]
// 			}},
// 			{"Overhaul", new StoryNode {
// 				oncePerRun = true,
// 				lookup = [
// 					"Overhaul"
// 				]
// 			}},
// 			{"OxiFuelCells", new StoryNode {
// 				oncePerRun = true,
// 				lookup = [
// 					"OxiFuelCells"
// 				]
// 			}},
// 			{"IllegalMods", new StoryNode {
// 				oncePerRun = true,
// 				lookup = [
// 					"IllegalMods"
// 				]
// 			}},
// 		};
// 		if (DB.story.all["EvilRiggsIsSeriousYouKnow_Multi_1"].nonePresent is { } thing) thing.Add(TranslateChar("Bucket"));
// 		else DB.story.all["EvilRiggsIsSeriousYouKnow_Multi_1"].nonePresent = [TranslateChar("Bucket")];

// 		InjectStory(nodePresets);
// 		ModEntry.Instance.Helper.Events.OnLoadStringsForLocale += (_, e) => InjectLocalizations(e);
// 	}


// 	internal override IFileInfo GetJsonFile()
// 	{
// 		return ModEntry.Instance.Package.PackageRoot.GetRelativeDirectory("I18n/en").GetRelativeFile("combat.json");
// 	}

// 	internal override NodeType GetNodeType()
// 	{
// 		return NodeType.combat;
// 	}

// 	internal override bool FlipPortrait(string who)
// 	{
// 		return false;
// 	}
// }