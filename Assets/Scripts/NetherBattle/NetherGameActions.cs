using UnityEngine;

/// <summary>
/// Production Nether battle actions for <see cref="OverhaulActionSystem"/> (same pattern as OverhaulNetherScripts).
/// </summary>
public class NetherDrawCardsGA : OverhaulGameAction
{
  public int Amount { get; }

  public NetherDrawCardsGA(int amount)
  {
    Amount = amount;
  }
}

public class NetherDiscardAllHandGA : OverhaulGameAction
{
}

/// <summary>
/// Enemy combat phase (intents execute). Used with performers registered on <see cref="NetherBattleActionBridge"/>.
/// </summary>
public class NetherEnemyPhaseGA : OverhaulGameAction
{
}
