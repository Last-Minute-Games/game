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
    ModifyHealth,
    ModifyPowerScale,
    ModifyShield,
    ModifyEnergy,
    EndTurn
}

public enum TimeUnit
{
    Turns,
    Rounds
}
