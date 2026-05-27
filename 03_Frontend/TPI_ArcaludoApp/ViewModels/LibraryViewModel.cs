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

        public ObservableCollection<Game> Games { get; } = new ObservableCollection<Game>();

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand GoToDetailCommand { get; }
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
                OnPropertyChanged(nameof(AvenirButtonColor));
                OnPropertyChanged(nameof(AZButtonColor));
                OnPropertyChanged(nameof(ZAButtonColor));
            }
        }

        public string NotesButtonColor => _activeSort == "notes" ? "#3A7AFE" : "#2a2a2a";
        public string AvenirButtonColor => _activeSort == "avenir" ? "#3A7AFE" : "#2a2a2a";
        public string AZButtonColor => _activeSort == "az" ? "#3A7AFE" : "#2a2a2a";
        public string ZAButtonColor => _activeSort == "za" ? "#3A7AFE" : "#2a2a2a";


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
            AddToWishlistCommand = new Command<Game>(async (Game game) => await AddToWishlist(game));
        }

        public async Task LoadGamesAsync()
        {

            string token = await SecureStorage.GetAsync("auth_token");

            Console.WriteLine("[Library] Token = " + (token ?? "NULL"));

            if (string.IsNullOrEmpty(token)) return;

            List<Game> result = await _apiService.GetTrendingAsync(token, _activeSort);
            Console.WriteLine("[Library] Nombre de jeux reçus : " + result.Count);

            Games.Clear();
            foreach (Game game in result)
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
        private async Task AddToWishlist(Game game)
        {
            if (game == null) return;

            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            bool success = await _apiService.AddToCollectionAsync(token, game, "wishlist");

            if (success)
            {
                game.InCollection = true;


                await LoadGamesAsync();
                await Shell.Current.DisplayAlert("✓", game.Title + " ajouté à la wishlist !", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible d'ajouter ce jeu.", "OK");
            }
        }
    }
}
