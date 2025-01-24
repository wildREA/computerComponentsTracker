using System;
using System.Globalization;
using System.Resources;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using static App;

public partial class App : Application, IAppLanguageServices
{
    public interface IAppLanguageServices
    {
        void ChangeLanguage(string language);
    }

    public void ChangeLanguage(string language)
    {
        // Clear existing dictionaries
        Resources.MergedDictionaries.Clear();

        // Load default language
        var defaultDict = new ResourceDictionary { Source = new Uri("Resources/Strings.xaml", UriKind.Relative) };
        Resources.MergedDictionaries.Add(defaultDict);

        // Load selected language
        if (language == "ru-RU")
        {
            var russianDict = new ResourceDictionary { Source = new Uri("Resources/Strings.ru.xaml", UriKind.Relative) };
            Resources.MergedDictionaries.Add(russianDict);
        }
    }

    // Static ServiceProvider
    public static IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();

        // Registering App class as IAppLanguageServices
        serviceCollection.AddSingleton<IAppLanguageServices>(this);

        // Build the service provider
        ServiceProvider = serviceCollection.BuildServiceProvider();

        base.OnStartup(e);
    }
}
