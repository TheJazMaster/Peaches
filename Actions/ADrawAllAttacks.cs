using System.Collections.Generic;

#nullable enable
namespace TheJazMaster.Peaches.Actions;

public class ADrawAllAttacks : CardAction
{   
    public override void Begin(G g, State s, Combat c)
    {
        int i = 0;
        while (i < s.deck.Count) {
            Card card = s.deck[i];
            foreach (CardAction action in card.GetActionsOverridden(s, c)) {
                if (action is AAttack) {
                    if (c.hand.Count >= 10) {
                        c.PulseFullHandWarning();
                        break;
                    }
                    c.DrawCardIdx(s, i);
                    continue;
                }
            }
            i++;
        }
    }


    public override Icon? GetIcon(State s) {
        return new Icon(ModEntry.Instance.DrawAttacksIcon.Sprite, null, Colors.textMain);
    }
}