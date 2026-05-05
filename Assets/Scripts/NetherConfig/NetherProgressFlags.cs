/// <summary>
/// GameFlags keys for Nether progression (card catalog unlock, etc.).
/// In this project, "the Nether" is the card battle — scene <c>BattleScene</c>.
/// </summary>
public static class NetherProgressFlags
{
    /// <summary>Set after the player finishes any run in BattleScene (win or lose) at least once.</summary>
    public const string HasVisitedNether = "nether.visited";
}
