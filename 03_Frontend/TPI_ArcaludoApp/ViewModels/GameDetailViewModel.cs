using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TPI_ArcaludoApp.Models;
using TPI_ArcaludoApp.Services;

namespace TPI_ArcaludoApp.ViewModels
{
    // QueryProperty reçoit le gamId depuis la navigation Shell
    [QueryProperty(nameof(GamId), "gamId")]
    public class GameDetailViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        // Info
        private Game _game;
        public Game CurrentGame
        {
            get => _game;
            set { _game = value; OnPropertyChanged(); }
        }

        private int _gamId;
        public int GamId
        {
            get => _gamId;
            set
            {
                _gamId = value;
                OnPropertyChanged();
                Task.Run(async () => await LoadGameAsync());
            }
        }

        // Status
        private string _selectedStatus = "";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                _selectedStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AcquisColor));
                OnPropertyChanged(nameof(PlayingColor));
                OnPropertyChanged(nameof(TermineColor));
                OnPropertyChanged(nameof(WishlistColor));
            }
        }

        public string AcquisColor => _selectedStatus == "acquis" ? "#3A7AFE" : "#2a2a2a";
        public string PlayingColor => _selectedStatus == "playing" ? "#3A7AFE" : "#2a2a2a";
        public string TermineColor => _selectedStatus == "termine" ? "#3A7AFE" : "#2a2a2a";
        public string WishlistColor => _selectedStatus == "wishlist" ? "#3A7AFE" : "#2a2a2a";

        // Plateformes
        public ObservableCollection<PlatformItem> Platforms { get; } = new ObservableCollection<PlatformItem>();

        // Données personnelles
        private int _playtime = 0;
        public int Playtime
        {
            get => _playtime;
            set { _playtime = value; OnPropertyChanged(); OnPropertyChanged(nameof(PlaytimeDisplay)); }
        }

        public string PlaytimeDisplay
        {
            get { return _playtime + "h"; }
        }

        private int _rating = 0;
        public int Rating
        {
            get => _rating;
            set
            {
                _rating = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Star1));
                OnPropertyChanged(nameof(Star2));
                OnPropertyChanged(nameof(Star3));
                OnPropertyChanged(nameof(Star4));
                OnPropertyChanged(nameof(Star5));
            }
        }

        // Étoiles Rating
        public string Star1 => _rating >= 1 ? "★" : "☆";
        public string Star2 => _rating >= 2 ? "★" : "☆";
        public string Star3 => _rating >= 3 ? "★" : "☆";
        public string Star4 => _rating >= 4 ? "★" : "☆";
        public string Star5 => _rating >= 5 ? "★" : "☆";

        private string _comment = "";
        public string Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(); }
        }


        private int? _colId = null;

        public ICommand SelectStatusCommand { get; }
        public ICommand TogglePlatformCommand { get; }
        public ICommand SetRatingCommand { get; }
        public ICommand AddPlaytimeCommand { get; }
        public ICommand RemovePlaytimeCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand RemoveFromCollectionCommand { get; }
        public ICommand GoBackCommand { get; }

        public GameDetailViewModel()
        {
            _apiService = new ApiService();

            SelectStatusCommand = new Command<string>((string s) => SelectedStatus = s);
            TogglePlatformCommand = new Command<PlatformItem>((PlatformItem p) => p.IsSelected = !p.IsSelected);
            SetRatingCommand = new Command<string>((string r) => Rating = int.Parse(r));
            AddPlaytimeCommand = new Command(() => Playtime = Playtime + 1);
            RemovePlaytimeCommand = new Command(() => { if (Playtime > 0) Playtime = Playtime - 1; });
            SaveCommand = new Command(async () => await ExecuteSave());
            RemoveFromCollectionCommand = new Command(async () => await ExecuteRemove());
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            InitPlatforms();
        }

        private void InitPlatforms()
        {
            List<string> platformNames = new List<string>
            {
                "PS5", "PC", "PS4", "Switch",
                "Xbox S", "Xbox X", "Xbox One"
            };

            foreach (string name in platformNames)
            {
                Platforms.Add(new PlatformItem { Name = name, IsSelected = false });
            }
        }

        public async Task LoadGameAsync()
        {
            string token = await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrEmpty(token)) return;

            Game game = await _apiService.GetGameDetailAsync(token, _gamId);

            if (game == null) return;

            // Mise à jour sur le thread UI
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                CurrentGame = game;

                // Si le jeu est déjà dans la collection → pré-remplir les champs
                if (game.CollectionEntry != null)
                {
                    CollectionEntry entry = game.CollectionEntry;
                    _colId = entry.ColId;
                    SelectedStatus = entry.ColStatus ?? "";
                    Rating = entry.ColRating ?? 0;
                    Comment = entry.ColComment ?? "";
                    Playtime = entry.ColPlaytime ?? 0;

                    // Cocher les plateformes déjà sélectionnées
                    if (!string.IsNullOrEmpty(entry.ColOwnPlatforms))
                    {
                        string[] saved = entry.ColOwnPlatforms.Split(',');

                        foreach (PlatformItem platform in Platforms)
                        {
                            platform.IsSelected = false;

                            foreach (string savedName in saved)
                            {
                                if (platform.Name == savedName.Trim())
                                {
                                    platform.IsSelected = true;
                                }
                            }
                        }
                    }
                }
            });
        }

        private string GetSelectedPlatforms()
        {
            List<string> selected = new List<string>();

            foreach (PlatformItem platform in Platforms)
            {
                if (platform.IsSelected)
                {
                    selected.Add(platform.Name);
                }
            }

            return string.Join(",", selected);
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrEmpty(_selectedStatus))
            {
                await Shell.Current.DisplayAlert("Statut requis", "Choisissez un statut pour ce jeu.", "OK");
                return;
            }

            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            string ownPlatforms = GetSelectedPlatforms();
            bool success = false;

            if (_colId != null)
            {
                // Jeu déjà dans la collection → PUT
                success = await _apiService.UpdateCollectionEntryAsync(
                    token, _colId.Value, _selectedStatus,
                    _rating > 0 ? _rating : null,
                    string.IsNullOrEmpty(_comment) ? null : _comment,
                    _playtime > 0 ? _playtime : null,
                    string.IsNullOrEmpty(ownPlatforms) ? null : ownPlatforms);
            }
            else
            {
                // Nouveau jeu → POST
                success = await _apiService.AddToCollectionAsync(
                    token, _game, _selectedStatus, ownPlatforms);
            }

            if (success)
            {
                await Shell.Current.DisplayAlert("✓", "Jeu enregistré dans votre collection !", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible d'enregistrer.", "OK");
            }
        }

        private async Task ExecuteRemove()
        {
            if (_colId == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Retirer", "Retirer ce jeu de votre collection ?", "Oui", "Non");

            if (!confirm) return;

            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            bool success = await _apiService.DeleteFromCollectionAsync(token, _colId.Value);

            if (success)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}