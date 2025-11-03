using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/FX Set", fileName = "NewEnemyFXData")]
public class EnemyFXData : ScriptableObject
{
    [Header("Core Animation Clips (Sprite Frames)")]
    [Tooltip("Looping idle animation frames.")]
    public Sprite[] idle;

    [Tooltip("Attack animation frames.")]
    public Sprite[] attack;

    [Tooltip("Hurt animation frames.")]
    public Sprite[] hurt;

    [Tooltip("Death animation frames.")]
    public Sprite[] death;

    [Tooltip("Action / special move animation frames.")]
    public Sprite[] action;

    [Header("Core Playback FPS (per animation)")]
    [Tooltip("Fallback FPS if no specific FPS is defined.")]
    public float defaultFps = 8f;
    public float idleFps = 8f;
    public float attackFps = 8f;
    public float hurtFps = 8f;
    public float deathFps = 8f;
    public float actionFps = 8f;

    [Header("Core SFX Cues")]
    [Tooltip("Sound effect played during idle animation.")]
    public SFXCueData sfx_idle;

    [Tooltip("Sound effect played during attack animation.")]
    public SFXCueData sfx_attack;

    [Tooltip("Sound effect played during hurt animation.")]
    public SFXCueData sfx_hurt;

    [Tooltip("Sound effect played during death animation.")]
    public SFXCueData sfx_death;

    [Tooltip("Sound effect played during action animation.")]
    public SFXCueData sfx_action;

    /// <summary>
    /// Returns the FPS associated with a given animation state.
    /// </summary>
    public float GetFps(EnemyAnim state)
    {
        switch (state)
        {
            case EnemyAnim.Idle:   return idleFps;
            case EnemyAnim.Attack: return attackFps;
            case EnemyAnim.Hurt:   return hurtFps;
            case EnemyAnim.Death:  return deathFps;
            case EnemyAnim.Action: return actionFps;
            default:                return defaultFps;
        }
    }
}
