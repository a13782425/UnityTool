using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Logic.Editor
{
    [CustomEditor(typeof(LogicGraphBase), true)]
    public sealed class LogicGraph_Inspector : UnityEditor.Editor
    {
        public LogicGraphBase logicGraph;

        void OnEnable()
        {
            logicGraph = target as LogicGraphBase;
        }
        public override void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("打开"))
            {
                string str = AssetDatabase.GetAssetPath(logicGraph);
                var logicData = LogicEditorConfig.ConfigData.GetGraphSummary(a => a.AssetPath == str);
                LogicPanel.ShowLogic(logicData);
            }
            GUILayout.EndHorizontal();
            UnityEditor.EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            UnityEditor.EditorGUI.EndDisabledGroup();
        }
    }
    [CustomEditor(typeof(LogicNodeBase), true)]
    public sealed class LogicNodeBase_Inspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            UnityEditor.EditorGUI.EndDisabledGroup();
        }
    }

    [CustomEditor(typeof(LogicGraphData), true)]
    public sealed class LogicGraphData_Inspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            UnityEditor.EditorGUI.EndDisabledGroup();
        }
    }

    [CustomEditor(typeof(LogicEditorConfigData))]
    public sealed class LogicEditorConfigData_Inspector: UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            UnityEditor.EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            UnityEditor.EditorGUI.EndDisabledGroup();
        }
    }
}
