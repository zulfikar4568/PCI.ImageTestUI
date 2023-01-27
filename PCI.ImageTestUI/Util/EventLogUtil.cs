using PCI.ImageTestUI.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.ImageTestUI.Util
{
    public class EventLogUtil
    {
        public static EventLog EventLogRef;
        public static int TraceLevel = int.Parse(ConfigurationManager.AppSettings["TraceLevel"]);
        public static string LastLog;
        public static string LastLogError;

        private static string sEventSource = AppSettings.AssemblyName + "Source";
        private static string sEventLog = AppSettings.AssemblyName + "Log";
        public static void InitEventLog()
        {
            EventLogRef = new EventLog();

            foreach (EventLog oEventLog in EventLog.GetEventLogs())
            {
                if (sEventLog.Substring(0, 3).ToUpper() == oEventLog.Log.Substring(0, 3).ToUpper())
                {
                    sEventLog = oEventLog.Log;
                    break;
                }
            }
            if (!EventLog.SourceExists(sEventSource))
            {
                EventLog.CreateEventSource(sEventSource, sEventLog);
            }
        }
        public static void LogEvent(string EventMessage, EventLogEntryType EventType = EventLogEntryType.Information, int _EventId = 0)
        {
            if (EventLogRef is null) InitEventLog();
            EventLogRef.Source = sEventSource;
            EventLogRef.Log = sEventLog;
            if (_EventId <= TraceLevel)
            {
                EventLogRef.WriteEntry(EventMessage, EventType, _EventId);
                LastLog = EventMessage;
            }
        }
        public static void LogErrorEvent(string Location, Exception Ex, int _Event_Id = 0)
        {
            if (EventLogRef is null) InitEventLog();
            EventLogRef.Source = sEventSource;
            EventLogRef.Log = sEventLog;
            LastLogError = "Error Location: " + Location + "\r\n" + "Error Source: " + Ex.Source + "\r\n" + "Error Message: " + Ex.Message;
            EventLogRef.WriteEntry(LastLogError, EventLogEntryType.Error, _Event_Id);
        }
        public static void LogErrorEvent(string Location, string ExceptionMsg, int _Event_Id = 0)
        {
            if (EventLogRef is null) InitEventLog();
            EventLogRef.Source = sEventSource;
            EventLogRef.Log = sEventLog;
            LastLogError = "Error Location: " + Location + "\r\n" + "Error Source: " + Location + "\r\n" + "Error Message: " + ExceptionMsg;
            EventLogRef.WriteEntry(LastLogError, EventLogEntryType.Error, _Event_Id);
        }
    }
}
