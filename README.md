# Castle of Time

We didn't have a readme so here's a free commit lol

enjoy this gpted readme on how to add a new **card** or new **enemy** using the new modular framework
- jester


## 🧩 Game Content Guide

### *How to Create New Cards & Enemies (Modular System)*

> All data-driven — no code editing required.

---

## 📁 Directory Overview

```
Assets/
└── Content/
    ├── Cards/
    │   ├── Data/              ← CardData assets (core definition)
    │   ├── Effects/           ← CardEffect ScriptableObjects (Damage, Heal, etc.)
    │   ├── Targeting/         ← TargetingRule assets (optional)
    │   ├── Prefabs/           ← Card prefabs (visuals)
    │   └── Resources/         ← Auto-loaded card data (for dynamic spawning)
    │
    └── Enemies/
        ├── Data/              ← EnemyData assets (stats, animations, deck)
        ├── AnimationSets/     ← EnemyAnimationSet assets (sprites & FPS)
        ├── Prefabs/           ← Enemy prefabs (BaseEnemy clone)
        ├── Aseprites/         ← Source .ase/.aseprite files
        └── Library/           ← EnemyLibrary asset (references all enemies)
```

---

## 🃏 **Creating a New Card**

Cards are defined using **CardData** assets and powered by **CardEffect** logic.

### 1. Create a New CardData Asset

1. Go to

   ```
   Assets/Content/Cards/Data/
   ```
2. Right-click →
   **Create → Cards → Card Data**
3. Fill out the fields:

   | Field                               | Description                               |
   | ----------------------------------- | ----------------------------------------- |
   | **Card Name**                       | Display name in UI                        |
   | **Description**                     | Tooltip and flavor text                   |
   | **Artwork**                         | Sprite displayed on the card              |
   | **Card Type**                       | Attack / Skill / Power (enum)             |
   | **Energy**                          | Cost to play the card                     |
   | **Intention Icon**                  | (Optional) Enemy preview icon             |
   | **Intention Text**                  | (Optional) Enemy preview text             |
   | **Min Multiplier / Max Multiplier** | Random effect variance (ex: 0.8–1.2)      |
   | **Effects**                         | Drag one or more `CardEffect` assets here |

---

### 2. Assign a Card Effect

1. In

   ```
   Assets/Content/Cards/Effects/
   ```

   you’ll find base effects like:

   * **DamageEffect**
   * **BlockEffect**
   * **HealEffect**

2. To create a new one:
   Right-click →
   **Create → Cards → Effects → Damage/Heal/Block**

3. Assign values such as:

   * **Base Damage / Heal / Block**
   * (Optional) add new custom logic by subclassing `CardEffect.cs`

---

### 3.  Assign Targeting

If your card has specific targeting behavior (like “Single Enemy” or “All Enemies”):

* Create a Targeting Rule under:

  ```
  Assets/Content/Cards/Targeting/
  ```
* In the card’s **CardData**, set the `targetingRule` field.

---

### 4. Test in Scene

Cards are dynamically spawned through `CardFactory` and `CardLibrary`.

* Verify your new card asset is inside a folder under `Resources/` (or registered in your `CardLibrary`).
* Run the game — the new card can now appear randomly or through manual deck assignment.

---

## 👾 **Creating a New Enemy**

Enemies are defined entirely through **EnemyData** and **EnemyAnimationSet** assets.
No code edits required.

---

### 1. Create an Animation Set

1. Go to

   ```
   Assets/Content/Enemies/AnimationSets/
   ```
2. Right-click →
   **Create → Enemies → Animation Set**
3. Fill in:

   | Field                                    | Description                      |
   | ---------------------------------------- | -------------------------------- |
   | **Idle / Float / Attack / Hurt / Death** | Sprite arrays or Aseprite frames |
   | **FPS per Animation**                    | Adjust speed per type            |
4. (Optional) Store your Aseprite sources in `/Aseprites/` for reference.

---

### 2. Create an Enemy Data Asset

1. Go to

   ```
   Assets/Content/Enemies/Data/
   ```
2. Right-click →
   **Create → Enemies → Enemy Data**
3. Fill out the fields:

   | Field                                  | Description                                           |
   | -------------------------------------- | ----------------------------------------------------- |
   | **Enemy Name**                         | Name displayed in logs or UI                          |
   | **Portrait**                           | Optional image for UI menus                           |
   | **Max Health / Base Damage / Defense** | Core stats                                            |
   | **Global Card Multiplier**             | Scales all card effects (ex: 1.5 for elites)          |
   | **Animation Set**                      | Reference from step 1                                 |
   | **Available Cards**                    | Drag existing `CardData` assets (the enemy’s AI deck) |
   | **Idle Chance**                        | (Optional) Behavior weighting for “do nothing” turns  |

---

### 3. Add to the Enemy Library

1. Navigate to

   ```
   Assets/Content/Enemies/Library/
   ```
2. Open `EnemyLibrary.asset` (if missing, create one via **Create → Enemies → Library**).
3. Add your new `EnemyData` to the **Available Enemies** list.

Now the enemy can spawn automatically in battles via `BattlefieldLayout`.

---

### 4. Create/Link the Prefab

1. Duplicate `BaseEnemy.prefab` under

   ```
   Assets/Content/Enemies/Prefabs/
   ```

   and rename it (e.g., `Eyeball.prefab`).

2. Ensure it contains:

   * `SpriteRenderer`
   * `Enemy` (script)
   * `EnemyAnimator2D`
   * `Collider2D`

3. In the `Enemy` component:

   * **Assign the HealthBar prefab**
   * **Leave Animation Set empty** — it’s assigned dynamically at runtime from `EnemyData`.

---

### 5. Testing

* Open your `BattleScene.unity`
* Select the **BattlefieldLayout** object.
* Ensure:

  * `enemyLibrary` is linked.
  * `enemyPrefab` is set to your **BaseEnemy.prefab**.
  * `playerPrefab` points to your player prefab.

When you play, the layout system spawns enemies from the library automatically.

---

## ⚙️ Design Philosophy

✅ **Data-Driven Design**
Cards and enemies are 100% modular — no need to edit scripts.
Everything comes from ScriptableObjects.

✅ **Shared Systems**

* Cards use `CardData` + `CardEffect`.
* Enemies use `EnemyData` + `EnemyAnimationSet`.
* Both plug into `BattleSystem`, `BattlefieldLayout`, and `CardRunner`.

✅ **Minimal Scene Setup**
The BattleScene hierarchy only needs:

```
BattleSystem
BattlefieldLayout
CardFactory
CardLibrary
EnemyLibrary
Player Prefab
```

✅ **Scaling System**

* Each card has its own **min/max random multiplier**.
* Enemies can apply a **global multiplier** to all cards they use.

---

## 🧱 Example Hierarchy (Battle Scene)

```
BattleScene
 ├── Main Camera
 ├── BattleSystem
 │    ├── HandView
 │    ├── EndTurnButton
 │    └── CardFactory
 ├── BattlefieldLayout
 │    ├── Player (instantiated)
 │    ├── Enemies (instantiated)
 └── Canvas
      └── TooltipUI
```

---

## 🧩 Quick Reference: Prefabs

| Prefab               | Purpose                                | Notes                                 |
| -------------------- | -------------------------------------- | ------------------------------------- |
| **BaseEnemy.prefab** | Generic spawn template for all enemies | Must have `Enemy` + `EnemyAnimator2D` |
| **Player.prefab**    | The player unit                        | Has `CharacterBase`                   |
| **Card.prefab**      | Card visuals                           | Used by `CardFactory`                 |
| **HealthBar.prefab** | World-space HP bar                     | Assigned to both Player and Enemies   |

---

## ✨ TL;DR – Adding New Content

| Task          | Steps                                                                          |
| ------------- | ------------------------------------------------------------------------------ |
| **New Card**  | Create `CardData` → Assign `CardEffect` → (Optional) Targeting → Test          |
| **New Enemy** | Create `EnemyAnimationSet` → Create `EnemyData` → Add to `EnemyLibrary` → Test |
