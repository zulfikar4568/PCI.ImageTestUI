using AForge.Video.DirectShow;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.ImageTestUI.Util
{
    public class CameraUtil
    {
        public FilterInfoCollection videoDevices;
        public VideoCaptureDevice videoDevice;
        public VideoCapabilities[] snapshotCapabilities;
        public ArrayList listCamera = new ArrayList();
        public Stopwatch stopWatch = null;

        public static string _usbcamera;
        public string Usbcamera
        {
            get { return _usbcamera; }
            set { _usbcamera = value; }
        }

        //Delegate Untuk Capture, insert database, update ke grid 
        public delegate void CaptureSnapshotManifast(Bitmap image);
    }
}
