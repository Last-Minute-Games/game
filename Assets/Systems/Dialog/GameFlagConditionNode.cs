using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Node Graph/Nodes/GameFlag Condition Node", 
        fileName = "New GameFlag Condition Node")]
    public class GameFlagConditionNode : Node
    {
        [SerializeField] private string _flagName;

        public List<Node> ParentNodes = new();
        public Node TrueChildNode;
        public Node FalseChildNode;

        public string FlagName => _flagName;

        private const float NodeWidth = 200f;
        private const float NodeHeight = 120f;

        /// <summary>
        /// Evaluates if the GameFlag exists
        /// </summary>
        /// <returns>True if flag exists, false otherwise</returns>
        public bool EvaluateCondition()
        {
            if (string.IsNullOrEmpty(_flagName))
            {
                Debug.LogWarning("GameFlag condition has empty flag name");
                return false;
            }

            bool flagExists = GameFlags.HasFlag(_flagName);
            
            Debug.Log($"[GameFlagCondition] Flag '{_flagName}' exists: {flagExists}");
            
            return flagExists;
        }

#if UNITY_EDITOR
        public override void Draw(GUIStyle nodeStyle, GUIStyle labelStyle)
        {
            base.Draw(nodeStyle, labelStyle);

            ParentNodes.RemoveAll(item => item == null);
            
            Rect.size = new Vector2(NodeWidth, NodeHeight);

            GUILayout.BeginArea(Rect, nodeStyle);
            
            EditorGUILayout.LabelField("GameFlag Condition", labelStyle);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Flag Name:", GUILayout.Width(70));
            _flagName = EditorGUILayout.TextField(_flagName, GUILayout.Width(110));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("Connections:", EditorStyles.boldLabel);
            
            // Show connection status with proper symbols
            string trueStatus = TrueChildNode != null ? "?" : "?";
            string falseStatus = FalseChildNode != null ? "?" : "?";
            
            EditorGUILayout.LabelField($"True: {trueStatus}", GUILayout.Width(180));
            EditorGUILayout.LabelField($"False: {falseStatus}", GUILayout.Width(180));

            GUILayout.EndArea();
        }

        public override void RemoveAllConnections()
        {
            ParentNodes.Clear();
            TrueChildNode = null;
            FalseChildNode = null;
        }

        public override bool AddToParentConnectedNode(Node nodeToAdd)
        {
            if (nodeToAdd == this)
                return false;

            if (ParentNodes.Contains(nodeToAdd))
                return false;

            if (nodeToAdd.GetType() == typeof(SentenceNode) 
                || nodeToAdd.GetType() == typeof(ModifyVariableNode)
                || nodeToAdd.GetType() == typeof(VariableConditionNode)
                || nodeToAdd.GetType() == typeof(GameFlagConditionNode)
                || nodeToAdd.GetType().Name == "SetGameFlagNode"
                || nodeToAdd.GetType() == typeof(ExternalFunctionNode))
            {
                ParentNodes.Add(nodeToAdd);
                return true;
            }

            return false;
        }

        public override bool RemoveFromParentConnectedNode(Node nodeToRemove) => 
            ParentNodes.Remove(nodeToRemove);

        public override bool AddToChildConnectedNode(Node nodeToAdd)
        {
            if (nodeToAdd == this)
                return false;

            if (nodeToAdd.GetType() == typeof(AnswerNode))
                return false;

            // If both connections are filled, can't add more
            if (TrueChildNode != null && FalseChildNode != null)
            {
                Debug.LogWarning("Both TRUE and FALSE paths are already connected");
                return false;
            }

            // Add to first available slot (TRUE first, then FALSE)
            if (TrueChildNode == null)
            {
                TrueChildNode = nodeToAdd;
                Debug.Log("Connected to TRUE path");
                return true;
            }
            else if (FalseChildNode == null)
            {
                FalseChildNode = nodeToAdd;
                Debug.Log("Connected to FALSE path");
                return true;
            }

            return false;
        }

        public override void RemoveChildConnection(Node childToRemove)
        {
            if (TrueChildNode == childToRemove)
            {
                TrueChildNode = null;
                Debug.Log("Removed TRUE path connection");
            }
            else if (FalseChildNode == childToRemove)
            {
                FalseChildNode = null;
                Debug.Log("Removed FALSE path connection");
            }
        }
#endif
    }
}
