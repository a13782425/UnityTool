using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Editor
{
    public sealed class InPortData : BasePortData
    {
        public event Func<LogicNodeBase, bool> onCanAcceptLink;

        public override bool CanAcceptLink(BasePortData waitLinkPort)
        {
            if (onCanAcceptLink != null)
            {
                onCanAcceptLink.Invoke(waitLinkPort.Target);
            }
            return true;
        }

        public override bool CanLink(BasePortData targetPort)
        {
            return false;
        }
    }
}
