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

        private bool _communityOptIn = false;
        public bool CommunityOptIn
        {
            get => _communityOptIn;
            set
            {
                _communityOptIn = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CommunityButtonText));
                OnPropertyChanged(nameof(CommunityButtonColor));
            }
        }

        public string CommunityButtonText => _communityOptIn ? "✓ Communauté activée" : "Participer à la Communauté";
        public string CommunityButtonColor => _communityOptIn ? "#76E16C" : "#2a2a2a";


        public ICommand LoadCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand ShareProfileCommand { get; }
        public ICommand ToggleCommunityCommand { get; }
        public ICommand ShowTermsCommand { get; }

        public ProfileViewModel()
        {
            _apiService = new ApiService();
            LoadCommand = new Command(async () => await LoadProfileAsync());
            LogoutCommand = new Command(async () => await ExecuteLogout());
            DeleteAccountCommand = new Command(async () => await ExecuteDeleteAccount());
            ShareProfileCommand = new Command(async () => await ExecuteShareProfile());
            ToggleCommunityCommand = new Command(async () => await ExecuteToggleCommunity());
            ShowTermsCommand = new Command(async () => await ExecuteShowTerms());
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
            bool currentOptIn = Preferences.Get("community_opt_in", false);
            CommunityOptIn = currentOptIn;
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

        private async Task ExecuteToggleCommunity()
        {
            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            bool newValue = !_communityOptIn;

            // Confirmer si l'utilisateur désactive
            if (!newValue)
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    "Quitter la Communauté",
                    "Vos statistiques ne seront plus visibles par les autres membres.",
                    "Confirmer", "Annuler");
                if (!confirm) return;
            }

            bool success = await _apiService.UpdateCommunityOptInAsync(token, newValue);

            if (success)
            {
                CommunityOptIn = newValue;
                Preferences.Set("community_opt_in", newValue);

                string message = newValue
                    ? "Vous participez maintenant à la Communauté !"
                    : "Vous avez quitté la Communauté.";
                await Shell.Current.DisplayAlert("✓", message, "OK");
            }
        }

        private async Task ExecuteShowTerms()
        {
            // ← Remplacez ce texte par vos CGU définitives
            string termsText =
                "CONDITIONS D'UTILISATION — ARCALUDO\n\n" +
                "1. Informations partagées\n" +
                "En activant la Communauté, votre nom d'utilisateur et vos statistiques " +
                "(jeux acquis, terminés, souhaités) sont visibles publiquement.\n\n" +
                "2. Données privées\n" +
                "Votre adresse email, vos commentaires et vos temps de jeu restent strictement privés.\n\n" +
                "3. Conformité nLPD et RGPD\n" +
                "Conformément à la loi suisse sur la protection des données, " +
                "vous pouvez retirer votre consentement à tout moment depuis cette page.\n\n" +
                "4. Droits\n" +
                "Vous disposez d'un droit d'accès, de rectification et d'effacement " +
                "de vos données via la fonction 'Supprimer mon compte'.\n\n" +
                "ETML · TPI 2165 · Adrian Toledo";

            await Shell.Current.DisplayAlert("Conditions d'utilisation", termsText, "Fermer");
        }

    }
}
