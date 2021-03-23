using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using static Logic.LogicGraphData;

namespace Logic.Editor
{

    [LogicNode(typeof(DebugNode), "系统/日志")]
    public class DebugNodeView : LogicNodeBaseView
    {
        private DebugNode node;


        public override void OnCreate()
        {
            Width = 200;
            node = Target as DebugNode;
        }
        public override void ShowUI()
        {
            var text = GetInputField("日志:", node.Log);
            text.RegisterCallback<InputEvent>(onInputEvent);
            this.AddUI(text);
        }

        private void onInputEvent(InputEvent evt)
        {
            node.Log = evt.newData;
        }
    }
}
