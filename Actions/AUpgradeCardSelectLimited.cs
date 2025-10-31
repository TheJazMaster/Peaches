using System.Collections.Generic;
using System.Linq;
using Nickel;

namespace TheJazMaster.UnseenEffort.Actions;

public class AUpgradeCardSelectLimited : AUpgradeCardSelect
{
    internal static readonly string LimitDeckKey = "LimitDeck";

    public required HashSet<Card> comparisonList;

    public override Route? BeginWithRoute(G g, State s, Combat c)
	{
		timer = 0.0;
		HashSet<Card> list = s.deck.Concat(c.discard).Concat(c.hand).Concat(c.exhausted).ToHashSet();
        comparisonList.ExceptWith(list);
        Deck? limitDeck = comparisonList.FirstOrDefault()?.GetMeta().deck;
        var ret = new CardBrowse
        {
            mode = CardBrowse.Mode.UpgradeCard,
            allowCancel = allowCancel,
        };
        if (limitDeck.HasValue) ret.ApplyModData(LimitDeckKey, limitDeck.Value);
        return ret;
    }
}