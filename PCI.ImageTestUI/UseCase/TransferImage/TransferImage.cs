using Camstar.WCF.ObjectStack;
using PCI.ImageTestUI.Config;
using PCI.ImageTestUI.Entity;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCI.ImageTestUI.UseCase
{
    public class TransferImage
    {
        private readonly Repository.Opcenter.ContainerTransaction _containerTxn;
        private readonly Util.PdfUtil _pdfUtil;
        private List<System.Drawing.Image> _images = new List<System.Drawing.Image>();
        public TransferImage(Repository.Opcenter.ContainerTransaction containerTxn, Util.PdfUtil pdfUtil)
        {
            _containerTxn = containerTxn;
            _pdfUtil = pdfUtil;
        }
        public ContainerModel DataContainerModel { get; set; }
        private int TotalTask { get; set; }
        public int CurrentTask { get; set; }
        public int GetTotalTask()
        {
            return TotalTask;
        }
        public ContainerModel ContainerStatusData(string Container)
        {
            /*ViewContainerStatus containerStatus = _containerTxn.GetCurrentContainer(Container);
            if (containerStatus == null) return null;
            DataContainerModel = new ContainerModel
            {
                Product = containerStatus.Product is null ? MessageDefinition.ObjectNotDefined : containerStatus.Product.ToString(),
                ProductDescription = containerStatus.ProductDescription is null ? MessageDefinition.ObjectNotDefined : containerStatus.ProductDescription.ToString(),
                Operation = containerStatus.Operation is null ? MessageDefinition.ObjectNotDefined : containerStatus.Operation.ToString(),
                Qty = containerStatus.Qty is null ? MessageDefinition.ObjectNotDefined : containerStatus.Qty.ToString(),
                Unit = containerStatus.UOM is null ? MessageDefinition.ObjectNotDefined : containerStatus.UOM.ToString(),
                TaskList = new List<Entity.Task>
                {
                    new Entity.Task{ TaskName = "Scan Image Test1" },
                    new Entity.Task{ TaskName = "Scan Image Test2" },
                    new Entity.Task{ TaskName = "Scan Image Test3" },
                    new Entity.Task{ TaskName = "Scan Image Test4" }
                }
            };*/

            //Mock for testing
            DataContainerModel = new ContainerModel()
            {
                Product = "Versana Model Test 1",
                Operation = "Visual Labelling",
                ProductDescription = "Versana Model Test export to China",
                Qty = "1",
                TaskList = new List<Entity.Task>()
                    {
                        new Entity.Task() { TaskName = "4C-RS Noise and Phantom check at B/CHI mode" },
                        new Entity.Task() { TaskName = "4C-RS Noise and Phantom check at CF/PW mode" },
                        new Entity.Task() { TaskName = "12-RS Noise and Phantom check at CWD mode" },
                        new Entity.Task() { TaskName = "L3-12-RS Noise Check at B/CHI mode" }
                    }
            };

            TotalTask = DataContainerModel.TaskList.Count;
            CurrentTask = 0;

            return DataContainerModel;
        }

        public void ResetState()
        {
            TotalTask = 0;
            CurrentTask = 0;
            _images.Clear();
            DataContainerModel = null;
        }

        public StatusMainLogic MainLogic(System.Drawing.Image image, string ContainerName, string DocumentName, string DocumentRevision, string DocumentDescription)
        {
            CurrentTask += 1;
            if (TotalTask >= CurrentTask)
            {
                _images.Add(image);
                if (TotalTask == CurrentTask)
                {
                    // We attach the Logic Convert and Send the file

                    // Reset
                    ResetState();

                    return new StatusMainLogic()
                    {
                        Status = Entity.StatusEnum.Done,
                        SendFileStatus = true,
                        Message = "Success Process all Task"
                    };

                } else
                {
                    return new StatusMainLogic()
                    {
                        Status = Entity.StatusEnum.InProgress,
                        SendFileStatus = false,
                        Message = $"Success Process the Task {DataContainerModel.TaskList[CurrentTask - 1]}"
                    };
                }
            }

            return new StatusMainLogic()
            {
                Status = Entity.StatusEnum.Error,
                SendFileStatus = false,
                Message = $"Failed when Process the Task"
            };

            /*string nameCapture = DocumentName + ".png";
            if (Directory.Exists(AppSettings.Folder))
            {
                PictureBoxObj.Image.Save($"{AppSettings.Folder}\\{nameCapture}", ImageFormat.Png);
            }
            else
            {
                Directory.CreateDirectory(AppSettings.Folder);
                PictureBoxObj.Image.Save($"{AppSettings.Folder}\\{nameCapture}", ImageFormat.Png);
            }

            string sourceFile = $"{AppSettings.Folder}\\{nameCapture}";
            bool statusAttachment = _containerTxn.AttachDocumentInContainer(ContainerName, AppSettings.ReuseDocument ? AttachmentTypeEnum.NewDocumentReuse : AttachmentTypeEnum.NewDocumentNOReuse, DocumentName, AppSettings.ReuseDocument ? DocumentRevision : "", sourceFile, DocumentDescription);
            if (statusAttachment && File.Exists(sourceFile))
            {
                File.Delete(sourceFile);
            }
            return statusAttachment;*/
        }
    }
}
