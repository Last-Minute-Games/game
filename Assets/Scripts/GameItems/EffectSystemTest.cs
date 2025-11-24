using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Test script to validate the new Effect struct system.
/// Attach this to a GameObject to run tests.
/// </summary>
public class EffectSystemTest : MonoBehaviour
{
    [Header("Test Card Data")]
    [Tooltip("Assign a CardData asset to test")]
    public CardData testCard;

    [Header("Test Results")]
    [SerializeField] private bool testsRun = false;
    [SerializeField] private string testResults = "";

    [ContextMenu("Run Effect System Tests")]
    public void RunTests()
    {
        testResults = "=== Effect System Tests ===\n\n";
        testsRun = true;

        // Test 1: Create default effect
        Test_CreateDefaultEffect();

        // Test 2: Clone effect with multiplier
        Test_CloneEffectWithMultiplier();

        // Test 3: Effect in list (like EnemyAction)
        Test_EffectList();

        // Test 4: CardData integration
        if (testCard != null)
        {
            Test_CardDataIntegration();
        }
        else
        {
            testResults += "❌ Test 4: Skipped (no testCard assigned)\n";
        }

        Debug.Log(testResults);
    }

    private void Test_CreateDefaultEffect()
    {
        try
        {
            Effect defaultEffect = Effect.CreateDefault();
            
            bool passed = defaultEffect.operationType == OperationType.None &&
                         defaultEffect.targetRule == TargetRule.None &&
                         defaultEffect.baseValue == 0 &&
                         defaultEffect.minMultiplier == 1f &&
                         defaultEffect.maxMultiplier == 1f;

            testResults += passed 
                ? "✓ Test 1: CreateDefault() - PASSED\n" 
                : "❌ Test 1: CreateDefault() - FAILED\n";
        }
        catch (System.Exception e)
        {
            testResults += $"❌ Test 1: CreateDefault() - EXCEPTION: {e.Message}\n";
        }
    }

    private void Test_CloneEffectWithMultiplier()
    {
        try
        {
            Effect original = new Effect
            {
                operationType = OperationType.Damage,
                targetRule = TargetRule.Enemy,
                baseValue = 10,
                minMultiplier = 1f,
                maxMultiplier = 2f,
                variableColor = Color.red
            };

            Effect clone = original.Clone(true);
            
            bool passed = clone.operationType == OperationType.Damage &&
                         clone.targetRule == TargetRule.Enemy &&
                         clone.postCopyValue >= 10 && clone.postCopyValue <= 20;

            testResults += passed 
                ? $"✓ Test 2: Clone() with multiplier - PASSED (rolled: {clone.postCopyValue})\n" 
                : "❌ Test 2: Clone() with multiplier - FAILED\n";
        }
        catch (System.Exception e)
        {
            testResults += $"❌ Test 2: Clone() - EXCEPTION: {e.Message}\n";
        }
    }

    private void Test_EffectList()
    {
        try
        {
            List<Effect> effects = new List<Effect>
            {
                new Effect { operationType = OperationType.Damage, baseValue = 5 },
                new Effect { operationType = OperationType.AddShield, baseValue = 3 },
                new Effect { operationType = OperationType.Heal, baseValue = 2 }
            };

            bool passed = effects.Count == 3 &&
                         effects[0].operationType == OperationType.Damage &&
                         effects[1].operationType == OperationType.AddShield &&
                         effects[2].operationType == OperationType.Heal;

            testResults += passed 
                ? "✓ Test 3: Effect in List - PASSED\n" 
                : "❌ Test 3: Effect in List - FAILED\n";
        }
        catch (System.Exception e)
        {
            testResults += $"❌ Test 3: Effect List - EXCEPTION: {e.Message}\n";
        }
    }

    private void Test_CardDataIntegration()
    {
        try
        {
            if (testCard.effects == null)
            {
                testResults += "❌ Test 4: CardData Integration - FAILED (effects is null)\n";
                return;
            }

            int effectCount = testCard.effects.Count;
            testResults += $"✓ Test 4: CardData Integration - PASSED ({effectCount} effects found)\n";

            // List each effect
            for (int i = 0; i < effectCount; i++)
            {
                Effect e = testCard.effects[i];
                testResults += $"  - Effect {i}: {e.operationType} (value: {e.baseValue}, target: {e.targetRule})\n";
            }
        }
        catch (System.Exception e)
        {
            testResults += $"❌ Test 4: CardData Integration - EXCEPTION: {e.Message}\n";
        }
    }

    private void OnValidate()
    {
        if (testsRun)
        {
            testsRun = false;
        }
    }
}

