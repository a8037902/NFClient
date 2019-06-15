using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFSDK
{
    public abstract class BaseState
    {
        public abstract void Execute(NFStateModule stateModule);
    }
}
