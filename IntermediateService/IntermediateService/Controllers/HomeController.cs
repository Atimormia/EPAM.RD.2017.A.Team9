using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using ToDoClient.Services;

namespace IntermediateService.Controllers
{
    public class HomeController : Controller
    {

        private readonly ToDoService todoService = new ToDoService();
        private readonly UserService userService = new UserService();

        public JsonResult GetTasks(int? userIdFromCookie)
        {
            var userId = userService.GetOrCreateUser(userIdFromCookie);
            return Json(todoService.GetItems(userId),JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Updates the existing todo-item.
        /// </summary>
        /// <param name="todo">The todo-item to update.</param>
        public void UpdateTask(Task todo)
        {
            todo.UserId = userService.GetOrCreateUser();
            todoService.UpdateItem(todo);
        }
    }
}