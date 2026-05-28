using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TPI_ArcaludoApp.Models
{
    public class Game : INotifyPropertyChanged
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("coverUrl")]
        public string CoverUrl { get; set; }

        [JsonProperty("genre")]
        public string Genre { get; set; }

        [JsonProperty("platforms")]
        public string Platforms { get; set; }

        [JsonProperty("developer")]
        public string Developer { get; set; }

        [JsonProperty("publisher")]
        public string Publisher { get; set; }

        [JsonProperty("metacritic")]
        public int? Metacritic { get; set; }

        [JsonProperty("releaseDate")]
        public string ReleaseDate { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        private bool _inCollection;

        [JsonProperty("inCollection")]
        public bool InCollection 
        {
            get => _inCollection;
            set
            {
                _inCollection = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NotInCollection));
            }
        }
        public bool NotInCollection => !_inCollection;

        // True si pas d'image de couverture → afficher logo
        public bool HasNoCover
        {
            get { return string.IsNullOrEmpty(CoverUrl); }
        }

        [JsonProperty("collectionEntry")]
        public CollectionEntry CollectionEntry { get; set; }

        public ImageSource CoverImageSource =>
            string.IsNullOrEmpty(CoverUrl) ? null : ImageSource.FromUri(new Uri(CoverUrl));

        public string ReleaseYear
        {
            get
            {
                if (ReleaseDate == null || ReleaseDate.Length < 4) return "";
                 
                return ReleaseDate.Substring(0, 4);
            }
        }
        public string MetacriticDisplay
        {
            get
            {
                if (Metacritic == null)
                {
                    return "N/A";
                }
                return "★ " + Metacritic;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    public class CollectionEntry
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
    }
}
