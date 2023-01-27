using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using PCI.ImageTestUI.Config;
using PCI.ImageTestUI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PCI.ImageTestUI.Repository.Opcenter
{
    public class ContainerTransaction
    {
        private readonly Driver.Opcenter.ContainerTransaction _containerTxn;
        private readonly Driver.Opcenter.Helper _helper;
        public ContainerTransaction(Driver.Opcenter.ContainerTransaction containerTxn, Driver.Opcenter.Helper helper)
        {
            _containerTxn = containerTxn;
            _helper = helper;
        }
        public bool ExecuteMoveIn(string ContainerName, string ResourceName, string DataCollectionName = "", string DataCollectionRev = "", DataPointDetails[] DataPoints = null, string CarrierName = "", string Comments = "", bool IgnoreException = true)
        {
            MoveInService service = new MoveInService(AppSettings.ExCoreUserProfile);
            MoveIn serviceObject = new MoveIn() { Container = new ContainerRef(ContainerName) };
            if (ResourceName != "") serviceObject.Resource = new NamedObjectRef(ResourceName);
            if (DataPoints != null)
            {
                if (DataCollectionName != "")
                {
                    serviceObject.DataCollectionDef = new RevisionedObjectRef() { Name = DataCollectionName, Revision = DataCollectionRev, RevisionOfRecord = (DataCollectionRev == "") };
                    serviceObject.ParametricData = _helper.SetDataPointSummary(serviceObject.DataCollectionDef, DataPoints);
                }
                else
                {
                    DataPointSummary oDataPointSummaryRef = _helper.GetDataPointSummaryRef(service, serviceObject, new MoveIn_Request(), new MoveIn_Info(), ref DataCollectionName, ref DataCollectionRev);
                    serviceObject.ParametricData = _helper.SetDataPointSummary(oDataPointSummaryRef, DataPoints);
                }
            }
            if (CarrierName != "") //Attach = true
            {
                serviceObject.Carrier = new NamedObjectRef(CarrierName);
            }
            if (Comments != "") serviceObject.Comments = Comments;
            return _containerTxn.MoveInTxn(serviceObject, service, IgnoreException);
        }
        public bool ExecuteMoveStd(string ContainerName, string ToResourceName = "", string Resource = "", string DataCollectionName = "", string DataCollectionRev = "", DataPointDetails[] DataPoints = null, string CarrierName = "", string Comments = "", bool IgnoreException = true)
        {
            MoveStdService service = new MoveStdService(AppSettings.ExCoreUserProfile);
            MoveStd serviceObject = new MoveStd() { Container = new ContainerRef(ContainerName) };
            if (Resource != "")
            {
                serviceObject.Resource = new NamedObjectRef() { Name = Resource };
            }
            if (ToResourceName != "")
            {
                serviceObject.ToResource = new NamedObjectRef() { Name = ToResourceName };
            }
            if (DataPoints != null)
            {
                if (DataCollectionName != "")
                {
                    serviceObject.DataCollectionDef = new RevisionedObjectRef() { Name = DataCollectionName, Revision = DataCollectionRev, RevisionOfRecord = (DataCollectionRev == "") };
                    serviceObject.ParametricData = _helper.SetDataPointSummary(serviceObject.DataCollectionDef, DataPoints);
                }
                else
                {
                    DataPointSummary oDataPointSummaryRef = _helper.GetDataPointSummaryRef(service, serviceObject, new MoveStd_Request(), new MoveStd_Info(), ref DataCollectionName, ref DataCollectionRev);
                    serviceObject.ParametricData = _helper.SetDataPointSummary(oDataPointSummaryRef, DataPoints);
                }
            }

            if (CarrierName != "") //Attach = true
            {
                serviceObject.Carrier = new NamedObjectRef(CarrierName);
            }
            if (Comments != "") serviceObject.Comments = Comments;
            return _containerTxn.MoveStdTxn(serviceObject, service, IgnoreException);
        }
        public bool ContainerExists(string ContainerName, bool IgnoreException = true)
        {
            // Container Info
            ViewContainerStatus_Info containerInfo = new ViewContainerStatus_Info();
            containerInfo.ContainerName = new Info(true);

            ViewContainerStatus containerStatus = _containerTxn.ContainerInfo(containerInfo, ContainerName, IgnoreException);

            if (containerStatus is null) return false;
            return true;
        }
        public bool AttachDocumentInContainer(string ContainerName, Enumeration<AttachmentTypeEnum, int> AttachmentType, string DocumentName, string DocumentRevision = "", string Identifier = "", string DocumentDescription = "", string Comments = "", bool IgnoreException = true)
        {
            string TxnId = Guid.NewGuid().ToString();
            AttachDocument serviceObject = new AttachDocument() { ContainerInstance = new ContainerRef(ContainerName) };
            serviceObject.DocumentName = new Primitive<string> { Value = DocumentName };
            if (DocumentDescription != "") serviceObject.DocumentDescription = new Primitive<string> { Value = DocumentDescription };

            // Common Object
            serviceObject.InstanceName = new Primitive<string> { Value = ContainerName };
            serviceObject.AttachmentType = AttachmentType;
            serviceObject.IsContainer = true; // Set Document for Container

            try
            {
                //Validation
                if (Identifier == "" && (AttachmentType == AttachmentTypeEnum.NewDocumentReuse || AttachmentType == AttachmentTypeEnum.NewDocumentNOReuse))
                {
                    throw new Exception("Identifier cannot null if you set attachment to NewDocumentReuse or NewDocumentNoReuse");
                }
                if (DocumentRevision == "" && AttachmentType == AttachmentTypeEnum.NewDocumentReuse)
                {
                    throw new Exception("DocumentRevision must be set or cannot empty if attachment type NewDocumentReuse");
                }
            }
            catch (Exception ex)
            {
                ex.Source = AppSettings.AssemblyName == ex.Source ? MethodBase.GetCurrentMethod().Name : MethodBase.GetCurrentMethod().Name + "." + ex.Source;
                string exceptionMsg = Logging.LoggingContainer(ContainerName, TxnId, ex.Message);
                EventLogUtil.LogErrorEvent(ex.Source, exceptionMsg);
                if (!IgnoreException) throw ex;
            }


            // Setting Object Again
            if (AttachmentType == AttachmentTypeEnum.NewDocumentReuse)
            {
                serviceObject.DocumentRevision = new Primitive<string> { Value = DocumentRevision };
            }
            else if (AttachmentType == AttachmentTypeEnum.ExistingDocument)
            {
                serviceObject.AttachedDocument = new RevisionedObjectRef(DocumentName, DocumentRevision);
            }

            // For NewDocumentReuse or NewDocumentNoReuse
            if (AttachmentType == AttachmentTypeEnum.NewDocumentReuse || AttachmentType == AttachmentTypeEnum.NewDocumentNOReuse)
            {
                serviceObject.Identifier = new Primitive<string> { Value = System.IO.Path.GetFileName(Identifier) };
                serviceObject.FilePath = new Primitive<string> { Value = System.IO.Path.GetDirectoryName(Identifier) };
                serviceObject.AttachedFileName = new Primitive<string> { Value = System.IO.Path.GetFileName(Identifier) };
                serviceObject.AttachedFileExtension = new Primitive<string> { Value = System.IO.Path.GetExtension(Identifier) };
            }

            if (Comments != "") serviceObject.Comments = new Primitive<string> { Value = Comments };

            return _containerTxn.AttachDocumentTxn(serviceObject, TxnId);
        }
        public ViewContainerStatus GetCurrentContainer(string ContainerName, bool IgnoreException = true)
        {
            ViewContainerStatus_Info containerInfo = new ViewContainerStatus_Info();
            containerInfo.RequestValue = true;
            containerInfo.ContainerName = new Info(true);
            containerInfo.Step = new Info(true);
            containerInfo.Qty = new Info(true);
            containerInfo.Product = new Info(true);
            containerInfo.Operation = new Info(true);
            containerInfo.Product = new Info(true);
            return _containerTxn.ContainerInfo(containerInfo, ContainerName, IgnoreException);
        }
        public CurrentContainerStatus GetContainerStatusDetails(string ContainerName, string DataCollectionName = "", string DataCollectionRev = "", bool IgnoreException = true)
        {
            ContainerTxn serviceObject = new ContainerTxn();
            serviceObject.Container = new ContainerRef(ContainerName);
            if (DataCollectionName != "")
            {
                serviceObject.DataCollectionDef = new RevisionedObjectRef() { Name = DataCollectionName, Revision = DataCollectionRev, RevisionOfRecord = (DataCollectionRev == "") };
            }
            return _containerTxn.ContainerStatusInfo(serviceObject, ContainerName, IgnoreException);
        }
    }
}
