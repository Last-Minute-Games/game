using UnityEngine;

[CreateAssetMenu(menuName = "Data/OverhaulCardCard")] // serves as template / asset / raw stored data

// for ex; setting stuff like sword, heal, mana boost, etc...

public class OverhaulCardData : ScriptableObject
{
  [field: SerializeField] public string Description { get; private set; } // anyone can read the value, but no one can change it

  [field: SerializeField] public int Mana { get; private set; }

  [field: SerializeField] public Sprite Icon { get; private set; }

  [field: SerializeField] public Color IconBackgroundColor { get; private set; }

  // affects, etc.. later
}
