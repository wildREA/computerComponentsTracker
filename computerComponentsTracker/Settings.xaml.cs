using System.Windows;
using System.Diagnostics;
using System.Windows.Controls;
using static computerComponentsTracker.App;
using System.Runtime.CompilerServices;

namespace computerComponentsTracker
{
    public partial class Settings : UserControl
    {
        public static string? refreshRate;
        private readonly IAppLanguageServices _languageService;
        private string? _pendingLanguage; // Store selected language temporarily

        public Settings(IAppLanguageServices languageService)
        {
            InitializeComponent();
            _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        }

        private void themeChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Theme Changed");   
            if (ThemeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string? theme = selectedItem.Tag.ToString();
                ApplyTheme(theme);
                Debug.WriteLine($"Applied Theme: {theme}");
            }
        }

        private void ApplyTheme(string? theme)
        {
            // Construct the URI
            Uri themeUri = new Uri($"Resources/Themes/{theme}.xaml", UriKind.Relative);

            // Create the new ResourceDictionary
            ResourceDictionary newDict = new ResourceDictionary() { Source = themeUri };

            // Save the user's theme setting
            Properties.Settings.Default.userTheme = theme;
            Properties.Settings.Default.Save();
            Debug.WriteLine("Saved Settings");

            var existingThemeDictionary = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Theme"));
            if (existingThemeDictionary != null)
            {
                int i = Application.Current.Resources.MergedDictionaries.IndexOf(existingThemeDictionary);
                Application.Current.Resources.MergedDictionaries.Remove(existingThemeDictionary);
                Application.Current.Resources.MergedDictionaries.Insert(i, newDict);
            }

            Debug.WriteLine("Switched out dict");

            // Force a layout update
            Application.Current.MainWindow?.UpdateLayout();
        }


        private void onLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            _pendingLanguage = ((ComboBoxItem?)e.AddedItems[0])?.Tag?.ToString();
            Debug.WriteLine($"Selected Language (Pending): {_pendingLanguage}");
        }

        private void refreshRateChanged(object sender, SelectionChangedEventArgs e)
        {
            refreshRate = (RefreshRateComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_pendingLanguage))
            {
                _languageService.ChangeLanguage(_pendingLanguage);
                Debug.WriteLine($"Applied Language: {_pendingLanguage}");
            }

            MainWindow.componentUsage = new ComponentUsage();
        }
    }
}
