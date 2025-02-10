using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace computerComponentsTracker
{
    public partial class App : Application
    {
        public interface IAppLanguageServices
        {
            void ChangeLanguage(string language);
        }

        public class AppLanguageServices : IAppLanguageServices
        {
            private readonly Application _application;
            private string _currentLanguage;

            // Dictionary mapping language codes to resource files
            private readonly Dictionary<string, string> _languageResources;

            public AppLanguageServices(Application application)
            {
                _application = application;
                _currentLanguage = "en-US"; // Default language

                // Dictionary with languages and their resource files
                _languageResources = new Dictionary<string, string>
            {
                { "ar-SA", "Resources/Strings.sa.xaml" },
                { "ar-IQ", "Resources/Strings.fa.xaml" },
                { "de-DE", "Resources/Strings.de.xaml" },
                { "el-GR", "Resources/Strings.el.xaml" },
                { "en-US", "Resources/Strings.xaml" },
                { "fr-FR", "Resources/Strings.fr.xaml" },
                { "he-IL", "Resources/Strings.he.xaml" },
                { "hi-IN", "Resources/Strings.hi.xaml" },
                { "ko-KR", "Resources/Strings.ko.xaml" },
                { "it-IT", "Resources/Strings.it.xaml" },
                { "ku-IQ", "Resources/Strings.ku.xaml" },
                { "lt-LT", "Resources/Strings.lt.xaml" },
                { "nl-NL", "Resources/Strings.nl.xaml" },
                { "jp-JP", "Resources/Strings.jp.xaml" },
                { "pl-PL", "Resources/Strings.pl.xaml" },
                { "pt-BR", "Resources/Strings.pt.xaml" },
                { "ru-RU", "Resources/Strings.ru.xaml" },
                { "th-TH", "Resources/Strings.th.xaml" },
                { "tr-TR", "Resources/Strings.tr.xaml" },
                { "zh-CN", "Resources/Strings.zh.xaml" },
                { "ar-LB", "Resources/Strings.lb.xaml" },
            };
            }

            public void ChangeLanguage(string language)
            {
                // Check if language is already set
                if (language == _currentLanguage) return;

                // Current language
                _currentLanguage = language;

                // Clear existing resource dictionaries
                _application.Resources.MergedDictionaries.Clear();

                // Load the default language resources first
                var defaultDict = new ResourceDictionary { Source = new Uri("Resources/Strings.xaml", UriKind.Relative) };
                _application.Resources.MergedDictionaries.Add(defaultDict);

                // Check if the resource file exists for the requested language
                if (_languageResources.ContainsKey(language))
                {
                    var languageResourceUri = _languageResources[language];
                    var languageDict = new ResourceDictionary { Source = new Uri(languageResourceUri, UriKind.Relative) };
                    _application.Resources.MergedDictionaries.Add(languageDict);
                }
                else
                {
                    // Optionally, fall back to the default language if the requested one isn't available
                    Console.WriteLine($"Warning: Language resource for '{language}' was not found. Falling back to default.");
                }
            }
        }

        private static IServiceProvider? ServiceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Language services
            var serviceCollection = new ServiceCollection();

            // Register services
            serviceCollection.AddSingleton<IAppLanguageServices, AppLanguageServices>(x => new(this));
            serviceCollection.AddTransient<ComponentUsage>();
            serviceCollection.AddTransient<MainWindow>();
            serviceCollection.AddTransient<Settings>();

            // Build service provider
            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Manually create and show MainWindow
            MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            MainWindow?.Show();

            // Persistency
            //string theme = Properties.Settings.Default.Theme;
        }
    }
}