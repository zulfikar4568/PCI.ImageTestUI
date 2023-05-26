using Camstar.WCF.ObjectStack;
using iText.IO.Image;
using PCI.ImageTestUI.Config;
using PCI.ImageTestUI.Entity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly Repository.Task _taskRepository;
        private List<iText.Layout.Element.Image> _images = new List<iText.Layout.Element.Image>();
        public TransferImage(Repository.Opcenter.ContainerTransaction containerTxn, Util.PdfUtil pdfUtil, Repository.Task taskRepository)
        {
            _containerTxn = containerTxn;
            _pdfUtil = pdfUtil;
            _taskRepository = taskRepository;
        }
        public ContainerModel ContainerStatusData(string Container)
        {
            ViewContainerStatus containerStatus = _containerTxn.GetCurrentContainer(Container);
            if (containerStatus == null) return null;
            return new ContainerModel
            {
                Product = containerStatus.Product is null ? MessageDefinition.ObjectNotDefined : containerStatus.Product.ToString(),
                ProductDescription = containerStatus.ProductDescription is null ? MessageDefinition.ObjectNotDefined : containerStatus.ProductDescription.ToString(),
                Operation = containerStatus.Operation is null ? MessageDefinition.ObjectNotDefined : containerStatus.Operation.ToString(),
                Qty = containerStatus.Qty is null ? MessageDefinition.ObjectNotDefined : containerStatus.Qty.ToString(),
                Unit = containerStatus.UOM is null ? MessageDefinition.ObjectNotDefined : containerStatus.UOM.ToString(),
                TaskList = _taskRepository.GetDataCollectionList(),
            };
        }
        public void ClearImages()
        {
            _images.Clear();
        }

        public Entity.StatusEnum SendAllImageToOpcenter(string ContainerName,string DocumentName, string DocumentRevision, string DocumentDescription)
        {
            // We attach the Logic Convert and Send the file
            string nameCapture = DocumentName + ".pdf";
            string sourceFile = $"{AppSettings.Folder}\\{nameCapture}";
            _pdfUtil.MergeImageToPdf(sourceFile, _images.ToArray());

            // Reset Images Data
            ClearImages();

            bool statusAttachment = _containerTxn.AttachDocumentInContainer(ContainerName, AppSettings.ReuseDocument ? AttachmentTypeEnum.NewDocumentReuse : AttachmentTypeEnum.NewDocumentNOReuse, DocumentName, AppSettings.ReuseDocument ? DocumentRevision : "", sourceFile, DocumentDescription);
            if (File.Exists(sourceFile)) File.Delete(sourceFile);

            // Return the result
            return statusAttachment ? Entity.StatusEnum.Done : Entity.StatusEnum.Error;
        }

        public Entity.StatusEnum MainLogic(System.Drawing.Image image, string ContainerName, string DocumentName, string DocumentRevision, string DocumentDescription, bool IsLastTask, bool IsFail = false, string TaskName = "")
        {
            ImageData data = ImageDataFactory.Create(image, null);
            _images.Add(new iText.Layout.Element.Image(data));

            // Return the result
            return IsLastTask || IsFail ? SendAllImageToOpcenter(ContainerName, DocumentName, DocumentRevision, DocumentDescription) : Entity.StatusEnum.InProgress;
        }
    
        public bool OperationEnforcement(ContainerModel data)
        {
            if (AppSettings.OperationName != null && AppSettings.OperationName != "")
            {
                if (data.Operation != AppSettings.OperationName)
                {
                    MessageBox.Show(MessageDefinition.OperationEnforcement + $"\nPosition: {data.Operation}", "Opcenter Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            return true;
        }
    }
}
