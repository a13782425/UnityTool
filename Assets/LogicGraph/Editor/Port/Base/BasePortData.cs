
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;

namespace Logic.Editor
{
    public abstract class BasePortData : IPortData
    {
        private LogicNodeBase _target;

        public LogicNodeBase Target => _target;
        private Port _port;

        public Port Port => _port;

        private void m_setData(LogicNodeBase target,Port port)
        {
            _target = target;
            _port = port;
        }

        /// <summary>
        /// 是否可接受连接
        /// </summary>
        /// <param name="waitLinkNode">等待连接端口数据</param>
        /// <returns></returns>
        public virtual bool CanAcceptLink(BasePortData waitLinkPort) { return true; }

        bool IPortData.CanAcceptLink(IPortData waitLinkPort)
        {
            return this.CanAcceptLink((BasePortData)waitLinkPort);
        }

        /// <summary>
        /// 可连接
        /// </summary>
        /// <param name="targetPort">目标端口数据</param>
        /// <returns></returns>
        public virtual bool CanLink(BasePortData targetPort) { return true; }

        bool IPortData.CanLink(IPortData targetPort)
        {
            return this.CanLink((BasePortData)targetPort);
        }
    }
}
