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
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PCI.ImageTestUI
{
    public partial class Main : Krypton.Toolkit.KryptonForm
    {
        private static bool needSnapshot = false;
        private readonly CameraUtil _camera;
        private readonly TransferImage _usecaseTransferImage;
        private readonly TaskUseCase _taskUsecase;
        private byte[] _currentImage = null;

        //Fields
        private ContainerModel _dataContainer = null;
        private Queue<Task> _queueTask = new Queue<Task>();
        private bool EnabledWrite = false;
        public Main(CameraUtil camera, TransferImage usecaseTransferImage, TaskUseCase taskUsecase)
        {

            // Component Initialization Default
            InitializeComponent();
            KeyPreview = true;

            // Initialize Form Position
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(0, 0);
            this.Size = new Size(Screen.PrimaryScreen.WorkingArea.Size.Width / 2, Screen.PrimaryScreen.WorkingArea.Size.Height);

            this.Text += $" | Build Version {Assembly.GetEntryAssembly().GetName().Version}";

            Tb_Message.Enabled = false;
            Tb_Container.Focus();
            Tb_Container.SelectionStart = Tb_Container.Text.Length;

            _camera = camera;
            _usecaseTransferImage = usecaseTransferImage;
            _taskUsecase = taskUsecase;

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
            catch(Exception ex)
            {
                ex.Source = AppSettings.AssemblyName == ex.Source ? MethodBase.GetCurrentMethod().Name : MethodBase.GetCurrentMethod().Name + "." + ex.Source;
                EventLogUtil.LogErrorEvent(ex.Source, ex);
            }
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            ExitCamera();
            Thread.Sleep(1000);
        }
        private void OnCapture()
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
        private void Bt_Capture_Click(object sender, EventArgs e)
        {
            OnCapture();
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

                // Generate Task Name
                if (_dataContainer != null && _queueTask.Count> 0)
                {
                    var taskName = _taskUsecase.GenerateTaskMsgForImage(_dataContainer.TaskList, _queueTask);
                    g.DrawString($"{taskName} {now}", font, brush, new PointF(5, 5));
                }

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

        private void EnabledCapturing()
        {
            if (Pb_Picture.Image != null)
            {
                Bt_RetryCapture.Enabled = true;
                Bt_PassCapture.Enabled = true;
                Bt_Fail.Enabled = true;
                Bt_Capture.Enabled = false;
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

                _currentImage = _camera.ImageToByte(image);

                EnabledCapturing();
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

        private void Bt_Camera_Click(object sender, EventArgs e)
        {
            StartCamera();
        }
        private void Bt_Container_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _dataContainer = _usecaseTransferImage.ContainerStatusData(Tb_Container.Text);
                if (_dataContainer is null)
                {
                    MessageBox.Show(MessageDefinition.ProductNotFound, "Opcenter Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    // Operation Enforcement
                    if (!_usecaseTransferImage.OperationEnforcement(_dataContainer)) return;

                    Bt_Capture.Enabled = true;
                    Tb_Container.Enabled = false;
                    Lb_Instruction.ForeColor = Color.White;
                    Lb_Instruction.BackColor = Color.YellowGreen;
                    Tb_Container.Focus();
                    Tb_Container.SelectionStart = Tb_Container.Text.Length;

                    Tb_Message.Text += $"Product: {_dataContainer.Product}\r\n";
                    Tb_Message.Text += $"Product Description: {_dataContainer.ProductDescription}\r\n";
                    Tb_Message.Text += $"Unit: {_dataContainer.Unit}\r\n";
                    Tb_Message.Text += $"Qty: {_dataContainer.Qty}\r\n";
                    Tb_Message.Text += $"Operation: {_dataContainer.Operation}\r\n\n";
                    
                    // Clear the Initial materials
                    listViewTask.Items.Clear();

                    // Regenerate DataGrid
                    RegenerateListView();

                    // Generate Pending Queue Task
                    _queueTask = _taskUsecase.GeneratePendingQueueTask(_dataContainer.TaskList);

                    if (_queueTask.Peek() != null) Lb_Instruction.Text = _queueTask.Peek().TaskName;
                }
            }
        }
        private void RegenerateListView()
        {
            // Clear the Initial materials
            listViewTask.Items.Clear();
            EnabledWrite = true;
            if (_dataContainer == null) return;
            if (_dataContainer.TaskList == null) return;
            _taskUsecase.GenerateListView(_dataContainer.TaskList, listViewTask);
            EnabledWrite = false;
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

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1 && Bt_Capture.Enabled)
            {
                OnCapture();
            }
            else if (e.KeyCode == Keys.F2 && Bt_RetryCapture.Enabled)
            {
                RetryCapturing();
            }
            else if (e.KeyCode == Keys.F3 && Bt_PassCapture.Enabled)
            {
                PassCapturing();
            }
            else if (e.KeyCode == Keys.F4 && Bt_Fail.Enabled)
            {
                FailCapturing();
            }
        }

        private void CreateTaskForMainLogic(bool IsFail)
        {
            if (_queueTask.Count == 0) return;

            string suffix = IsFail ? "FAIL" : "PASS";
            using (var ms = new MemoryStream(_currentImage))
            {
                Bitmap bmp = new Bitmap(ms);
                Pb_Picture.Image = bmp;
                Pb_Picture.Update();

                // To define whether this is the las step
                bool isLastTask = _queueTask.Count == 1;
                
                StatusEnum statusMainLogic = _usecaseTransferImage.MainLogic(bmp, Tb_Container.Text, $"{AppSettings.PrefixDocumentName}{Tb_Container.Text}_{DateTime.Now:yyyyMMddHHmmss}_{suffix}", AppSettings.DocumentRevision, AppSettings.DocumentDescription, isLastTask, IsFail, _queueTask.Peek().TaskName);
                
                // Store the current task and dequeue the task from peek
                var currentTask = _queueTask.Dequeue();
                var nextTask = _queueTask.Count == 0 ? null : _queueTask.Peek();
                StatusSendingImage(statusMainLogic, currentTask, nextTask, IsFail);

                // Update the task list
                UpdateListTask(currentTask);

                // Regenerate DataGrid
                RegenerateListView();
            }
        }

        private void UpdateListTask(Task task)
        {
            if (_dataContainer == null) return;
            if (_dataContainer.TaskList == null) return;

            var newTasks = _dataContainer.TaskList;

            _taskUsecase.ChangeListTask(ref newTasks, task);

            _dataContainer.TaskList = newTasks;
        }
        private void StatusSendingImage(StatusEnum statusEnum, Task currentTask, Task nextTask, bool IsSuccess)
        {
            if (statusEnum == StatusEnum.InProgress && currentTask != null && nextTask != null)
            {
                MessageDefinition.GenerateMessageWhenSending(currentTask.TaskName, nextTask.TaskName, IsSuccess);
                Lb_Instruction.Text = nextTask.TaskName;
                RetryCapturing();
            }
            else if (statusEnum == StatusEnum.Done || statusEnum == StatusEnum.Error)
            {
                var status = MessageDefinition.GenerateStatusSendingImage(statusEnum);

                if (status != null) MessageBox.Show(status.Msg, status.Caption, MessageBoxButtons.OK, status.Icon);

                ResetState();
                return;
            }
        }
        private void PassCapturing()
        {
            CreateTaskForMainLogic(false);
        }
        private void FailCapturing()
        {
            // asking for confirmation
            DialogResult dialogResult = MessageBox.Show(MessageDefinition.FinishedTheTask, "Notification", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (dialogResult == DialogResult.Yes)
            {
                CreateTaskForMainLogic(true);
            }
        }
        private void RetryCapturing()
        {
            Pb_Picture.Image = null;
            Bt_Capture.Enabled = true;
            Bt_RetryCapture.Enabled = false;
            Bt_PassCapture.Enabled = false;
            Bt_Fail.Enabled = false;
        }

        private void Bt_PassCapture_Click(object sender, EventArgs e)
        {
            PassCapturing();
        }

        private void Bt_RetryCapture_Click(object sender, EventArgs e)
        {
            RetryCapturing();
        }

        private void Bt_Fail_Click(object sender, EventArgs e)
        {
            FailCapturing();
        }
        private void ResetState()
        {
            Tb_Container.Enabled = true;
            Tb_Container.Text = "";
            Tb_Container.Focus();
            Tb_Container.SelectionStart = Tb_Container.Text.Length;
            Bt_Fail.Enabled = false;
            Pb_Picture.Image = null;
            Bt_Capture.Enabled = false;
            Tb_Message.Text = "";
            Lb_Instruction.Text = MessageDefinition.MessageBeforeScan;
            Lb_Instruction.ForeColor = Color.White;
            Lb_Instruction.BackColor = Color.Green;

            Bt_RetryCapture.Enabled = false;
            Bt_PassCapture.Enabled = false;
            Bt_Fail.Enabled = false;

            _dataContainer = null;
            _queueTask.Clear();
            _usecaseTransferImage.ClearImages();
            listViewTask.Items.Clear();
        }

        private void listViewTask_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!EnabledWrite) e.NewValue = e.CurrentValue;
        }
    }
}
