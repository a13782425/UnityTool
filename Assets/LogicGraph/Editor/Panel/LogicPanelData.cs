using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Logic.Editor.LogicEditorConfig;
using static Logic.Editor.LogicEditorConfigData;
namespace Logic.Editor
{
    public class LogicPanelData
    {
        /// <summary>
        /// 逻辑图窗口
        /// </summary>
        public LogicPanel LogicPanel = default;
        /// <summary>
        /// 当前逻辑图概要
        /// </summary>
        public LogicGraphSummary LogicGraphSummary = default;

        /// <summary>
        /// 当前逻辑图对象
        /// </summary>
        public LogicGraphBase LogicGraph = default;
        /// <summary>
        /// 当前逻辑图的一些信息
        /// </summary>
        public LogicGraphData LogicGraphData => LogicGraph == null ? null : LogicGraph.LogicGraphData;


        /// <summary>
        /// 窗口里的界面
        /// </summary>
        public LogicPanelView LogicPanelView = default;
        /// <summary>
        /// 窗口里的界面
        /// </summary>
        public LogicPanelGraphView LogicPanelGraphView = default;

        /// <summary>
        /// 当组模板被删除时
        /// </summary>
        public Action onGroupConfigDelete;

        private LogicGraphConfig _logicGraphConfig = default;
        /// <summary>
        /// 当前类型得逻辑图配置
        /// </summary>
        public LogicGraphConfig LogicGraphConfig
        {
            get
            {
                if (LogicGraphSummary == null)
                {
                    return _logicGraphConfig;
                }
                if (_logicGraphConfig == null)
                {
                    _logicGraphConfig = ConfigData.LogicGraphConifgs.FirstOrDefault(a => a.GraphClassName == LogicGraphSummary.GraphClassName);
                }
                return _logicGraphConfig;
            }
        }


        internal void Save()
        {
            if (LogicGraph != null)
            {
                EditorUtility.SetDirty(LogicGraph);
                EditorUtility.SetDirty(ConfigData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }
}
