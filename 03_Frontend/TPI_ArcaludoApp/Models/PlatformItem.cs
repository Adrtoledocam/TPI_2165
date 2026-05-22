using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TPI_ArcaludoApp.Models
{
    public class PlatformItem : INotifyPropertyChanged
    {
        public string Name { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChipBackground));
                OnPropertyChanged(nameof(ChipTextColor));
            }
        }

        // Couleur du bouton selon sélection 
        public string ChipBackground
        {
            get
            {
                if (_isSelected) return "#3A7AFE";
                return "#2a2a2a";
            }
        }
        public string ChipTextColor
        {
            get
            {
                if (_isSelected) return "White";
                return "#aaa";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
