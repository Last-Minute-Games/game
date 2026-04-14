using UnityEngine;
// this is the runtime of the actual logic cards

public class OverhaulCard
{
  public string Title => data.name; // name of actual scriptable obj

  public string Description => data.Description;

  public Sprite Icon => data.Icon;

  public Color IconBackgroundColor => data.IconBackgroundColor;

  public int Mana { get; private set; }

  private readonly OverhaulCardData data;

  public OverhaulCard(OverhaulCardData cardData)
  {
    data = cardData;
    Mana = cardData.Mana; // if mana value changed, only changed for this card instance
  }
}
