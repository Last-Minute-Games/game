#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GameItems;
using System.Collections.Generic;

namespace GameItems.Editor
{
    public class WaveConfigCreator : EditorWindow
    {
        private WaveConfig _targetConfig;

        private Vector2 _scrollPos;

        // Temporary data used for creating new waves
        private WaveData _tempWave = new WaveData();

        [MenuItem("Tools/Battle/Wave Config Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<WaveConfigCreator>("Wave Config Creator");
            window.minSize = new Vector2(550, 600);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Day-Based Wave Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _targetConfig = (WaveConfig)EditorGUILayout.ObjectField(
                "Wave Config",
                _targetConfig,
                typeof(WaveConfig),
                false
            );

            if (_targetConfig == null)
            {
                EditorGUILayout.HelpBox(
                    "Create or select a WaveConfig asset to begin.\n\n" +
                    "Right-click in Project → Create → Battle → Wave Configuration",
                    MessageType.Info
                );

                if (GUILayout.Button("Create New WaveConfig"))
                {
                    CreateNewWaveConfig();
                }
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawDaySection("DAY 1 WAVES", _targetConfig.day1Waves);
            DrawDaySection("DAY 2 WAVES", _targetConfig.day2Waves);
            DrawDaySection("DAY 3 WAVES", _targetConfig.day3Waves);
            DrawDaySection("DAY 4 WAVES", _targetConfig.day4Waves);
            DrawDaySection("DAY 5 WAVES", _targetConfig.day5Waves);

            EditorGUILayout.EndScrollView();

            EditorGUIUtility.labelWidth = 110;
            EditorGUILayout.Space(10);
            DrawAddWaveSection();
        }

        // ---------------------------
        // DRAW DAY WAVE LIST
        // ---------------------------
        private void DrawDaySection(string label, List<WaveData> dayList)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            if (dayList == null || dayList.Count == 0)
            {
                EditorGUILayout.HelpBox("No waves for this day yet.", MessageType.Info);
            }
            else
            {
                for (int i = 0; i < dayList.Count; i++)
                {
                    DrawWaveEditor(dayList, i);
                }
            }
        }

        // ---------------------------
        // DRAW WAVE EDITOR
        // ---------------------------
        private void DrawWaveEditor(List<WaveData> list, int index)
        {
            var wave = list[index];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Wave {index + 1}: {wave.waveName}", EditorStyles.boldLabel);
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                Undo.RecordObject(_targetConfig, "Remove Wave");
                list.RemoveAt(index);
                EditorUtility.SetDirty(_targetConfig);
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            wave.waveName = EditorGUILayout.TextField("Name", wave.waveName);
            wave.waveMessage = EditorGUILayout.TextField("Message", wave.waveMessage);
            wave.delayBeforeWave = EditorGUILayout.FloatField("Delay Before Wave", wave.delayBeforeWave);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Guaranteed Enemies", EditorStyles.boldLabel);

            for (int i = 0; i < wave.guaranteedEnemies.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                wave.guaranteedEnemies[i] = (EnemyConfig)EditorGUILayout.ObjectField(wave.guaranteedEnemies[i], typeof(EnemyConfig), false);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    wave.guaranteedEnemies.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Guaranteed Enemy"))
                wave.guaranteedEnemies.Add(null);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Random Enemy Pool", EditorStyles.boldLabel);

            for (int i = 0; i < wave.randomEnemyPool.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                wave.randomEnemyPool[i] = (EnemyConfig)EditorGUILayout.ObjectField(wave.randomEnemyPool[i], typeof(EnemyConfig), false);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    wave.randomEnemyPool.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Random Enemy"))
                wave.randomEnemyPool.Add(null);

            EditorGUILayout.Space(6);
            wave.minEnemiesPerWave = EditorGUILayout.IntField("Min Random", wave.minEnemiesPerWave);
            wave.maxEnemiesPerWave = EditorGUILayout.IntField("Max Random", wave.maxEnemiesPerWave);

            if (wave.maxEnemiesPerWave < wave.minEnemiesPerWave)
                wave.maxEnemiesPerWave = wave.minEnemiesPerWave;

            wave.statMultiplierIncrease = EditorGUILayout.FloatField("Stat Increase", wave.statMultiplierIncrease);

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorUtility.SetDirty(_targetConfig);
        }

        // ---------------------------
        // ADD NEW WAVE SECTION
        // ---------------------------
        private void DrawAddWaveSection()
        {
            EditorGUILayout.LabelField("Create New Wave", EditorStyles.boldLabel);

            _tempWave.waveName = EditorGUILayout.TextField("Wave Name", _tempWave.waveName);
            _tempWave.waveMessage = EditorGUILayout.TextField("Message", _tempWave.waveMessage);
            _tempWave.delayBeforeWave = EditorGUILayout.FloatField("Delay Before Wave", _tempWave.delayBeforeWave);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Guaranteed Enemies", EditorStyles.boldLabel);

            if (_tempWave.guaranteedEnemies == null)
                _tempWave.guaranteedEnemies = new List<EnemyConfig>();

            for (int i = 0; i < _tempWave.guaranteedEnemies.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _tempWave.guaranteedEnemies[i] = (EnemyConfig)EditorGUILayout.ObjectField(_tempWave.guaranteedEnemies[i], typeof(EnemyConfig), false);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    _tempWave.guaranteedEnemies.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Guaranteed Enemy"))
                _tempWave.guaranteedEnemies.Add(null);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Random Pool", EditorStyles.boldLabel);

            if (_tempWave.randomEnemyPool == null)
                _tempWave.randomEnemyPool = new List<EnemyConfig>();

            for (int i = 0; i < _tempWave.randomEnemyPool.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _tempWave.randomEnemyPool[i] = (EnemyConfig)EditorGUILayout.ObjectField(_tempWave.randomEnemyPool[i], typeof(EnemyConfig), false);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    _tempWave.randomEnemyPool.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Random Enemy"))
                _tempWave.randomEnemyPool.Add(null);

            _tempWave.minEnemiesPerWave = EditorGUILayout.IntField("Min Random", _tempWave.minEnemiesPerWave);
            _tempWave.maxEnemiesPerWave = EditorGUILayout.IntField("Max Random", _tempWave.maxEnemiesPerWave);

            if (_tempWave.maxEnemiesPerWave < _tempWave.minEnemiesPerWave)
                _tempWave.maxEnemiesPerWave = _tempWave.minEnemiesPerWave;

            _tempWave.statMultiplierIncrease = EditorGUILayout.FloatField("Stat Increase", _tempWave.statMultiplierIncrease);

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Add Wave To:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Day 1")) AddWaveToDay(_targetConfig.day1Waves);
            if (GUILayout.Button("Day 2")) AddWaveToDay(_targetConfig.day2Waves);
            if (GUILayout.Button("Day 3")) AddWaveToDay(_targetConfig.day3Waves);
            if (GUILayout.Button("Day 4")) AddWaveToDay(_targetConfig.day4Waves);
            if (GUILayout.Button("Day 5")) AddWaveToDay(_targetConfig.day5Waves);

            EditorGUILayout.EndHorizontal();
        }

        private void AddWaveToDay(List<WaveData> list)
        {
            Undo.RecordObject(_targetConfig, "Add Wave");

            list.Add(CloneWave(_tempWave));
            EditorUtility.SetDirty(_targetConfig);

            _tempWave = new WaveData(); // reset
        }

        private WaveData CloneWave(WaveData original)
        {
            return new WaveData
            {
                waveName = original.waveName,
                waveMessage = original.waveMessage,
                delayBeforeWave = original.delayBeforeWave,
                guaranteedEnemies = new List<EnemyConfig>(original.guaranteedEnemies),
                randomEnemyPool = new List<EnemyConfig>(original.randomEnemyPool),
                minEnemiesPerWave = original.minEnemiesPerWave,
                maxEnemiesPerWave = original.maxEnemiesPerWave,
                statMultiplierIncrease = original.statMultiplierIncrease
            };
        }

        private void CreateNewWaveConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create WaveConfig",
                "NewWaveConfig",
                "asset",
                "Choose a location to save the WaveConfig"
            );

            if (!string.IsNullOrEmpty(path))
            {
                var config = CreateInstance<WaveConfig>();
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();

                _targetConfig = config;
                EditorGUIUtility.PingObject(config);
            }
        }
    }
}
#endif
