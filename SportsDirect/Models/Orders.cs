using Newtonsoft.Json;
using System.Collections.Generic;

namespace SportsDirect.Models
{
    public class Orders
    {
        [JsonProperty(PropertyName = "id")]
        public string OrderId { get; set; }

        [JsonProperty(PropertyName = "UserId")]
        public string OrderUserId { get; set; }

        [JsonProperty(PropertyName = "Status")]
        public string OrderStatus { get; set; }

        [JsonProperty(PropertyName = "OrderedProducts")]
        public List<string> OrderedProducts { get; set; }
    }
}