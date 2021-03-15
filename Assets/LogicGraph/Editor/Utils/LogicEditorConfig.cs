using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using static Logic.Editor.LogicEditorConfigData;
using Object = UnityEngine.Object;

namespace Logic.Editor
{
    public static class LogicEditorConfig
    {
        public const string DEFAULT_GROUP_NAME = "DefaultGroup";
        private static readonly string CONFIG_FILE_PATH = default;
        private static LogicEditorConfigData _logicEditorConfigData = null;
        public static LogicEditorConfigData ConfigData => _logicEditorConfigData;

        static LogicEditorConfig()
        {

            string[] grids = AssetDatabase.FindAssets("LogicEditorConfig");
            if (grids.Length < 1)
            {
                throw new System.Exception("没有找到LogicEditorConfig文件所在地");
            }
            string path = AssetDatabase.GUIDToAssetPath(grids[0]);
            CONFIG_FILE_PATH = Path.Combine(Path.GetDirectoryName(path), "LogicEditorConfigData.asset");
            _logicEditorConfigData = AssetDatabase.LoadAssetAtPath<LogicEditorConfigData>(CONFIG_FILE_PATH);
            if (_logicEditorConfigData == null)
            {
                _logicEditorConfigData = ScriptableObject.CreateInstance<LogicEditorConfigData>();
                AssetDatabase.CreateAsset(_logicEditorConfigData, CONFIG_FILE_PATH);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 当数据变化的时候刷新数据
        /// </summary>
        public static void Refresh()
        {
            List<Type> types = new List<Type>();
            types.AddRange(typeof(LogicNodeBaseView).Assembly.GetTypes());
            types.AddRange(typeof(LogicNodeBase).Assembly.GetTypes());
            CheckTypes(types);
            RefreshGroup();
            EditorUtility.SetDirty(ConfigData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 刷新组
        /// </summary>
        private static void RefreshGroup()
        {
            Type defaultGroupType = typeof(LogicGroupView);
            string groupFullName = defaultGroupType.FullName;
            foreach (var item in ConfigData.LogicGraphConifgs)
            {
                LogicGroupConfig defaultGroup = item.LogicGroups.FirstOrDefault(a => a.GroupName == DEFAULT_GROUP_NAME);
                if (defaultGroup == null)
                {
                    defaultGroup = new LogicGroupConfig();
                    defaultGroup.GroupName = DEFAULT_GROUP_NAME;
                    defaultGroup.TempId = Guid.NewGuid().ToString();
                    defaultGroup.UseCount = int.MinValue;
                    item.LogicGroups.Add(defaultGroup);
                }
                foreach (var group in item.LogicGroups)
                {
                    group.NodeConfigs.RemoveAll(a =>
                    {
                        return !item.LogicNodes.Any(b => b.NodeClassName == a.NodeClassName);
                    });
                }
            }
        }

        /// <summary>
        /// 添加一个逻辑图
        /// </summary>
        /// <param name="graphPath"></param>
        public static void AddLogicGraph(string graphPath)
        {
            if (ConfigData.LogicGraphs.FirstOrDefault(a => a.AssetPath == graphPath) != null)
            {
                return;
            }
            LogicGraphBase logicGraph = AssetDatabase.LoadAssetAtPath<LogicGraphBase>(graphPath);
            if (logicGraph == null)
                return;

            string fileName = Path.GetFileNameWithoutExtension(graphPath);
            string logicTypeName = logicGraph.GetType().FullName;
            LogicGraphSummary graphSummary = new LogicGraphSummary();
            graphSummary.LogicName = logicGraph.LogicGraphData.Title;
            graphSummary.GraphClassName = logicTypeName;
            graphSummary.AssetPath = graphPath;
            graphSummary.FileName = Path.GetFileNameWithoutExtension(graphPath);
            ConfigData.LogicGraphs.Add(graphSummary);
            logicGraph = null;
            EditorUtility.SetDirty(ConfigData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        /// <summary>
        /// 删除一个逻辑图
        /// </summary>
        /// <param name="graphPath"></param>
        public static void RemoveLogicGraph(string graphPath)
        {
            ConfigData.LogicGraphs.RemoveAll(a => a.AssetPath == graphPath);
            if (ConfigData != null)
            {
                EditorUtility.SetDirty(ConfigData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private static void CheckTypes(List<Type> types)
        {
            foreach (var item in ConfigData.LogicGraphConifgs)
            {
                item.IsRefresh = false;
                foreach (var node in item.LogicNodes)
                {
                    node.IsRefresh = false;
                }
                item.LogicFormats.Clear();
            }
            RefreshLogicGraph(types);
            RefreshLogicNode(types);
            RefreshFormat(types);

            ConfigData.LogicGraphConifgs.RemoveAll(a => !a.IsRefresh);
            foreach (var item in ConfigData.LogicGraphConifgs)
            {
                item.LogicNodes.RemoveAll(a => !a.IsRefresh);
                item.LogicNodes.Sort((entry1, entry2) =>
                {
                    for (var i = 0; i < entry1.NodeLayers.Length; i++)
                    {
                        if (i >= entry2.NodeLayers.Length)
                            return 1;
                        var value = entry1.NodeLayers[i].CompareTo(entry2.NodeLayers[i]);
                        if (value != 0)
                        {
                            // Make sure that leaves go before nodes
                            if (entry1.NodeLayers.Length != entry2.NodeLayers.Length && (i == entry1.NodeLayers.Length - 1 || i == entry2.NodeLayers.Length - 1))
                                return entry1.NodeLayers.Length < entry2.NodeLayers.Length ? -1 : 1;
                            return value;
                        }
                    }
                    return 0;
                });

            }
        }

        /// <summary>
        /// 刷新逻辑图
        /// </summary>
        /// <param name="types"></param>
        private static void RefreshLogicGraph(List<Type> types)
        {
            //逻辑图类型
            Type _logicGraphType = typeof(ILogicGraph);
            Type _logicGraphAttr = typeof(LogicGraphAttribute);
            //循环查询逻辑图
            foreach (var item in types)
            {
                if (!item.IsAbstract && !item.IsInterface)
                {
                    if (_logicGraphType.IsAssignableFrom(item))
                    {
                        //如果当前类型是逻辑图
                        object[] graphAttrs = item.GetCustomAttributes(_logicGraphAttr, false);
                        if (graphAttrs != null && graphAttrs.Length > 0)
                        {
                            LogicGraphAttribute logicGraph = graphAttrs[0] as LogicGraphAttribute;
                            LogicEditorConfigData.LogicGraphConfig graphData = ConfigData.LogicGraphConifgs.FirstOrDefault(a => a.GraphClassName == item.FullName);
                            if (graphData == null)
                            {
                                graphData = new LogicEditorConfigData.LogicGraphConfig();
                                graphData.GraphName = logicGraph.LogicName;
                                graphData.GraphClassName = item.FullName;
                                ConfigData.LogicGraphConifgs.Add(graphData);
                            }
                            graphData.IsRefresh = true;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 刷新逻辑图节点
        /// </summary>
        /// <param name="types"></param>
        private static void RefreshLogicNode(List<Type> types)
        {
            //逻辑图节点类型
            Type _logicNodeViewType = typeof(ILogicNodeView);
            Type _logicNodeAttr = typeof(LogicNodeAttribute);
            //循环查询逻辑图节点视图
            foreach (var item in types)
            {
                if (!item.IsAbstract && !item.IsInterface)
                {
                    if (_logicNodeViewType.IsAssignableFrom(item))
                    {
                        //如果当前类型是逻辑图节点
                        object[] nodeAttrs = item.GetCustomAttributes(_logicNodeAttr, false);
                        if (nodeAttrs != null && nodeAttrs.Length > 0)
                        {
                            LogicNodeAttribute logicNode = nodeAttrs[0] as LogicNodeAttribute;
                            Type nodeType = logicNode.NodeType;
                            foreach (var graphData in ConfigData.LogicGraphConifgs)
                            {
                                if (logicNode.HasType(graphData.GetGraphType()))
                                {
                                    LogicEditorConfigData.LogicNodeConfig nodeData = graphData.LogicNodes.FirstOrDefault(a => a.NodeClassName == nodeType.FullName);
                                    if (nodeData == null)
                                    {
                                        nodeData = new LogicEditorConfigData.LogicNodeConfig();
                                        nodeData.NodeClassName = nodeType.FullName;
                                        nodeData.NodeViewClassName = item.FullName;
                                        nodeData.UseCount = int.MinValue;
                                        graphData.LogicNodes.Add(nodeData);
                                    }
                                    string[] strs = logicNode.MenuText.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                                    nodeData.NodeLayers = strs;
                                    nodeData.NodeName = strs[strs.Length - 1];
                                    nodeData.NodeFullName = logicNode.MenuText;
                                    nodeData.IsRefresh = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 刷新格式化信息
        /// </summary>
        /// <param name="types"></param>
        private static void RefreshFormat(List<Type> types)
        {
            //逻辑图格式化类型
            Type _logicFormatType = typeof(ILogicFormat);
            Type _logicFormatAttr = typeof(LogicFormatAttribute);
            foreach (var item in types)
            {
                if (!item.IsAbstract && !item.IsInterface)
                {
                    if (_logicFormatType.IsAssignableFrom(item))
                    {
                        //如果当前类型是逻辑图节点
                        object[] formatAttrs = item.GetCustomAttributes(_logicFormatAttr, false);
                        if (formatAttrs != null && formatAttrs.Length > 0)
                        {
                            LogicFormatAttribute logicFormat = formatAttrs[0] as LogicFormatAttribute;
                            Type graphType = logicFormat.LogicGraphType;
                            var graphConfig = ConfigData.LogicGraphConifgs.FirstOrDefault(a => a.GraphClassName == graphType.FullName);
                            var formatConfig = graphConfig.LogicFormats.FirstOrDefault(a => a.FormatName == logicFormat.Name);
                            if (formatConfig != null)
                            {
                                Debug.LogError($"格式化名称相同:{logicFormat.Name}");
                            }
                            formatConfig = new LogicFormatConfig();
                            formatConfig.FormatName = logicFormat.Name;
                            formatConfig.FormatClassName = item.FullName;
                            formatConfig.Extension = logicFormat.Extension;
                            graphConfig.LogicFormats.Add(formatConfig);
                        }
                    }
                }
            }
        }
    }

}
