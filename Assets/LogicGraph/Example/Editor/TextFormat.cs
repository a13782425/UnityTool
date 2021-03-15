using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic.Editor
{
    [LogicFormat("文本", typeof(DefaultLogicGraph))]
    public class TextFormat : ILogicFormat
    {

        public bool ToFormat(LogicGraphBase graph, string path)
        {
            Debug.LogError("导出成功");
            return true;
        }

    }
}
