using Camstar.WCF.ObjectStack;
using PCI.ImageTestUI.Config;
using PCI.ImageTestUI.Repository.Opcenter;
using System;
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
        public List<Entity.Task> GetDataCollectionList()
        {
            List<Entity.Task> result = new List<Entity.Task>();
            var data = _maintenanceTransaction.GetUserDataCollectionDef(AppSettings.UserDataCollectionDefName, AppSettings.UserDataCollectionDefRevision);
            foreach (var dataPoint in data.DataPoints)
            {
                result.Add(new Entity.Task() { TaskName = dataPoint.Name.ToString() });
            }
            return result;
        }
    }
}
