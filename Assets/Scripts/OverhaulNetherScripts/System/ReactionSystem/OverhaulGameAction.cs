using System.Collections.Generic;
using UnityEngine;

public abstract class OverhaulGameAction
{
  public List<OverhaulGameAction> PreReactions { get; private set; } = new();

  public List<OverhaulGameAction> PerformReactions { get; private set; } = new();

  public List<OverhaulGameAction> PostReactions { get; private set; } = new();
}
