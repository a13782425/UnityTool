using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;



namespace Logic.Editor
{
    public class LogicEditorConfigData : ScriptableObject
    {
        /// <summary>
        /// 当前项目中的逻辑图
        /// </summary>
        [SerializeField]
        public List<LogicGraphSummary> LogicGraphs = new List<LogicGraphSummary>();
        /// <summary>
        /// 当前项目中的逻辑图配置
        /// </summary>
        [SerializeField]
        public List<LogicGraphConfig> LogicGraphConifgs = new List<LogicGraphConfig>();

        /// <summary>
        /// 获取一个逻辑图数据
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public LogicGraphSummary GetGraphSummary(Func<LogicGraphSummary, bool> predicate)
        {
            return LogicGraphs.FirstOrDefault(predicate);
        }

        /// <summary>
        /// 逻辑图摘要
        /// </summary>
        [Serializable]
        public class LogicGraphSummary
        {
            /// <summary>
            /// 逻辑图名
            /// </summary>
            public string LogicName;
            /// <summary>
            /// 图类型名全称,含命名空间
            /// </summary>
            public string GraphClassName;
            /// <summary>
            /// 文件名
            /// </summary>
            public string FileName;
            /// <summary>
            /// 资源路径
            /// </summary>
            public string AssetPath;
        }

        /// <summary>
        /// 逻辑图配置信息
        /// </summary>
        [Serializable]
        public class LogicGraphConfig
        {
            /// <summary>
            /// 图名
            /// </summary>
            public string GraphName;
            /// <summary>
            /// 图类型名全称,含命名空间
            /// </summary>
            public string GraphClassName;
            /// <summary>
            /// 是否刷新过
            /// </summary>
            public bool IsRefresh;
            /// <summary>
            /// 当前逻辑图所对应的节点
            /// </summary>
            public List<LogicNodeConfig> LogicNodes = new List<LogicNodeConfig>();
            /// <summary>
            /// 当前项目中的组配置
            /// </summary>
            public List<LogicGroupConfig> LogicGroups = new List<LogicGroupConfig>();
            /// <summary>
            /// 当前逻辑图适用的格式化
            /// </summary>
            public List<LogicFormatConfig> LogicFormats = new List<LogicFormatConfig>();
            private Type _curType = null;
            /// <summary>
            /// 获取逻辑图的类型
            /// </summary>
            /// <returns></returns>
            public Type GetGraphType()
            {
                if (_curType == null)
                {
                    _curType = typeof(LogicNodeBase).Assembly.GetType(GraphClassName);
                    if (_curType == null)
                    {
                        _curType = typeof(LogicPanel).Assembly.GetType(GraphClassName);
                    }
                }
                return _curType;
            }
        }
        /// <summary>
        /// 逻辑图格式化
        /// </summary>
        [Serializable]
        public class LogicFormatConfig
        {
            /// <summary>
            /// 格式化名
            /// </summary>
            public string FormatName;
            /// <summary>
            /// 格式化类型名全称,含命名空间
            /// </summary>
            public string FormatClassName;
            /// <summary>
            /// 格式化后缀
            /// </summary>
            public string Extension;
            private Type _curType = null;
            public Type GetFormatType()
            {
                if (_curType == null)
                {
                    _curType = typeof(LogicNodeBase).Assembly.GetType(FormatClassName);
                    if (_curType == null)
                    {
                        _curType = typeof(LogicPanel).Assembly.GetType(FormatClassName);
                    }
                }
                return _curType;
            }
        }
        /// <summary>
        /// 逻辑图组信息
        /// </summary>
        [Serializable]
        public class LogicGroupConfig
        {
            /// <summary>
            /// 组名
            /// </summary>
            public string GroupName;
            /// <summary>
            /// 逻辑图组下的节点信息
            /// </summary>
            public List<LogicGroupNodeConfig> NodeConfigs = new List<LogicGroupNodeConfig>();
            /// <summary>
            /// 使用次数,用于做使用计数
            /// </summary>
            public int UseCount = int.MinValue;
            /// <summary>
            /// 模板ID
            /// </summary>
            public string TempId;
            public void AddUseCount()
            {
                if (UseCount == int.MaxValue)
                {
                    UseCount = int.MaxValue;
                }
                UseCount++;
            }
            public void SubUseCount()
            {
                if (UseCount == int.MinValue)
                {
                    UseCount = int.MinValue;
                }
                UseCount--;
            }
        }

        /// <summary>
        /// 组节点配置信息
        /// </summary>
        [Serializable]
        public class LogicGroupNodeConfig
        {
            /// <summary>
            /// 唯一ID
            /// 如果按组保存成模板,则需要复刻齐连线
            /// </summary>
            public string OnlyId;
            /// <summary>
            /// 节点名
            /// </summary>
            public string NodeName;
            /// <summary>
            /// 节点类型全称,含命名空间
            /// </summary>
            public string NodeClassName;
            /// <summary>
            /// 当前节点所在的坐标
            /// </summary>
            public Vector2 Pos;
            /// <summary>
            /// 节点视图类型全称,含命名空间
            /// </summary>
            public string NodeViewClassName;
            /// <summary>
            /// 父节点
            /// </summary>
            public List<string> Parents = new List<string>();
            /// <summary>
            /// 子节点
            /// </summary>
            public List<string> Childs = new List<string>();

            private Type _curType = null;
            /// <summary>
            /// 获取节点类型
            /// </summary>
            /// <returns></returns>
            public Type GetNodeType()
            {
                if (_curType == null)
                {
                    _curType = typeof(LogicNodeBase).Assembly.GetType(NodeClassName);
                    if (_curType == null)
                    {
                        _curType = typeof(LogicPanel).Assembly.GetType(NodeClassName);
                    }
                }
                return _curType;
            }
            private Type _curViewType = null;
            /// <summary>
            /// 获取节点视图类型
            /// </summary>
            /// <returns></returns>
            public Type GetNodeViewType()
            {
                if (_curViewType == null)
                {
                    _curViewType = typeof(LogicNodeBase).Assembly.GetType(NodeViewClassName);
                    if (_curViewType == null)
                    {
                        _curViewType = typeof(LogicPanel).Assembly.GetType(NodeViewClassName);
                    }
                }
                return _curViewType;
            }
        }
        /// <summary>
        /// 逻辑图节点配置信息
        /// </summary>
        [Serializable]
        public class LogicNodeConfig
        {
            /// <summary>
            /// 节点名
            /// </summary>
            public string NodeName;
            /// <summary>
            /// 节点全名
            /// </summary>
            public string NodeFullName;
            /// <summary>
            /// 节点层级
            /// </summary>
            public string[] NodeLayers;
            /// <summary>
            /// 节点类型全称,含命名空间
            /// </summary>
            public string NodeClassName;
            /// <summary>
            /// 节点视图类型全称,含命名空间
            /// </summary>
            public string NodeViewClassName;
            /// <summary>
            /// 是否刷新过
            /// </summary>
            public bool IsRefresh;
            /// <summary>
            /// 使用次数,用于做使用计数
            /// </summary>
            public int UseCount = int.MinValue;
            private Type _curType = null;
            /// <summary>
            /// 获取节点类型
            /// </summary>
            /// <returns></returns>
            public Type GetNodeType()
            {
                if (_curType == null)
                {
                    _curType = typeof(LogicNodeBase).Assembly.GetType(NodeClassName);
                    if (_curType == null)
                    {
                        _curType = typeof(LogicPanel).Assembly.GetType(NodeClassName);
                    }
                }
                return _curType;
            }
            private Type _curViewType = null;
            /// <summary>
            /// 获取节点视图类型
            /// </summary>
            /// <returns></returns>
            public Type GetNodeViewType()
            {
                if (_curViewType == null)
                {
                    _curViewType = typeof(LogicNodeBase).Assembly.GetType(NodeViewClassName);
                    if (_curViewType == null)
                    {
                        _curViewType = typeof(LogicPanel).Assembly.GetType(NodeViewClassName);
                    }
                }
                return _curViewType;
            }
            public void AddUseCount()
            {
                if (UseCount == int.MaxValue)
                {
                    UseCount = int.MaxValue;
                    return;
                }
                UseCount += 10;
            }
            public void SubUseCount()
            {
                if (UseCount == int.MinValue)
                {
                    UseCount = int.MinValue;
                    return;
                }
                UseCount -= 1;
            }
        }
    }
}
