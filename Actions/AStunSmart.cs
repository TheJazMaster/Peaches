namespace TheJazMaster.UnseenEffort.Actions;

public class AStunSmart : AStunPart
{
    public required Part part;
    public override void Begin(G g, State s, Combat c)
    {
        int index = c.otherShip.parts.FindIndex(p => p == part);
        if (index == -1) return;
        new AStunPart {
            worldX = c.otherShip.x + index
        }.Begin(g, s, c);
    }


    public override Icon? GetIcon(State s) {
        return new Icon(ModEntry.Instance.DrawAttacksIcon, null, Colors.textMain);
    }
}