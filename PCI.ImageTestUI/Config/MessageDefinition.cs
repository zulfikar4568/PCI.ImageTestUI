using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.ImageTestUI.Config
{
    internal static class MessageDefinition
    {
        public static string NoDeviceFound = ConfigurationManager.AppSettings["NoDeviceFound"] is null || ConfigurationManager.AppSettings["NoDeviceFound"] == "" ? "Camera devices not found!" : ConfigurationManager.AppSettings["NoDeviceFound"];
        public static string Waiting = ConfigurationManager.AppSettings["Waiting"] is null || ConfigurationManager.AppSettings["Waiting"] == "" ? "Please wait..." : ConfigurationManager.AppSettings["Waiting"];
        public static string SendImageSuccess = ConfigurationManager.AppSettings["SendImageSuccess"] is null || ConfigurationManager.AppSettings["SendImageSuccess"] == "" ? "Send image successfully" : ConfigurationManager.AppSettings["SendImageSuccess"];
        public static string SendImageFailed = ConfigurationManager.AppSettings["SendImageFailed"] is null || ConfigurationManager.AppSettings["SendImageFailed"] == "" ? "Send image failed!" : ConfigurationManager.AppSettings["SendImageFailed"];
        public static string CameraNotConnected = ConfigurationManager.AppSettings["CameraNotConnected"] is null || ConfigurationManager.AppSettings["CameraNotConnected"] == "" ? "Please Connect your device camera!" : ConfigurationManager.AppSettings["CameraNotConnected"];
        public static string MessageBeforeScan = ConfigurationManager.AppSettings["MessageBeforeScan"] is null || ConfigurationManager.AppSettings["MessageBeforeScan"] == "" ? "Please scan the Serial Number of Product!" : ConfigurationManager.AppSettings["MessageBeforeScan"];
        public static string ProductNotFound = ConfigurationManager.AppSettings["ProductNotFound"] is null || ConfigurationManager.AppSettings["ProductNotFound"] == "" ? "Container / Product Serial Number doesn't exists!" : ConfigurationManager.AppSettings["ProductNotFound"];
        public static string ObjectNotDefined = ConfigurationManager.AppSettings["ObjectNotDefined"] is null || ConfigurationManager.AppSettings["ObjectNotDefined"] == "" ? "Not defined" : ConfigurationManager.AppSettings["ObjectNotDefined"];
        public static string FinishedTheTask = ConfigurationManager.AppSettings["FinishedTheTask"] is null || ConfigurationManager.AppSettings["FinishedTheTask"] == "" ? "Do you want to finished the process? \nThere's Task still remaining to be executed.\nClick to finish process and send all image to opcenter without complete all task!" : ConfigurationManager.AppSettings["FinishedTheTask"];
        public static string OperationEnforcement = ConfigurationManager.AppSettings["OperationEnforcement"] is null || ConfigurationManager.AppSettings["OperationEnforcement"] == "" ? "Identifier / Product Unit incorrect position!" : ConfigurationManager.AppSettings["OperationEnforcement"];
    }
}
