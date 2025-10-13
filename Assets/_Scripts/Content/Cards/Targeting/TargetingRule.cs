using UnityEngine;

public abstract class TargetingRule : ScriptableObject {
    public abstract CharacterBase ResolveTarget(CharacterBase user, Collider2D hit);
}

