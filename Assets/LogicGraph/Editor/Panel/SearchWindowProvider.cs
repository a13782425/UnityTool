using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Logic.Editor.LogicEditorConfig;
using static Logic.Editor.LogicEditorConfigData;

namespace Logic.Editor
{
    /// <summary>
    /// 创建节点搜索界面提供者
    /// </summary>
    public class SearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private LogicGraphSummary _graphSummary;
        public Action<SearchTreeEntry, SearchWindowContext> OnCreateLogicHandler;
        private LogicGraphConfig _graphConfigData;
        public void Init(LogicGraphConfig graphConfig)
        {
            _graphConfigData = graphConfig;
        }
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTrees = new List<SearchTreeEntry>();
            searchTrees.Add(new SearchTreeGroupEntry(new GUIContent("创建节点")));

            AddRecommendTree(searchTrees);
            AddGroupTree(searchTrees);
            AddNodeTree(searchTrees);

            return searchTrees;
        }



        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            OnCreateLogicHandler?.Invoke(searchTreeEntry, context);

            return true;
        }

        /// <summary>
        /// 添加常用节点树
        /// </summary>
        /// <param name="searchTrees"></param>
        /// <param name="logicNodes"></param>
        private void AddRecommendTree(List<SearchTreeEntry> searchTrees)
        {
            Type startType = typeof(StartNode);
            List<LogicNodeConfig> logicNodes = _graphConfigData.LogicNodes.ToList();
            logicNodes.Sort((a, b) =>
            {
                if (a.UseCount == b.UseCount)
                {
                    return 0;
                }
                else if (a.UseCount < b.UseCount)
                {
                    return 1;
                }
                return -1;
            });

            var recommends = logicNodes.Take(10);
            searchTrees.Add(new SearchTreeGroupEntry(new GUIContent("常用")) { level = 1 });
            foreach (LogicNodeConfig nodeConfig in recommends)
            {
                if (nodeConfig.GetNodeType() == startType)
                {
                    continue;
                }
                searchTrees.Add(new SearchTreeEntry(new GUIContent(nodeConfig.NodeFullName)) { level = 2, userData = nodeConfig });
            }
        }
        /// <summary>
        /// 添加组
        /// </summary>
        /// <param name="searchTrees"></param>
        private void AddGroupTree(List<SearchTreeEntry> searchTrees)
        {
            List<LogicGroupConfig> logicGroups = _graphConfigData.LogicGroups;
            logicGroups.Sort((a, b) =>
            {
                if (a.UseCount == b.UseCount)
                {
                    return 0;
                }
                else if (a.UseCount < b.UseCount)
                {
                    return 1;
                }
                return -1;
            });
            searchTrees.Add(new SearchTreeGroupEntry(new GUIContent("组")) { level = 1 });
            foreach (LogicGroupConfig nodeConfig in logicGroups)
            {
                searchTrees.Add(new SearchTreeEntry(new GUIContent(nodeConfig.GroupName)) { level = 2, userData = nodeConfig });
            }
        }
        /// <summary>
        /// 添加节点树
        /// </summary>
        /// <param name="searchTrees"></param>
        private void AddNodeTree(List<SearchTreeEntry> searchTrees)
        {
            Type startType = typeof(StartNode);
            List<string> groups = new List<string>();
            foreach (LogicNodeConfig nodeConfig in _graphConfigData.LogicNodes)
            {
                if (nodeConfig.GetNodeType() == startType)
                {
                    continue;
                }
                int createIndex = int.MaxValue;

                for (int i = 0; i < nodeConfig.NodeLayers.Length - 1; i++)
                {
                    string group = nodeConfig.NodeLayers[i];
                    if (i >= groups.Count)
                    {
                        createIndex = i;
                        break;
                    }
                    if (groups[i] != group)
                    {
                        groups.RemoveRange(i, groups.Count - i);
                        createIndex = i;
                        break;
                    }
                }
                for (int i = createIndex; i < nodeConfig.NodeLayers.Length - 1; i++)
                {
                    string group = nodeConfig.NodeLayers[i];
                    groups.Add(group);
                    searchTrees.Add(new SearchTreeGroupEntry(new GUIContent(group)) { level = i + 1 });
                }

                searchTrees.Add(new SearchTreeEntry(new GUIContent(nodeConfig.NodeName)) { level = nodeConfig.NodeLayers.Length, userData = nodeConfig });
            }
        }

    }
}
