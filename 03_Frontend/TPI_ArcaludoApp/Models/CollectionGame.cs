using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPI_ArcaludoApp.Models
{
    public class CollectionGame
    {
        [JsonProperty("colId")]
        public int ColId { get; set; }

        [JsonProperty("colStatus")]
        public string ColStatus { get; set; }

        [JsonProperty("colRating")]
        public int? ColRating { get; set; }

        [JsonProperty("colComment")]
        public string ColComment { get; set; }

        [JsonProperty("colPlaytime")]
        public int? ColPlaytime { get; set; }

        [JsonProperty("colOwnPlatforms")]
        public string ColOwnPlatforms { get; set; }

        [JsonProperty("colAddedAt")]
        public DateTime ColAddedAt { get; set; }

        [JsonProperty("gamId")]
        public int GamId { get; set; }

        [JsonProperty("gamTitle")]
        public string GamTitle { get; set; }

        [JsonProperty("gamCoverUrl")]
        public string GamCoverUrl { get; set; }

        [JsonProperty("gamGenre")]
        public string GamGenre { get; set; }

        [JsonProperty("gamPlatforms")]
        public string GamPlatforms { get; set; }

        [JsonProperty("gamDeveloper")]
        public string GamDeveloper { get; set; }

        [JsonProperty("gamMetacritic")]
        public int? GamMetacritic { get; set; }

        [JsonProperty("gamReleaseDate")]
        public string GamReleaseDate { get; set; }

        public ImageSource CoverImageSource =>
            string.IsNullOrEmpty(GamCoverUrl) ? null : ImageSource.FromUri(new Uri(GamCoverUrl));

        public string ReleaseYear => GamReleaseDate?.Length >= 4 ? GamReleaseDate[..4] : "";
        public string StatusDisplay => ColStatus switch
        {
            "acquis" => "Acquis",
            "playing" => "En cours",
            "termine" => "Terminé",
            "wishlist" => "Souhait",
            _ => ColStatus
        };

        public string MetacriticDisplay
        {
            get
            {
                if (GamMetacritic == null)
                {
                    return "N/A";
                }
                return "★ " + GamMetacritic;
            }
        }
        public string StatusColor
        {
            get
            {
                if (ColStatus == "acquis") return "#F5C42B";
                if (ColStatus == "playing") return "#3A7AFE";
                if (ColStatus == "termine") return "#76E16C";
                if (ColStatus == "wishlist") return "#FF6B9D";
                return "#777777";
            }
        }

        public Brush StatusColorBorder
        {
            get { return new SolidColorBrush(Color.FromArgb(StatusColor)); }
        }

        // True si l'utilisateur a des plateformes renseignées
        public bool HasOwnPlatforms
        {
            get { return !string.IsNullOrEmpty(ColOwnPlatforms); }
        }
    }
}
