using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TeamsPresenceLight_PhilipsHue.Common
{
    public class Notifications
    {
        public List<Notification> Value { get; set; }
    }
    public class Notification
    {
        // The type of change.
        [JsonProperty(PropertyName = "changeType")]
        public string ChangeType { get; set; }

        // The client state used to verify that the notification is from Microsoft Graph. Compare the value received with the notification to the value you sent with the subscription request.
        [JsonProperty(PropertyName = "clientState")]
        public string ClientState { get; set; }

        // The endpoint of the resource that changed. 
        [JsonProperty(PropertyName = "resource")]
        public string Resource { get; set; }

        // The UTC date and time when the webhooks subscription expires.
        [JsonProperty(PropertyName = "subscriptionExpirationDateTime")]
        public DateTimeOffset SubscriptionExpirationDateTime { get; set; }

        // The unique identifier for the webhooks subscription.
        [JsonProperty(PropertyName = "subscriptionId")]
        public string SubscriptionId { get; set; }
        // Properties of the changed resource.
        [JsonProperty(PropertyName = "resourceData")]
        public ResourceData ResourceData { get; set; }

    }
    public class ResourceData
    {
        // The UPN ID of the teams user
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        // The OData ID of the  teams user
        [JsonProperty(PropertyName = "@odata.id")]
        public string ODataId { get; set; }

        // The OData type of the resource: "#Microsoft.Graph.presence".
        [JsonProperty(PropertyName = "@odata.type")]
        public string ODataType { get; set; }

        // Users activity in Teams
        [JsonProperty(PropertyName = "activity")]
        public string Activity { get; set; }
        // Users Availability or Presence status in Teams
        [JsonProperty(PropertyName = "availability")]
        public string Availability { get; set; }

    }
}
