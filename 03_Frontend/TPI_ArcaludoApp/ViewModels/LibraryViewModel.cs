using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TPI_ArcaludoApp.Models;
using TPI_ArcaludoApp.Services;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPI_ArcaludoApp.ViewModels
{
    public class LibraryViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        // Liste complète chargée depuis l'API
        private List<Game> _allGames = new List<Game>();

        // Liste affichée (filtrée)
        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand GoToDetailCommand { get; }
        public ICommand AddAcquisCommand { get; }
        public ICommand AddToWishlistCommand { get; }

        //Filtres
        private string _activeSort = "notes";
        public string ActiveSort
        {
            get => _activeSort;
            set
            {
                _activeSort = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NotesButtonColor));
                OnPropertyChanged(nameof(RecentButtonColor));
                OnPropertyChanged(nameof(AvenirButtonColor));
                OnPropertyChanged(nameof(YearButtonColor));
                OnPropertyChanged(nameof(YearInputVisible));
            }
        }

        public string NotesButtonColor  => _activeSort == "notes"  ? "#3A7AFE" : "#2a2a2a";
        public string RecentButtonColor => _activeSort == "recent" ? "#3A7AFE" : "#2a2a2a";
        public string AvenirButtonColor => _activeSort == "avenir" ? "#3A7AFE" : "#2a2a2a";
        public string YearButtonColor   => _activeSort == "year"   ? "#3A7AFE" : "#2a2a2a";

        // Filtre par année — visible seulement quand Année est actif
        public bool YearInputVisible => _activeSort == "year";

        private string _yearText = "";
        public string YearText
        {
            get => _yearText;
            set
            {
                _yearText = value;
                OnPropertyChanged();
                ApplyYearFilter();
            }
        }

        //Recherche de texte
        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }


        public LibraryViewModel()
        {
            _apiService = new ApiService();

            LoadCommand = new Command(async () => await LoadGamesAsync());
            SearchCommand = new Command(async () => await SearchGamesAsync());
            SortCommand = new Command<string>(async (string sort) => await ApplySort(sort));
            GoToDetailCommand = new Command<Game>(async (Game game) => await GoToDetail(game));
            AddAcquisCommand = new Command<Game>(async (Game game) => await AddDirectAcquis(game));
            AddToWishlistCommand = new Command<Game>(async (Game game) => await AddToWishlist(game));
        }

        public async Task LoadGamesAsync()
        {
            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            // "year" utilise le tri "recent" côté backend, puis filtre local par année
            string backendSort = _activeSort == "year" ? "recent" : _activeSort;

            List<Game> result = await _apiService.GetTrendingAsync(token, backendSort);

            _allGames = result;
            ApplyYearFilter();
        }

        // Filtre local par année depuis _allGames
        private void ApplyYearFilter()
        {
            Games.Clear();

            IEnumerable<Game> filtered = _allGames;

            if (_activeSort == "year" && !string.IsNullOrWhiteSpace(_yearText) && _yearText.Length == 4)
            {
                filtered = _allGames.Where(g => g.ReleaseYear == _yearText);
            }

            foreach (Game game in filtered)
            {
                Games.Add(game);
            }
        }

        private async Task SearchGamesAsync()
        {
            if (string.IsNullOrWhiteSpace(_searchText) || _searchText.Length < 3)
            {
                await LoadGamesAsync();
                return;
            }

            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            List<Game> result = await _apiService.SearchGamesAsync(token, _searchText, _activeSort);

            _allGames = result;
            Games.Clear();
            foreach (Game game in result)
            {
                Games.Add(game);
            }
        }

        private async Task ApplySort(string sort)
        {
            ActiveSort = sort;

            // Réinitialiser le filtre année si on change de tri
            if (sort != "year")
            {
                _yearText = "";
                OnPropertyChanged(nameof(YearText));
            }

            if (!string.IsNullOrEmpty(_searchText) && _searchText.Length >= 3)
            {
                await SearchGamesAsync();
            }
            else
            {
                await LoadGamesAsync();
            }
        }

        private async Task GoToDetail(Game game)
        {
            if (game == null) return;
            await Shell.Current.GoToAsync("GameDetailPage?gamId=" + game.Id);
        }
        // Ajouter directement comme "acquis" sans passer par le détail
        private async Task AddDirectAcquis(Game game)
        {
            if (game == null) return;

            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            bool success = await _apiService.AddToCollectionAsync(token, game, "acquis");

            if (success)
            {
                game.InCollection = true;
                await Shell.Current.DisplayAlert("✓", game.Title + " ajouté à votre collection !", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible d'ajouter ce jeu.", "OK");
            }
        }

        private async Task AddToWishlist(Game game)
        {
            if (game == null) return;

            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            bool success = await _apiService.AddToCollectionAsync(token, game, "wishlist");

            if (success)
            {
                game.InCollection = true;
                await Shell.Current.DisplayAlert("✓", game.Title + " ajouté à la wishlist !", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible d'ajouter ce jeu.", "OK");
            }
        }
    }
}
