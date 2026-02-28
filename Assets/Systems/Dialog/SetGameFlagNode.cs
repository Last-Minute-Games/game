using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace cherrydev
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Node Graph/Nodes/Set GameFlag Node", 
        fileName = "New Set GameFlag Node")]
    public class SetGameFlagNode : Node
    {
        [SerializeField] private string _flagName;
        [SerializeField] private bool _removeFlag = false;

        public List<Node> ParentNodes = new();
        public Node ChildNode;

        public string FlagName => _flagName;
        public bool RemoveFlag => _removeFlag;

        private const float NodeWidth = 170f;
        private const float NodeHeight = 170f;
        private const float TextAreaWidth = 150f;

        /// <summary>
        /// Executes the flag setting/removal
        /// </summary>
        public void ExecuteFlag()
        {
            if (string.IsNullOrEmpty(_flagName))
            {
                Debug.LogWarning("SetGameFlag has empty flag name");
                return;
            }

            if (_removeFlag)
            {
                GameFlags.RemoveFlag(_flagName);
                Debug.Log($"[SetGameFlag] Removed flag: {_flagName}");
            }
            else
            {
                GameFlags.SetFlag(_flagName);
                Debug.Log($"[SetGameFlag] Set flag: {_flagName}");
            }
        }

#if UNITY_EDITOR
        public override void Draw(GUIStyle nodeStyle, GUIStyle labelStyle)
        {
            base.Draw(nodeStyle, labelStyle);

            ParentNodes.RemoveAll(item => item == null);
            
            CalculateNodeHeight();
            Rect.width = NodeWidth;

            GUILayout.BeginArea(Rect, nodeStyle);
            
            EditorGUILayout.LabelField("Set GameFlag", labelStyle);
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("Flag Name:");
            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
            _flagName = EditorGUILayout.TextArea(_flagName, textAreaStyle, GUILayout.Width(TextAreaWidth), GUILayout.Height(45));
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Remove:", GUILayout.Width(70));
            _removeFlag = EditorGUILayout.Toggle(_removeFlag, GUILayout.Width(110));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            string action = _removeFlag ? "REMOVE" : "SET";
            EditorGUILayout.LabelField($"Action: {action}", EditorStyles.helpBox);

            GUILayout.EndArea();
        }

        /// <summary>
        /// Calculate node height (currently fixed, but following SentenceNode pattern)
        /// </summary>
        public void CalculateNodeHeight()
        {
            Rect.height = NodeHeight;
        }

        public override void RemoveAllConnections()
        {
            ParentNodes.Clear();
            ChildNode = null;
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
                || nodeToAdd.GetType() == typeof(SetGameFlagNode)
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

            if (ChildNode != null && ChildNode != nodeToAdd)
                ChildNode.RemoveFromParentConnectedNode(this);

            ChildNode = nodeToAdd;
            return true;
        }

        public override void RemoveChildConnection(Node childToRemove)
        {
            if (ChildNode == childToRemove)
            {
                ChildNode = null;
                Debug.Log("Removed child connection");
            }
        }
#endif
    }
}
