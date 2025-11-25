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
        private string _newWaveName = "New Wave";

        private List<EnemyConfig> _newGuaranteedEnemies = new();
        private List<EnemyConfig> _newRandomPool = new();
        private int _newMinEnemies = 1;
        private int _newMaxEnemies = 3;
        private float _newStatIncrease = 0f;
        private string _newMessage = "";
        private float _newDelay = 0f;

        private Vector2 _scrollPos;

        [MenuItem("Tools/Battle/Wave Config Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<WaveConfigCreator>("Wave Config Creator");
            window.minSize = new Vector2(500, 550);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Wave Configuration Tool", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Select WaveConfig asset
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

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Current Waves", EditorStyles.boldLabel);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            if (_targetConfig.waves != null && _targetConfig.waves.Count > 0)
            {
                for (int i = 0; i < _targetConfig.waves.Count; i++)
                {
                    DrawWaveInfo(i);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No waves configured yet.", MessageType.Warning);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();
            DrawAddWaveSection();
        }

        private void DrawWaveInfo(int index)
        {
            var wave = _targetConfig.waves[index];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUI.indentLevel++;

            // Wave header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Wave {index + 1}: {wave.waveName}", EditorStyles.boldLabel);

            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                RemoveWave(index);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // Wave identity
            wave.waveName = EditorGUILayout.TextField("Name", wave.waveName);
            wave.waveMessage = EditorGUILayout.TextField("Wave Message", wave.waveMessage);
            wave.delayBeforeWave = EditorGUILayout.FloatField("Delay Before Wave", wave.delayBeforeWave);

            EditorGUILayout.Space(5);

            // Guaranteed enemies
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

            EditorGUILayout.Space(8);

            // Random enemy pool
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

            if (GUILayout.Button("+ Add Random Pool Enemy"))
                wave.randomEnemyPool.Add(null);

            EditorGUILayout.Space(8);

            // Random settings
            wave.minEnemiesPerWave = EditorGUILayout.IntField("Min Random Enemies", wave.minEnemiesPerWave);
            wave.maxEnemiesPerWave = EditorGUILayout.IntField("Max Random Enemies", wave.maxEnemiesPerWave);

            if (wave.maxEnemiesPerWave < wave.minEnemiesPerWave)
                wave.maxEnemiesPerWave = wave.minEnemiesPerWave;

            EditorGUILayout.Space(8);

            // Scaling
            wave.statMultiplierIncrease = EditorGUILayout.FloatField("Stat Multiplier Increase", wave.statMultiplierIncrease);

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);

            EditorUtility.SetDirty(_targetConfig);
        }

        private void DrawAddWaveSection()
        {
            EditorGUILayout.LabelField("Add New Wave", EditorStyles.boldLabel);

            _newWaveName = EditorGUILayout.TextField("Wave Name", _newWaveName);
            _newMessage = EditorGUILayout.TextField("Wave Message", _newMessage);
            _newDelay = EditorGUILayout.FloatField("Delay Before Wave", _newDelay);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Guaranteed Enemies", EditorStyles.boldLabel);

            for (int i = 0; i < _newGuaranteedEnemies.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _newGuaranteedEnemies[i] = (EnemyConfig)EditorGUILayout.ObjectField(_newGuaranteedEnemies[i], typeof(EnemyConfig), false);

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    _newGuaranteedEnemies.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+ Add Guaranteed Enemy"))
                _newGuaranteedEnemies.Add(null);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Random Enemy Pool", EditorStyles.boldLabel);

            for (int i = 0; i < _newRandomPool.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _newRandomPool[i] = (EnemyConfig)EditorGUILayout.ObjectField(_newRandomPool[i], typeof(EnemyConfig), false);

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    _newRandomPool.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+ Add Random Pool Enemy"))
                _newRandomPool.Add(null);

            EditorGUILayout.Space(5);

            _newMinEnemies = EditorGUILayout.IntField("Min Random Enemies", _newMinEnemies);
            _newMaxEnemies = EditorGUILayout.IntField("Max Random Enemies", _newMaxEnemies);

            if (_newMaxEnemies < _newMinEnemies)
                _newMaxEnemies = _newMinEnemies;

            EditorGUILayout.Space(5);
            _newStatIncrease = EditorGUILayout.FloatField("Stat Multiplier Increase", _newStatIncrease);

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Add Wave To Config", GUILayout.Height(30)))
                AddWave();
        }

        private void AddWave()
        {
            if (_targetConfig == null) return;

            var wave = new WaveData
            {
                waveName = _newWaveName,
                waveMessage = _newMessage,
                delayBeforeWave = _newDelay,
                guaranteedEnemies = new List<EnemyConfig>(_newGuaranteedEnemies),
                randomEnemyPool = new List<EnemyConfig>(_newRandomPool),
                minEnemiesPerWave = _newMinEnemies,
                maxEnemiesPerWave = _newMaxEnemies,
                statMultiplierIncrease = _newStatIncrease
            };

            Undo.RecordObject(_targetConfig, "Add Wave");
            _targetConfig.waves.Add(wave);
            EditorUtility.SetDirty(_targetConfig);

            // Reset
            _newWaveName = $"Wave {_targetConfig.waves.Count + 1}";
            _newGuaranteedEnemies.Clear();
            _newRandomPool.Clear();
            _newMessage = "";
            _newDelay = 0f;
            _newStatIncrease = 0f;
        }

        private void RemoveWave(int index)
        {
            Undo.RecordObject(_targetConfig, "Remove Wave");
            _targetConfig.waves.RemoveAt(index);
            EditorUtility.SetDirty(_targetConfig);
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
                config.waves = new List<WaveData>();

                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();

                _targetConfig = config;
                EditorGUIUtility.PingObject(config);
            }
        }
    }
}
#endif
