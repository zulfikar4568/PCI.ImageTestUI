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
        public TransferImage(Repository.Opcenter.ContainerTransaction containerTxn)
        {
            _containerTxn = containerTxn;
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
                Unit = containerStatus.UOM is null ? MessageDefinition.ObjectNotDefined : containerStatus.UOM.ToString()
            };
        }

        public bool MainLogic(PictureBox PictureBoxObj, string ContainerName, string DocumentName, string DocumentRevision, string DocumentDescription)
        {
            string nameCapture = DocumentName + ".png";
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
            return statusAttachment;
        }
    }
}
