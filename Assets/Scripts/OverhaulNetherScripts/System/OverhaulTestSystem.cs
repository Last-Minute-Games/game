using System.Collections.Generic;
using UnityEngine;

public class OverhaulTestSystem : MonoBehaviour
{
  [SerializeField] private List<OverhaulCardData> deckData;

  private void Start()
  {
    OverhaulCardSystem.Instance.Setup(deckData);
  }
}
