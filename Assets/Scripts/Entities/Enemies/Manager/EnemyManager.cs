using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("Data")]
    public List<EnemyData> enemies = new();

    [Header("Rendering")]
    [Tooltip("Parent container where enemy GameObjects (SpriteRenderer + Animator) will be spawned.")]
    public RectTransform uiContainer;

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

        foreach (var enemy in enemies)
        {
            var go = new GameObject(string.IsNullOrEmpty(enemy.enemyName) ? "Enemy" : enemy.enemyName, typeof(RectTransform), typeof(Animator), typeof(SpriteRenderer), typeof(EnemyRender));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(uiContainer, false);

            // Match the RectTransform look from the screenshot
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition3D = Vector3.zero; // Pos X/Y/Z = 0
            rt.sizeDelta = new Vector2(100f, 100f); // Width/Height = 100
            rt.localScale = Vector3.one * 10f;
            rt.localRotation = Quaternion.identity;

            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = enemy.artwork;
            // Optional: reflect screenshot Order in Layer = 99
            sr.sortingOrder = 99;

            var render = go.GetComponent<EnemyRender>();
            render.Bind(enemy);

            _activeRenders.Add(render);
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