using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    /// <summary>
    /// 逻辑图运行时
    /// 负责逻辑图执行操作
    /// 也可以是Manager,此类仅供参考
    /// </summary>
    public class LogicRuntime : MonoBehaviour
    {
        private Dictionary<int, LogicDto> _logicDtoDic = new Dictionary<int, LogicDto>();
        private Dictionary<int, LogicDto> _waitBeginLogicDic = new Dictionary<int, LogicDto>();
        private int _onlyId = int.MinValue;
        private List<int> _waitRemoveList = new List<int>();
        private static LogicRuntime _instance = null;
        public static LogicRuntime Instance => _instance;

        /// <summary>
        /// 开始一个逻辑图
        /// </summary>
        /// <param name="logic"></param>
        /// <returns>逻辑图执行的Id</returns>
        public int Begin(LogicGraphBase logic)
        {
            return Begin(logic, null);
        }

        /// <summary>
        /// 开始一个逻辑图
        /// </summary>
        /// <param name="logic">逻辑图</param>
        /// <param name="finishCallback">逻辑图执行完毕回调</param>
        /// <returns>逻辑图执行的Id</returns>
        public int Begin(LogicGraphBase logic, Action finishCallback)
        {
            if (logic == null)
            {
                throw new NullReferenceException("LogicBox is null");
            }
            LogicDto logicDto = new LogicDto(logic, finishCallback);
            _waitBeginLogicDic.Add(logicDto.OnlyId, logicDto);
            return logicDto.OnlyId;
        }
        /// <summary>
        /// 停止一个逻辑图
        /// </summary>
        /// <param name="logicId">逻辑图执行Id</param>
        public void Stop(int logicId)
        {
            if (_logicDtoDic.ContainsKey(logicId))
            {
                _logicDtoDic[logicId].Stop();
                _waitRemoveList.Add(logicId);
            }
        }
        /// <summary>
        /// 停止当前所有正在执行的逻辑图
        /// </summary>
        public void StopAll()
        {
            _waitBeginLogicDic.Clear();
            foreach (var item in _logicDtoDic)
            {
                item.Value.Stop();
                _waitRemoveList.Add(item.Key);
            }
        }
        private void Remove()
        {
            foreach (var item in _waitRemoveList)
            {
                if (_logicDtoDic.ContainsKey(item))
                {
                    _logicDtoDic.Remove(item);
                }
                if (_waitBeginLogicDic.ContainsKey(item))
                {
                    _waitBeginLogicDic.Remove(item);
                }
            }
            _waitRemoveList.Clear();
        }
        private void RemoveLogicDto(LogicDto logicDto)
        {
            _waitRemoveList.Add(logicDto.OnlyId);
        }

        private void Update()
        {
            try
            {
                foreach (var item in _waitBeginLogicDic)
                {
                    item.Value.Begin();
                    _logicDtoDic.Add(item.Key, item.Value);
                }
                _waitBeginLogicDic.Clear();
                Remove();
                foreach (var item in _logicDtoDic)
                {
                    item.Value.Update();
                }
                Remove();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
        private void Awake()
        {
            _instance = this;
        }

        private int GetOnlyId()
        {
            return _onlyId++;
        }



        private class LogicDto
        {
            private LogicGraphBase _logicGraph;
            public LogicGraphBase CurrentLogic { get { return _logicGraph; } }
            private Action _onFinish;
            public int OnlyId { get; private set; }
            public LogicDto(LogicGraphBase logic) : this(logic, null)
            {
            }

            public LogicDto(LogicGraphBase logic, Action finishCallback)
            {
                _logicGraph = logic;
                if (finishCallback != null)
                {
                    _onFinish += finishCallback;
                }
                OnlyId = LogicRuntime.Instance.GetOnlyId();
                _onFinish += onFinish;
            }

            private void onFinish()
            {
                Debug.LogError("逻辑图:" + _logicGraph.name + ",执行完毕");
                LogicRuntime.Instance.RemoveLogicDto(this);
            }

            internal void Begin()
            {
                _logicGraph.Begin(_onFinish);
            }

            internal void Update()
            {
                _logicGraph.Update();
            }

            internal void Stop()
            {
                _logicGraph.Stop();
            }
        }
    }
}
