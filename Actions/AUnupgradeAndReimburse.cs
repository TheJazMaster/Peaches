using System.Collections.Generic;
using FSPRO;
using Nickel;
using TheJazMaster.UnseenEffort.Cards.Carrie;

namespace TheJazMaster.UnseenEffort.Actions;

public class AUnupgradeAndReimburse : CardAction
{
	public required int reimburseAmount;

	public override void Begin(G g, State s, Combat c)
	{
		if (selectedCard == null) return;

		selectedCard.upgrade = Upgrade.None;

		c.QueueImmediate(new AAddCard {
			card = new ReimburseCard(),
			amount = reimburseAmount,
			destination = CardDestination.Hand
		});
		Audio.Play(Event.Status_PowerDown);
	}

    public override List<Tooltip> GetTooltips(State s) => [
        new TTCard {
            card = new ReimburseCard()
        }
    ];
}