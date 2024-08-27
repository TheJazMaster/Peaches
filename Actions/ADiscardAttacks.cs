using System.Collections.Generic;
using System.Threading;

#nullable enable
namespace TheJazMaster.Peaches.Actions;

public class ADiscardAttacks : CardAction
{
    public ADrawCard? drawAction;
    public bool exhaust = false;
    public int mult = 1;
    public int flat = 0;
    
    public override void Begin(G g, State s, Combat c)
    {
        List<Card> candidates = [];
        foreach (Card card in c.hand) {
            foreach (CardAction action in card.GetActionsOverridden(s, c)) {
                if (action is AAttack) {
                    candidates.Add(card);
                    break;
                }
            }
        }

        int i = 0;
        foreach (Card card in candidates) {
            if (exhaust) {
                card.OnDiscard(s, c);
				card.ExhaustFX();
                c.SendCardToExhaust(s, card);
				c.QueueImmediate(new ADelay());
            } else {
                c.hand.Remove(card);
                card.waitBeforeMoving = i * 0.05;
                card.OnDiscard(s, c);
                c.SendCardToDiscard(s, card);
            }
            i++;
        }

        if (drawAction != null)
            drawAction.count = i*mult + flat;
    }


    public override Icon? GetIcon(State s) {
        return new Icon(StableSpr.icons_discardCard, null, Colors.textMain);
    }

	public override List<Tooltip> GetTooltips(State s) => [
        new TTGlossary("action.drawCard", "?"),
        new TTGlossary("action.attack", "?")
    ];
}