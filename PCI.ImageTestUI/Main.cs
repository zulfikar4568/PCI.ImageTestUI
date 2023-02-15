using AForge.Video.DirectShow;
using AForge.Video;
using PCI.ImageTestUI.Config;
using PCI.ImageTestUI.UseCase;
using PCI.ImageTestUI.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PCI.ImageTestUI.Util.CameraUtil;
using PCI.ImageTestUI.Entity;

namespace PCI.ImageTestUI
{
    public partial class Main : Krypton.Toolkit.KryptonForm
    {
        private static bool needSnapshot = false;
        private readonly CameraUtil _camera;
        private readonly TransferImage _usecaseTransferImage;
        public Main(CameraUtil camera, TransferImage usecaseTransferImage)
        {

            // Component Initialization Default
            InitializeComponent();

            // Initialize Form Position
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            this.Size = new Size(Screen.PrimaryScreen.WorkingArea.Size.Width / 2, Screen.PrimaryScreen.WorkingArea.Size.Height);

            _camera = camera;
            _usecaseTransferImage = usecaseTransferImage;

            // Initialize Camera
            GetListCameraUSB();

            //Reset the State
            ResetState();


            Bt_TurnOffCamera.Enabled = false;
            Bt_Camera.Enabled = true;
            Cb_VideoInput.DropDownStyle = ComboBoxStyle.DropDownList;
            Tb_Message.ReadOnly = true;
            Tb_Message.BackColor = Color.White;
        }
        private void ExitCamera()
        {
            Vsc_Source.SignalToStop();
            Vsc_Source = null;
        }

        private void GetListCameraUSB()
        {
            _camera.videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (_camera.videoDevices.Count != 0)
            {
                foreach (FilterInfo device in _camera.videoDevices)
                {
                    Cb_VideoInput.Items.Add(device.Name);
                }
            }
            else
            {
                Cb_VideoInput.Items.Add(MessageDefinition.NoDeviceFound);
            }

            Cb_VideoInput.SelectedIndex = 0;
        }

        private void StartCamera()
        {
            try
            {
                _camera.Usbcamera = Cb_VideoInput.SelectedIndex.ToString();
                _camera.videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (_camera.videoDevices.Count != 0)
                {
                    // add all devices to combo
                    foreach (FilterInfo device in _camera.videoDevices)
                    {
                        _camera.listCamera.Add(device.Name);
                    }
                }
                else
                {
                    MessageBox.Show(MessageDefinition.NoDeviceFound);
                }

                _camera.videoDevice = new VideoCaptureDevice(_camera.videoDevices[Convert.ToInt32(_camera.Usbcamera)].MonikerString);
                _camera.snapshotCapabilities = _camera.videoDevice.SnapshotCapabilities;
                if (_camera.snapshotCapabilities.Length == 0)
                {
                    // MessageBox.Show("Camera Capture Not supported");
                }
                OpenVideoSource(_camera.videoDevice);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }
        }
        public void OpenVideoSource(IVideoSource source)
        {
            try
            {
                // set busy cursor
                this.Cursor = Cursors.WaitCursor;
                // stop current video source
                CloseCurrentVideoSource();
                // start new video source
                Vsc_Source.VideoSource = source;
                Vsc_Source.Start();
                // reset stop watch
                _camera.stopWatch = null;
                this.Cursor = Cursors.Default;

                Bt_TurnOffCamera.Enabled = true;
                Bt_Camera.Enabled = false;
            }
            catch (Exception ex)
            {
                ex.Source = AppSettings.AssemblyName == ex.Source ? MethodBase.GetCurrentMethod().Name : MethodBase.GetCurrentMethod().Name + "." + ex.Source;
                EventLogUtil.LogErrorEvent(ex.Source, ex);
            }
        }

        public void CloseCurrentVideoSource()
        {
            try
            {
                if (Vsc_Source.VideoSource != null)
                {
                    Vsc_Source.SignalToStop();
                    // wait ~ 3 seconds
                    for (int i = 0; i < 30; i++)
                    {
                        if (!Vsc_Source.IsRunning)
                            break;
                        Thread.Sleep(100);
                    }
                    if (Vsc_Source.IsRunning)
                    {
                        Vsc_Source.Stop();
                    }
                    Vsc_Source.VideoSource = null;
                }
            }
            catch { }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            ExitCamera();
            Thread.Sleep(1000);
        }

        private void Bt_Capture_Click(object sender, EventArgs e)
        {
            if (Vsc_Source.VideoSource == null)
            {
                MessageBox.Show(MessageDefinition.CameraNotConnected);
            }
            else
            {
                needSnapshot = true;
            }
        }

        private void Vsc_Source_NewFrame(object sender, ref Bitmap image)
        {
            try
            {
                DateTime now = DateTime.Now;
                Graphics g = Graphics.FromImage(image);
                // paint current time
                SolidBrush brush = new SolidBrush(Color.White);
                FontFamily fontFamily = new FontFamily("Arial");
                Font font = new Font(
                   fontFamily,
                   25,
                   FontStyle.Regular,
                   GraphicsUnit.Pixel);

                g.DrawString(now.ToString(), font, brush, new PointF(5, 5));
                brush.Dispose();
                if (needSnapshot)
                {
                    this.Invoke(new CaptureSnapshotManifast(UpdateCaptureSnapshotManifast), image);
                }
                g.Dispose();
            }
            catch (Exception ex)
            {
                ex.Source = AppSettings.AssemblyName == ex.Source ? MethodBase.GetCurrentMethod().Name : MethodBase.GetCurrentMethod().Name + "." + ex.Source;
                EventLogUtil.LogErrorEvent(ex.Source, ex.Message);
            }
        }

        public void UpdateCaptureSnapshotManifast(Bitmap image)
        {
            try
            {
                needSnapshot = false;
                if (Pb_Picture.Image != null)
                {
                    Pb_Picture.Image.Dispose();
                }
                Pb_Picture.Image = image;
                Pb_Picture.Update();

                /*if (_usecaseTransferImage.GetTotalTask() - 1 == _usecaseTransferImage.CurrentTask)
                {
                    Lb_Instruction.Text = MessageDefinition.Waiting;
                    Lb_Instruction.ForeColor = Color.White;
                    Lb_Instruction.BackColor = Color.Blue;
                }*/
                StatusMainLogic statusMainLogic = _usecaseTransferImage.MainLogic(image, Tb_Container.Text, $"{AppSettings.PrefixDocumentName}{Tb_Container.Text}_{DateTime.Now:yyyyMMddHHmmss}", AppSettings.DocumentRevision, AppSettings.DocumentDescription);
                if (statusMainLogic.Status == StatusEnum.InProgress)
                {
                    Lb_Instruction.Text = _usecaseTransferImage.DataContainerModel.TaskList[_usecaseTransferImage.CurrentTask].TaskName;
                }
                else if (statusMainLogic.Status == StatusEnum.Done)
                {
                    if (statusMainLogic.SendFileStatus)
                    {
                        MessageBox.Show(MessageDefinition.SendImageSuccess);
                        ResetState();
                    } else
                    {
                        MessageBox.Show(MessageDefinition.SendImageFailed);
                        ResetState();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Source = AppSettings.AssemblyName == ex.Source ? MethodBase.GetCurrentMethod().Name : MethodBase.GetCurrentMethod().Name + "." + ex.Source;
                EventLogUtil.LogErrorEvent(ex.Source, ex);
            }
        }

        private void Bt_Reset_Click(object sender, EventArgs e)
        {
            ResetState();
        }

        private void ResetState()
        {
            Pb_Picture.Image = null;
            Bt_Capture.Enabled = false;
            Tb_Container.Enabled = true;
            Tb_Message.Text = "";
            Tb_Container.Text = "";
            Lb_Instruction.Text = MessageDefinition.MessageBeforeScan;
            Lb_Instruction.ForeColor = Color.White;
            Lb_Instruction.BackColor = Color.Green;
        }

        private void Bt_Camera_Click(object sender, EventArgs e)
        {
            StartCamera();
        }
        private void Bt_Container_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ContainerModel dataContainer = _usecaseTransferImage.ContainerStatusData(Tb_Container.Text);
                if (dataContainer is null)
                {
                    MessageBox.Show(MessageDefinition.ProductNotFound);
                }
                else
                { 
                    Bt_Capture.Enabled = true;
                    Tb_Container.Enabled = false;
                    Lb_Instruction.ForeColor = Color.White;
                    Lb_Instruction.BackColor = Color.YellowGreen;

                    Tb_Message.Text += $"Product: {dataContainer.Product}\r\n";
                    Tb_Message.Text += $"Product Description: {dataContainer.ProductDescription}\r\n";
                    Tb_Message.Text += $"Unit: {dataContainer.Unit}\r\n";
                    Tb_Message.Text += $"Qty: {dataContainer.Qty}\r\n";
                    Tb_Message.Text += $"Operation: {dataContainer.Operation}\r\n";
                    foreach (var item in dataContainer.TaskList)
                    {
                        Tb_Message.Text += item.TaskName;
                    }
                    Lb_Instruction.Text = _usecaseTransferImage.DataContainerModel.TaskList[_usecaseTransferImage.CurrentTask].TaskName;
                }
            }
        }

        private void Bt_TurnOffCamera_Click(object sender, EventArgs e)
        {
            if (Vsc_Source.VideoSource == null)
            {
                MessageBox.Show(MessageDefinition.CameraNotConnected);
            }
            else
            {
                CloseCurrentVideoSource();
                Bt_TurnOffCamera.Enabled = false;
                Bt_Camera.Enabled = true;
            }
        }
    }
}
