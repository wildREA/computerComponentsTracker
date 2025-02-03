using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using computerComponentsTracker;

namespace computerComponentsTracker;

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
            { "en-US", "Resources/Strings.xaml" },
            { "ru-RU", "Resources/Strings.ru.xaml" },
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
        var serviceCollection = new ServiceCollection();

        // Register services
        serviceCollection.AddSingleton<IAppLanguageServices, AppLanguageServices>();
        serviceCollection.AddTransient<ComponentUsage>();
        serviceCollection.AddTransient<MainWindow>();
        serviceCollection.AddTransient<Settings>();

        // Build service provider
        ServiceProvider = serviceCollection.BuildServiceProvider();

        // Manually create and show MainWindow
        MainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        MainWindow?.Show();

        base.OnStartup(e);
    }
}