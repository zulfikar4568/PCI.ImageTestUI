using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.ImageTestUI.Util
{
    public class Logging
    {
        internal static string LoggingContainer(string Container, string TxnId, string Message = "")
        {
            return $"Container: {Container}, LogId: {TxnId}, Message: {Message}";
        }
    }
}
