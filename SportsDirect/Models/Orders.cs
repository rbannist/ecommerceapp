using Newtonsoft.Json;
using System.Collections.Generic;

namespace SportsDirect.Models
{
    public class Orders
    {
        [JsonProperty(PropertyName = "id")]
        public int OrderId { get; set; }

        [JsonProperty(PropertyName = "OrderedProducts")]
        public List<string> OrderedProducts { get; set; }
    }
}