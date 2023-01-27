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

namespace PCI.ImageTestUI.Driver.Opcenter
{
    public class ContainerTransaction
    {
        private readonly Helper _helper;
        public ContainerTransaction(Helper helper)
        {
            _helper = helper;
        }
        public bool MoveInTxn(MoveIn ServiceObject, MoveInService Service, bool IgnoreException = true)
        {
            string TxnId = Guid.NewGuid().ToString();
            try
            {
                string sMessage = "";
                MoveIn oServiceObject = null;
                ResultStatus oResulstStatus = null;
                EventLogUtil.LogEvent(Logging.LoggingContainer(ServiceObject.Container.Name, TxnId, "Setting input data for MoveIn ..."), System.Diagnostics.EventLogEntryType.Information, 2);
                oServiceObject = ServiceObject;

                // Execute Transaction
                EventLogUtil.LogEvent(Logging.LoggingContainer(ServiceObject.Container.Name, TxnId, "Execution a Move In"), System.Diagnostics.EventLogEntryType.Information, 2);
                oResulstStatus = Service.ExecuteTransaction(oServiceObject);

                // Process Result
                bool statusMoveIn = _helper.ProcessResult(oResulstStatus, ref sMessage, false);
                EventLogUtil.LogEvent(Logging.LoggingContainer(ServiceObject.Container.Name, TxnId, sMessage), System.Diagnostics.EventLogEntryType.Information, 2);
                return statusMoveIn;
            }
            catch (Exception ex)
            {
                ex.Source = AppSettings.AssemblyName == ex.Source ? MethodBase.GetCurrentMethod().Name : MethodBase.GetCurrentMethod().Name + "." + ex.Source;
                string exceptionMsg = Logging.LoggingContainer(ServiceObject.Container.Name, TxnId, ex.Message);
                EventLogUtil.LogErrorEvent(ex.Source, exceptionMsg);
                if (!IgnoreException) throw ex;
                return false;
            }
            finally
            {
                if (!(Service is null)) Service.Close();
            }
        }
        public bool MoveStdTxn(MoveStd ServiceObject, MoveStdService Service, bool IgnoreException = true)
        {
            string TxnId = Guid.NewGuid().ToString();
            try
            {
                string sMessage = "";
                MoveStd oServiceObject = null;
                ResultStatus oResultStatus = null;
                EventLogUtil.LogEvent(Logging.LoggingContainer(ServiceObject.Container.Name, TxnId, "Setting input data for MoveStd ..."), System.Diagnostics.EventLogEntryType.Information, 2);
                oServiceObject = ServiceObject;

                EventLogUtil.LogEvent(Logging.LoggingContainer(ServiceObject.Container.Name, TxnId, "Execution MoveStd ...."), System.Diagnostics.EventLogEntryType.Information, 2);
                oResultStatus = Service.ExecuteTransaction(oServiceObject);
                bool statusMoveStd = _helper.ProcessResult(oResultStatus, ref sMessage, false);
                EventLogUtil.LogEvent(Logging.LoggingContainer(ServiceObject.Container.Name, TxnId, sMessage), System.Diagnostics.EventLogEntryType.Information, 2);
                return statusMoveStd;
            }
            catch (Exception ex)
            {
                ex.Source = AppSettings.AssemblyName == ex.Source ? MethodBase.GetCurrentMethod().Name : MethodBase.GetCurrentMethod().Name + "." + ex.Source;
                EventLogUtil.LogErrorEvent(Logging.LoggingContainer(ServiceObject.Container.Name, TxnId, ex.Source), ex);
                if (!IgnoreException) throw ex;
                return false;
            }
            finally
            {
                if (!(Service is null)) Service.Close();
            }
        }
        public bool AttachDocumentTxn(AttachDocument ServiceObject, string TxnId, bool IgnoreException = true)
        {
            AttachDocumentService oService = null;
            try
            {
                string sMessage = "";
                AttachDocument oServiceObject = null;
                ResultStatus oResultStatus = null;
                oService = new AttachDocumentService(AppSettings.ExCoreUserProfile);
                EventLogUtil.LogEvent(Logging.LoggingContainer(ServiceObject.InstanceName.Value, TxnId, "Setting input data for AttachDocument ..."), System.Diagnostics.EventLogEntryType.Information, 2);

                // Setting InputObject
                oServiceObject = ServiceObject;

                // Execute Transaction
                EventLogUtil.LogEvent(Logging.LoggingContainer(ServiceObject.InstanceName.Value, TxnId, "Execution a AttachDocument"), System.Diagnostics.EventLogEntryType.Information, 2);
                oResultStatus = oService.ExecuteTransaction(oServiceObject);

                // Process Result
                bool statusMoveIn = _helper.ProcessResult(oResultStatus, ref sMessage, false);
                EventLogUtil.LogEvent(Logging.LoggingContainer(ServiceObject.InstanceName.Value, TxnId, sMessage), System.Diagnostics.EventLogEntryType.Information, 2);
                return statusMoveIn;
            }
            catch (Exception ex)
            {
                ex.Source = AppSettings.AssemblyName == ex.Source ? MethodBase.GetCurrentMethod().Name : MethodBase.GetCurrentMethod().Name + "." + ex.Source;
                string exceptionMsg = Logging.LoggingContainer(ServiceObject.InstanceName.Value, TxnId, ex.Message);
                EventLogUtil.LogErrorEvent(ex.Source, exceptionMsg);
                if (!IgnoreException) throw ex;
                return false;
            }
            finally
            {
                if (!(oService is null)) oService.Close();
            }
        }
        public ViewContainerStatus ContainerInfo(ViewContainerStatus_Info ContainerInfo, string ContainerName, bool IgnoreException = true)
        {
            string TxnId = Guid.NewGuid().ToString();
            ViewContainerStatusService oService = null;
            try
            {
                oService = new ViewContainerStatusService(AppSettings.ExCoreUserProfile);

                //Set input Data
                ViewContainerStatus oServiceObject = new ViewContainerStatus();
                oServiceObject.Container = new ContainerRef(ContainerName);
                ViewContainerStatus_Request oServiceRequest = new ViewContainerStatus_Request
                {
                    Info = ContainerInfo
                };

                //Request the Data
                ResultStatus oResultStatus = oService.ExecuteTransaction(oServiceObject, oServiceRequest, out ViewContainerStatus_Result oServiceResult);

                //Return Result
                string sMessage = "";
                bool processResult = _helper.ProcessResult(oResultStatus, ref sMessage, false);
                EventLogUtil.LogEvent(Logging.LoggingContainer(ContainerName, TxnId, sMessage), System.Diagnostics.EventLogEntryType.Information, 3);
                if (processResult)
                {
                    return oServiceResult.Value;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ex.Source = AppSettings.AssemblyName == ex.Source ? MethodBase.GetCurrentMethod().Name : MethodBase.GetCurrentMethod().Name + "." + ex.Source;
                string exceptionMsg = Logging.LoggingContainer(ContainerName, TxnId, ex.Message);
                EventLogUtil.LogErrorEvent(ex.Source, exceptionMsg);
                if (!IgnoreException) throw ex;
                return null;
            }
            finally
            {
                if (!(oService is null)) oService.Close();
            }
        }
        public CurrentContainerStatus ContainerStatusInfo(ContainerTxn ServiceObject, string ContainerName, bool IgnoreException = true)
        {
            string TxnId = Guid.NewGuid().ToString();
            ContainerTxnService oService = null;
            try
            {
                oService = new ContainerTxnService(AppSettings.ExCoreUserProfile);

                //Set input Data
                ContainerTxn oServiceObject = ServiceObject;
                ContainerTxn_Request oServiceRequest = new ContainerTxn_Request();
                oServiceRequest.Info = new ContainerTxn_Info();
                oServiceRequest.Info.CurrentContainerStatus = new CurrentContainerStatus_Info();
                oServiceRequest.Info.CurrentContainerStatus.RequestValue = true;

                //Requets the Data
                ResultStatus oResultStatus = oService.GetEnvironment(oServiceObject, oServiceRequest, out ContainerTxn_Result oServiceResult);

                //Return Result
                string sMessage = "";
                bool processResult = _helper.ProcessResult(oResultStatus, ref sMessage, false);
                EventLogUtil.LogEvent(Logging.LoggingContainer(ContainerName, TxnId, sMessage), System.Diagnostics.EventLogEntryType.Information, 3);
                if (processResult)
                {
                    return oServiceResult.Value.CurrentContainerStatus;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                ex.Source = AppSettings.AssemblyName == ex.Source ? MethodBase.GetCurrentMethod().Name : MethodBase.GetCurrentMethod().Name + "." + ex.Source;
                string exceptionMsg = Logging.LoggingContainer(ContainerName, TxnId, ex.Message);
                EventLogUtil.LogErrorEvent(ex.Source, exceptionMsg);
                if (!IgnoreException) throw ex;
                return null;
            }
            finally
            {
                if (!(oService is null)) oService.Close();
            }
        }
    }
}
