using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFCampShared.Messages.Customer
{
    public class CustomerMessage : WorkflowMessage
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

    }


    public class CustomerResultMessage : CustomerMessage
    {
        public string Status { get; set; }
        public string? Error { get; set; }
    }
}
