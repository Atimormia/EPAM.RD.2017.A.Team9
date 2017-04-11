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
        private readonly ReaderWriterLockSlim userIdlocker = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim deletinglocker = new ReaderWriterLockSlim();
        private readonly ReaderWriterLockSlim updatinglocker = new ReaderWriterLockSlim();
        private DbContext context = new ToDoDataBaseEntities();
        private static List<ToDoItem> deletingQueue = new List<ToDoItem>();
        private static List<ToDoItem> updatingQueue = new List<ToDoItem>();
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
            if (todo.ToDoId == 0)
            {
                updatinglocker.EnterWriteLock();
                try
                {
                    updatingQueue.Add(todo);
                }
                finally
                {
                    updatinglocker.ExitWriteLock();
                }
            }
            else
            {
                Task.Run(() => todoService.UpdateItem(currentTask));
            }
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
                deletinglocker.EnterWriteLock();
                try
                {
                    deletingQueue.Add(currentTask);
                }
                finally
                {
                    deletinglocker.ExitWriteLock();
                }
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
            Task.Run(() =>
            {
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
            userIdlocker.EnterReadLock();
            try
            {
                if (usersId.Contains(id))
                    return;
            }
            finally
            {
                userIdlocker.ExitReadLock();
            }
            usersId.Add(id);
            UpdateDB(id);
        }

        private void UpdateDB(int id)
        {
            userIdlocker.EnterWriteLock();
            try
            {
                var currentTasks = todoService.GetItems(id);
                foreach (var task in currentTasks)
                {
                    var str = new StringBuilder(task.Name);
                    while (str[str.Length - 1] == ' ')
                    {
                        str.Remove(str.Length - 1, 1);
                    }
                    task.Name = str.ToString();
                    var currTask = context.Set<ToDoItem>().FirstOrDefault(t => t.ToDoId == task.ToDoId);
                    if (currTask != null)
                    {
                        ToDoItem taskForUpdate = null;
                        updatinglocker.EnterReadLock();
                        try
                        {
                            taskForUpdate = updatingQueue.FirstOrDefault(t => t.Id == task.Id);
                        }
                        finally
                        {
                            updatinglocker.ExitReadLock();
                        }
                        if (taskForUpdate != null)
                        {
                            Task.Run(() => todoService.UpdateItem(taskForUpdate));
                            updatinglocker.EnterWriteLock();
                            try
                            {
                                updatingQueue.Remove(taskForUpdate);
                            }
                            finally
                            {
                                updatinglocker.ExitWriteLock();
                            }
                        }
                    }
                    else
                    {
                        currTask = context.Set<ToDoItem>().FirstOrDefault(t => t.ToDoId == 0);
                        if (currTask != null)
                        {
                            currTask.ToDoId = task.ToDoId;
                        }
                        else
                        {
                            ToDoItem taskForRemove = null;
                            deletinglocker.ExitReadLock();
                            try
                            {
                                deletingQueue.FirstOrDefault(t => t.Name == task.Name && t.IsCompleted == task.IsCompleted);
                            }
                            finally
                            {
                                deletinglocker.ExitReadLock();
                            }
                            if (taskForRemove != null)
                            {
                                deletinglocker.EnterWriteLock();
                                try
                                {
                                    deletingQueue.Remove(taskForRemove);
                                }
                                finally
                                {
                                    deletinglocker.ExitWriteLock();
                                }
                                Task.Run(() => todoService.DeleteItem((int)task.ToDoId));
                            }
                            else
                            {
                                context.Set<ToDoItem>().Add(task);
                                context.SaveChanges();
                            }
                        }
                    }
                }
            }
            finally
            {
                userIdlocker.ExitWriteLock();
            }
        }
    }
}