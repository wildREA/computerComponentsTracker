using System;
using System.Windows;
using System.Resources;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using computerComponentsTracker;

public partial class App : Application
{
    public interface IAppLanguageServices
    {
        void ChangeLanguage(string language);
    }

    public class AppLanguageServices : IAppLanguageServices
    {
        private readonly Application _application;

        public AppLanguageServices(Application application)
        {
            _application = application;
        }

        public void ChangeLanguage(string language)
        {
            _application.Resources.MergedDictionaries.Clear();

            var defaultDict = new ResourceDictionary { Source = new Uri("Resources/Strings.xaml", UriKind.Relative) };
            _application.Resources.MergedDictionaries.Add(defaultDict);

            if (language == "ru-RU")
            {
                var russianDict = new ResourceDictionary { Source = new Uri("Resources/Strings.ru.xaml", UriKind.Relative) };
                _application.Resources.MergedDictionaries.Add(russianDict);
            }
        }
    }

    // Static ServiceProvider
    public static IServiceProvider? ServiceProvider { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();

        // Register AppLanguageServices as IAppLanguageServices
        serviceCollection.AddSingleton<IAppLanguageServices>(sp => new AppLanguageServices(this));

        // Register the Settings UserControl
        serviceCollection.AddTransient<computerComponentsTracker.Settings>();

        // Build service provider
        ServiceProvider = serviceCollection.BuildServiceProvider();

        base.OnStartup(e);
    }

}
