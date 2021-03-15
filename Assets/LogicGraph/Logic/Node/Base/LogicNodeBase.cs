using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
#endif
namespace Logic
{
    /// <summary>
    /// 逻辑节点基类
    /// </summary>
    [Serializable]
    public abstract class LogicNodeBase : ScriptableObject, ILogicNode
    {
        [SerializeField]
        private string _onlyId = "";
        public string OnlyId => _onlyId;
        /// <summary>
        /// 父节点
        /// </summary>
        [SerializeField]
        private List<LogicNodeBase> _parents = new List<LogicNodeBase>();
        /// <summary>
        /// 父节点
        /// </summary>
        public List<LogicNodeBase> Parents => _parents;
        /// <summary>
        /// 子节点
        /// </summary>
        [SerializeField]
        private List<LogicNodeBase> _childs = new List<LogicNodeBase>();
        /// <summary>
        /// 子节点
        /// </summary>
        public List<LogicNodeBase> Childs => _childs;
        /// <summary>
        /// 是否执行完毕
        /// </summary>
        public bool IsComplete { get; protected set; }

        /// <summary>
        /// 是否跳过子节点
        /// </summary>
        public bool IsSkip { get; protected set; }

        public LogicNodeBase()
        {
            _onlyId = Guid.NewGuid().ToString();
        }
        /// <summary>
        /// 节点初始化
        /// </summary>
        /// <returns></returns>
        public virtual bool Init()
        {
            IsComplete = false;
            IsSkip = false;
            return true;
        }
        /// <summary>
        /// 进入节点
        /// </summary>
        /// <returns></returns>
        public virtual bool OnEnter()
        {
            IsComplete = false;
            return true;
        }
        /// <summary>
        /// 节点执行
        /// </summary>
        /// <returns></returns>
        public virtual bool OnExecute()
        {
            IsComplete = true;
            return true;
        }
        /// <summary>
        /// 节点停止调用
        /// </summary>
        /// <returns></returns>
        public virtual bool OnStop()
        {
            return true;
        }

    }
}
