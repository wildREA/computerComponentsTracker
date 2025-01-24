using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
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
using computerComponentsTracker;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;
using computerComponentsTracker;
using static App;

namespace computerComponentsTracker
{
    public partial class Settings : UserControl
    {
        public static string refreshRate;
        // asd
        public Settings()
        {
            InitializeComponent();
        }
        private void refreshRateChanged(object sender, SelectionChangedEventArgs e)
        {
            refreshRate = (this.RefreshRateComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
        }

        private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            string? selectedLanguage = ((ComboBoxItem?)e.AddedItems[0]).Tag.ToString();
            Debug.WriteLine(selectedLanguage);

            // Access ChangeLanguage method via DI
            var languageService = (IAppLanguageServices)App.ServiceProvider.GetService(typeof(IAppLanguageServices));
            languageService?.ChangeLanguage(selectedLanguage);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.componentUsage = new ComponentUsage();
        }
    }
}
