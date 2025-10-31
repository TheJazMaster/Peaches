using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using TheJazMaster.UnseenEffort.Cards;
using TheJazMaster.UnseenEffort.Cards.Peaches;

namespace TheJazMaster.UnseenEffort.Actions;

public class AVariableHintSingleUse : AVariableHint
{
    public int? value = null;

    public AVariableHintSingleUse() : base() {
        hand = true;
    }

    public override Icon? GetIcon(State s) {
        return new Icon(StableSpr.icons_singleUse, null, Colors.textMain);
    }

	public override List<Tooltip> GetTooltips(State s)
	{
        DefaultInterpolatedStringHandler stringHandler = new(22, 1);
        if (value.HasValue) {
            stringHandler.AppendLiteral(" (" + value.Value + ")");
        }
		return [
            new TTText(ModEntry.Instance.Localizations.Localize(["action", "variableHintSingleUse"], new { Parentheses = stringHandler.ToStringAndClear() })),
            new TTGlossary("cardtrait.singleUse")
        ];
	}
}