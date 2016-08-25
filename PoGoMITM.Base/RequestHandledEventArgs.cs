using System;
using PoGoMITM.Base.Models;

namespace PoGoMITM.Base
{
    public class RequestHandledEventArgs : EventArgs
    {
        public ulong RequestId { get; set; }
        public MessageBlock RequestBlock { get; set; }
        public MessageBlock ResponseBlock { get; set; }
    }
}
