using System;
using System.Collections.Generic;
using System.Windows.Input;
using TPI_ArcaludoApp.Models;
using TPI_ArcaludoApp.Services;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPI_ArcaludoApp.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        private User _currentUser;
        public User CurrentUser
            {
            get => _currentUser;
            set 
            { 
                _currentUser = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(Username));
                OnPropertyChanged(nameof(Email));
                OnPropertyChanged(nameof(StatAcquis));
                OnPropertyChanged(nameof(StatTermine));
                OnPropertyChanged(nameof(StatWishlist));
            }
        }

        public string Username
        {
            get
            {
                if (_currentUser == null) return "";
                return _currentUser.UseUsername;
            }
        }
        public string Email
        {
            get
            {
                if (_currentUser == null) return "";
                return _currentUser.UseEmail;
            }
        }

        public int StatAcquis
        {
            get
            {
                if (_currentUser == null || _currentUser.Stats == null) return 0;
                return _currentUser.Stats.Acquis;
            }
        }

        public int StatTermine
        {
            get
            {
                if (_currentUser == null || _currentUser.Stats == null) return 0;
                return _currentUser.Stats.Termine;
            }
        }

        public int StatWishlist
        {
            get
            {
                if (_currentUser == null || _currentUser.Stats == null) return 0;
                return _currentUser.Stats.Wishlist;
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand ShareProfileCommand { get; }

        public ProfileViewModel()
        {
            _apiService = new ApiService();
            LoadCommand = new Command(async () => await LoadProfileAsync());
            LogoutCommand = new Command(async () => await ExecuteLogout());
            DeleteAccountCommand = new Command(async () => await ExecuteDeleteAccount());
            ShareProfileCommand = new Command(async () => await ExecuteShareProfile());
        }

        public async Task LoadProfileAsync()
        {
            string token = await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            User user = await _apiService.GetProfileAsync(token);

            if (user != null)
            {
                CurrentUser = user;
            }
        }

        private async Task ExecuteLogout()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Déconnexion", "Voulez-vous vous déconnecter ?", "Oui", "Non");

            if (!confirm) return;

            // Supprimer le token et les préférences
            SecureStorage.Remove("auth_token");
            Preferences.Clear();

            await Shell.Current.GoToAsync("//LoginPage");
        }

        private async Task ExecuteShareProfile()
        {
            string username = _currentUser?.UseUsername ?? "utilisateur";
            int acquis  = _currentUser?.Stats?.Acquis  ?? 0;
            int termine = _currentUser?.Stats?.Termine ?? 0;
            int wishlist = _currentUser?.Stats?.Wishlist ?? 0;

            await Share.RequestAsync(new ShareTextRequest
            {
                Title = "Mon profil Arcaludo",
                Text  = $"Découvrez le profil de {username} sur Arcaludo !\n" +
                        $"{acquis} jeux acquis · {termine} terminés · {wishlist} souhaits"
            });
        }

        private async Task ExecuteDeleteAccount()
        {
            bool confirm = await Shell.Current.DisplayAlert(
                "Supprimer le compte",
                "Cette action est irréversible. Toutes vos données seront effacées.",
                "Supprimer", "Annuler");

            if (!confirm) return;

            string token = await SecureStorage.GetAsync("auth_token");

            if (string.IsNullOrEmpty(token)) return;

            bool success = await _apiService.DeleteAccountAsync(token);

            if (success)
            {
                SecureStorage.Remove("auth_token");
                Preferences.Clear();
                await Shell.Current.GoToAsync("//LoginPage");
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    "Erreur", "Impossible de supprimer le compte.", "OK");
            }
        }
    }
}
