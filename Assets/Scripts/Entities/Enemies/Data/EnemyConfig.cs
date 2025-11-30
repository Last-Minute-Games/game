using System;
using System.Collections.Generic;
using UnityEngine;
using Entities.Enemies.Helpers;

[CreateAssetMenu(menuName = "Enemies/Enemy Data", fileName = "NewEnemy")]
public class EnemyConfig : ScriptableObject
{
    [Header("Core")]
    public string enemyName;
    public int maxHealth;

    [Tooltip("Fallback values if an actionPattern element's value is 0.")]
    public int attackPower = 10;
    public int defensePower = 5;
    public int healPower = 5;
    public int buffPower = 1;

    public Sprite artwork;
    public List<EnemyAction> actionPattern;

    // ─────────────────────────────────────────────────────────────────────
    // Per-Intent Variability Sliders (multipliers applied to actionPattern.value)
    // ─────────────────────────────────────────────────────────────────────
    [Serializable]
    public struct IntentMultiplierRange
    {
        [Range(0.3f, 4f)] public float min;
        [Range(0.3f, 4f)] public float max;

        public void Normalize()
        {
            if (min > max) min = max;
            min = Mathf.Clamp(min, 0.3f, 4f);
            max = Mathf.Clamp(max, 0.3f, 4f);
        }

        public IntentMultiplierRange Scaled(float factor)
        {
            var r = new IntentMultiplierRange { min = min * factor, max = max * factor };
            r.Normalize(); // clamp + order
            return r;
        }
    }

    [Header("Intent Multipliers (0.3–4, triangle-sampled each use)")]
    public IntentMultiplierRange attackRange = new IntentMultiplierRange { min = 1f, max = 1f };
    public IntentMultiplierRange blockRange  = new IntentMultiplierRange { min = 1f, max = 1f };
    public IntentMultiplierRange healRange   = new IntentMultiplierRange { min = 1f, max = 1f };
    public IntentMultiplierRange buffRange   = new IntentMultiplierRange { min = 1f, max = 1f };

    [Header("Animator (Optional)")]
    [Tooltip("If assigned, this RuntimeAnimatorController will drive the enemy's animation states (Idle/Attack/Hurt/Death). Drag & drop here.")]
    public RuntimeAnimatorController animatorController;

    [Header("Sprite Animations (Lightweight, used if no Animator Controller)")]
    public SpriteAnimation idleAnim;
    public SpriteAnimation attackAnim;
    public SpriteAnimation hurtAnim;
    public SpriteAnimation deathAnim;

    [Header("Visual Adjustments")]
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 scaleOffset = Vector3.one;

    [Header("Global Enemy Variability Multiplier (applies to HP + intent sliders)")]
    [Range(0f, 2f)] public float minMultiplier = 1f;
    [Range(0f, 2f)] public float maxMultiplier = 1f;

    [Header("Identity")]
    public string uniqueID;

    public EnemyData CreateRuntimeInstance()
    {
        var data = new EnemyData();
        data.sourceConfig = this;

        // Global (triangle) multiplier
        float globalMul = GetMiddleBiasedMultiplier();
        Debug.Log($"{globalMul} enemy multiplier applied");

        // Initialize runtime data; scale max HP by global
        data.Initialize(enemyName, (int)(maxHealth * globalMul), attackPower, defensePower);

        // Also pass heal/buff fallbacks (scaled) into runtime
        data.healPower = Mathf.RoundToInt(healPower * globalMul);
        data.buffPower = Mathf.RoundToInt(buffPower * globalMul);

        // Copy pattern & visuals
        data.actionPattern = actionPattern != null ? new List<EnemyAction>(actionPattern) : new List<EnemyAction>();
        data.artwork = artwork;
        data.animatorController = animatorController;
        data.idleAnim   = idleAnim;
        data.attackAnim = attackAnim;
        data.hurtAnim   = hurtAnim;
        data.deathAnim  = deathAnim;
        data.positionOffset = positionOffset;
        data.scaleOffset    = scaleOffset;

        // Effective ranges = sliders scaled by global multiplier
        var effAttack = attackRange; effAttack.Normalize();
        var effBlock  = blockRange;  effBlock.Normalize();
        var effHeal   = healRange;   effHeal.Normalize();
        var effBuff   = buffRange;   effBuff.Normalize();

        data.attackRange = effAttack.Scaled(globalMul);
        data.blockRange  = effBlock .Scaled(globalMul);
        data.healRange   = effHeal  .Scaled(globalMul);
        data.buffRange   = effBuff  .Scaled(globalMul);

        // Ensure pattern zeros use fallbacks at runtime too
        data.NormalizeActionPatternWithFallbacks();

        return data;
    }

    public float GetMiddleBiasedMultiplier()
    {
        float a = UnityEngine.Random.Range(minMultiplier, maxMultiplier);
        float b = UnityEngine.Random.Range(minMultiplier, maxMultiplier);
        return (a + b) * 0.5f;
    }

    protected void OnValidate()
    {
        // global clamps
        if (minMultiplier > maxMultiplier) minMultiplier = maxMultiplier;
        minMultiplier = Mathf.Max(0f, minMultiplier);
        maxMultiplier = Mathf.Max(minMultiplier, maxMultiplier);

        // ranges
        attackRange.Normalize();
        blockRange .Normalize();
        healRange  .Normalize();
        buffRange  .Normalize();

        // assign id
        if (string.IsNullOrWhiteSpace(uniqueID))
            uniqueID = Guid.NewGuid().ToString();

        // Fill zero-valued pattern entries with fallbacks (edit-time convenience)
        if (actionPattern != null)
        {
            for (int i = 0; i < actionPattern.Count; i++)
            {
                var a = actionPattern[i];
                if (a.value <= 0)
                {
                    a.value = a.intent switch
                    {
                        EnemyIntent.Attack => Mathf.Max(0, attackPower),
                        EnemyIntent.Block  => Mathf.Max(0, defensePower),
                        EnemyIntent.Heal   => Mathf.Max(0, healPower),
                        EnemyIntent.Buff   => Mathf.Max(0, buffPower),
                        _ => a.value
                    };
                    actionPattern[i] = a;
                }
            }
        }
    }
}
