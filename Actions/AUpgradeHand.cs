using System.Linq;
using FSPRO;

namespace TheJazMaster.UnseenEffort.Actions;

public class AUpgradeHand : CardAction
{
	public bool tempOnly = false;
	public bool fromEverywhere = false;

	public override void Begin(G g, State s, Combat c)
	{
		bool works = false;
		foreach (Card card in fromEverywhere ? s.deck.Concat(c.hand).Concat(c.discard).Concat(c.exhausted) : c.hand) {
			Upgrade[] upgrades = card.GetMeta().upgradesTo;
			if (card.upgrade == Upgrade.None && upgrades.Length > 0 && (!tempOnly || ModEntry.Instance.Helper.Content.Cards.IsCardTraitActive(s, card, ModEntry.Instance.Helper.Content.Cards.TemporaryCardTrait))) {
				works = true;
				card.upgrade = upgrades[s.rngActions.NextInt() % upgrades.Length];
			}
		}
		if (works) Audio.Play(Event.Status_PowerUp);
	}
}