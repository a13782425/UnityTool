using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Logic.Editor.LogicEditorConfig;
using static Logic.Editor.LogicEditorConfigData;
using static Logic.LogicGraphData;

namespace Logic.Editor
{
    /// <summary>
    /// 逻辑图界面元素
    /// </summary>
    public class LogicPanelView : VisualElement
    {
        private LogicPanelData _panelData;
        /// <summary>
        /// 右键功能
        /// </summary>
        private ContextualMenuManipulator _contextMenu;

        public LogicPanelView(LogicPanelData panelData)
        {
            this.style.flexBasis = Length.Percent(100);
            _panelData = panelData;
            CreatePanelHead();
            _contextMenu = new ContextualMenuManipulator(OnContextualMenu);
            this.AddManipulator(_contextMenu);
            this.RegisterCallback<GeometryChangedEvent>(OnPostLayout);
        }
        void OnPostLayout(GeometryChangedEvent evt)
        {
            if (_panelData.LogicPanelGraphView != null)
            {
                this.UnregisterCallback<GeometryChangedEvent>(OnPostLayout);
                this._panelData.LogicPanelGraphView.FrameAll();
            }
        }
        #region 头部UI
        /// <summary>
        /// 逻辑图文件
        /// </summary>
        private Label _titleItem = null;
        private TextField _titleEditor;
        private bool _editTitleCancelled = false;
        private void CreatePanelHead()
        {
            var titleContent = new VisualElement { name = "titleContent" };
            {
                titleContent.style.height = 21;
                Toolbar toolbar = new Toolbar();

                _titleItem = new Label("逻辑图: 无");
                _titleItem.style.minWidth = 50;
                _titleItem.style.unityTextAlign = TextAnchor.MiddleLeft;
                _titleItem.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
                toolbar.Add(_titleItem);
                _titleEditor = new TextField();
                toolbar.Add(_titleEditor);
                _titleEditor.style.flexGrow = 1;
                _titleEditor.style.marginRight = 6;
                _titleEditor.style.maxWidth = 200;
                _titleEditor.style.unityTextAlign = TextAnchor.MiddleCenter;
                _titleEditor.style.display = DisplayStyle.None;
                VisualElement visualElement2 = _titleEditor.Q(TextInputBaseField<string>.textInputUssName);
                visualElement2.RegisterCallback<FocusOutEvent>(OnEditTitleFinished);
                visualElement2.RegisterCallback<KeyDownEvent>(OnTitleEditorOnKeyDown);
                titleContent.Add(toolbar);
            }
            Add(titleContent);
        }

        private void OnTitleEditorOnKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Escape:
                    _editTitleCancelled = true;
                    _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Blur();
                    break;
                case KeyCode.Return:
                    _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Blur();
                    break;
            }
        }

        private void OnEditTitleFinished(FocusOutEvent evt)
        {
            _titleItem.style.display = DisplayStyle.Flex;
            _titleEditor.style.display = DisplayStyle.None;
            if (!_editTitleCancelled)
            {
                string newTitle = this._titleEditor.text;
                _titleItem.text = $"逻辑图: {newTitle}";
                this._panelData.LogicGraphSummary.LogicName = newTitle;
                this._panelData.LogicGraphData.Title = newTitle;
                this._panelData.LogicPanel.titleContent = new GUIContent(newTitle);
            }
            _editTitleCancelled = true;
        }

        private void OnMouseDownEvent(MouseDownEvent evt)
        {
            if (evt.clickCount == 2 && evt.button == 0 && _panelData.LogicGraphData != null)
            {
                _titleItem.style.display = DisplayStyle.None;
                _titleEditor.style.display = DisplayStyle.Flex;
                _titleEditor.value = _panelData.LogicGraphData.Title;
                _titleEditor.SelectAll();
                _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Focus();
                _editTitleCancelled = false;
            }
        }

        #endregion
        /// <summary>
        /// 显示逻辑图
        /// </summary>
        /// <param name="graphSummary"></param>
        public void ShowLogicGraph()
        {
            this.RemoveManipulator(_contextMenu);
            this._titleItem.text = $"逻辑图: {_panelData.LogicGraphData.Title}";
            var content = new VisualElement { name = "graphContent" };
            {
                content.style.flexGrow = 1;
                _panelData.LogicPanelGraphView = new LogicPanelGraphView(_panelData);
                _panelData.LogicPanelGraphView.name = "logicGraph";
                content.Add(_panelData.LogicPanelGraphView);
            }
            Add(content);
            //this.MarkDirtyRepaint();
        }

        private void OnContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("创建逻辑图", onCreateLogic);
            evt.menu.AppendAction("打开逻辑图", onOpenLogic);
        }
        /// <summary>
        /// 创建逻辑图
        /// </summary>
        /// <param name="obj"></param>
        private void onCreateLogic(DropdownMenuAction obj)
        {
            var menuWindowProvider = ScriptableObject.CreateInstance<CreateSearchWindowProvider>();
            menuWindowProvider.OnCreateLogicHandler = onCreateMenuSelectEntry;
            Vector2 screenPos = _panelData.LogicPanel.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), menuWindowProvider);
        }
        /// <summary>
        /// 打开逻辑图
        /// </summary>
        /// <param name="obj"></param>
        private void onOpenLogic(DropdownMenuAction obj)
        {
            var menuWindowProvider = ScriptableObject.CreateInstance<OpenSearchWindowProvider>();
            menuWindowProvider.OnOpenLogicHandler = onOpenMenuSelectEntry;

            Vector2 screenPos = _panelData.LogicPanel.GetScreenPosition(obj.eventInfo.mousePosition);
            SearchWindow.Open(new SearchWindowContext(screenPos), menuWindowProvider);
        }
        /// <summary>
        /// 创建一个逻辑图
        /// </summary>
        /// <param name="searchTreeEntry"></param>
        /// <param name="context"></param>
        private void onCreateMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            LogicGraphConfig configData = searchTreeEntry.userData as LogicGraphConfig;
            string path = EditorUtility.SaveFilePanel("创建逻辑图", Application.dataPath, "LogicGraph", "asset");
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("错误", "路径为空", "确定");
                return;
            }
            if (File.Exists(path))
            {
                EditorUtility.DisplayDialog("错误", "创建文件已存在", "确定");
                return;
            }
            string file = Path.GetFileNameWithoutExtension(path);
            LogicGraphBase obj = ScriptableObject.CreateInstance(configData.GraphClassName) as LogicGraphBase;
            obj.name = file;
            path = path.Replace(Application.dataPath, "Assets");
            LogicGraphData graphData = ScriptableObject.CreateInstance<LogicGraphData>();
            graphData.name = typeof(LogicGraphData).Name;
            graphData.Title = file;
            obj.LogicGraphData = graphData;
            StartNode start = CreateStartNode(obj, configData);
            obj.DefaultNode = start;
            obj.LogicNodeList.Add(start);
            AssetDatabase.CreateAsset(obj, path);
            AssetDatabase.AddObjectToAsset(graphData, obj);
            AssetDatabase.AddObjectToAsset(start, obj);
            LogicGraphSummary graphSummary = ConfigData.GetGraphSummary(a => a.AssetPath == path);
            LogicPanel.SetLogicGraph(_panelData.LogicPanel, graphSummary);
        }

        private StartNode CreateStartNode(LogicGraphBase graph, LogicGraphConfig configData)
        {
            StartNode start = ScriptableObject.CreateInstance<StartNode>();
            LogicNodeConfig nodeConfig = configData.LogicNodes.FirstOrDefault(a => a.NodeClassName == typeof(StartNode).FullName);
            LogicNodeData nodeData = new LogicNodeData();
            start.name = start.GetType().Name;
            nodeData.Pos = Vector2.zero;
            nodeData.OnlyId = start.OnlyId;
            nodeData.Title = nodeConfig.NodeName;
            graph.LogicGraphData.LogicNodes.Add(nodeData);
            return start;
        }

        /// <summary>
        /// 打开一个逻辑图
        /// </summary>
        /// <param name="searchTreeEntry"></param>
        /// <param name="context"></param>
        private void onOpenMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            LogicGraphSummary graphSummary = searchTreeEntry.userData as LogicGraphSummary;
            LogicPanel.SetLogicGraph(_panelData.LogicPanel, graphSummary);
        }
        /// <summary>
        /// 创建逻辑图搜索界面提供者
        /// </summary>
        private class CreateSearchWindowProvider : ScriptableObject, ISearchWindowProvider
        {
            public Action<SearchTreeEntry, SearchWindowContext> OnCreateLogicHandler;

            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                var entries = new List<SearchTreeEntry>();
                entries.Add(new SearchTreeGroupEntry(new GUIContent("创建逻辑图")));

                foreach (var item in ConfigData.LogicGraphConifgs)
                {
                    entries.Add(new SearchTreeEntry(new GUIContent(item.GraphName)) { level = 1, userData = item });
                }
                return entries;
            }

            public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
            {
                OnCreateLogicHandler?.Invoke(searchTreeEntry, context);
                return true;
            }
        }
        /// <summary>
        /// 打开逻辑图搜索界面提供者
        /// </summary>
        private class OpenSearchWindowProvider : ScriptableObject, ISearchWindowProvider
        {
            public Action<SearchTreeEntry, SearchWindowContext> OnOpenLogicHandler;

            public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
            {
                var entries = new List<SearchTreeEntry>();
                entries.Add(new SearchTreeGroupEntry(new GUIContent("打开逻辑图")));
                foreach (var item in ConfigData.LogicGraphConifgs)
                {
                    entries.Add(new SearchTreeGroupEntry(new GUIContent(item.GraphName)) { level = 1, userData = item });
                    var datas = ConfigData.LogicGraphs.Where(a => a.GraphClassName == item.GraphClassName).ToList();
                    foreach (var graph in datas)
                    {
                        entries.Add(new SearchTreeEntry(new GUIContent(graph.LogicName)) { level = 2, userData = graph });
                    }
                }
                return entries;
            }

            public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
            {
                OnOpenLogicHandler?.Invoke(searchTreeEntry, context);
                return true;
            }
        }
    }
}
