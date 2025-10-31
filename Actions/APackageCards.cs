using System.Collections.Generic;
using System.Linq;
using FSPRO;
using Nickel;
using TheJazMaster.UnseenEffort.Artifacts.Carrie;

namespace TheJazMaster.UnseenEffort.Actions;

public class APackageCards : CardAction
{
	public override void Begin(G g, State s, Combat c)
	{
		var affectedCards = ModEntry.Instance.KokoroApi.MultiCardBrowse.GetSelectedCards(this)?.ToList() ?? [selectedCard];
	
		if (affectedCards.Count == 0) return;

		foreach (Card card in affectedCards) {
			c.Queue(new ARemoveSelectedCard {
				selectedCard = card
			});
		}
		foreach (Artifact item in s.EnumerateAllArtifacts()) {
			if (item is PackageArtifact artifact) {
				artifact.packagedCards.AddRange(affectedCards);
                artifact.Pulse();
                return;
			}
		}
		Audio.Play(Event.CardHandling);
		
		c.Queue(new AAddArtifact {
			artifact = new PackageArtifact {
				packagedCards = affectedCards
			}
		});
	}

	public override List<Tooltip> GetTooltips(State s) => new PackageArtifact().GetTooltips();
}