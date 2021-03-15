using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Logic.Editor
{
    public class LogicPanel : EditorWindow
    {
        [MenuItem("Framework/逻辑图/打开逻辑图")]
        public static void ShowLogic()
        {
            ShowPanel(null);
        }

        public static void ShowLogic(LogicEditorConfigData.LogicGraphSummary graphData)
        {
            ShowPanel(graphData);
        }

        static LogicPanel ShowPanel(LogicEditorConfigData.LogicGraphSummary graphData)
        {
            Object[] panels = Resources.FindObjectsOfTypeAll(typeof(LogicPanel));
            LogicPanel panel = null;
            foreach (var item in panels)
            {
                LogicPanel p = item as LogicPanel;
                if (p != null)
                {
                    if (p._graphData == graphData)
                    {
                        panel = p;
                        break;
                    }
                }
            }
            if (panel == null)
            {

                panel = CreateWindow<LogicPanel>();
                SetLogicGraph(panel, graphData);
            }
            panel.Show();
            panel.Focus();
            return panel;
        }

        private LogicEditorConfigData.LogicGraphSummary _graphData = default;

        /// <summary>
        /// 界面数据
        /// </summary>
        private LogicPanelData _panelData;

        public static void SetLogicGraph(LogicPanel panel, LogicEditorConfigData.LogicGraphSummary graphSummary)
        {
            panel._panelData.LogicGraphSummary = graphSummary;
            if (graphSummary != null)
            {
                panel.titleContent = new GUIContent(graphSummary.LogicName);
                panel._panelData.LogicGraph = AssetDatabase.LoadAssetAtPath<LogicGraphBase>(panel._panelData.LogicGraphSummary.AssetPath);
                panel._panelData.LogicPanelView.ShowLogicGraph();
            }
        }
        private void OnEnable()
        {
            this._panelData = new LogicPanelData();
            this._panelData.LogicPanel = this;
            titleContent = new GUIContent("逻辑图");
            ShowGui();
        }
        /// <summary>
        /// 当rootVisualElement准备完成后调用
        /// </summary>
        private void ShowGui()
        {
            VisualElement root = rootVisualElement;
            this._panelData.LogicPanelView = new LogicPanelView(this._panelData);
            this.rootVisualElement.Add(this._panelData.LogicPanelView);
        }


        private void OnDestroy()
        {
            this._panelData.Save();
        }
    }

}
