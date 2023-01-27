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
        public static string NoDeviceFound = ConfigurationManager.AppSettings["NoDeviceFound"] is null ? "Camera devices not found!" : ConfigurationManager.AppSettings["NoDeviceFound"];
        public static string Waiting = ConfigurationManager.AppSettings["Waiting"] is null ? "Please wait..." : ConfigurationManager.AppSettings["Waiting"];
        public static string SendImageSuccess = ConfigurationManager.AppSettings["SendImageSuccess"] is null ? "Send image successfully" : ConfigurationManager.AppSettings["SendImageSuccess"];
        public static string SendImageFailed = ConfigurationManager.AppSettings["SendImageFailed"] is null ? "Send image failed!" : ConfigurationManager.AppSettings["SendImageFailed"];
        public static string CameraNotConnected = ConfigurationManager.AppSettings["CameraNotConnected"] is null ? "Please Connect your device camera!" : ConfigurationManager.AppSettings["CameraNotConnected"];
        public static string MessageBeforeScan = ConfigurationManager.AppSettings["MessageBeforeScan"] is null ? "Please scan the Serial Number of Product!" : ConfigurationManager.AppSettings["MessageBeforeScan"];
        public static string MessageAfterScan = ConfigurationManager.AppSettings["MessageAfterScan"] is null ? "Put the camera in correct position then please click capture button to catch the image!" : ConfigurationManager.AppSettings["MessageAfterScan"];
        public static string ProductNotFound = ConfigurationManager.AppSettings["ProductNotFound"] is null ? "Container / Product Serial Number doesn't exists!" : ConfigurationManager.AppSettings["ProductNotFound"];
        public static string ObjectNotDefined = ConfigurationManager.AppSettings["ObjectNotDefined"] is null ? "Not defined" : ConfigurationManager.AppSettings["ObjectNotDefined"];
    }
}
