using ORM;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Threading.Tasks;
using ToDoClient.Services;
using System.Text;

namespace IntermediateService.Controllers
{
    public class HomeController : Controller
    {

        private readonly ToDoService todoService = new ToDoService();
        private readonly UserService userService = new UserService();
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        private DbContext context = new ToDoDataBaseEntities();
        private static List<ToDoItem> deletingQueue = new List<ToDoItem>();
        private static List<int> usersId = new List<int>();

        /// <summary>
        /// Returns all todo-items for the current user.
        /// </summary>
        /// <returns>The list of todo-items.</returns>
        public JsonResult GetTasks(int userId)
        {
            CheckUserId(userId);
            return Json(context.Set<ToDoItem>().Where(t => t.UserId == userId).ToList(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the existing todo-item.
        /// </summary>
        /// <param name="todo">The todo-item to update.</param>
        public void UpdateTask(ToDoItem todo)
        {
            var currentTask = context.Set<ToDoItem>().First(t => t.Id == todo.Id);
            currentTask.IsCompleted = todo.IsCompleted;
            context.SaveChanges();
            Task.Run(() => todoService.UpdateItem(currentTask));
        }

        /// <summary>
        /// Deletes the specified todo-item.
        /// </summary>
        /// <param name="id">The todo item identifier.</param>
        public void Delete(int id)
        {
            var currentTask = context.Set<ToDoItem>().First(t => t.Id == id);
            int currentTaskToDoId = (int)currentTask.ToDoId;
            if (currentTaskToDoId == 0)
            {
                deletingQueue.Add(currentTask);
            }
            else
            {
                Task.Run(() => todoService.DeleteItem(currentTaskToDoId));
            }
            context.Set<ToDoItem>().Remove(currentTask);
            context.SaveChanges();
        }

        /// <summary>
        /// Creates a new todo-item.
        /// </summary>
        /// <param name="todo">The todo-item to create.</param>
        public void CreateTask(ToDoItem todo)
        {
            Task.Run(() => {
                todoService.CreateItem(todo);
                UpdateDB(todo.UserId);
            });
            context.Set<ToDoItem>().Add(todo);
            context.SaveChanges();
        }

        public JsonResult Users(string name)
        {
            int id = userService.CreateUser($"Noname: {name}");
            return Json(id, JsonRequestBehavior.AllowGet);
        }

        private void CheckUserId(int id)
        {
            locker.EnterReadLock();
            try
            {
                if (usersId.Contains(id))
                    return;
            }
            finally
            {
                locker.ExitReadLock();
            }
            usersId.Add(id);
            UpdateDB(id);
        }

        private void UpdateDB(int id)
        {
            locker.EnterWriteLock();
            try
            {
                var currentTasks = todoService.GetItems(id);
                foreach(var task in currentTasks)
                {
                    var str = new StringBuilder(task.Name);
                    while (str[str.Length-1]==' ')
                    {
                        str.Remove(str.Length - 1, 1);
                    }
                    task.Name = str.ToString();
                    var currTask = context.Set<ToDoItem>().FirstOrDefault(t => t.ToDoId == task.ToDoId);
                    if (currTask == null)
                    {
                        currTask = context.Set<ToDoItem>().FirstOrDefault(t => t.ToDoId == 0);
                    }
                    if (currTask != null)
                    {
                        currTask.ToDoId = task.ToDoId;
                    }
                    else
                    {
                        var taskForRemove = deletingQueue.FirstOrDefault(t => t.Name == task.Name && t.IsCompleted == task.IsCompleted);
                        if (taskForRemove != null)
                        {
                            deletingQueue.Remove(taskForRemove);
                            todoService.DeleteItem((int)task.ToDoId);
                        }
                        else
                        {
                            context.Set<ToDoItem>().Add(task);
                            context.SaveChanges();
                        }
                    }
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }
    }
}