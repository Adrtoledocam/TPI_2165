using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TPI_ArcaludoApp.Services;
using TPI_ArcaludoApp.Models;

namespace TPI_ArcaludoApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }

        public LoginViewModel()
        {
            _apiService = new ApiService();
            LoginCommand = new Command(async () => await ExecuteLogin());
            GoToRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("RegisterPage"));
        }

        private async Task ExecuteLogin()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Champs requis", "Veuillez remplir l'email et le mot de passe.", "OK");
                return;
            }
            try
            {
                var result = await _apiService.LoginAsync(Email, Password);

                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    // Sauvegarder le token 
                    await SecureStorage.SetAsync("auth_token", result.Token);

                    // Sauvegarder les infos 
                    Preferences.Set("user_id", result.User.UseId);
                    Preferences.Set("user_name", result.User.UseUsername);
                    Preferences.Set("user_email", result.User.UseEmail);

                    await Shell.Current.DisplayAlert("Connexion réussie", $"Bienvenue, {result.User.UseUsername} !", "Continuer");

                    // Nav vers la page principale
                    await Shell.Current.GoToAsync("//Main");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erreur", "Email ou mot de passe incorrect.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginViewModel] {ex.Message}");
                await Shell.Current.DisplayAlert(
                    "Erreur réseau", "Impossible de contacter le serveur.", "OK");
            }
            finally
            {
                Password = string.Empty;
            }
        }
    }
}
