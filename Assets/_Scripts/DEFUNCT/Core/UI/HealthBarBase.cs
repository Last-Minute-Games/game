using UnityEngine;

public abstract class HealthBarBase : MonoBehaviour
{
    public abstract void Initialize(CharacterBase target);
    public abstract void UpdateHealth(int current, int max);
    public abstract void UpdateBlock(int block);
}
