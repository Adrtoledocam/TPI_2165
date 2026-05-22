using System.Collections.ObjectModel;
using System.Windows.Input;
using TPI_ArcaludoApp.Models;
using TPI_ArcaludoApp.Services;

namespace TPI_ArcaludoApp.ViewModels
{
    public class CollectionViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public ObservableCollection<CollectionGame> Games { get; } = new();

        // Filtre par statut
        private string _activeStatus = "";
        public string ActiveStatus
        {
            get => _activeStatus;
            set
            {
                _activeStatus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CollectionTitle));
                OnPropertyChanged(nameof(AcquisButtonColor));
                OnPropertyChanged(nameof(PlayingButtonColor));
                OnPropertyChanged(nameof(TermineButtonColor));
            }
        }

        // Tri A-Z
        private string _currentSort = "recent";
        public string CurrentSort
        {
            get => _currentSort;
            set
            {
                _currentSort = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AZButtonColor));
            }
        }

        public string CollectionTitle => "Ma collection (" + Games.Count + ")";

        public string AcquisButtonColor  => _activeStatus == "acquis"  ? "#3a3a3a" : "#1a1a1a";
        public string PlayingButtonColor => _activeStatus == "playing" ? "#3a3a3a" : "#1a1a1a";
        public string TermineButtonColor => _activeStatus == "termine" ? "#3a3a3a" : "#1a1a1a";
        public string AZButtonColor      => _currentSort  == "az"      ? "#3a3a3a" : "#1a1a1a";

        // Recherche
        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand SortAZCommand { get; }
        public ICommand SelectGameCommand { get; }
        public ICommand DeleteGameCommand { get; }

        public CollectionViewModel()
        {
            _apiService = new ApiService();

            LoadCommand       = new Command(async () => await LoadCollectionAsync());
            SearchCommand     = new Command(async () => await LoadCollectionAsync());
            FilterCommand     = new Command<string>(async status => await ApplyFilter(status));
            SortAZCommand     = new Command(async () => await ToggleSortAZ());
            SelectGameCommand = new Command<CollectionGame>(async game => await GoToDetail(game));
            DeleteGameCommand = new Command<CollectionGame>(async game => await DeleteGame(game));
        }

        public async Task LoadCollectionAsync()
        {
            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            var result = await _apiService.GetCollectionAsync(token, _activeStatus, _currentSort, _searchText);

            Games.Clear();
            foreach (var game in result)
                Games.Add(game);

            OnPropertyChanged(nameof(CollectionTitle));
        }

        private async Task ApplyFilter(string status)
        {
            ActiveStatus = (_activeStatus == status) ? "" : status;
            await LoadCollectionAsync();
        }

        private async Task ToggleSortAZ()
        {
            CurrentSort = (_currentSort == "az") ? "recent" : "az";
            await LoadCollectionAsync();
        }

        private async Task GoToDetail(CollectionGame game)
        {
            if (game == null) return;
            await Shell.Current.GoToAsync("GameDetailPage?gamId=" + game.GamId);
        }

        private async Task DeleteGame(CollectionGame game)
        {
            if (game == null) return;
            bool confirm = await Shell.Current.DisplayAlert(
                "Supprimer", "Retirer " + game.GamTitle + " de votre collection ?", "Oui", "Non");
            if (!confirm) return;

            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            bool success = await _apiService.DeleteFromCollectionAsync(token, game.ColId);
            if (success)
            {
                Games.Remove(game);
                OnPropertyChanged(nameof(CollectionTitle));
            }
        }
    }
}
