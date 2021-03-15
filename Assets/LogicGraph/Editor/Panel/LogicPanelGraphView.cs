using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static Logic.Editor.LogicEditorConfig;
using static Logic.Editor.LogicEditorConfigData;
using static Logic.LogicGraphData;
using Object = UnityEngine.Object;

namespace Logic.Editor
{
    /// <summary>
    /// 逻辑图图形视图
    /// </summary>
    public sealed partial class LogicPanelGraphView : GraphView
    {
        private LogicPanelData _panelData;
        private Dictionary<string, LogicNodeBaseView> _allNodeViews = new Dictionary<string, LogicNodeBaseView>();
        public Dictionary<string, LogicNodeBaseView> AllNodeViews => _allNodeViews;
        public LogicPanelGraphView(LogicPanelData panelData)
        {
            _panelData = panelData;
            this.style.flexGrow = 1;
            this.SetupZoom(0.05f, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            AddGridBackGround();

            ReplayLogicGraph();
            //界面发生变化时候调用
            graphViewChanged = onGraphViewChanged;
            groupTitleChanged = onGroupTitleChanged;
            this.RegisterCallback<KeyDownEvent>(onKeyDownEvent);
        }

        /// <summary>
        /// 获取可连线的点
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="nodeAdapter"></param>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            if (startPort.direction == Direction.Input)
            {
                return compatiblePorts;
            }
            LogicNodeBaseView startNodeView = startPort.node as LogicNodeBaseView;
            foreach (var port in ports.ToList())
            {
                if (startPort.node == port.node ||
                    port.direction == Direction.Output ||
                    startPort.portType != port.portType)
                {
                    continue;
                }
                if (startPort.connections.FirstOrDefault(a => a.input == port) != null)
                {
                    continue;
                }
                var nodeView = port.node as LogicNodeBaseView;
                if (nodeView.CanAcceptLink(startNodeView))
                {
                    compatiblePorts.Add(port);
                }
            }
            return compatiblePorts;
        }

        /// <summary>
        /// 当组标题发生改变调用
        /// </summary>
        /// <param name="group"></param>
        /// <param name="title"></param>
        private void onGroupTitleChanged(Group group, string title)
        {
            (group as LogicGroupView).GroupData.Title = title;
        }
        /// <summary>
        /// 界面发生变化时候调用
        /// </summary>
        private GraphViewChange onGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements != null)
            {
                foreach (GraphElement item in graphViewChange.movedElements)
                {
                    switch (item)
                    {
                        case LogicNodeBaseView nodeView:
                            {
                                nodeView.NodeData.Pos = item.GetPosition().position;
                            }
                            break;
                        case LogicGroupView groupView:
                            {
                                groupView.GroupData.Pos = item.GetPosition().position;
                                foreach (var graphElement in groupView.containedElements)
                                {
                                    if (graphElement is LogicNodeBaseView nodeView)
                                    {
                                        nodeView.NodeData.Pos = graphElement.GetPosition().position;
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            if (graphViewChange.edgesToCreate != null)
            {
                foreach (Edge item in graphViewChange.edgesToCreate)
                {
                    LogicNodeBaseView input = item.input.node as LogicNodeBaseView;
                    LogicNodeBaseView output = item.output.node as LogicNodeBaseView;
                    input.AddParent(output);
                    output.AddChild(input);
                }
            }
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.RemoveAll(a => a is StartNodeView);
                foreach (GraphElement item in graphViewChange.elementsToRemove)
                {
                    switch (item)
                    {
                        case LogicNodeBaseView nodeView:
                            {
                                RemoveNodeView(nodeView);
                            }
                            break;
                        case LogicGroupView groupView:
                            {
                                _panelData.LogicGraphData.LogicGroups.Remove(groupView.GroupData);
                            }
                            break;
                        case Edge edge:
                            {
                                LogicNodeBaseView input = edge.input.node as LogicNodeBaseView;
                                LogicNodeBaseView output = edge.output.node as LogicNodeBaseView;
                                input.RemoveParent(output);
                                output.RemoveChild(input);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            return graphViewChange;
        }

        /// <summary>
        /// 删除一个节点
        /// </summary>
        /// <param name="nodeView"></param>
        private void RemoveNodeView(LogicNodeBaseView nodeView)
        {
            if (nodeView.Input != null)
            {
                //可能存在父亲
                foreach (var parent in nodeView.Input.connections)
                {
                    (parent.output.node as LogicNodeBaseView).RemoveChild(nodeView);
                }
            }
            if (nodeView.Output != null)
            {
                //可能存在儿子
                foreach (var child in nodeView.Output.connections)
                {
                    (child.input.node as LogicNodeBaseView).RemoveParent(nodeView);
                }
            }
            _panelData.LogicGraph.LogicNodeList.Remove(nodeView.Target);
            _panelData.LogicGraphData.LogicNodes.Remove(nodeView.NodeData);
            if (_allNodeViews.ContainsKey(nodeView.OnlyId))
            {
                _allNodeViews.Remove(nodeView.OnlyId);
            }
            Object.DestroyImmediate(nodeView.Target, true);
            _panelData.Save();
        }

        /// <summary>
        /// 右键创建按钮
        /// </summary>
        /// <param name="obj"></param>
        private void OnCreateCallback(DropdownMenuAction obj)
        {
            var menuWindowProvider = ScriptableObject.CreateInstance<SearchWindowProvider>();
            menuWindowProvider.OnCreateLogicHandler = onCreateMenuSelectEntry;
            menuWindowProvider.Init(this._panelData.LogicGraphConfig);
            Vector2 screenPos = _panelData.LogicPanel.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), menuWindowProvider);
        }
        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="searchTreeEntry"></param>
        /// <param name="context"></param>
        private void onCreateMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            LogicPanel panel = _panelData.LogicPanel;
            //经过计算得出节点的位置
            var windowMousePosition = panel.rootVisualElement.ChangeCoordinatesTo(panel.rootVisualElement.parent, context.screenMousePosition - panel.position.position);
            var nodePosition = this.contentViewContainer.WorldToLocal(windowMousePosition);
            if (searchTreeEntry.userData is LogicNodeConfig nodeConfig)
            {
                CreateLogicNodeView(nodeConfig, nodePosition);
            }
            else if (searchTreeEntry.userData is LogicGroupConfig groupConfig)
            {
                CreateLogicGroupView(groupConfig, nodePosition);
            }

        }

        /// <summary>
        /// 创建逻辑图群组
        /// </summary>
        /// <param name="groupConfig"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private LogicGroupView CreateLogicGroupView(LogicGroupConfig groupConfig, Vector2 pos, bool record = true)
        {
            LogicGroupData groupData = new LogicGroupData();
            groupData.TempId = groupConfig.TempId;
            groupData.Pos = pos;
            LogicGroupView view = new LogicGroupView(this._panelData, groupData);
            this.AddElement(view);
            view.SetPosition(new Rect(pos, Vector2.zero));
            view.title = groupConfig.GroupName;
            groupData.Title = groupConfig.GroupName;
            _panelData.LogicGraphData.LogicGroups.Add(groupData);

            Dictionary<string, LogicNodeBaseView> tempDic = new Dictionary<string, LogicNodeBaseView>();
            foreach (var item in groupConfig.NodeConfigs)
            {
                LogicNodeConfig nodeConfig = _panelData.LogicGraphConfig.LogicNodes.FirstOrDefault(a => a.NodeClassName == item.NodeClassName);
                if (nodeConfig != null)
                {
                    LogicNodeBaseView nodeView = CreateLogicNodeView(nodeConfig, item.Pos + pos, false);
                    tempDic.Add(item.OnlyId, nodeView);
                    view.AddElement(nodeView);
                }
            }
            foreach (var item in groupConfig.NodeConfigs)
            {
                if (tempDic.ContainsKey(item.OnlyId))
                {
                    LogicNodeBaseView nodeView = tempDic[item.OnlyId];

                    List<string> waitRemoveLits = new List<string>();
                    foreach (var child in item.Childs)
                    {
                        if (tempDic.ContainsKey(child))
                        {
                            var childView = tempDic[child];
                            Edge edge = nodeView.Output.ConnectTo(childView.Input);
                            nodeView.AddChild(childView);
                            childView.AddParent(nodeView);
                            AddElement(edge);
                        }
                        else
                            waitRemoveLits.Add(child);
                    }
                    foreach (var remove in waitRemoveLits)
                    {
                        item.Childs.Remove(remove);
                    }
                }
            }
            if (record)
            {
                groupConfig.AddUseCount();
                _panelData.LogicGraphConfig.LogicGroups.ForEach(a =>
                {
                    if (a != groupConfig)
                    {
                        a.SubUseCount();
                    }
                });
            }
            return view;
        }

        /// <summary>
        /// 创建一个节点
        /// </summary>
        /// <param name="nodeConfig"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private LogicNodeBaseView CreateLogicNodeView(LogicNodeConfig nodeConfig, Vector2 pos, bool record = true)
        {
            LogicNodeBase logicNode = ScriptableObject.CreateInstance(nodeConfig.GetNodeType()) as LogicNodeBase;
            logicNode.name = logicNode.GetType().Name;
            _panelData.LogicGraph.LogicNodeList.Add(logicNode);
            LogicNodeData nodeData = new LogicNodeData();
            nodeData.OnlyId = logicNode.OnlyId;
            nodeData.BelongNode = logicNode;
            _panelData.LogicGraphData.LogicNodes.Add(nodeData);
            AssetDatabase.AddObjectToAsset(logicNode, _panelData.LogicGraph);
            EditorUtility.SetDirty(_panelData.LogicGraph);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            LogicNodeBaseView view = Activator.CreateInstance(nodeConfig.GetNodeViewType()) as LogicNodeBaseView;
            view.Initialize(_panelData, nodeData);
            this.AddElement(view);
            view.SetPosition(new Rect(pos, Vector2.zero));
            nodeData.Pos = pos;
            view.title = nodeConfig.NodeName;
            view.OnCreate();
            nodeData.Title = nodeConfig.NodeName;
            view.ShowUI();
            _allNodeViews.Add(view.OnlyId, view);
            if (record)
            {
                nodeConfig.AddUseCount();
                _panelData.LogicGraphConfig.LogicNodes.ForEach(a =>
                {
                    if (a != nodeConfig)
                    {
                        a.SubUseCount();
                    }
                });
            }
            return view;
        }
    }

    //复盘逻辑图
    public sealed partial class LogicPanelGraphView
    {
        /// <summary>
        /// 复盘逻辑图
        /// </summary>
        private void ReplayLogicGraph()
        {
            var graphData = _panelData.LogicGraph.LogicGraphData;
            List<string> replayNodes = new List<string>();


            foreach (var item in _panelData.LogicGraph.LogicNodeList)
            {
                LogicNodeData nodeData = graphData.LogicNodes.FirstOrDefault(a => a.OnlyId == item.OnlyId);
                nodeData.BelongNode = item;
                ReplayLogicNodeView(nodeData);
            }
            foreach (LogicGroupData item in graphData.LogicGroups)
            {
                LogicGroupView groupView = ReplayLogicGroupView(item);
                foreach (string nodeId in item.Nodes)
                {
                    if (_allNodeViews.ContainsKey(nodeId))
                    {
                        LogicNodeBaseView nodeView = _allNodeViews[nodeId];
                        groupView.AddElement(nodeView);
                    }
                }
            }
            List<Node> list = nodes.ToList();
            nodes.ForEach(item =>
            {
                LogicNodeBaseView view = item as LogicNodeBaseView;
                if (view != null)
                {
                    foreach (var child in view.Target.Childs)
                    {
                        var inNode = list.FirstOrDefault(a => (a as LogicNodeBaseView).Target == child) as LogicNodeBaseView;
                        Edge edge = view.Output.ConnectTo(inNode.Input);
                        AddElement(edge);
                    }
                }
            });
        }
        /// <summary>
        /// 复盘一个逻辑图群组
        /// </summary>
        /// <param name="groupConfig"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        private LogicGroupView ReplayLogicGroupView(LogicGroupData groupData)
        {
            LogicGroupView view = new LogicGroupView(this._panelData, groupData);
            this.AddElement(view);
            view.SetPosition(new Rect(groupData.Pos, Vector2.zero));
            view.title = groupData.Title;
            return view;
        }
        /// <summary>
        /// 复盘一个节点
        /// </summary>
        /// <param name="nodeData"></param>
        /// <returns></returns>
        private LogicNodeBaseView ReplayLogicNodeView(LogicNodeData nodeData)
        {
            string fullName = nodeData.BelongNode.GetType().FullName;
            LogicNodeConfig nodeConfig = _panelData.LogicGraphConfig.LogicNodes.FirstOrDefault(a => a.NodeClassName == fullName);
            LogicNodeBaseView view = Activator.CreateInstance(nodeConfig.GetNodeViewType()) as LogicNodeBaseView;
            view.Initialize(_panelData, nodeData);
            this.AddElement(view);
            view.SetPosition(new Rect(nodeData.Pos, Vector2.zero));
            view.OnCreate();
            view.title = nodeData.Title;
            view.ShowUI();
            _allNodeViews.Add(view.OnlyId, view);
            return view;
        }
    }

    //事件
    public sealed partial class LogicPanelGraphView
    {
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target is GraphView)
            {
                //evt.menu.AppendAction("Create Node", OnContextMenuNodeCreate, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendAction("创建节点", OnCreateCallback, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("保存", onSaveCallback, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendAction("另存为", onSaveAsCallback, DropdownMenuAction.AlwaysEnabled);
                evt.menu.AppendSeparator();
                foreach (var item in _panelData.LogicGraphConfig.LogicFormats)
                {
                    evt.menu.AppendAction("生成: " + item.FormatName, onFormatCallback, DropdownMenuAction.AlwaysEnabled, item);
                }
            }
        }
        /// <summary>
        /// 格式化
        /// </summary>
        /// <param name="obj"></param>
        private void onFormatCallback(DropdownMenuAction obj)
        {
            var formatConfig = obj.userData as LogicFormatConfig;
            if (formatConfig != null)
            {
                string filePath = EditorUtility.SaveFilePanel("导出", Application.dataPath, "undefined", formatConfig.Extension);
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    _panelData.LogicPanel.ShowNotification(new GUIContent("请选择导出路径"));
                    return;
                }
                var logicFormat = Activator.CreateInstance(formatConfig.GetFormatType()) as ILogicFormat;
                bool res = logicFormat.ToFormat(_panelData.LogicGraph, filePath);
                if (res)
                {
                    _panelData.LogicPanel.ShowNotification(new GUIContent($"导出: {formatConfig.FormatName} 成功"));
                }
                else
                {
                    _panelData.LogicPanel.ShowNotification(new GUIContent($"导出: {formatConfig.FormatName} 失败"));
                }
            }
        }

        /// <summary>
        /// 另存为
        /// </summary>
        /// <param name="evt"></param>
        private void onSaveAsCallback(DropdownMenuAction evt)
        {
            string file = EditorUtility.SaveFilePanel("另存为", Application.dataPath, _panelData.LogicGraphSummary.FileName + " 副本", "asset");
            if (string.IsNullOrWhiteSpace(file))
            {
                _panelData.LogicPanel.ShowNotification(new GUIContent("请选择另存为路径"));
                return;
            }
            file = file.Replace(Application.dataPath, "Assets");
            if (file == _panelData.LogicGraphSummary.AssetPath)
            {
                _panelData.LogicPanel.ShowNotification(new GUIContent("目标位置和源文件位置一样"));
                return;
            }
            bool res = AssetDatabase.CopyAsset(_panelData.LogicGraphSummary.AssetPath, file);
            if (res)
            {
                LogicGraphBase graph = AssetDatabase.LoadAssetAtPath<LogicGraphBase>(file);
                AssetDatabase.SetMainObject(graph, file);
                EditorUtility.SetDirty(graph);
                AssetDatabase.SaveAssets();
                _panelData.LogicPanel.ShowNotification(new GUIContent("另存为成功"));
            }
            else
            {
                _panelData.LogicPanel.ShowNotification(new GUIContent("另存为失败"));
                return;
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="evt"></param>
        private void onSaveCallback(DropdownMenuAction evt)
        {
            if (_panelData != null)
            {
                _panelData.Save();
            }
        }

        /// <summary>
        /// 当案件按下
        /// </summary>
        /// <param name="evt"></param>
        private void onKeyDownEvent(KeyDownEvent evt)
        {
            if (evt.ctrlKey && evt.keyCode == KeyCode.S)
            {
                //保存
                _panelData.Save();
                evt.StopImmediatePropagation();
            }
            if (evt.ctrlKey && evt.keyCode == KeyCode.D)
            {
                Vector2 screenPos = _panelData.LogicPanel.GetScreenPosition(evt.originalMousePosition);
                //经过计算得出节点的位置
                var windowMousePosition = _panelData.LogicPanel.rootVisualElement.ChangeCoordinatesTo(_panelData.LogicPanel.rootVisualElement.parent, screenPos - _panelData.LogicPanel.position.position);
                var nodePosition = this.contentViewContainer.WorldToLocal(windowMousePosition);
                //复制
                foreach (ISelectable item in selection)
                {
                    switch (item)
                    {
                        case LogicNodeBaseView nodeView:
                            DuplicateNodeView(nodeView, nodePosition);
                            break;
                        case LogicGroupView groupView:
                            DuplicateGroupView(groupView, nodePosition);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 创建一个节点的副本
        /// </summary>
        private void DuplicateNodeView(LogicNodeBaseView nodeView, Vector2 pos)
        {

            string nodeFullName = nodeView.Target.GetType().FullName;
            var nodeConfig = _panelData.LogicGraphConfig.LogicNodes.FirstOrDefault(a => a.NodeClassName == nodeFullName);
            CreateLogicNodeView(nodeConfig, pos, false);
        }
        /// <summary>
        /// 创建一个节点的副本
        /// </summary>
        private void DuplicateGroupView(LogicGroupView nodeView, Vector2 pos)
        {

            string tempId = nodeView.GroupData.TempId;
            var graphConfig = _panelData.LogicGraphConfig.LogicGroups.FirstOrDefault(a => a.TempId == tempId);
            CreateLogicGroupView(graphConfig, pos, false);
        }
    }

    //网格
    public sealed partial class LogicPanelGraphView
    {
        private class LogicGridBackground : GridBackground { }
        /// <summary>
        /// 添加背景网格
        /// </summary>
        void AddGridBackGround()
        {
            //添加网格背景
            GridBackground gridBackground = new LogicGridBackground();
            gridBackground.name = "GridBackground";
            Insert(0, gridBackground);
            //设置背景缩放范围
            this.SetupZoom(0.05f, ContentZoomer.DefaultMaxScale);

            //扩展大小与父对象相同
            this.StretchToParentSize();
        }
    }
}
