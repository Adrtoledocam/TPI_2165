using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPI_ArcaludoApp.Models
{
    public class User
    {
        [JsonProperty("id")]
        public int UseId { get; set; }

        [JsonProperty("username")]
        public string UseUsername { get; set; }

        [JsonProperty("email")]
        public string UseEmail { get; set; }

        [JsonProperty("createdAt")]
        public DateTime UseCreatedAt { get; set; }

        [JsonProperty("stats")]
        public UserStats Stats { get; set; }
    }
    public class UserStats
    {
        [JsonProperty("acquis")]
        public int Acquis { get; set; }

        [JsonProperty("termine")]
        public int Termine { get; set; }

        [JsonProperty("wishlist")]
        public int Wishlist { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
