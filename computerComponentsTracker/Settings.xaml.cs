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

namespace computerComponentsTracker
{
    public partial class Settings : UserControl
    {
        public static string refreshRate;

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
            //App.ChangeLanguage(selectedLanguage);
            //((App)Application.Current).ChangeLanguage(selectedLanguage);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.componentUsage = new ComponentUsage();
        }
    }
}
