public enum TargetRule // do not change target rules specifically
{
    None,
    Self,
    Enemy,
    AllEnemies
}

public enum OperationType // player and enemy manager must define handling all op types
{
    None,
    Damage, 
    AddShield,
    Heal,
    EndTurn,
    ShuffleDeck,
    MultiplyPowerScale,
    AddEnergy
}

public enum TimeUnit
{
    Turns,
    Rounds
}
