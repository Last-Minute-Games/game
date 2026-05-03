using UnityEngine;

public class OverhaulDrawCardsGA : OverhaulGameAction
{
  public int Amount { get; set; }

  public OverhaulDrawCardsGA(int amount)
  {
    Amount = amount;
  }
}
