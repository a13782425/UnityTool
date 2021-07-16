using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;

namespace Logic.Editor
{
    /// <summary>
    /// 端口数据
    /// 每个端口都必须要有一个
    /// </summary>
    internal interface IPortData
    {
        /// <summary>
        /// 当前端口对应的节点数据
        /// </summary>
        LogicNodeBase Target { get; }
        /// <summary>
        /// 当前数据对应的端口
        /// </summary>
        Port Port { get; }
        /// <summary>
        /// 是否可接受连接
        /// </summary>
        /// <param name="waitLinkNode">等待连接端口数据</param>
        /// <returns></returns>
        bool CanAcceptLink(IPortData waitLinkPort);

        /// <summary>
        /// 是否可连接
        /// </summary>
        /// <param name="targetPort">目标端口数据</param>
        /// <returns></returns>
        bool CanLink(IPortData targetPort);
    }
}
