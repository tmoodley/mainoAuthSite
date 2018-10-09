using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace SampleMvcApp.Controllers
{
    public class HomeController : Controller
    {
        public IConfiguration Configuration { get; }
        public HomeController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            // If the user is authenticated, then this is how you can get the access_token and id_token
            if (User.Identity.IsAuthenticated)
            {
                string accessToken = await HttpContext.GetTokenAsync("access_token");
                string idToken = await HttpContext.GetTokenAsync("id_token");

                var token = await GetTokenAsync();
                var obj = await GetUserIdAsync(token, accessToken);
                string userName = obj["username"];
                HttpContext.Session.SetString("auth0UserName", userName);
                var equitable_id = obj["user_metadata"];
                string equitableId = equitable_id["equitable_id"];
                HttpContext.Session.SetString("equitable_id", equitableId);
                ViewBag.UserName = HttpContext.Session.GetString("auth0UserName");
                ViewBag.EquitableId = equitableId;
            }

            return View();
        }

        public IActionResult About()
        { 
            ViewBag.UserName = HttpContext.Session.GetString("auth0UserName"); 
            var equitableId = HttpContext.Session.GetString("equitable_id");
            ViewBag.EquitableId = equitableId;

            return View();
        }


        private static async Task<dynamic> GetUserAsync(string userId, string token)
        {
            var client = new RestClient("https://mainsite.auth0.com/api/v2/users/" + userId);
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer " + token);

            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();

            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));

            RestResponse response = (RestResponse)(await taskCompletion.Task);

            dynamic blogObject = JsonConvert.DeserializeObject(response.Content);
           
            return blogObject;
        }

        private static async Task<dynamic> GetUserIdAsync(string token, string accessToken)
        {
            var client = new RestClient("https://mainsite.auth0.com/userinfo");
            var request = new RestRequest(Method.GET);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer " + accessToken); 
          
            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();

            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));

            RestResponse response = (RestResponse)(await taskCompletion.Task);

            dynamic blogObject = JsonConvert.DeserializeObject(response.Content);
            string userId = blogObject["sub"];

            Task<dynamic> obj = GetUserAsync(userId, token);
            return await obj;
        }
          
        public async Task<string> GetTokenAsync()
        {
            var client = new RestClient("https://mainsite.auth0.com/oauth/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            // Configure the Auth0 Client ID and Client Secret 
            string client_id = Configuration["Auth0:ClientId"];
            string client_secret = Configuration["Auth0:ClientSecret"];
            request.AddParameter("application/json", "{\"client_id\":\"" + client_id
                                + "\",\"client_secret\":\"" + client_secret
                                + "\",\"audience\":\"https://mainsite.auth0.com/api/v2/\",\"grant_type\":\"client_credentials\"}", ParameterType.RequestBody);

            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();

            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));

            RestResponse response = (RestResponse)(await taskCompletion.Task);

            dynamic blogObject = JsonConvert.DeserializeObject(response.Content);
            string token = blogObject["access_token"];
            return token;
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
