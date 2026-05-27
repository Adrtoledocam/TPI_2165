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
    public class WishlistViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public ObservableCollection<CollectionGame> Games { get; } = new ObservableCollection<CollectionGame>();

        private string _activeSort = "recent";
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
                OnPropertyChanged(nameof(WishlistTitle));
            }
        }

        public string NotesButtonColor => _activeSort == "notes" ? "#3A7AFE" : "#2a2a2a";
        public string AvenirButtonColor => _activeSort == "avenir" ? "#3A7AFE" : "#2a2a2a";
        public string AZButtonColor => _activeSort == "az" ? "#3A7AFE" : "#2a2a2a";
        public string ZAButtonColor => _activeSort == "za" ? "#3A7AFE" : "#2a2a2a";

        public string WishlistTitle
        {
            get { return "Liste de souhaits (" + Games.Count + ")"; }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand GoToDetailCommand { get; }
        public ICommand MoveToCollectionCommand { get; }
        public ICommand DeleteCommand { get; }

        public WishlistViewModel()
        {
            _apiService = new ApiService();

            LoadCommand = new Command(async () => await LoadWishlistAsync());
            SearchCommand = new Command(async () => await LoadWishlistAsync());
            SortCommand = new Command<string>(async (string sort) => await ApplySort(sort));
            GoToDetailCommand = new Command<CollectionGame>(async (CollectionGame game) => await GoToDetail(game));
            MoveToCollectionCommand = new Command<CollectionGame>(async (CollectionGame game) => await MoveToCollection(game));
            DeleteCommand = new Command<CollectionGame>(async (CollectionGame game) => await DeleteFromWishlist(game));
        }

        public async Task LoadWishlistAsync()
        {
            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            List<CollectionGame> result = await _apiService.GetWishlistAsync(token, _activeSort, _searchText);

            Games.Clear();
            foreach (CollectionGame game in result)
            {
                Games.Add(game);
            }

            OnPropertyChanged(nameof(WishlistTitle));
        }

        private async Task ApplySort(string sort)
        {
            ActiveSort = sort;
            await LoadWishlistAsync();
        }

        private async Task GoToDetail(CollectionGame game)
        {
            if (game == null) return;
            await Shell.Current.GoToAsync("GameDetailPage?gamId=" + game.GamId);
        }

        private async Task MoveToCollection(CollectionGame game)
        {
            if (game == null) return;

            // L'utilisateur choisit le statut
            string status = await Shell.Current.DisplayActionSheet(
                "Ajouter à la collection", "Annuler", null,
                "Acquis", "En cours", "Terminé");

            if (status == null || status == "Annuler") return;

            // Convertir le texte affiché en valeur backend
            string statusValue = "";
            if (status == "Acquis") statusValue = "acquis";
            if (status == "En cours") statusValue = "playing";
            if (status == "Terminé") statusValue = "termine";

            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            bool success = await _apiService.MoveToCollectionAsync(token, game.ColId, statusValue);

            if (success)
            {
                Games.Remove(game);
                OnPropertyChanged(nameof(WishlistTitle));
                await Shell.Current.DisplayAlert("✓", game.GamTitle + " ajouté à la collection !", "OK");
            }
            else
            {
                await Shell.Current.DisplayAlert("Erreur", "Impossible de déplacer ce jeu.", "OK");
            }
        }

        private async Task DeleteFromWishlist(CollectionGame game)
        {
            if (game == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Retirer", "Retirer " + game.GamTitle + " de vos souhaits ?", "Oui", "Non");

            if (!confirm) return;

            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            bool success = await _apiService.DeleteFromWishlistAsync(token, game.ColId);

            if (success)
            {
                Games.Remove(game);
                OnPropertyChanged(nameof(WishlistTitle));
            }
        }
    }
}
