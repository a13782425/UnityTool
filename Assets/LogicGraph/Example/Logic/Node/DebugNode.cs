using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public class DebugNode : LogicNodeBase
    {
        /// <summary>
        /// 日志
        /// </summary>
        public string Log = "";

        public override bool OnExecute()
        {
            Debug.LogError(Log);
            return base.OnExecute();
        }
    }
}
