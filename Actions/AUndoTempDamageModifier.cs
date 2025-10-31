using FSPRO;
using Nickel;
using TheJazMaster.UnseenEffort.Features;

namespace TheJazMaster.UnseenEffort.Actions;

public class AUndoTempDamageModifier : CardAction
{
    public required Part part;
    public required PDamMod? oldDamageModifier;
    private static IModData ModData => ModEntry.Instance.Helper.ModData;
    
    public override void Begin(G g, State s, Combat c)
    {
        timer = 0;

        if (oldDamageModifier.HasValue && (part.damageModifier == PDamMod.weak || part.damageModifier == PDamMod.brittle)) {
            part.damageModifier = oldDamageModifier.Value;
        }

        ModData.RemoveModData(part, SingleDamModManager.OldDamageModifierKey);
    }

}