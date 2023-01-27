using Camstar.WCF.ObjectStack;
using Camstar.WCF.Services;
using PCI.ImageTestUI.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PCI.ImageTestUI.Driver.Opcenter
{
    public class Helper
    {
        public string LastResultMessage = "";
        public bool ProcessResult(ResultStatus Result, ref string ResultMessage, bool IgnoreException = true)
        {
            try
            {
                ResultMessage = "";
                if (Result is null) { return false; }
                if (!Result.IsSuccess)
                {
                    ExceptionDataType oExceptionData = Result.ExceptionData;
                    if (oExceptionData is null)
                    {
                        ResultMessage = Result.Message;
                    }
                    else
                    {
                        ResultMessage = oExceptionData.Description;
                    }
                    throw new Exception(ResultMessage);
                }
                else
                {
                    ResultMessage = Result.Message;
                }
                return Result.IsSuccess;
            }
            catch (Exception ex)
            {
                ex.Source = AppSettings.AssemblyName == ex.Source ? MethodBase.GetCurrentMethod().Name : MethodBase.GetCurrentMethod().Name + "." + ex.Source;
                if (!IgnoreException) throw (ex);
                return false;
            }
            finally
            {
                LastResultMessage = ResultMessage;
            }
        }
        public bool ObjectExists(dynamic ServiceRef, dynamic ServiceObject, string Name)
        {
            string sMessage = "";
            ResultStatus oResultStatus;
            ServiceObject.ObjectToChange = new NamedObjectRef(Name);
            oResultStatus = ServiceRef.Load(ServiceObject);
            return ProcessResult(oResultStatus, ref sMessage, true);
        }
        public bool ObjectExists(dynamic ServiceRef, dynamic ServiceObject, string Name, string Revision)
        {
            ResultStatus oResultStatus;
            if (Revision != "")
            {
                ServiceObject.ObjectToChange = new RevisionedObjectRef(Name, Revision);
            }
            else
            {
                ServiceObject.ObjectToChange = new RevisionedObjectRef(Name);
            }
            oResultStatus = ServiceRef.Load(ServiceObject);
            string sMessage = "";
            return ProcessResult(oResultStatus, ref sMessage, true);
        }
        public DataPointSummary GetDataPointSummaryRef(dynamic Service, dynamic ServiceObject, dynamic ServiceObject_Request, dynamic ServiceObject_Info, ref string DataCollectionName, ref string DataCollectionRev)
        {
            string sMessage = "";
            MoveIn_Result oServiceObject_Result = null;
            ServiceObject_Request.Info = ServiceObject_Info;
            if (DataCollectionName != "")
            {
                ServiceObject.DataCollectionDef = new RevisionedObjectRef() { Name = DataCollectionName, Revision = DataCollectionRev, RevisionOfRecord = (DataCollectionRev == "") };
            }
            else
            {
                ServiceObject_Request.Info.DataCollectionDef = new Info(true);
            }
            ServiceObject_Request.Info.ParametricData = new ParametricData_Info();
            ServiceObject_Request.Info.ParametricData.RequestValue = true;
            ResultStatus oResultStatus = Service.GetDataPoints(ServiceObject, ServiceObject_Request, out oServiceObject_Result);
            if (ProcessResult(oResultStatus, ref sMessage, false))
            {
                if (oServiceObject_Result.Value != null)
                {
                    if ((oServiceObject_Result.Value.DataCollectionDef != null) && (oServiceObject_Result.Value.ParametricData != null))
                    {
                        var withBlock = (DataPointSummary)oServiceObject_Result.Value.ParametricData;
                        if (withBlock.DataPointDetails.Count() > 0)
                        {
                            dynamic oDataCollectionDef = withBlock.DataPointDetails[0].DataPoint.Parent;
                            DataCollectionName = oDataCollectionDef.Name;
                            DataCollectionRev = oDataCollectionDef.Revision;
                            return (DataPointSummary)oServiceObject_Result.Value.ParametricData;
                        }
                    }
                }
            }
            return null;
        }
        public DataPointSummary SetDataPointSummary(DataPointSummary DataPointSummaryRef, DataPointDetails[] DataPoints)
        {
            DataPointSummary oDataPointSummary = null;
            string sDataName = "";
            foreach (DataPointDetails oDataPointRef in DataPointSummaryRef.DataPointDetails)
            {
                foreach (DataPointDetails oDataPoint in DataPoints)
                {
                    if (!(oDataPoint is null))
                    {
                        sDataName = "";
                        if (oDataPoint.DataName != "")
                        {
                            sDataName = (string)oDataPoint.DataName;
                        }
                        else if (!(oDataPoint.DataPoint is null))
                        {
                            sDataName = oDataPoint.DataPoint.Name;
                        }
                        if ((sDataName != "") && (sDataName == oDataPointRef.DataName))
                        {
                            if (oDataPointSummary is null)
                            {
                                oDataPointSummary = new DataPointSummary() { FieldAction = Camstar.WCF.ObjectStack.Action.Create };
                            }
                            if (oDataPointSummary.DataPointDetails is null)
                            {
                                var objDataPointDetails = oDataPointSummary.DataPointDetails;
                                Array.Resize(ref objDataPointDetails, 1);
                                oDataPointSummary.DataPointDetails = objDataPointDetails;
                            }
                            else
                            {
                                var objDataPointDetails = oDataPointSummary.DataPointDetails;
                                Array.Resize(ref objDataPointDetails, oDataPointSummary.DataPointDetails.Count() + 1);
                                oDataPointSummary.DataPointDetails = objDataPointDetails;
                            }
                            oDataPointSummary.DataPointDetails[oDataPointSummary.DataPointDetails.Count() - 1] = new DataPointDetails()
                            {
                                CDOTypeName = "DataPointDetails",
                                ListItemAction = ListItemAction.Add,
                                DataPoint = new NamedSubentityRef(sDataName) { Parent = oDataPointRef.DataPoint.Parent },
                                DataType = oDataPointRef.DataType,
                                DataValue = oDataPoint.DataValue
                            };
                        }
                    }
                }
            }
            return oDataPointSummary;
        }
        public DataPointSummary SetDataPointSummary(object DataCollectionRef, DataPointDetails[] DataPoints)
        {
            DataPointSummary oDataPointSummary = null;
            string sDataName = "";
            foreach (DataPointDetails oDataPoint in DataPoints)
            {
                if (!(oDataPoint is null))
                {
                    sDataName = "";
                    if (oDataPoint.DataName != "")
                    {
                        sDataName = (string)oDataPoint.DataName;
                    }
                    else if (!(oDataPoint.DataPoint is null))
                    {
                        sDataName = oDataPoint.DataPoint.Name;
                    }
                    if (sDataName != "")
                    {
                        if (oDataPointSummary is null)
                        {
                            oDataPointSummary = new DataPointSummary() { FieldAction = Camstar.WCF.ObjectStack.Action.Create };
                        }
                        if (oDataPointSummary.DataPointDetails is null)
                        {
                            var objDataPointDetails = oDataPointSummary.DataPointDetails;
                            Array.Resize(ref objDataPointDetails, 1);
                            oDataPointSummary.DataPointDetails = objDataPointDetails;
                        }
                        else
                        {
                            var objDataPointDetails = oDataPointSummary.DataPointDetails;
                            Array.Resize(ref objDataPointDetails, oDataPointSummary.DataPointDetails.Count() + 1);
                            oDataPointSummary.DataPointDetails = objDataPointDetails;
                        }
                        oDataPointSummary.DataPointDetails[oDataPointSummary.DataPointDetails.Count() - 1] = new DataPointDetails()
                        {
                            CDOTypeName = "DataPointDetails",
                            ListItemAction = ListItemAction.Add,
                            DataPoint = new NamedSubentityRef(sDataName) { Parent = (BaseObjectRef)DataCollectionRef },
                            DataType = oDataPoint.DataType,
                            DataValue = oDataPoint.DataValue
                        };
                    }
                }
            }
            return oDataPointSummary;
        }
    }
}
