using Nickel;

namespace TheJazMaster.Peaches;

internal interface IPeachesCard
{
	static abstract void Register(IModHelper helper);
}

internal interface IPeachesArtifact
{
	static abstract void Register(IModHelper helper);
}