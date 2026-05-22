using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;



namespace TPI_ArcaludoApp.Models
{
    public class CommunityMember
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("stats")]
        public UserStats Stats { get; set; }
    }
}
