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

        private static Action<BasePortData, LogicNodeBase, Port> onSetPortTarget;
        #endregion

        static LogicNodeBaseView()
        {
            System.Reflection.MethodInfo method = typeof(BasePortData).GetMethod("m_setData", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            onSetPortTarget = (Action<BasePortData, LogicNodeBase, Port>)Delegate.CreateDelegate(typeof(Action<BasePortData, LogicNodeBase, Port>), null, method);
        }

        public LogicNodeBaseView()
        {
            //this.title = this.GetType().Name;
            //移除右上角折叠按钮
            this.titleContainer.Remove(titleButtonContainer);
            topContainer.style.height = 24;
            m_content = topContainer.parent;
            m_content.style.backgroundColor = new Color(0, 0, 0, 0.5f);
            m_checkTitle();

        }


        #region 公共方法
        public void Initialize(LogicPanelData panelData, LogicNodeData nodeData)
        {
            _nodeData = nodeData;
            _panelData = panelData;
            OnlyId = _nodeData.OnlyId;
            m_addPort();
            RefreshExpandedState();
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
        /// 是否可接受连接
        /// </summary>
        /// <param name="waitLinkNode">准备连接的节点</param>
        /// <returns></returns>
        public virtual bool CanAcceptLink(LogicNodeBase waitLinkNode) => true;
        /// <summary>
        /// 可连接
        /// </summary>
        /// <param name="targetPort">目标端口数据</param>
        /// <returns></returns>
        public virtual bool CanLink(LogicNodeBase targetPort) => true;
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

        /// <summary>
        /// 获得一个端口
        /// </summary>
        /// <returns></returns>
        protected T GetPort<T>(string titleText, Color color, Direction dir = Direction.Output) where T : BasePortData, new()
        {
            var port = Port.Create<Edge>(Orientation.Horizontal, dir, Port.Capacity.Multi, typeof(Port));
            port.portColor = color;
            port.portName = titleText;
            T portData = new T();
            onSetPortTarget(portData, this.Target, port);
            port.userData = portData;
            return portData;
        }

        protected Label GetLabel(string defaultValue = "")
        {
            Label label = new Label();
            label.text = defaultValue;
            return label;
        }

        protected TextField GetInputField(string titleText, string defaultValue)
        {
            TextField textField = new TextField();
            textField.label = titleText;
            SetBaseFieldStyle(textField);
            textField.multiline = true;
            textField.value = defaultValue;
            return textField;
        }
        protected IntegerField GetInputField(string titleText, int defaultValue)
        {
            IntegerField intField = new IntegerField();
            intField.label = titleText;
            SetBaseFieldStyle(intField);
            intField.value = defaultValue;
            return intField;
        }
        protected FloatField GetInputField(string titleText, float defaultValue)
        {
            FloatField floatField = new FloatField();
            floatField.label = titleText;
            SetBaseFieldStyle(floatField);
            floatField.value = defaultValue;
            return floatField;
        }
        protected DoubleField GetInputField(string titleText, double defaultValue)
        {
            DoubleField doubleField = new DoubleField();
            doubleField.label = titleText;
            SetBaseFieldStyle(doubleField);
            doubleField.value = defaultValue;
            return doubleField;
        }
        protected EnumField GetInputField(string titleText, Enum defaultValue)
        {
            EnumField enumField = new EnumField();
            enumField.Init(defaultValue);
            enumField.label = titleText;
            SetBaseFieldStyle(enumField);
            enumField.value = defaultValue;
            return enumField;
        }
        protected Vector2Field GetInputField(string titleText, Vector2 defaultValue)
        {
            Vector2Field vector2Field = new Vector2Field();
            vector2Field.label = titleText;
            SetBaseFieldStyle(vector2Field);

            vector2Field.value = defaultValue;
            return vector2Field;
        }
        protected Vector3Field GetInputField(string titleText, Vector3 defaultValue)
        {
            Vector3Field vector3Field = new Vector3Field();
            vector3Field.label = titleText;
            SetBaseFieldStyle(vector3Field);
            vector3Field.value = defaultValue;
            return vector3Field;
        }
        protected Vector4Field GetInputField(string titleText, Vector4 defaultValue)
        {
            Vector4Field vector4Field = new Vector4Field();
            vector4Field.label = titleText;
            SetBaseFieldStyle(vector4Field);
            vector4Field.value = defaultValue;
            return vector4Field;
        }
        protected Vector2IntField GetInputField(string titleText, Vector2Int defaultValue)
        {
            Vector2IntField vector2IntField = new Vector2IntField();
            vector2IntField.label = titleText;
            SetBaseFieldStyle(vector2IntField);
            vector2IntField.value = defaultValue;
            return vector2IntField;
        }
        protected Vector3IntField GetInputField(string titleText, Vector3Int defaultValue)
        {
            Vector3IntField vector3IntField = new Vector3IntField();
            vector3IntField.label = titleText;
            SetBaseFieldStyle(vector3IntField);
            vector3IntField.value = defaultValue;
            return vector3IntField;
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

        #region 私有方法
        /// <summary>
        /// 添加端口
        /// </summary>
        private void m_addPort()
        {
            PortConfig portConfig = GetPortConfig();
            if ((portConfig.PortType & PortEnum.In) > PortEnum.None)
            {
                //存在进端口
                Input = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Port));
                Input.portColor = Color.blue;
                Input.portName = "In";
                var portData = new InPortData();
                onSetPortTarget(portData, this.Target, Input);
                portData.onCanAcceptLink += CanAcceptLink;
                Input.userData = portData;
                inputContainer.Add(Input);
            }
            if ((portConfig.PortType & PortEnum.Out) > PortEnum.None)
            {
                //存在出端口
                Output = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Port));
                Output.portColor = Color.red;
                Output.portName = "Out";
                var portData = new OutPortData();
                onSetPortTarget(portData, this.Target, Output);
                Output.userData = portData;
                outputContainer.Add(Output);
            }
        }
        #endregion

        #region Title

        private TextField _titleEditor;
        private Label _titleItem;
        private bool _editTitleCancelled = false;
        private void m_checkTitle()
        {
            //找到Title对应的元素
            _titleItem = this.Q<Label>("title-label");
            _titleItem.style.flexGrow = 1;
            _titleItem.style.marginRight = 6;
            _titleItem.style.unityTextAlign = TextAnchor.MiddleCenter;
            _titleItem.RegisterCallback<MouseDownEvent>(m_onMouseDownEvent);
            _titleEditor = new TextField();
            _titleItem.parent.Add(_titleEditor);
            _titleEditor.style.flexGrow = 1;
            _titleEditor.style.marginRight = 6;
            _titleEditor.style.unityTextAlign = TextAnchor.MiddleCenter;
            _titleEditor.style.display = DisplayStyle.None;
            VisualElement visualElement2 = _titleEditor.Q(TextInputBaseField<string>.textInputUssName);
            visualElement2.RegisterCallback<FocusOutEvent>(m_onEditTitleFinished);
            visualElement2.RegisterCallback<KeyDownEvent>(m_onTitleEditorOnKeyDown);
        }
        private void m_onTitleEditorOnKeyDown(KeyDownEvent evt)
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

        private void m_onEditTitleFinished(FocusOutEvent evt)
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

        private void m_onMouseDownEvent(MouseDownEvent evt)
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
