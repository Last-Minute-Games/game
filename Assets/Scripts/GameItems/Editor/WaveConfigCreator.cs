using UnityEngine;
using UnityEditor;
using GameItems;
using System.Collections.Generic;

#if UNITY_EDITOR
namespace GameItems.Editor
{
    /// <summary>
    /// Custom editor window for quickly creating and managing WaveConfigs
    /// </summary>
    public class WaveConfigCreator : EditorWindow
    {
        private WaveConfig _targetConfig;
        private string _newWaveName = "New Wave";
        private List<EnemyConfig> _selectedEnemies = new();
        private Vector2 _scrollPos;

        [MenuItem("Tools/Battle/Wave Config Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<WaveConfigCreator>("Wave Config Creator");
            window.minSize = new Vector2(400, 500);
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
            EditorGUILayout.LabelField("Current Configuration", EditorStyles.boldLabel);
            
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            
            // Display current waves
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
            EditorGUILayout.LabelField("Add New Wave", EditorStyles.boldLabel);
            
            _newWaveName = EditorGUILayout.TextField("Wave Name", _newWaveName);
            
            // Add enemies to new wave
            EditorGUILayout.LabelField("Enemies:");
            for (int i = 0; i < _selectedEnemies.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _selectedEnemies[i] = (EnemyConfig)EditorGUILayout.ObjectField(
                    _selectedEnemies[i], 
                    typeof(EnemyConfig), 
                    false
                );
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    _selectedEnemies.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("+ Add Enemy Slot"))
            {
                _selectedEnemies.Add(null);
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Add Wave to Config", GUILayout.Height(30)))
            {
                AddWave();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Clear All Waves"))
            {
                if (EditorUtility.DisplayDialog(
                    "Clear All Waves", 
                    "Are you sure you want to remove all waves?", 
                    "Yes", 
                    "Cancel"))
                {
                    ClearAllWaves();
                }
            }
        }

        private void DrawWaveInfo(int index)
        {
            var wave = _targetConfig.waves[index];
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField($"Wave {index + 1}: {wave.waveName}", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                RemoveWave(index);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"Enemies: {wave.enemies.Count}");
            foreach (var enemy in wave.enemies)
            {
                if (enemy != null)
                {
                    EditorGUILayout.LabelField($"  • {enemy.enemyName}", EditorStyles.miniLabel);
                }
            }
            
            if (!string.IsNullOrEmpty(wave.waveMessage))
            {
                EditorGUILayout.LabelField($"Message: \"{wave.waveMessage}\"", EditorStyles.miniLabel);
            }
            
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }

        private void AddWave()
        {
            if (_targetConfig == null) return;
            
            // Filter out null enemies
            var validEnemies = new List<EnemyConfig>();
            foreach (var enemy in _selectedEnemies)
            {
                if (enemy != null)
                {
                    validEnemies.Add(enemy);
                }
            }
            
            if (validEnemies.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Enemies", 
                    "Please add at least one enemy to the wave.", 
                    "OK"
                );
                return;
            }
            
            var newWave = new WaveData
            {
                waveName = _newWaveName,
                enemies = new List<EnemyConfig>(validEnemies),
                waveMessage = $"{_newWaveName} begins!"
            };
            
            Undo.RecordObject(_targetConfig, "Add Wave");
            _targetConfig.waves.Add(newWave);
            EditorUtility.SetDirty(_targetConfig);
            
            // Reset
            _selectedEnemies.Clear();
            _newWaveName = $"Wave {_targetConfig.waves.Count + 1}";
            
            Debug.Log($"Added wave '{newWave.waveName}' with {validEnemies.Count} enemies");
        }

        private void RemoveWave(int index)
        {
            if (_targetConfig == null) return;
            
            Undo.RecordObject(_targetConfig, "Remove Wave");
            _targetConfig.waves.RemoveAt(index);
            EditorUtility.SetDirty(_targetConfig);
        }

        private void ClearAllWaves()
        {
            if (_targetConfig == null) return;
            
            Undo.RecordObject(_targetConfig, "Clear All Waves");
            _targetConfig.waves.Clear();
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
                var newConfig = CreateInstance<WaveConfig>();
                newConfig.waves = new List<WaveData>();
                
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                _targetConfig = newConfig;
                EditorGUIUtility.PingObject(newConfig);
                
                Debug.Log($"Created WaveConfig at {path}");
            }
        }
    }
}
#endif

