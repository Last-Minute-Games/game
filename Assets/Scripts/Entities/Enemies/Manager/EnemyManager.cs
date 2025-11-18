namespace Entities.Enemies.Manager
{
    using System.Collections.Generic;
    using UnityEngine;

    public class EnemyManager : MonoBehaviour
    {
        [Header("Data")]
        public List<EnemyData> enemies = new();

        [Header("Rendering")]
        [Tooltip("Parent container where enemy GameObjects (SpriteRenderer + Animator) will be spawned in world space.")]
        public Transform uiContainer; // now a Transform, not a RectTransform

        [Header("Layout (Line-up)")]
        [Tooltip("Horizontal spacing between enemies in world units.")]
        [SerializeField] private float horizontalSpacing = 2f;
        [Tooltip("Fixed Y offset for the line-up (local to container).")]
        [SerializeField] private float yOffset = 0f;
        [Tooltip("Center the lineup around X=0. If false, lineup starts at X=0 and grows positive.")]
        [SerializeField] private bool centerAlign = true;
        [Tooltip("Automatically layout enemies after (re)building.")]
        [SerializeField] private bool autoLayoutOnBuild = true;

        private readonly List<EnemyRender> _activeRenders = new();

        public void InitializeEnemies(List<EnemyData> enemyList)
        {
            enemies = enemyList;
            BuildEnemyUI();
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
                if (!enemy.entity.isAlive) continue;
                enemy.DecideNextIntent();

                // Ensure idle is playing so player sees them idling before attack
                var r = GetRenderFor(enemy);
                if (r != null) r.PlayIdle();
            }
        }

        // Enemies execute their previously decided intents
        public void ExecuteEnemyTurn(ref EntityData player)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (!enemy.entity.isAlive) continue;

                // Play attack animation right before executing if intent is attack
                if (enemy.currentIntent == EnemyIntent.Attack)
                {
                    var r = GetRenderFor(enemy);
                    if (r != null) r.PlayAttack();
                }

                enemy.ExecuteIntent(ref player);
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
                if (e.entity.isAlive) return false;
            return true;
        }
    }
}