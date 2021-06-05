using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Microsoft.Graph;
using TeamsPresenceLight_PhilipsHue.Common;
using System.Net.Http;
using System.Linq;
using System.Text;

namespace TeamsPresenceLight_PhilipsHue
{
    public static class Teams_Presence
    {
        private static GraphServiceClient graphClient;
        private static string azureADappId = Configuration.AzureAdClientId;
        private static string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

        [FunctionName("Teams_Presence")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // parse query parameter
            var validationToken = req.Query["validationToken"];
            if (!string.IsNullOrEmpty(validationToken))
            {
                log.LogInformation("validationToken: " + validationToken);
                return new ContentResult { Content = validationToken, ContentType = "text/plain" };
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<Notifications>(requestBody);
            if (!data.Value.FirstOrDefault().ClientState.Equals("SecretClientState", StringComparison.OrdinalIgnoreCase))
            {
                //client state is not valid (doesn't match the one submitted with the subscription)
                return new BadRequestResult();
            }
            
            // Initialize the auth provider to get SharePoint List item values using Client Credentials flow

            var authProvider = new ClientCredentialsAuthProvider(azureADappId, scopes);
            graphClient = new GraphServiceClient(authProvider);

            var spListItems = await graphClient.Sites[Configuration.siteResourceorId].Lists[Configuration.listNameorId].Items.Request().Expand("Fields($select=Title,value)").GetAsync();
            var accessTokenPhilipsHue = GetConfigurationListValue(spListItems, "accessToken");
            var userNamePhilipsHue = GetConfigurationListValue(spListItems, "userName");
            var lightNoPhilipsHue = GetConfigurationListValue(spListItems, "lightNo");

            // Change Philips Hue light colors based on users teams presence 
            foreach (var notification in data.Value)
            {
                var userTeamsAvailabilityStatus = notification.ResourceData?.Availability;
                log.LogInformation(userTeamsAvailabilityStatus);
                await TurnOnPhilipsHue(userTeamsAvailabilityStatus, accessTokenPhilipsHue, userNamePhilipsHue, lightNoPhilipsHue);
            }

            return new OkResult();
        }
        private static string GetConfigurationListValue(IListItemsCollectionPage spListItems, string configValue)
        {
            var configItem = from listItem in spListItems
                             where listItem.Fields.AdditionalData["Title"].ToString() == configValue
                             select new { listItemColumnValue = listItem.Fields.AdditionalData["value"].ToString() };
            return configItem.FirstOrDefault().listItemColumnValue;
        }

        private static async Task<string> TurnOnPhilipsHue(string availability, string accessTokenPhilipsHue, string userNamePhilipsHue, string lightNoPhilipsHue)
        {
            string requestUrl = "https://api.meethue.com/bridge/" + userNamePhilipsHue + "/lights/"+ lightNoPhilipsHue + "/state";
            using var client = new HttpClient();

            var payload = "";
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessTokenPhilipsHue);
            if (availability == "Available")
            { payload = "{\"on\": true,\"xy\": [0.358189,0.556853],\"bri\": 102,\"transitiontime\": 0}"; }
            else if (availability == "Busy")
            { payload = "{\"on\": true,\"xy\": [0.626564,0.256591],\"bri\": 102,\"transitiontime\": 0}"; }
            else if (availability == "BusyIdle")
            { payload = "{\"on\": true,\"xy\": [0.626564,0.256591],\"bri\": 25,\"transitiontime\": 0}"; }
            else if (availability == "DoNotDisturb")
            { payload = "{\"on\": true,\"xy\": [0.726564,0.256591],\"bri\": 254,\"transitiontime\": 0}"; }
            else if (availability == "Away" || availability == "BeRightBack")
            { payload = "{\"on\": true,\"xy\": [0.517102,0.474840],\"bri\": 75,\"transitiontime\": 0}"; }
            else
                payload = "{\"on\": true,\"xy\": [0.3146,0.3303],\"bri\": 25,\"transitiontime\": 0}";

            var requestData = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(String.Format(requestUrl), requestData);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
    }
}
