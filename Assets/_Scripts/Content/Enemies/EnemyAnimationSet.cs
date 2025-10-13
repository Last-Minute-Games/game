using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Animation Set")]
public class EnemyAnimationSet : ScriptableObject
{
    [Header("Each array should hold 4 frames (or any length)")]
    public Sprite[] idle;
    public Sprite[] floatLoop;
    public Sprite[] attack;
    public Sprite[] hurt;
    public Sprite[] death;

    public Sprite idleSprite => (idle != null && idle.Length > 0) ? idle[0] : null;

    [Header("Playback FPS (per animation)")]
    public float defaultFps = 8f;
    public float idleFps = 8f;
    public float floatFps = 10f;
    public float attackFps = 12f;
    public float hurtFps = 14f;
    public float deathFps = 6f;

    public float GetFps(EnemyAnim state) =>
        state switch
        {
            EnemyAnim.Idle   => idleFps,
            EnemyAnim.Float  => floatFps,
            EnemyAnim.Attack => attackFps,
            EnemyAnim.Hurt   => hurtFps,
            EnemyAnim.Death  => deathFps,
            _ => defaultFps
        };
}
