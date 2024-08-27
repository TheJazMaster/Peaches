namespace TheJazMaster.Peaches.Artifacts;

public interface IOnDisarmArtifact
{
    public void OnDisarm(State s, Combat c, AAttack attack);
}