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
    public class RegisterViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _username;
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }
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
        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }
        public RegisterViewModel()
        {
            _apiService = new ApiService();
            RegisterCommand = new Command(async () => await ExecuteRegister());
            GoToLoginCommand = new Command(async () => await Shell.Current.GoToAsync("LoginPage"));
        }
        private async Task ExecuteRegister()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Champs requis", "Veuillez remplir tous les champs.", "OK");
                return;
            }
            try
            {
                bool success = await _apiService.RegisterAsync(Username, Email, Password);

                if (success)
                {
                    await Shell.Current.DisplayAlert("Compte créé !", "Connectez-vous maintenant.", "OK");
                    await Shell.Current.GoToAsync("..");  // retour à LoginPage
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erreur", "Email déjà utilisé ou données invalides.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erreur réseau", $"Une erreur est survenue: {ex.Message}", "OK");
            }
            finally
            {
                // Clear password for security
                Password = string.Empty;
            }
        }
    }
}
