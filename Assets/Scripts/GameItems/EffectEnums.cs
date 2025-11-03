public enum TargetRule // do not change target rules specifically
{
    None,
    Self,
    Enemy,
    AllEnemies
}

public enum OperationType // ts mainly dealt thru feedbackmanager and datainteractionhelper
{
    None, 
    DamageHealth,
    HealHealth,
    MultiplyPowerScale,
    DamageShield,
    AddShield,
    AddEnergy,
    ShuffleDeck,
    EndTurn
}

public enum TimeUnit
{
    Turns,
    Rounds
}
