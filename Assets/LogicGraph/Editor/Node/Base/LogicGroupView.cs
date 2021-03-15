using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Logic.LogicGraphData;
using static Logic.Editor.LogicEditorConfig;
using static Logic.Editor.LogicEditorConfigData;
using System;
using System.Linq;

namespace Logic.Editor
{
    public class LogicGroupView : Group
    {
        public LogicGroupData GroupData { get; private set; }
        private LogicPanelData _panelData;
        private LogicGroupConfig _groupConfig;
        private bool _isDefaultGroup = false;
        public LogicGroupView(LogicPanelData panelData, LogicGroupData groupData)
        {
            this._panelData = panelData;
            this.GroupData = groupData;
            _groupConfig = panelData.LogicGraphConfig.LogicGroups.FirstOrDefault(a => a.TempId == groupData.TempId);
            if (_groupConfig == null)
            {
                _groupConfig = panelData.LogicGraphConfig.LogicGroups.FirstOrDefault(a => a.GroupName == DEFAULT_GROUP_NAME);
                this.GroupData.TempId = _groupConfig.TempId;
            }
            _isDefaultGroup = _groupConfig.GroupName == DEFAULT_GROUP_NAME;
            _panelData.onGroupConfigDelete += onGroupConfigDelete;
            this.AddManipulator(new ContextualMenuManipulator(OnContextualMenu));
        }

        private void onGroupConfigDelete()
        {
            var config = _panelData.LogicGraphConfig.LogicGroups.FirstOrDefault(a => a.TempId == GroupData.TempId);
            if (config == null)
            {
                _groupConfig = _panelData.LogicGraphConfig.LogicGroups.FirstOrDefault(a => a.GroupName == DEFAULT_GROUP_NAME);
                this.GroupData.TempId = _groupConfig.TempId;
            }
            _isDefaultGroup = _groupConfig.GroupName == DEFAULT_GROUP_NAME;
        }

        /// <summary>
        /// 右键菜单
        /// </summary>
        /// <param name="obj"></param>
        private void OnContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!this.selected || this._panelData.LogicPanelGraphView.selection.Count > 1)
            {
                return;
            }
            //this._panelData.LogicPanelGraphView.AddToSelection
            //this.selected = true;
            evt.menu.AppendAction("删除", onRemoveGroup);
            evt.menu.AppendSeparator();
            if (!_isDefaultGroup)
            {
                evt.menu.AppendAction("保存当前组", onSaveTemplate);
                evt.menu.AppendAction("删除当前组", onRemoveTemplate);
                evt.menu.AppendSeparator();
            }
            evt.menu.AppendAction("另存为新组", onSaveAsTemplate);
        }



        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            base.OnElementsAdded(elements);
            foreach (var item in elements)
            {
                switch (item)
                {
                    case LogicNodeBaseView nodeView:
                        if (!GroupData.Nodes.Contains(nodeView.NodeData.OnlyId))
                        {
                            GroupData.Nodes.Add(nodeView.NodeData.OnlyId);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            base.OnElementsRemoved(elements);
            foreach (var item in elements)
            {
                switch (item)
                {
                    case LogicNodeBaseView nodeView:
                        GroupData.Nodes.Remove(nodeView.NodeData.OnlyId);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 删除组
        /// </summary>
        /// <param name="obj"></param>
        private void onRemoveGroup(DropdownMenuAction obj)
        {
            _panelData.LogicPanelGraphView.DeleteSelection();
        }
        /// <summary>
        /// 保存模板
        /// </summary>
        /// <param name="obj"></param>
        private void onSaveTemplate(DropdownMenuAction obj)
        {
            RefreshGroupNodeConfigs();
            _panelData.Save();
            _panelData.LogicPanel.ShowNotification(new GUIContent("模板保存成功"));
        }
        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="obj"></param>
        private void onRemoveTemplate(DropdownMenuAction obj)
        {
            _panelData.LogicGraphConfig.LogicGroups.Remove(_groupConfig);
            _groupConfig = _panelData.LogicGraphConfig.LogicGroups.FirstOrDefault(a => a.GroupName == DEFAULT_GROUP_NAME);
            GroupData.TempId = _groupConfig.TempId;
            _isDefaultGroup = true;
            _panelData.Save();
            _panelData.onGroupConfigDelete?.Invoke();
            _panelData.LogicPanel.ShowNotification(new GUIContent("模板删除成功"));
        }
        /// <summary>
        /// 另存为
        /// </summary>
        /// <param name="obj"></param>
        private void onSaveAsTemplate(DropdownMenuAction obj)
        {
            if (_panelData.LogicGraphConfig.LogicGroups.Any(a => a.GroupName == this.title))
            {
                _panelData.LogicPanel.ShowNotification(new GUIContent("已存在相同组名的模板"));
                return;
            }
            _groupConfig = new LogicGroupConfig();
            _groupConfig.TempId = Guid.NewGuid().ToString();
            _groupConfig.UseCount = int.MinValue;
            _groupConfig.GroupName = this.title;
            RefreshGroupNodeConfigs();
            _panelData.LogicGraphConfig.LogicGroups.Add(_groupConfig);

            GroupData.TempId = _groupConfig.TempId;
            _isDefaultGroup = _groupConfig.GroupName == DEFAULT_GROUP_NAME;

            _panelData.Save();
        }
        private void RefreshGroupNodeConfigs()
        {
            _groupConfig.NodeConfigs.Clear();
            List<LogicNodeData> nodeDatas = _panelData.LogicGraphData.LogicNodes;
            foreach (var item in GroupData.Nodes)
            {
                LogicGroupNodeConfig nodeConfig = new LogicGroupNodeConfig();
                var nodeData = nodeDatas.FirstOrDefault(a => a.OnlyId == item);
                nodeConfig.OnlyId = nodeData.OnlyId;
                nodeConfig.Pos = nodeData.Pos - this.GetPosition().position;
                nodeConfig.NodeClassName = nodeData.BelongNode.GetType().FullName;
                foreach (var child in nodeData.Childs)
                {
                    if (GroupData.Nodes.Any(a => a == child))
                    {
                        nodeConfig.Childs.Add(child);
                    }
                }
                foreach (var parent in nodeData.Parents)
                {
                    if (GroupData.Nodes.Any(a => a == parent))
                    {
                        nodeConfig.Parents.Add(parent);
                    }
                }
                _groupConfig.NodeConfigs.Add(nodeConfig);
            }
        }
    }
}
