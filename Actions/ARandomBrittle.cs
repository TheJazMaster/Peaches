using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheJazMaster.Peaches.Features;

namespace TheJazMaster.Peaches.Actions;

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
}