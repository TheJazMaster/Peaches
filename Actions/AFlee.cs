using Nickel;
using TheJazMaster.UnseenEffort.Features;

namespace TheJazMaster.UnseenEffort.Actions;

public class AFlee : AEscape
{
    public bool keepTemp;
    public bool skipRewards;

    public override void Begin(G g, State s, Combat c)
    {
        ModEntry.Instance.Helper.ModData.SetModData(c, FleeManager.KeepTempKey, keepTemp);
        ModEntry.Instance.Helper.ModData.SetModData(c, FleeManager.LoseRewardsKey, skipRewards);
        base.Begin(g, s, c);
    }
}