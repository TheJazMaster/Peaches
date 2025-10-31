using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nickel;
using TheJazMaster.UnseenEffort.Features;

namespace TheJazMaster.UnseenEffort.Actions;

public class ARandomBrittle : CardAction
{
    public bool hidden = false;
    public bool single = false;
    public int count = 1;
    public bool targetPlayer = false;
    public bool weakInstead = false;
    
    public override void Begin(G g, State s, Combat c)
    {
        Ship ship = targetPlayer ? s.ship : c.otherShip;
        PDamMod mod = weakInstead ? PDamMod.weak : PDamMod.brittle;
        
        var list = (from pair in ship.parts.Select((Part part, int x) => new {part, x})
            where pair.part.damageModifier != mod && pair.part.damageModifier != PDamMod.brittle && pair.part.type != PType.empty select pair).ToList();
        
        list = list.Shuffle(s.rngActions).Take(count).ToList();

        foreach (var pair in list) {
            var part = pair.part;
            var x = pair.x;

            SingleDamModManager.SetData(part, single);

            c.QueueImmediate(weakInstead ? new AWeaken
			{
				targetPlayer = targetPlayer,
				worldX = x + ship.x
			} : new ABrittle
			{
				targetPlayer = targetPlayer,
				worldX = x + ship.x,
				makeTheBrittlenessInvisible = hidden
			});
        }
    }

    private Spr GetSprite() => hidden ? (single ? ModEntry.Instance.SecretSingleBrittleIcon : ModEntry.Instance.SecretBrittleIcon)
        : (single ? ModEntry.Instance.SingleBrittleIcon : StableSpr.icons_brittle);

	public override Icon? GetIcon(State s) =>
		new Icon(GetSprite(), count, Colors.redd);

	public override List<Tooltip> GetTooltips(State s) => [
        new GlossaryTooltip($"action.{GetType().Namespace!}::RandomBrittle" + single + hidden) {
            Icon = GetSprite(),
            TitleColor = Colors.action,
            Title = ModEntry.Instance.Localizations.Localize(["action", "brittle", hidden ? "hidden" : "normal", single ? "single" : "normal", "name"]),
            Description = ModEntry.Instance.Localizations.Localize(["action", "brittle", hidden ? "hidden" : "normal", single ? "single" : "normal", "description", count == 1 ? "singular" : "plural"], new { Amount = count })
        },
        new TTGlossary("parttrait.brittle")
    ];

}