using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using static computerComponentsTracker.App;

namespace computerComponentsTracker
{
    public partial class Settings : UserControl
    {
<<<<<<< HEAD
        public static string? refreshRate;

=======
        public static string refreshRate;
>>>>>>> development
        private readonly IAppLanguageServices _languageService;
        private string? _pendingLanguage; // Store selected language temporarily

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
            _pendingLanguage = ((ComboBoxItem?)e.AddedItems[0])?.Tag?.ToString();
            Debug.WriteLine($"Selected Language (Pending): {_pendingLanguage}");
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
