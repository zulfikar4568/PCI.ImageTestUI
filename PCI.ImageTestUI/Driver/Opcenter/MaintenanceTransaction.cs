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
    public class MaintenanceTransaction
    {
        private readonly Helper _helper;
        public MaintenanceTransaction(Helper helper)
        {
            _helper = helper;
        }
        public bool SaveDocument(DocumentChanges ObjectChanges, bool IgnoreException = true)
        {
            DocumentMaintService oService = null;
            try
            {
                DocumentMaint oServiceObject = null;
                oService = new DocumentMaintService(AppSettings.ExCoreUserProfile);
                EventLogUtil.LogEvent($"Check Document {ObjectChanges.Name} : {ObjectChanges.Revision}...", System.Diagnostics.EventLogEntryType.Information, 3);
                bool bBaseExists = _helper.ObjectExists(oService, new DocumentMaint(), ObjectChanges.Name.ToString(), "");
                bool bObjectExists = _helper.ObjectExists(oService, new DocumentMaint(), ObjectChanges.Name.ToString(), ObjectChanges.Revision.ToString());
                EventLogUtil.LogEvent($"Prepare Document {ObjectChanges.Name} : {ObjectChanges.Revision}...", System.Diagnostics.EventLogEntryType.Information, 3);
                if (bObjectExists)
                {
                    oServiceObject.ObjectToChange = new RevisionedObjectRef(ObjectChanges.Name.ToString(), ObjectChanges.Revision.ToString());
                    oService.BeginTransaction();
                    oService.Load(oServiceObject);
                }
                else if (bBaseExists)
                {
                    oService.BeginTransaction();
                    oServiceObject.BaseToChange = new RevisionedObjectRef();
                    oServiceObject.BaseToChange.Name = ObjectChanges.Name.ToString();
                    oService.NewRev(oServiceObject);
                }

                oServiceObject = new DocumentMaint();
                oServiceObject.ObjectChanges = ObjectChanges;

                //Save the Data
                if (bObjectExists)
                {
                    EventLogUtil.LogEvent($"Updating Document {ObjectChanges.Name} : {ObjectChanges.Revision}...", System.Diagnostics.EventLogEntryType.Information, 3);
                    oService.ExecuteTransaction(oServiceObject);
                }
                else if (bBaseExists)
                {
                    EventLogUtil.LogEvent($"Creating Document {ObjectChanges.Name} : {ObjectChanges.Revision}...", System.Diagnostics.EventLogEntryType.Information, 3);
                    oService.ExecuteTransaction(oServiceObject);
                }
                else
                {
                    EventLogUtil.LogEvent($"Creating Document {ObjectChanges.Name} : {ObjectChanges.Revision}...", System.Diagnostics.EventLogEntryType.Information, 3);
                    oService.BeginTransaction();
                    oService.New(oServiceObject);
                    oService.ExecuteTransaction();
                }
                string sMessage = "";
                bool statusDocument = _helper.ProcessResult(oService.CommitTransaction(), ref sMessage, false);
                EventLogUtil.LogEvent(sMessage, System.Diagnostics.EventLogEntryType.Information, 2);
                return statusDocument;
            }
            catch (Exception ex)
            {
                ex.Source = AppSettings.AssemblyName == ex.Source ? MethodBase.GetCurrentMethod().Name : MethodBase.GetCurrentMethod().Name + "." + ex.Source;
                EventLogUtil.LogErrorEvent(ex.Source, ex);
                if (!IgnoreException) throw ex;
                return false;
            }
            finally
            {
                if (oService != null) oService.Close();
            }
        }

    }
}
