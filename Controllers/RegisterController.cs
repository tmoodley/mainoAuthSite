using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using SampleMvcApp.Models;

namespace SampleMvcApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        public IConfiguration Configuration { get; }
        public RegisterController(IConfiguration configuration)
        {
            Configuration = configuration; 
        }
        // GET: api/Regiser
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Regiser/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Register  
        [HttpPost()]
        public async Task Post([FromBody]User user)
        {
            // Get Token from auth0
            string token = await GetTokenAsync();
            // Register user with auth0
            await RegisterUserAsync(user, token);
        }

        private async Task RegisterUserAsync(User user, string token)
        {
            string domain = Configuration["Auth0:Domain"];
            var client = new RestClient(domain + "/api/v2/users");
            RestRequest request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer " + token);

            request.AddParameter("application/json",
                                "{\"email\": \"" + user.Email
                                + "\",\"password\": \"" + user.Password
                                + "\",\"username\": \"" + user.Username
                                + "\",\"name\": \"" + user.Name
                                + "\",\"connection\": \"" + user.Connection
                                + "\", \"user_metadata\": {\"type\": \"plan-member\"}, \"app_metadata\": {\"equitableId\": \"132456\"}}",
                                ParameterType.RequestBody);
            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();

            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));

            RestResponse response = (RestResponse)(await taskCompletion.Task);
        }

        public async Task<string> GetTokenAsync()
        {
            string domain = Configuration["Auth0:Domain"];
            var client = new RestClient(domain + "/oauth/token");
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/json");
            // Configure the Auth0 Client ID and Client Secret 
            string client_id = Configuration["Auth0:ClientId"];
            string client_secret = Configuration["Auth0:ClientSecret"];
            request.AddParameter("application/json", "{\"client_id\":\"" + client_id
                                + "\",\"client_secret\":\"" + client_secret
                                + "\",\"audience\":\"https://equitablelife.auth0.com/api/v2/\",\"grant_type\":\"client_credentials\"}", ParameterType.RequestBody);
            
            TaskCompletionSource<IRestResponse> taskCompletion = new TaskCompletionSource<IRestResponse>();

            RestRequestAsyncHandle handle = client.ExecuteAsync(request, r => taskCompletion.SetResult(r));

            RestResponse response = (RestResponse)(await taskCompletion.Task);

            dynamic blogObject = JsonConvert.DeserializeObject(response.Content);
            string token = blogObject["access_token"];
            return token;
        }

        // PUT: api/Regiser/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
