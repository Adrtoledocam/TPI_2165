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

        //Pagines scroll
        private int _currentPage = 1;
        private bool _hasMore = true;

        private bool _isLoadingMore = false;
        public bool IsLoadingMore
        {
            get => _isLoadingMore;
            set { _isLoadingMore = value; OnPropertyChanged(); }
        }

        public bool HasMore
        {
            get => _hasMore;
            set { _hasMore = value; OnPropertyChanged(); }
        }

        //Commandes
        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand GoToDetailCommand { get; }
        public ICommand AddAcquisCommand { get; }
        public ICommand AddToWishlistCommand { get; }
        public ICommand LoadMoreCommand { get; }


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
                OnPropertyChanged(nameof(AZButtonColor));
            }
        }

        public string NotesButtonColor  => _activeSort == "notes"  ? "#3A7AFE" : "#2a2a2a";
        public string RecentButtonColor => _activeSort == "recent" ? "#3A7AFE" : "#2a2a2a";
        public string AvenirButtonColor => _activeSort == "avenir" ? "#3A7AFE" : "#2a2a2a";
        public string AZButtonColor     => _activeSort == "az"     ? "#3A7AFE" : "#2a2a2a";

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
            LoadMoreCommand = new Command(async () => await LoadMoreAsync(), () => HasMore && !IsLoadingMore);
        }

        public async Task LoadGamesAsync()
        {
            _currentPage = 1;
            HasMore = true;

            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            List<Game> result = await _apiService.GetTrendingAsync(token, _activeSort, page: 1);

            _allGames = result;
            HasMore = result.Count >= 10;
            Games.Clear();
            foreach (Game game in result) { Games.Add(game); }
        }

        private async Task LoadMoreAsync()
        {
            if (IsLoadingMore || !HasMore) return;

            IsLoadingMore = true;
            ((Command)LoadMoreCommand).ChangeCanExecute();

            string token = await SecureStorage.GetAsync("auth_token");
            if (!string.IsNullOrEmpty(token))
            {
                _currentPage++;
                List<Game> more = await _apiService.GetTrendingAsync(token, _activeSort, page: _currentPage);

                if (more.Count == 0)
                {
                    HasMore = false;
                }
                else
                {
                    _allGames.AddRange(more);
                    HasMore = more.Count >= 10;
                    foreach (Game game in more) { Games.Add(game); }
                }
            }

            IsLoadingMore = false;
            ((Command)LoadMoreCommand).ChangeCanExecute();
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
            HasMore = false;
            Games.Clear();
            foreach (Game game in result)
            {
                Games.Add(game);
            }
        }

        private async Task ApplySort(string sort)
        {
            ActiveSort = sort;

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
