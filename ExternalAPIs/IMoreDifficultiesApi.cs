namespace TheJazMaster.UnseenEffort;

public interface IMoreDifficultiesApi
{
	void RegisterAltStarters(Deck deck, StarterDeck starterDeck);
    bool HasAltStarters(Deck deck);
    bool AreAltStartersEnabled(State state, Deck deck);
	StarterDeck? GetAltStarters(Deck deck);
	bool IsBanned(State state, Deck deck);
	bool IsLocked(State state, Deck deck);

}
