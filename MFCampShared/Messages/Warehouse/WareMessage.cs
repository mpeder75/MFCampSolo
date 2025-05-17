using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFCampShared.Messages.Warehouse
{
    public class WareMessage : WorkflowMessage
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }


    public class WareResultMessage : WareMessage
    {
        public string Status {  get; set; }
    }
}
