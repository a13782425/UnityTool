using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Logic
{
    /// <summary>
    /// 逻辑图的一些信息
    /// </summary>
    public sealed class LogicGraphData : ScriptableObject
    {

        public string Title;

        public List<LogicGroupData> LogicGroups = new List<LogicGroupData>();

        public List<LogicNodeData> LogicNodes = new List<LogicNodeData>();

        [Serializable]
        public class LogicGroupData
        {
            /// <summary>
            /// 模板Id
            /// </summary>
            public string TempId;
            public string Title;
            public Vector2 Pos;
            /// <summary>
            /// 保存唯一Id
            /// </summary>
            public List<string> Nodes = new List<string>();
        }

        [Serializable]
        public class LogicNodeData
        {
            /// <summary>
            /// 唯一Id
            /// </summary>
            public string OnlyId;
            public string Title;
            public Vector2 Pos;
            public List<string> Parents = new List<string>();
            public List<string> Childs = new List<string>();

            [NonSerialized]
            public LogicNodeBase BelongNode;
        }
    }
}
