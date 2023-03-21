using AForge.Video.DirectShow;
using AForge.Video;
using PCI.ImageTestUI.Config;
using PCI.ImageTestUI.UseCase;
using PCI.ImageTestUI.Util;
using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using static PCI.ImageTestUI.Util.CameraUtil;
using PCI.ImageTestUI.Entity;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

            Tb_Message.Enabled = false;
            Tb_Container.Focus();
            Tb_Container.SelectionStart = Tb_Container.Text.Length;

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
                    MessageBox.Show(MessageDefinition.NoDeviceFound, "Device Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show(err.ToString(), "Device Information", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                Tb_Container.Focus();
                Tb_Container.SelectionStart = Tb_Container.Text.Length;
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
                MessageBox.Show(MessageDefinition.CameraNotConnected, "Device Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                Pb_Picture.Image?.Dispose();
                Pb_Picture.Image = image;
                Pb_Picture.Update();

                StatusMainLogic statusMainLogic = _usecaseTransferImage.MainLogic(image, Tb_Container.Text, $"{AppSettings.PrefixDocumentName}{Tb_Container.Text}_{DateTime.Now:yyyyMMddHHmmss}", AppSettings.DocumentRevision, AppSettings.DocumentDescription);
                if (statusMainLogic.Status == StatusEnum.InProgress)
                {
                    if (!Bt_Finished.Enabled) Bt_Finished.Enabled = true;
                    MessageBox.Show($"Task {_usecaseTransferImage.DataContainerModel.TaskList[_usecaseTransferImage.CurrentTask - 1].TaskName} captured successfully!\r\n\n*Notes: \r\n1. Please click submit in opcenter to continue to the next Task {_usecaseTransferImage.DataContainerModel.TaskList[_usecaseTransferImage.CurrentTask].TaskName}. \r\n2. After that open again the MES Image Test Camera Picture Software", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Lb_Instruction.Text = _usecaseTransferImage.DataContainerModel.TaskList[_usecaseTransferImage.CurrentTask].TaskName;
                }
                else if (statusMainLogic.Status == StatusEnum.Done)
                {
                    MessageBox.Show(MessageDefinition.SendImageSuccess, "Sending the Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (statusMainLogic.Status == StatusEnum.Error)
                {
                    MessageBox.Show(MessageDefinition.SendImageFailed, "Sending the Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (statusMainLogic.Status != StatusEnum.InProgress) ResetState();
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
            Tb_Container.Enabled = true;
            Tb_Container.Text = "";
            Tb_Container.Focus();
            Tb_Container.SelectionStart = Tb_Container.Text.Length;
            Bt_Finished.Enabled = false;
            Pb_Picture.Image = null;
            Bt_Capture.Enabled = false;
            Tb_Message.Text = "";
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
                    MessageBox.Show(MessageDefinition.ProductNotFound, "Opcenter Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    // Operation Enforcement
                    if (!_usecaseTransferImage.OperationEnforcement(dataContainer)) return;

                    Bt_Capture.Enabled = true;
                    Tb_Container.Enabled = false;
                    Lb_Instruction.ForeColor = Color.White;
                    Lb_Instruction.BackColor = Color.YellowGreen;
                    Tb_Container.Focus();
                    Tb_Container.SelectionStart = Tb_Container.Text.Length;

                    Tb_Message.Text += $"Product: {dataContainer.Product}\r\n";
                    Tb_Message.Text += $"Product Description: {dataContainer.ProductDescription}\r\n";
                    Tb_Message.Text += $"Unit: {dataContainer.Unit}\r\n";
                    Tb_Message.Text += $"Qty: {dataContainer.Qty}\r\n";
                    Tb_Message.Text += $"Operation: {dataContainer.Operation}\r\n\n";
                    int counter = 1;
                    foreach (var item in dataContainer.TaskList)
                    {
                        Tb_Message.Text += $"Task {counter}: {item.TaskName}\r\n";
                        counter++;
                    }
                    Lb_Instruction.Text = _usecaseTransferImage.DataContainerModel.TaskList[_usecaseTransferImage.CurrentTask].TaskName;
                }
            }
        }

        private void Bt_TurnOffCamera_Click(object sender, EventArgs e)
        {
            if (Vsc_Source.VideoSource == null)
            {
                MessageBox.Show(MessageDefinition.CameraNotConnected, "Device Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                CloseCurrentVideoSource();
                Bt_TurnOffCamera.Enabled = false;
                Bt_Camera.Enabled = true;
            }
        }

        private void Bt_Finished_Click(object sender, EventArgs e)
        {
            if (_usecaseTransferImage.CurrentTask < _usecaseTransferImage.TotalTask)
            {
                DialogResult dialogResult = MessageBox.Show(MessageDefinition.FinishedTheTask, "Notification", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    var statusMainLogic = _usecaseTransferImage.SendAllImageToOpcenter(Tb_Container.Text, $"{AppSettings.PrefixDocumentName}{Tb_Container.Text}_{DateTime.Now:yyyyMMddHHmmss}", AppSettings.DocumentRevision, AppSettings.DocumentDescription);
                    if (statusMainLogic.Status == StatusEnum.Done)
                    {
                        MessageBox.Show(MessageDefinition.SendImageSuccess, "Sending the Image", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (statusMainLogic.Status == StatusEnum.Error)
                    {
                        MessageBox.Show(MessageDefinition.SendImageFailed, "Sending the Image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    ResetState();
                    Bt_Finished.Enabled = false;
                }
            }
        }
    }
}
