using Nanoray.PluginManager;
using System.Collections.Generic;
using System.Linq;
using TheJazMaster.UnseenEffort.Actions;
using TheJazMaster.UnseenEffort.Artifacts.Carrie;

namespace TheJazMaster.UnseenEffort.Dialogue.Carrie;

internal class EventDialogue(string charKey) : BaseDialogue(charKey)
{
	internal void Inject() {
		DB.eventChoiceFns["TheJazMaster.UnseenEffort::FairTrade"] = GetType().GetMethod(nameof(FairTradeChoiceFn))!;
		var nodePresets = new Dictionary<string, StoryNode> {
			{"FairTrade_Accept", new StoryNode {}},
			{"FairTrade_Refuse", new StoryNode {}},
		};

		// Each shop node after doing something other than a skip (with no choice)
		foreach ((string key, StoryNode node) in DB.story.all.Where(kvp => kvp.Value.bg == "BGShop" && kvp.Value.choiceFunc == null && kvp.Value.lines.Last() is not Jump && kvp.Value.lookup != null && !kvp.Value.lookup.Contains("shopSkip"))) {
			StoryNode newNode = Mutil.DeepCopy(node);
			if (newNode.hasArtifacts == null) newNode.hasArtifacts = [];
			newNode.hasArtifacts.Add(new FairTradeArtifact().Key());
			newNode.priority = true;
			newNode.choiceFunc = "TheJazMaster.UnseenEffort::FairTrade";
			nodePresets.Add("FairTrade_" + key, newNode);
		}

		InjectStory(nodePresets);
		ModEntry.Instance.Helper.Events.OnLoadStringsForLocale += (_, e) => InjectLocalizations(e);
	}

	internal override IFileInfo GetJsonFile()
	{
		return ModEntry.Instance.Package.PackageRoot.GetRelativeDirectory("I18n/Dialogue/en").GetRelativeFile("events.json");
	}

	internal override NodeType GetNodeType()
	{
		return NodeType.@event;
	}

	internal override bool FlipPortrait(string who)
	{
		return who != TranslateChar("Bucket");
	}


	private static string Keyed(string str) {
		return "TheJazMaster.UnseenEffort::" + str;
	}

	private static List<Choice> FairTradeChoiceFn(State s) {
		string keyYes = "FairTrade_Accept";
		string keyNo = "FairTrade_Refuse";
		return [
			new Choice {
				label = ModEntry.Instance.Localizations.Localize(["dialogueChoice", keyYes]),
				key = Keyed(keyNo),
				actions = [
					new ARemoveFairTrade()
				]
			},
			new Choice
			{
				label = ModEntry.Instance.Localizations.Localize(["dialogueChoice", keyNo]),
				key = ".shopSkip_Confirm"
			}
		];
	}
}