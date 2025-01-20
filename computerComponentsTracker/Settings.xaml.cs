using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace computerComponentsTracker
{
    public partial class Settings : UserControl, INotifyPropertyChanged
    {
        private string _selectedTheme;
        private string _selectedNetworkAdapter;
        private string _selectedRefreshRate;
        private string _selectedLanguage;

        public event EventHandler<string> NetworkAdapterChanged;
        public event EventHandler<string> RefreshRateChanged;

        public Settings()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (_selectedTheme != value)
                {
                    _selectedTheme = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedNetworkAdapter
        {
            get => _selectedNetworkAdapter;
            set
            {
                if (_selectedNetworkAdapter != value)
                {
                    _selectedNetworkAdapter = value;
                    OnPropertyChanged();
                    NetworkAdapterChanged?.Invoke(this, _selectedNetworkAdapter);
                }
            }
        }

        public string SelectedRefreshRate
        {
            get => _selectedRefreshRate;
            set
            {
                if (_selectedRefreshRate != value)
                {
                    _selectedRefreshRate = value;
                    OnPropertyChanged();
                    RefreshRateChanged?.Invoke(this, _selectedRefreshRate);
                }
            }
        }

        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged();
                    ApplyLanguage(_selectedLanguage);
                }
            }
        }

        private void NetworkComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedNetworkAdapter = NetworkComboBox.SelectedItem as string;
        }

        private void RefreshRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedRefreshRate = RefreshRateComboBox.SelectedItem as string;
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedLanguage = LanguageComboBox.SelectedItem as string;
        }

        private void ApplyLanguage(string language)
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (language)
            {
                case "Русский":
                    dict.Source = new Uri("Resources/Strings.ru.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("Resources/Strings.en.xaml", UriKind.Relative);
                    break;
            }

            // Remove existing language dictionaries
            var existingDictionaries = Application.Current.Resources.MergedDictionaries
                .Where(d => d.Source != null && (d.Source.OriginalString.EndsWith("Strings.en.xaml") || d.Source.OriginalString.EndsWith("Strings.ru.xaml")))
                .ToList();

            foreach (var dictionary in existingDictionaries)
            {
                Application.Current.Resources.MergedDictionaries.Remove(dictionary);
            }

            // Add the selected language dictionary
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
