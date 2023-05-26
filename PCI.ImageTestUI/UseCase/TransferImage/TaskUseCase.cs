using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCI.ImageTestUI.UseCase
{
    public class TaskUseCase
    {
        public Queue<Entity.Task> GeneratePendingQueueTask(Dictionary<string, Entity.Task> tasks)
        {
            Queue<Entity.Task> queue = new Queue<Entity.Task>();
            foreach (var task in tasks)
            {
                if (task.Value is null) continue;
                if (task.Value.TaskName is null) continue;
                if (task.Value.Id is null) continue;
                if (task.Value.IsDone == true) continue;

                queue.Enqueue(task.Value);
            }
            return queue;
        }

        public void ChangeListTask(ref Dictionary<string, Entity.Task> list, Entity.Task task)
        {
            if (list.ContainsKey(task.Id))
            {
                var getTask = list[task.Id];
                getTask.IsDone = true;
                list[task.Id] = getTask;
            }
        }

        public ListView.ListViewItemCollection GenerateListView(Dictionary<string, Entity.Task> Tasks, ListView owner)
        {
            ListView.ListViewItemCollection listViewItemCollection = new ListView.ListViewItemCollection(owner);

            foreach (var item in Tasks)
            {
                if (item.Value == null) continue;
                if (item.Value.TaskName == "" || item.Value.TaskName is null) continue;

                string[] data = { item.Value.TaskName };
                var listViewItem = new ListViewItem(data)
                {
                    Checked = item.Value.IsDone
                };
                listViewItemCollection.Add(listViewItem);
            }
            return listViewItemCollection;
        }
        public string GenerateTaskMsgForImage(Dictionary<string, Entity.Task> taskList, Queue<Entity.Task> queue)
        {
            var taskName = "";
            var currentTask = taskList.Count() - queue.Count();
            if (taskList != null)
            {
                taskName = queue.Peek().TaskName == null ? "" : queue.Peek().TaskName + $" ({currentTask + 1} of {taskList.Count()})";
            }
            return taskName;
        }
    }
}
