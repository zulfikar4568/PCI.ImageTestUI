using Camstar.WCF.ObjectStack;
using iText.IO.Image;
using PCI.ImageTestUI.Config;
using PCI.ImageTestUI.Entity;
using PCI.ImageTestUI.Test;
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
        private List<iText.Layout.Element.Image> _images = new List<iText.Layout.Element.Image>();
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
            ViewContainerStatus containerStatus = _containerTxn.GetCurrentContainer(Container);
            if (containerStatus == null) return null;
            DataContainerModel = new ContainerModel
            {
                Product = containerStatus.Product is null ? MessageDefinition.ObjectNotDefined : containerStatus.Product.ToString(),
                ProductDescription = containerStatus.ProductDescription is null ? MessageDefinition.ObjectNotDefined : containerStatus.ProductDescription.ToString(),
                Operation = containerStatus.Operation is null ? MessageDefinition.ObjectNotDefined : containerStatus.Operation.ToString(),
                Qty = containerStatus.Qty is null ? MessageDefinition.ObjectNotDefined : containerStatus.Qty.ToString(),
                Unit = containerStatus.UOM is null ? MessageDefinition.ObjectNotDefined : containerStatus.UOM.ToString(),
                TaskList = ContainerDataMock.GetTaskList,
            };

            //Mock for testing
            //DataContainerModel = ContainerDataMock.GenerateContainerDataMock;

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
                ImageData data = ImageDataFactory.Create(image, null);
                _images.Add(new iText.Layout.Element.Image(data));
                if (TotalTask == CurrentTask)
                {
                    // We attach the Logic Convert and Send the file
                    string nameCapture = DocumentName + ".pdf";
                    string sourceFile = $"{AppSettings.Folder}\\{nameCapture}";
                    _pdfUtil.MergeImageToPdf(sourceFile, _images.ToArray());

                    // Reset
                    ResetState();

                    bool statusAttachment = _containerTxn.AttachDocumentInContainer(ContainerName, AppSettings.ReuseDocument ? AttachmentTypeEnum.NewDocumentReuse : AttachmentTypeEnum.NewDocumentNOReuse, DocumentName, AppSettings.ReuseDocument ? DocumentRevision : "", sourceFile, DocumentDescription);
                    if (statusAttachment && File.Exists(sourceFile))
                    {
                        File.Delete(sourceFile);
                        return new StatusMainLogic()
                        {
                            Status = Entity.StatusEnum.Done,
                            SendFileStatus = true,
                            Message = "Success Process all Task"
                        };
                    } else if (!statusAttachment && File.Exists(sourceFile))
                    {
                        File.Delete(sourceFile);
                        return new StatusMainLogic()
                        {
                            Status = Entity.StatusEnum.Error,
                            SendFileStatus = false,
                            Message = $"Failed when Process the Task"
                        };
                    }

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
        }
    }
}
