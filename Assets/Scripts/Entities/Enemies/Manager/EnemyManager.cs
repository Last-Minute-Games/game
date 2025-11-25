using Entities.Enemies.Helpers;
using Entities.Players.Data;
using GameItems.Cards;

namespace Entities.Enemies.Manager
{
    using System.Collections.Generic;
    using UnityEngine;

    public class EnemyManager : MonoBehaviour
    {
        [Header("Data")]
        public List<EnemyData> enemies = new();

        [Header("Intent Icons")]
        [Tooltip("Reference to the global card icon library for enemy intents.")]
        public CardIconLibrary iconLibrary;

        [Header("Rendering")]
        [Tooltip("Parent container where enemy GameObjects (SpriteRenderer + Animator) will be spawned in world space.")]
        public Transform uiContainer; // now a Transform, not a RectTransform
        [Tooltip("Prefab for the enemy health bar UI (must have EnemyHealth component).")]
        [SerializeField] private GameObject healthBarPrefab;

        [Header("Layout (Line-up)")]
        [Tooltip("Horizontal spacing between enemies in world units.")]
        [SerializeField] private float horizontalSpacing = 2f;
        [Tooltip("Fixed Y offset for the line-up (local to container).")]
        [SerializeField] private float yOffset;
        [Tooltip("Center the lineup around X=0. If false, lineup starts at X=0 and grows positive.")]
        [SerializeField] private bool centerAlign = true;
        [Tooltip("Automatically layout enemies after (re)building.")]
        [SerializeField] private bool autoLayoutOnBuild = true;

        [Header("Health Bar Settings")]
        [Tooltip("Local position offset for health bar relative to enemy.")]
        [SerializeField] private Vector3 healthBarOffset = new Vector3(0f, 0.17f, 0f);
        
        private readonly List<EnemyRender> _activeRenders = new();

        public void InitializeEnemies(List<EnemyData> enemyList)
        {
            enemies = enemyList;
            BuildEnemyUI();

            foreach (var e in enemies)
            {
                // track the enemy encounter in list
                TrackEnemyEncounter(e.sourceConfig);
            }
        }

        private void TrackEnemyEncounter(EnemyConfig config)
        {
            if (config == null) return;

            string flagName = $"monster.{config.enemyName.ToLower()}";

            if (!GameFlags.HasFlag(flagName))
            {
                GameFlags.SetFlag(flagName);
                Debug.Log($"[Flags] First encounter with {config.enemyName}. Set flag: {flagName}");
            }

            // If you prefer using unique IDs instead:
            // string flagName = $"monster.{config.uniqueID}";
        }

        private void BuildEnemyUI()
        {
            // Cleanup old
            foreach (var r in _activeRenders)
            {
                if (r != null) Destroy(r.gameObject);
            }
            _activeRenders.Clear();

            if (uiContainer == null)
            {
                Debug.LogWarning("EnemyManager: uiContainer not set. Enemies will not be rendered.");
                return;
            }

            if (healthBarPrefab == null)
            {
                Debug.LogWarning("EnemyManager: healthBarPrefab not set. Health bars will not be created.");
            }

            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                var go = new GameObject(string.IsNullOrEmpty(enemy.enemyName) ? $"Enemy_{i}" : enemy.enemyName,
                    typeof(Animator), typeof(SpriteRenderer), typeof(EnemyRender));

                // Parent under the provided container
                var t = go.transform;
                t.SetParent(uiContainer, false);

                // World-space transform defaults
                t.localPosition = Vector3.zero;
                t.localScale = Vector3.one * 10f;
                t.localRotation = Quaternion.identity;

                var sr = go.GetComponent<SpriteRenderer>();
                sr.sprite = enemy.artwork;
                sr.sortingOrder = i; // order by spawn index (front-to-back or vice versa as needed)

                // Add health bar if prefab is available
                if (healthBarPrefab != null)
                {
                    var healthBarGo = Instantiate(healthBarPrefab, go.transform);
                    var healthComp = healthBarGo.GetComponent<EnemyHealth>();
                    if (healthComp != null)
                    {
                        healthComp.SetLocalPosition(healthBarOffset);
                        // Initial health will be set in EnemyRender.Bind
                    }
                    else
                    {
                        Debug.LogWarning($"EnemyManager: healthBarPrefab for {enemy.enemyName} does not have EnemyHealth component.");
                    }
                }

                var render = go.GetComponent<EnemyRender>();
                render.Bind(enemy);

                _activeRenders.Add(render);
            }

            if (autoLayoutOnBuild)
                ApplyLineUpLayout();
        }

        private void ApplyLineUpLayout()
        {
            int count = _activeRenders.Count;
            if (count == 0) return;

            float startX = centerAlign ? -((count - 1) * horizontalSpacing) * 0.5f : 0f;

            for (int i = 0; i < count; i++)
            {
                var r = _activeRenders[i];
                if (r == null) continue;

                var t = r.transform;
                t.localPosition = new Vector3(startX + i * horizontalSpacing, yOffset, 0f);
            }
        }

        // Roll/decide next intents for all alive enemies (called at round start)
        public void RollNextIntents()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (!enemy.isAlive) continue;

                // DON'T reset block here - let it persist through the player's turn

                enemy.DecideNextIntent();

                // Ensure idle is playing so player sees them idling before attack
                var r = GetRenderFor(enemy);
                if (r != null)
                {
                    r.PlayIdle();
                    r.UpdateIntentIcon(); // Show the intent icon
                    r.UpdateHealth(); // Keep existing block visible
                }
            }
        }


        public void RemoveDeadEnemies()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i].isAlive)
                {
                    enemies.RemoveAt(i);

                    if (_activeRenders.Count > i)
                        _activeRenders.RemoveAt(i);
                }
            }

            // re-layout remaining enemies
            // ApplyLineUpLayout();
        }

        // Enemies execute their previously decided intents with delays (turn-based feel)
        public System.Collections.IEnumerator ExecuteEnemyTurnSequence(PlayerData player)
        {

            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (!enemy.isAlive) continue;

                Debug.Log($"[EnemyManager] {enemy.enemyName} is taking their turn...");

                // Get the render for animation
                var r = GetRenderFor(enemy);

                // Show move name popup BEFORE executing the move
                if (r != null)
                {
                    // Use GetMoveNameForAction to support custom names
                    string moveName = r.GetMoveNameForAction(enemy.currentAction);
                    
                    // Show popup and wait for it to be visible
                    r.ShowMoveNamePopup(moveName);
                    yield return new WaitForSeconds(0.3f); // Brief pause to let player see the move name
                }

                // Play intent animation based on type
                if (enemy.currentIntent == EnemyIntent.Attack)
                {
                    if (r != null) r.PlayAttack();
                    
                    // Wait for attack animation to play out
                    yield return new WaitForSeconds(0.5f);
                }
                else if (enemy.currentIntent == EnemyIntent.Block)
                {
                    if (r != null) r.PlayIdle(); // Or a defend animation if you have one
                    yield return new WaitForSeconds(0.3f);
                }
                else
                {
                    // Default for Heal/Buff/other intents
                    if (r != null) r.PlayIdle();
                    yield return new WaitForSeconds(0.3f);
                }

                // Execute the intent (apply damage, gain block, etc.)
                enemy.ExecuteIntent(player);

                // Update the enemy's health display to show new shield/health values
                if (r != null)
                {
                    r.UpdateHealth();
                }

                // Brief pause to show the effect
                yield return new WaitForSeconds(0.4f);

                // Return to idle after action
                if (r != null) r.PlayIdle();

                // Delay before next enemy acts
                yield return new WaitForSeconds(0.6f);
            }

            Debug.Log("[EnemyManager] All enemies have completed their turns.");
        }

        // Legacy synchronous method - kept for backwards compatibility but deprecated
        [System.Obsolete("Use ExecuteEnemyTurnSequence coroutine instead for turn-based delays")]
        public void ExecuteEnemyTurn(ref PlayerData player)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (!enemy.isAlive) continue;

                // Play attack animation right before executing if intent is attack
                if (enemy.currentIntent == EnemyIntent.Attack)
                {
                    var r = GetRenderFor(enemy);
                    if (r != null) r.PlayAttack();
                }

                enemy.ExecuteIntent(player);
            }
        }

        // Call after enemy takes damage to update health display
        public void UpdateEnemyHealth(EnemyData enemy)
        {
            var render = GetRenderFor(enemy);
            if (render != null)
            {
                render.UpdateHealth();
                
                // Play hurt animation if still alive
                if (enemy.isAlive)
                {
                    render.PlayHurt();
                }
                else
                {
                    render.PlayDeath();

                    RemoveDeadEnemies(); // remove from manager list
                }
            }
            FindFirstObjectByType<RoundManager>()?.CheckImmediateEndConditions();
        }

        // Age enemy block and reset old block (1+ turns old)
        // This allows block gained THIS turn to persist through the next round
        public void ResetAllEnemyBlock()
        {
            foreach (var enemy in enemies)
            {
                if (!enemy.isAlive) continue;

                // Increment block age
                if (enemy.block > 0)
                {
                    enemy.blockAge++;
                    
                    // Only reset block that's 1 or more turns old
                    if (enemy.blockAge >= 2)
                    {
                        Debug.Log($"[EnemyManager] {enemy.enemyName} block expired (age {enemy.blockAge}): {enemy.block} → 0");
                        enemy.block = 0;
                        enemy.blockAge = 0;
                        
                        var r = GetRenderFor(enemy);
                        if (r != null)
                            r.UpdateHealth();
                    }
                    else
                    {
                        Debug.Log($"[EnemyManager] {enemy.enemyName} block persists (age {enemy.blockAge}): {enemy.block} block");
                    }
                }
            }
        }

        private EnemyRender GetRenderFor(EnemyData enemy)
        {
            // Renders are built in the same order as enemies
            int idx = enemies.IndexOf(enemy);
            if (idx >= 0 && idx < _activeRenders.Count)
                return _activeRenders[idx];
            return null;
        }

        public bool AllEnemiesDefeated()
        {
            foreach (var e in enemies)
                if (e.isAlive) return false;
            return true;
        }
    }
}
