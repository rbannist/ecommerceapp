using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SportsDirect
{
    public class AzureADB2CGraphClient
    {
        //Config initialiser for ADB2CGraphOptions
        AzureADB2CGraphOptions _azureADB2CGraphOptions;
        public AzureADB2CGraphClient(IOptions<AzureADB2CGraphOptions> azureADB2CGraphOptions)
        {
            _azureADB2CGraphOptions = azureADB2CGraphOptions.Value;
        }

        private string clientId { get; set; }
        private string clientSecret { get; set; }
        private string tenant { get; set; }

        private AuthenticationContext authContext;
        private ClientCredential credential;

        public AzureADB2CGraphClient()
        {
            // The client_id, client_secret, and tenant are pulled in from the App.config file
            this.clientId = _azureADB2CGraphOptions.ClientId;
            this.clientSecret = _azureADB2CGraphOptions.ClientSecret;
            this.tenant = _azureADB2CGraphOptions.Tenant;

            // The AuthenticationContext is ADAL's primary class, in which you indicate the directory to use.
            this.authContext = new AuthenticationContext("https://login.microsoftonline.com/" + tenant);

            // The ClientCredential is where you pass in your client_id and client_secret, which are 
            // provided to Azure AD in order to receive an access_token using the app's identity.
            this.credential = new ClientCredential(clientId, clientSecret);
        }

        public async Task<string> GetUserByObjectId(string objectId)
        {
            return await SendGraphGetRequest("/users/" + objectId, null);
        }

        public async Task<string> GetUserSignInNameByObjectId(string objectId)
        {
            return await SendGraphGetRequest("/users/" + objectId + "/signInNames", null);
        }

        public async Task<string> GetAllUsers(string query)
        {
            return await SendGraphGetRequest("/users", query);
        }

        public async Task<string> CreateUser(string json)
        {
            return await SendGraphPostRequest("/users", json);
        }

        public async Task<string> UpdateUser(string userUPN, string json)
        {
            return await SendGraphPatchRequest("/users/" + userUPN, json);
        }

        public async Task<string> DeleteUser(string objectId)
        {
            return await SendGraphDeleteRequest("/users/" + objectId);
        }

        public async Task<string> RegisterExtension(string objectId, string body)
        {
            return await SendGraphPostRequest("/applications/" + objectId + "/extensionProperties", body);
        }

        public async Task<string> UnregisterExtension(string appObjectId, string extensionObjectId)
        {
            return await SendGraphDeleteRequest("/applications/" + appObjectId + "/extensionProperties/" + extensionObjectId);
        }

        public async Task<string> GetExtensions(string appObjectId)
        {
            return await SendGraphGetRequest("/applications/" + appObjectId + "/extensionProperties", null);
        }

        public async Task<string> GetApplications(string query)
        {
            return await SendGraphGetRequest("/applications", query);
        }

        private async Task<string> SendGraphDeleteRequest(string api)
        {
            // NOTE: This client uses ADAL v2, not ADAL v4
            AuthenticationResult result = await authContext.AcquireTokenAsync(_azureADB2CGraphOptions.GraphResourceId, credential);
            HttpClient http = new HttpClient();
            string url = _azureADB2CGraphOptions.GraphEndpoint + tenant + api + "?" + _azureADB2CGraphOptions.GraphVersion;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await http.SendAsync(request);


            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> SendGraphPatchRequest(string api, string json)
        {
            // NOTE: This client uses ADAL v2, not ADAL v4
            AuthenticationResult result = await authContext.AcquireTokenAsync(_azureADB2CGraphOptions.GraphResourceId, credential);
            HttpClient http = new HttpClient();
            string url = _azureADB2CGraphOptions.GraphEndpoint + tenant + api + "?" + _azureADB2CGraphOptions.GraphVersion;

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> SendGraphPostRequest(string api, string json)
        {
            // NOTE: This client uses ADAL v2, not ADAL v4
            AuthenticationResult result = await authContext.AcquireTokenAsync(_azureADB2CGraphOptions.GraphResourceId, credential);
            HttpClient http = new HttpClient();
            string url = _azureADB2CGraphOptions.GraphEndpoint + tenant + api + "?" + _azureADB2CGraphOptions.GraphVersion;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> SendGraphGetRequest(string api, string query)
        {
            // First, use ADAL to acquire a token using the app's identity (the credential)
            // The first parameter is the resource we want an access_token for; in this case, the Graph API.
            AuthenticationResult result = await authContext.AcquireTokenAsync("https://graph.windows.net", credential);

            // For B2C user managment, be sure to use the 1.6 Graph API version.
            HttpClient http = new HttpClient();
            string url = "https://graph.windows.net/" + tenant + api + "?" + _azureADB2CGraphOptions.GraphVersion;
            if (!string.IsNullOrEmpty(query))
            {
                url += "&" + query;
            }

            // Append the access token for the Graph API to the Authorization header of the request, using the Bearer scheme.
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
            HttpResponseMessage response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                object formatted = JsonConvert.DeserializeObject(error);
                throw new WebException("Error Calling the Graph API: \n" + JsonConvert.SerializeObject(formatted, Formatting.Indented));
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
