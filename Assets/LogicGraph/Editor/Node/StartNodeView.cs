using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    [LogicNode(typeof(StartNode), "系统/开始")]
    public class StartNodeView : LogicNodeBaseView
    {
        protected override PortConfig GetPortConfig()
        {
            PortConfig config = base.GetPortConfig();
            config.PortType = PortEnum.Out;
            return config;
        }

        public override void ShowUI()
        {
            Label label = GetLabel("开始");
            label.style.fontSize = 40;
            label.style.color = Color.black;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.paddingBottom = 6;
            label.style.paddingRight = 6;
            label.style.paddingLeft = 6;
            label.style.paddingBottom = 6;
            label.style.backgroundColor = Color.green;
            this.AddUI(label);
        }
    }
}
