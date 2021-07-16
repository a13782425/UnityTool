using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Editor
{
    public sealed class OutPortData : BasePortData
    {
        public event Func<LogicNodeBase, bool> onCanLink;


        public override bool CanLink(BasePortData targetPort)
        {
            if (onCanLink != null)
            {
                onCanLink.Invoke(targetPort.Target);
            }
            return true;
        }
        public override bool CanAcceptLink(BasePortData waitLinkPort)
        {
            return false;
        }
    }
}
