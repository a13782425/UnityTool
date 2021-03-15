using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Logic.LogicGraphData;
using static Logic.LogicNodeBase;

namespace Logic.Editor
{
    /// <summary>
    /// 逻辑节点显示
    /// </summary>
    public abstract class LogicNodeBaseView : Node, ILogicNodeView
    {
        #region 字段和属性

        /// <summary>
        /// 当前视图的唯一Id,与数据,节点的唯一Id一致
        /// </summary>
        public string OnlyId { get; private set; }
        private LogicNodeData _nodeData;
        /// <summary>
        /// 当前节点的数据
        /// </summary>
        public LogicNodeData NodeData => _nodeData;

        private LogicPanelData _panelData;
        /// <summary>
        /// 当前逻辑图窗口的数据
        /// </summary>
        public LogicPanelData PanelData => _panelData;
        /// <summary>
        /// 当前节点视图对应的节点
        /// </summary>
        public LogicNodeBase Target => NodeData.BelongNode;
        /// <summary>
        /// 进端口(子端口)
        /// </summary>
        public Port Input { get; private set; }
        /// <summary>
        /// 出端口(副端口)
        /// </summary>
        public Port Output { get; private set; }

        private float _width = 100;
        /// <summary>
        /// 当前节点视图的宽度
        /// </summary>
        public float Width
        {
            get => _width;
            protected set
            {
                _width = value;
                this.style.width = _width;
            }
        }

        /// <summary>
        /// 重新定义的内容容器
        /// </summary>
        protected VisualElement m_content { get; private set; }

        #endregion

        public LogicNodeBaseView()
        {
            //this.title = this.GetType().Name;
            //移除右上角折叠按钮
            this.titleContainer.Remove(titleButtonContainer);
            topContainer.style.height = 24;
            m_content = topContainer.parent;
            m_content.style.backgroundColor = new Color(0, 0, 0, 0.5f);
            CheckTitle();
            AddPort();
            RefreshExpandedState();
        }
        /// <summary>
        /// 添加端口
        /// </summary>
        private void AddPort()
        {
            PortConfig portConfig = GetPortConfig();
            if ((portConfig.PortType & PortEnum.In) > PortEnum.None)
            {
                //存在进端口
                Input = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Port));
                Input.portColor = Color.blue;
                Input.portName = "In";
                inputContainer.Add(Input);
            }
            if ((portConfig.PortType & PortEnum.Out) > PortEnum.None)
            {
                //存在出端口
                Output = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Port));
                Output.portColor = Color.red;
                Output.portName = "Out";
                outputContainer.Add(Output);
            }
        }

        #region 公共方法
        public void Initialize(LogicPanelData panelData, LogicNodeData nodeData)
        {
            _nodeData = nodeData;
            _panelData = panelData;
            OnlyId = _nodeData.OnlyId;
        }

        /// <summary>
        /// 添加父节点
        /// </summary>
        /// <param name="parent"></param>
        public void AddParent(LogicNodeBaseView parent)
        {
            this.NodeData.Parents.Add(parent.Target.OnlyId);
            this.Target.Parents.Add(parent.Target);
        }
        /// <summary>
        /// 移除父节点
        /// </summary>
        /// <param name="parent"></param>
        public void RemoveParent(LogicNodeBaseView parent)
        {
            this.NodeData.Parents.Remove(parent.Target.OnlyId);
            this.Target.Parents.Remove(parent.Target);
        }
        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(LogicNodeBaseView child)
        {
            this.NodeData.Childs.Add(child.Target.OnlyId);
            this.Target.Childs.Add(child.Target);
        }
        /// <summary>
        /// 移除子节点
        /// </summary>
        /// <param name="child"></param>
        public void RemoveChild(LogicNodeBaseView child)
        {
            this.NodeData.Childs.Remove(child.Target.OnlyId);
            this.Target.Childs.Remove(child.Target);
        }
        #endregion

        #region 虚方法
        /// <summary>
        /// 当节点被创建时调用
        /// </summary>
        public virtual void OnCreate()
        {
        }
        /// <summary>
        /// 可以接受那种节点连接进来
        /// </summary>
        /// <param name="waitLinkNode">准备连接的节点</param>
        /// <returns></returns>
        public virtual bool CanAcceptLink(LogicNodeBaseView waitLinkNode)
        {
            return true;
        }
        /// <summary>
        /// 获取到端口配置
        /// </summary>
        /// <returns></returns>
        protected virtual PortConfig GetPortConfig()
        {
            return PortConfig.Default;
        }
        /// <summary>
        /// 显示UI
        /// </summary>
        public virtual void ShowUI()
        {

        }

        #endregion

        #region 子类方法
        /// <summary>
        /// 添加一个UI元素到节点视图中
        /// </summary>
        /// <param name="ui"></param>
        protected void AddUI(VisualElement ui)
        {
            m_content.Add(ui);
        }
        protected Label GetLabel(string defaultValue = "")
        {
            Label label = new Label();
            label.text = defaultValue;
            return label;
        }
        protected TextField GetTextField(string titleText = "TestField", string defaultValue = "")
        {
            TextField textField = new TextField();
            textField.label = titleText;
            SetBaseFieldStyle(textField);
            textField.multiline = true;
            textField.value = defaultValue;
            return textField;
        }
        protected IntegerField GetIntergerField(string titleText = "IntField", int defaultValue = 0)
        {
            IntegerField intField = new IntegerField();
            intField.label = titleText;
            SetBaseFieldStyle(intField);
            intField.value = defaultValue;
            return intField;
        }
        /// <summary>
        /// 设置字段组件的默认样式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        protected void SetBaseFieldStyle<T>(BaseField<T> element)
        {
            element.style.minHeight = 24;
            element.style.marginTop = 2;
            element.style.marginRight = 2;
            element.style.marginLeft = 2;
            element.style.marginBottom = 2;
            element.labelElement.style.minWidth = 50;
            element.labelElement.style.fontSize = 12;
        }
        #endregion

        #region 右键

        /// <summary>
        /// 绑定右键
        /// </summary>
        /// <param name="evt"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!this.selected || this._panelData.LogicPanelGraphView.selection.Count > 1)
            {
                return;
            }
            var graphView = _panelData.LogicPanelGraphView;
            for (int i = 0; i < NodeData.Parents.Count; i++)
            {
                var item = NodeData.Parents[i];
                var nodeView = graphView.AllNodeViews[item];
                evt.menu.AppendAction($"断开连接:{i + 1}.{nodeView.title}", onDisconnectParent, (a) => DropdownMenuAction.Status.Normal, item);
            }
            if (NodeData.Parents.Count > 0)
            {
                evt.menu.AppendSeparator();
            }
            for (int i = 0; i < NodeData.Childs.Count; i++)
            {
                var item = NodeData.Childs[i];
                var nodeView = graphView.AllNodeViews[item];
                evt.menu.AppendAction($"移除连接:{i + 1}.{nodeView.title}", onRemoveParent, (a) => DropdownMenuAction.Status.Normal, item);
            }
            if (NodeData.Childs.Count > 0)
            {
                evt.menu.AppendSeparator();
            }
            evt.menu.AppendAction("查看节点代码", onOpenNodeScript);
            evt.menu.AppendAction("查看界面代码", onOpenNodeViewScript);
        }
        /// <summary>
        /// 移除子节点
        /// </summary>
        /// <param name="obj"></param>
        private void onRemoveParent(DropdownMenuAction obj)
        {
            string removeId = obj.userData.ToString();

            if (_panelData.LogicPanelGraphView.AllNodeViews.ContainsKey(removeId))
            {
                LogicNodeBaseView view = _panelData.LogicPanelGraphView.AllNodeViews[removeId];
                Edge edge = this.Output.connections.FirstOrDefault(a => a.input.node == view);
                this._panelData.LogicPanelGraphView.DeleteElements(new Edge[] { edge });
                _panelData.Save();
            }
        }

        /// <summary>
        /// 断开父节点
        /// </summary>
        /// <param name="obj"></param>
        private void onDisconnectParent(DropdownMenuAction obj)
        {
            string disconnectId = obj.userData.ToString();

            if (_panelData.LogicPanelGraphView.AllNodeViews.ContainsKey(disconnectId))
            {
                LogicNodeBaseView view = _panelData.LogicPanelGraphView.AllNodeViews[disconnectId];
                Edge edge = this.Input.connections.FirstOrDefault(a => a.input.node == view);
                this._panelData.LogicPanelGraphView.DeleteElements(new Edge[] { edge });
                _panelData.Save();
            }
        }
        /// <summary>
        /// 查看节点代码
        /// </summary>
        /// <param name="obj"></param>
        private void onOpenNodeScript(DropdownMenuAction obj)
        {
            string[] guids = AssetDatabase.FindAssets(this.Target.GetType().Name);
            foreach (var item in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(item);
                if (Path.GetFileNameWithoutExtension(path) == this.Target.GetType().Name)
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), -1);
                    break;
                }
            }
        }
        /// <summary>
        /// 查看界面代码
        /// </summary>
        /// <param name="obj"></param>
        private void onOpenNodeViewScript(DropdownMenuAction obj)
        {
            string[] guids = AssetDatabase.FindAssets(this.GetType().Name);
            foreach (var item in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(item);
                if (Path.GetFileNameWithoutExtension(path) == this.GetType().Name)
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object)), -1);
                    break;
                }
            }
        }

        #endregion

        #region Title

        private TextField _titleEditor;
        private Label _titleItem;
        private bool _editTitleCancelled = false;
        private void CheckTitle()
        {
            //找到Title对应的元素
            _titleItem = this.Q<Label>("title-label");
            _titleItem.style.flexGrow = 1;
            _titleItem.style.marginRight = 6;
            _titleItem.style.unityTextAlign = TextAnchor.MiddleCenter;
            _titleItem.RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
            _titleEditor = new TextField();
            _titleItem.parent.Add(_titleEditor);
            _titleEditor.style.flexGrow = 1;
            _titleEditor.style.marginRight = 6;
            _titleEditor.style.unityTextAlign = TextAnchor.MiddleCenter;
            _titleEditor.style.display = DisplayStyle.None;
            VisualElement visualElement2 = _titleEditor.Q(TextInputBaseField<string>.textInputUssName);
            visualElement2.RegisterCallback<FocusOutEvent>(OnEditTitleFinished);
            visualElement2.RegisterCallback<KeyDownEvent>(OnTitleEditorOnKeyDown);
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
                this.title = _titleEditor.text;
                this.NodeData.Title = this.title;
            }
            _editTitleCancelled = true;
        }

        private void OnMouseDownEvent(MouseDownEvent evt)
        {
            if (evt.clickCount == 2 && evt.button == 0)
            {
                _titleItem.style.display = DisplayStyle.None;
                _titleEditor.style.display = DisplayStyle.Flex;
                _titleEditor.value = _titleItem.text;
                _titleEditor.SelectAll();
                _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Focus();
                _editTitleCancelled = false;
            }
        }

        #endregion

        #region 类
        /// <summary>
        /// 端口配置文件
        /// </summary>
        public class PortConfig
        {
            /// <summary>
            /// 存在什么样的端口
            /// </summary>
            public PortEnum PortType = PortEnum.All;
            /// <summary>
            /// 输入端口的颜色
            /// </summary>
            public Color InColor = Color.blue;
            /// <summary>
            /// 输出端口的颜色
            /// </summary>
            public Color OutColor = Color.red;

            public static PortConfig Default => new PortConfig();
        }
        /// <summary>
        /// 端口枚举
        /// </summary>
        [Flags]
        public enum PortEnum : byte
        {
            None = 0,
            /// <summary>
            /// 只有进
            /// </summary>
            In = 1,
            /// <summary>
            /// 只有出
            /// </summary>
            Out = 2,
            /// <summary>
            /// 二者皆有
            /// </summary>
            All = In | Out
        } 
        #endregion
    }
}
