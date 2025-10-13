using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EnemyAnimator2D : MonoBehaviour
{
    [SerializeField] private EnemyAnimationSet animSet;
    private SpriteRenderer sr;
    private EnemyAnim current;
    private float timer;
    private int frame;

    private Sprite[] ActiveFrames =>
        current switch
        {
            EnemyAnim.Idle   => animSet?.idle,
            EnemyAnim.Float  => animSet?.floatLoop,
            EnemyAnim.Attack => animSet?.attack,
            EnemyAnim.Hurt   => animSet?.hurt,
            EnemyAnim.Death  => animSet?.death,
            _ => null
        };

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (animSet != null) ResetState(EnemyAnim.Idle);
    }

    public void SetAnimSet(EnemyAnimationSet set)
    {
        animSet = set;
        ResetState(EnemyAnim.Idle);
    }

    public void Play(EnemyAnim state)
    {
        if (current == state) return;
        ResetState(state);
    }

    private void ResetState(EnemyAnim state)
    {
        current = state;
        timer = 0f;
        frame = 0;
        var frames = ActiveFrames;
        if (frames != null && frames.Length > 0)
            sr.sprite = frames[0];
    }

    private void Update()
    {
        var frames = ActiveFrames;
        if (animSet == null || frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;
        float frameDur = 1f / Mathf.Max(1f, animSet.GetFps(current));
        if (timer >= frameDur)
        {
            timer -= frameDur;
            bool loop = current != EnemyAnim.Death;
            frame = loop ? (frame + 1) % frames.Length : Mathf.Min(frame + 1, frames.Length - 1);
            sr.sprite = frames[frame];
        }
    }
}
