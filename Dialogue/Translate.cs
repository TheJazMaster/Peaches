using System.Collections.Generic;
using System.Linq;

namespace TheJazMaster.UnseenEffort;

static class Translate {
	internal static string TC(string name) {
		return name.ToLower() switch {
			"isaac" => "goat",
			"drake" => "eunice",
			"max" => "hacker",
			"books" => "shard",
			"cat" => "comp",
			"cleo" => "nerd",
			"wizbo" => "wizard",
			"nibbs" => "TheJazMaster.Nibbs::Nibbs",
			"ix" => "TheJazMaster.Nibbs::Ix",
			"peaches" => "TheJazMaster.UnseenEffort::Peaches",
			"carrie" => "TheJazMaster.UnseenEffort::Carrie",
			"johnson" => "Shockah.Johnson::Johnson",
			"nola" => "Mezz.TwosCompany.NolaDeck",
			"isabelle" => "Mezz.TwosCompany.IsabelleDeck",
			"ilya" => "Mezz.TwosCompany.IlyaDeck",
			"gauss" => "Mezz.TwosCompany.GaussDeck",
			"jost" => "Mezz.TwosCompany.JostDeck",
			"sorrel" => "Mezz.TwosCompany.SorrelDeck",
			"eddie" => "Eddie.EddieDeck",
			"tera" => "Teratto.TeraMod.Tera",
			_ => name
		};
	}

	private static void TranslateStuff(StoryNode n) {
		if (n.allPresent != null) {
			n.allPresent = n.allPresent.Select(TC).ToHashSet();
		}
		if (n.nonePresent != null) {
			n.nonePresent = n.nonePresent.Select(TC).ToHashSet();
		}
	}

	internal static Dictionary<string, DialogueMachine> TC(Dictionary<string, DialogueMachine> dict) {
		foreach (var val in dict) {
            TranslateStuff(val.Value);

            val.Value.edit?.ForEach(et => {
				et.who = et.who != null ? TC(et.who) : null;
			});
        }
        return dict;
    }
}