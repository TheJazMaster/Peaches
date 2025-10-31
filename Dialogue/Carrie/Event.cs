using HarmonyLib;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using System.Collections.Generic;
using System.Linq;
using TheJazMaster.UnseenEffort.Actions;
using TheJazMaster.UnseenEffort.Artifacts.Carrie;

namespace TheJazMaster.UnseenEffort.Dialogue.Carrie;

internal class EventDialogue
{

	internal static void Initialize(IPluginPackage<IModManifest> package, IModHelper helper) {
		DB.eventChoiceFns.Add(K("FairTrade"), AccessTools.DeclaredMethod(typeof(EventDialogue), nameof(EventDialogue.FairTradeChoiceFn)));

        LocalDB.DumpStoryToLocalLocale("en", new Dictionary<string, DialogueMachine>()
        {
            {K("FairTrade_Accept"), new() {
                type = NodeType.@event,
                never = true,
				bg = "BGShop",
                dialogue = [
                    new("cleo", "Sure, what do you need?", true)
                ],
                choiceFunc = nameof(Events.NewShop)
            }},
        });
    }

	private static Dictionary<string, StoryNode> fairTradeNodes = [];
	internal static void MakeFairTradeNodes() {
        // Each shop node after doing something other than a skip (with no choice)
        foreach ((string key, StoryNode node) in DB.story.all.Where(kvp => kvp.Value.bg == "BGShop" && kvp.Value.choiceFunc == null && kvp.Value.lines.Last() is not Jump && kvp.Value.lookup != null && !kvp.Value.lookup.Contains("shopSkip"))) {
            var newNode = Mutil.DeepCopy(node);
            newNode.priority = true;
            newNode.hasArtifacts ??= [];
            newNode.hasArtifacts.Add(new FairTradeArtifact().Key());
            newNode.choiceFunc = K("FairTrade");
            fairTradeNodes.Add(key, newNode);
		}
		foreach ((string key, StoryNode newNode) in fairTradeNodes) {
			DB.story.all.Add(K("FairTrade_") + key, newNode);
		}
	}

	internal static void AddFairTradeNodeLines(LoadStringsForLocaleEventArgs args) {
		foreach ((string key, StoryNode newNode) in fairTradeNodes) {
            foreach (Say say in newNode.lines.Where(i => i is Say))
            {
				args.Localizations.Add(Say.GetLocKey(K("FairTrade_") + key, say.hash), Loc.T(Say.GetLocKey(key, say.hash)));
            }
        }
	}

	private static string K(string str) {
		return "UnseenEffort_" + str;
	}

	private static List<Choice> FairTradeChoiceFn(State s) {
		string keyYes = "FairTrade_Accept";
		string keyNo = "FairTrade_Refuse";
		return [
			new Choice {
				label = ModEntry.Instance.Localizations.Localize(["dialogueChoice", keyYes]),
				key = K(keyYes),
				actions = [
					new ARemoveFairTrade()
				]
			},
			new Choice
			{
				label = ModEntry.Instance.Localizations.Localize(["dialogueChoice", keyNo]),
			}
		];
	}
}