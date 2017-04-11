using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using ToDoClient.Models;

namespace ToDoClient.Services
{
    /// <summary>
    /// Works with ToDo backend.
    /// </summary>
    public class ToDoService
    {
        /// <summary>
        /// The service URL.
        /// </summary>
        private readonly string serviceApiUrl = ConfigurationManager.AppSettings["ToDoServiceUrl"];

        /// <summary>
        /// The url for getting all todos.
        /// </summary>
        private const string GetAllUrl = "ToDos?userId={0}";

        /// <summary>
        /// The url for updating a todo.
        /// </summary>
        private const string UpdateUrl = "ToDos";

        /// <summary>
        /// The url for a todo's creation.
        /// </summary>
        private const string CreateUrl = "ToDos";

        /// <summary>
        /// The url for a todo's deletion.
        /// </summary>
        private const string DeleteUrl = "ToDos/{0}";

        private readonly HttpClient httpClient;

        /// <summary>
        /// Creates the service.
        /// </summary>
        public ToDoService()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Gets all todos for the user.
        /// </summary>
        /// <param name="userId">The User Id.</param>
        /// <returns>The list of todos.</returns>
        public IList<ToDoItemViewModel> GetItems(int userId)
        {
            var dataAsString = httpClient.GetStringAsync(string.Format(serviceApiUrl + GetAllUrl, userId)).Result;
            return JsonConvert.DeserializeObject<IList<ToDoItemViewModel>>(dataAsString);
        }

        /// <summary>
        /// Creates a todo. UserId is taken from the model.
        /// </summary>
        /// <param name="item">The todo to create.</param>
        public void CreateItem(ToDoItemViewModel item)
        {
            httpClient.PostAsJsonAsync(serviceApiUrl + CreateUrl, item)
                .Result.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Updates a todo.
        /// </summary>
        /// <param name="item">The todo to update.</param>
        public void UpdateItem(ToDoItemViewModel item)
        {
            httpClient.PutAsJsonAsync(serviceApiUrl + UpdateUrl, item)
                .Result.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Deletes a todo.
        /// </summary>
        /// <param name="id">The todo Id to delete.</param>
        public void DeleteItem(int id)
        {
            httpClient.DeleteAsync(string.Format(serviceApiUrl + DeleteUrl, id))
                .Result.EnsureSuccessStatusCode();
        }
    }
}