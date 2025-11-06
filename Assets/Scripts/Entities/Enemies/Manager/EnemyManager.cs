using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    [Header("Data")]
    public List<EnemyData> enemies = new();

    [Header("UI Rendering")]
    [Tooltip("Parent UI container where enemy UI items will be spawned (e.g., a HorizontalLayoutGroup under a Canvas).")]
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
            var go = new GameObject(string.IsNullOrEmpty(enemy.enemyName) ? "EnemyUI" : enemy.enemyName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Animator), typeof(EnemyRender));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(uiContainer, false);
            rt.sizeDelta = new Vector2(100, 100);

            var img = go.GetComponent<Image>();
            img.preserveAspect = true;

            var render = go.GetComponent<EnemyRender>();
            render.artworkImage = img;
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