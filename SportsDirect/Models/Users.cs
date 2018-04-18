using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SportsDirect.Models
{
    public class Users
    {
        [JsonProperty(PropertyName = "id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "FirstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "LastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "ShoppingCart")]
        public Dictionary<string, string> ShoppingCart { get; set; }

        [JsonProperty(PropertyName = "OrderHistory")]
        public List<string> OrderHistory { get; set; }
    }
}