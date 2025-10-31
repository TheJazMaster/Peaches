namespace TheJazMaster.UnseenEffort.Artifacts;

public interface IDisarmAffectorArtifact
{
    public bool IgnoreDisarm(State s, Combat c, AAttack? attack);
}