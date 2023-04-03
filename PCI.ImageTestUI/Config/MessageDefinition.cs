using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        public static string FinishedTheTask = ConfigurationManager.AppSettings["FinishedTheTask"] is null || ConfigurationManager.AppSettings["FinishedTheTask"] == "" ? "Do you want consider this task as failed? \nClick to OK to proceed and all image will send to Opcenter!" : ConfigurationManager.AppSettings["FinishedTheTask"];
        public static string OperationEnforcement = ConfigurationManager.AppSettings["OperationEnforcement"] is null || ConfigurationManager.AppSettings["OperationEnforcement"] == "" ? "Identifier / Product Unit incorrect position!" : ConfigurationManager.AppSettings["OperationEnforcement"];
        
        public static void GenerateMessageWhenSending(string prevTask, string currentTask, bool isSuccess)
        {
            if (isSuccess)
            {
                var msgSuccess = $"Task {prevTask} captured successfully!\r\n\n*Notes: \r\n1. Please click submit in opcenter to continue to the next Task {currentTask}. \r\n2. After that open again the MES Image Test Camera Picture Software";
                MessageBox.Show(msgSuccess, "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else
            {
                var msgFail = $"{prevTask} image failed!\r\n\n*Notes: \r\n All previous image will send to opcenter!";
                MessageBox.Show(msgFail, "Image State Fail!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
