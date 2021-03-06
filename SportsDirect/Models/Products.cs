using Newtonsoft.Json;
using System.Collections.Generic;

namespace SportsDirect.Models
{
    public class Products
    {
        [JsonProperty(PropertyName = "id")]
        public string ProductId { get; set; }

        [JsonProperty(PropertyName = "ProductName")]
        public string ProductName { get; set; }

        [JsonProperty(PropertyName = "ProductDetails")]
        public string ProductDetails { get; set; }

        [JsonProperty(PropertyName = "ProductCategory")]
        public string ProductCategory { get; set; }

        [JsonProperty(PropertyName = "ProductTags")]
        public List<string> ProductTags { get; set; }

        [JsonProperty(PropertyName = "ProductCost")]
        public int ProductCost { get; set; }

        [JsonProperty(PropertyName = "Featured")]
        public bool Featured { get; set; }
    }
}