using PCI.ImageTestUI.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.ImageTestUI.Test
{
    public static class ContainerDataMock
    {
        public static ContainerModel GenerateContainerDataMock
        {
            get
            {
                return new ContainerModel()
                {
                    Product = "Versana Model Test 1",
                    Operation = "Visual Labelling",
                    ProductDescription = "Versana Model Test export to China",
                    Qty = "1",
                    TaskList = GetTaskList,
                };
            }
        }

        public static List<Entity.Task> GetTaskList
        {
            get
            {
                return new List<Entity.Task>()
                    {
                        new Entity.Task() { TaskName = "4C-RS Noise and Phantom check at B/CHI mode" },
                        new Entity.Task() { TaskName = "4C-RS Noise and Phantom check at CF/PW mode" },
                        new Entity.Task() { TaskName = "12-RS Noise and Phantom check at CWD mode" },
                        new Entity.Task() { TaskName = "L3-12-RS Noise Check at B/CHI mode" }
                    };
            }
        }
    }
}
