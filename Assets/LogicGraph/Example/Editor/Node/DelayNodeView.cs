using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UIElements;

namespace Logic.Editor
{

    [LogicNode(typeof(DelayNode), "系统/延时", typeof(DefaultLogicGraph))]
    public class DelayNodeView : LogicNodeBaseView
    {
        private DelayNode _node;
        public override void OnCreate()
        {
            _node = Target as DelayNode;
        }

        public override void ShowUI()
        {
            //显示UI
            var floatField = GetInputField("延时:", _node.time);
            floatField.RegisterCallback<InputEvent>(onInputEvent);
            this.AddUI(floatField);
            //请使用this.AddUI(UIElement)添加UI
        }

        private void onInputEvent(InputEvent evt)
        {
            _node.time = float.Parse(evt.newData);
        }
    }
}
