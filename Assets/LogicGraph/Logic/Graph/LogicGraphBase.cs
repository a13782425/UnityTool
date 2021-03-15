using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public abstract class LogicGraphBase : ScriptableObject, ILogicGraph
    {
        [SerializeField]
        private List<LogicNodeBase> _logicNodeList = new List<LogicNodeBase>();
        /// <summary>
        /// 逻辑图节点
        /// </summary>
        public List<LogicNodeBase> LogicNodeList => _logicNodeList;

#if UNITY_EDITOR
        /// <summary>
        /// 逻辑图Editor数据
        /// </summary>
        public LogicGraphData LogicGraphData;

#endif

        /// <summary>
        /// 默认开始节点
        /// </summary>
        public LogicNodeBase DefaultNode;
        /// <summary>
        /// 执行结束的回调
        /// </summary>
        private Action _finishCallback;
        /// <summary>
        /// node待执行队列
        /// </summary>
        private Queue<LogicNodeBase> _waitExecuteNodes = new Queue<LogicNodeBase>();
        /// <summary>
        /// node执行队列
        /// </summary>
        private List<LogicNodeBase> _executeNodes = new List<LogicNodeBase>();
        /// <summary>
        /// 执行完毕等待删除的node
        /// </summary>
        private List<LogicNodeBase> _waitRemoveNodes = new List<LogicNodeBase>();

        /// <summary>
        /// 开始
        /// </summary>
        public void Begin()
        {
            Begin(null);
        }

        /// <summary>
        /// 开始
        /// </summary>
        public void Begin(Action finish)
        {
            if (DefaultNode == null)
            {
                Debug.LogError("没有开始节点");
                return;
            }
            if (finish != null)
            {
                _finishCallback += finish;
            }
            LogicNodeList.ForEach(x => { x.Init(); });
            _executeNodes.Clear();
            _waitRemoveNodes.Clear();
            _waitExecuteNodes.Clear();

            _waitExecuteNodes.Enqueue(DefaultNode);
        }

        /// <summary>
        /// 更新逻辑图
        /// </summary>
        public void Update()
        {
            while (_waitExecuteNodes.Count > 0)
            {
                LogicNodeBase logicNode = _waitExecuteNodes.Dequeue();
                try
                {
                    logicNode.OnExecute();
                    _executeNodes.Add(logicNode);
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    Debug.LogError("逻辑图:" + name + "中:" + logicNode.GetType().Name + "节点执行失败");
#endif
                    Debug.LogError(ex.Message);
                }
            }
            _waitRemoveNodes.Clear();
            foreach (var item in _executeNodes)
            {
                if (item.IsComplete)
                {
                    _waitRemoveNodes.Add(item);
                    if (item.IsSkip)//当前节点后续子节点需要跳过
                        continue;
                    if (item.Childs != null)
                        item.Childs.ForEach(x => { if (x != null) _waitExecuteNodes.Enqueue(x); });
                }
            }
            foreach (var item in _waitRemoveNodes)
            {
                _executeNodes.Remove(item);
            }
            _waitRemoveNodes.Clear();
            //正在执行和等待执行的节点数量,则逻辑图执行完毕
            if (_executeNodes.Count == 0 && _waitExecuteNodes.Count == 0)
            {
                Stop();
            }
        }
        /// <summary>
        /// 结束或停止逻辑图
        /// </summary>
        public void Stop()
        {
            _executeNodes.ForEach(x => x.OnStop());
            _executeNodes.Clear();
            _waitExecuteNodes.Clear();
            _waitRemoveNodes.Clear();
            if (_finishCallback != null)
            {
                _finishCallback.Invoke();
            }
            _finishCallback = null;
        }
    }
}
