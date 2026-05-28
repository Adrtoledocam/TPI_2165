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
    [QueryProperty(nameof(GamId), "gamId")]
    public class GameDetailViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

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

        public string AcquisColor  => _selectedStatus == "acquis"   ? "#3A7AFE" : "#2a2a2a";
        public string PlayingColor => _selectedStatus == "playing"  ? "#3A7AFE" : "#2a2a2a";
        public string TermineColor => _selectedStatus == "termine"  ? "#3A7AFE" : "#2a2a2a";
        public string WishlistColor => _selectedStatus == "wishlist" ? "#3A7AFE" : "#2a2a2a";

        // Plateformes
        public ObservableCollection<PlatformItem> Platforms { get; } = new ObservableCollection<PlatformItem>();

        // Temps de jeu
        private int _playtime = 0;
        public int Playtime
        {
            get => _playtime;
            set { _playtime = value; OnPropertyChanged(); OnPropertyChanged(nameof(PlaytimeText)); }
        }

        public string PlaytimeText
        {
            get => _playtime > 0 ? _playtime.ToString() : "";
            set
            {
                _playtime = int.TryParse(value, out int h) ? Math.Max(0, h) : 0;
                OnPropertyChanged();
            }
        }

        // Note
        private int _rating = 0;
        public int Rating
        {
            get => _rating;
            set { _rating = value; OnPropertyChanged(); OnPropertyChanged(nameof(RatingText)); }
        }

        public string RatingText
        {
            get => _rating > 0 ? _rating.ToString() : "";
            set
            {
                _rating = int.TryParse(value, out int r) ? Math.Max(0, Math.Min(5, r)) : 0;
                OnPropertyChanged();
            }
        }

        private string _comment = "";
        public string Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(); }
        }

        private int? _colId = null;

        public ICommand SelectStatusCommand { get; }
        public ICommand TogglePlatformCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand RemoveFromCollectionCommand { get; }
        public ICommand GoBackCommand { get; }

        public GameDetailViewModel()
        {
            _apiService = new ApiService();

            SelectStatusCommand         = new Command<string>((string s) => SelectedStatus = s);
            TogglePlatformCommand       = new Command<PlatformItem>((PlatformItem p) => p.IsSelected = !p.IsSelected);
            SaveCommand                 = new Command(async () => await ExecuteSave());
            RemoveFromCollectionCommand = new Command(async () => await ExecuteRemove());
            GoBackCommand               = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public async Task LoadGameAsync()
        {
            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            Game game = await _apiService.GetGameDetailAsync(token, _gamId);
            if (game == null) return;

            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                CurrentGame = game;

                // Générer les plateformes depuis les données RAWG (gamPlatforms)
                Platforms.Clear();
                if (!string.IsNullOrEmpty(game.Platforms))
                {
                    string[] rawgPlatforms = game.Platforms.Split('|');
                    foreach (string platformName in rawgPlatforms)
                    {
                        string name = platformName.Trim();
                        if (!string.IsNullOrEmpty(name))
                        {
                            Platforms.Add(new PlatformItem { Name = name, IsSelected = false });
                        }
                    }
                }

                // Pré-remplir si jeu déjà dans la collection
                if (game.CollectionEntry != null)
                {
                    CollectionEntry entry = game.CollectionEntry;
                    _colId         = entry.ColId;
                    SelectedStatus = entry.ColStatus ?? "";
                    Rating         = entry.ColRating ?? 0;
                    Comment        = entry.ColComment ?? "";
                    Playtime       = entry.ColPlaytime ?? 0;

                    // Cocher les plateformes déjà sélectionnées
                    if (!string.IsNullOrEmpty(entry.ColOwnPlatforms))
                    {
                        string[] savedPlatforms = entry.ColOwnPlatforms.Split(',');

                        foreach (PlatformItem platform in Platforms)
                        {
                            platform.IsSelected = false;

                            foreach (string saved in savedPlatforms)
                            {
                                if (platform.Name == saved.Trim())
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
                    selected.Add(platform.Name);
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
                success = await _apiService.UpdateCollectionEntryAsync(
                    token, _colId.Value, _selectedStatus,
                    _rating > 0 ? _rating : null,
                    string.IsNullOrEmpty(_comment) ? null : _comment,
                    _playtime > 0 ? _playtime : null,
                    string.IsNullOrEmpty(ownPlatforms) ? null : ownPlatforms);
            }
            else
            {
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
                await Shell.Current.GoToAsync("..");
        }
    }
}
