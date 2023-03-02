using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.ImageTestUI.Entity
{
    public class StatusMainLogic
    {
        public StatusEnum Status { get; set; }
        public string Message { get; set; }
    }
    public enum StatusEnum
    {
        Error,
        InProgress,
        Done
    }
}
