using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using static computerComponentsTracker.App;

namespace computerComponentsTracker
{
    public partial class Settings : UserControl
    {
        public static string? refreshRate;

        private readonly IAppLanguageServices _languageService;

        public Settings(IAppLanguageServices languageService)
        {
            InitializeComponent();
            _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        }

        private void refreshRateChanged(object sender, SelectionChangedEventArgs e)
        {
            refreshRate = (this.RefreshRateComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        }

        private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            string? selectedLanguage = ((ComboBoxItem?)e.AddedItems[0])?.Tag?.ToString();
            Debug.WriteLine($"Selected Language: {selectedLanguage}");

            if (string.IsNullOrEmpty(selectedLanguage))
                return;

            _languageService.ChangeLanguage(selectedLanguage);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.componentUsage = new ComponentUsage();
        }
    }
}
