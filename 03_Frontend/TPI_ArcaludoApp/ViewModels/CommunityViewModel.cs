using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPI_ArcaludoApp.Models;
using TPI_ArcaludoApp.Services;
namespace TPI_ArcaludoApp.ViewModels
{
    public class CommunityViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public ObservableCollection<CommunityMember> Members { get; } = new ObservableCollection<CommunityMember>();

        // true = opt-in actif, false = accès refusé (écran cadenas)
        private bool _isOptIn = true;
        public bool IsOptIn
        {
            get => _isOptIn;
            set
            {
                _isOptIn = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotOptIn));
                OnPropertyChanged(nameof(CommunityTitle));
            }
        }
        public bool IsNotOptIn => !_isOptIn;

        public string CommunityTitle
        {
            get
            {
                if (!_isOptIn) return "Communauté";
                return "Communauté (" + Members.Count + ")";
            }
        }

        public ICommand LoadCommand { get; }
        public ICommand GoToProfileCommand { get; }

        public CommunityViewModel()
        {
            _apiService = new ApiService();
            LoadCommand = new Command(async () => await LoadCommunityAsync());
            // Navigation directe vers l'onglet Profil 
            GoToProfileCommand = new Command(async () =>
            {
                try { await Shell.Current.GoToAsync("//ProfilePage"); }
                catch { /*Shell.Current.CurrentItem = Shell.Current.Items[Shell.Current.Items.Count - 1]; */}
            });
        }

        public async Task LoadCommunityAsync()
        {
            string token = await SecureStorage.GetAsync("auth_token");
            if (string.IsNullOrEmpty(token)) return;

            // null = 403 Forbidden → utilisateur non opt-in
            List<CommunityMember> result = await _apiService.GetCommunityAsync(token);

            if (result == null)
            {
                IsOptIn = false;
                return;
            }

            IsOptIn = true;
            Members.Clear();

            foreach (CommunityMember member in result)
            {
                Members.Add(member);
            }

            OnPropertyChanged(nameof(CommunityTitle));
        }
    }
}
