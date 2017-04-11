using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace ToDoClient.Services
{
    /// <summary>
    /// Works with Users backend.
    /// </summary>
    public class UserService
    {
        /// <summary>
        /// The service URL.
        /// </summary>
        private readonly string serviceApiUrl = ConfigurationManager.AppSettings["ToDoServiceUrl"];

        /// <summary>
        /// The url for users' creation.
        /// </summary>
        private const string CreateUrl = "Users";

        private readonly HttpClient httpClient;

        /// <summary>
        /// Creates the service.
        /// </summary>
        public UserService()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Creates a user.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <returns>The User Id.</returns>
        public int CreateUser(string userName)
        {
            var response = httpClient.PostAsJsonAsync(serviceApiUrl + CreateUrl, userName).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsAsync<int>().Result;
        }
    }
}