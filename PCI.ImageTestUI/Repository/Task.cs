using Camstar.WCF.ObjectStack;
using PCI.ImageTestUI.Config;
using PCI.ImageTestUI.Repository.Opcenter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.ImageTestUI.Repository
{
    public class Task
    {
        private readonly MaintenanceTransaction _maintenanceTransaction;
        public Task(MaintenanceTransaction maintenanceTransaction) 
        {
            _maintenanceTransaction = maintenanceTransaction;
        }
        public Dictionary<string, Entity.Task> GetDataCollectionList()
        {
            Dictionary<string, Entity.Task> result = new Dictionary<string, Entity.Task>();
            var data = _maintenanceTransaction.GetUserDataCollectionDef(AppSettings.UserDataCollectionDefName, AppSettings.UserDataCollectionDefRevision);
            foreach (var dataPoint in data.DataPoints)
            {
                var Id = Guid.NewGuid().ToString();
                result.Add(Id, new Entity.Task() { TaskName = dataPoint.Name.ToString(), IsDone = false, Id = Id });
            }
            return result;
        }
    }
}
