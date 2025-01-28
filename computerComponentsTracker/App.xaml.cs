using System;
using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using computerComponentsTracker;

public partial class App : Application
{
    public static IHost? Host { get; private set; }


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

    public static IServiceProvider? ServiceProvider { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();

        // Register services
        serviceCollection.AddSingleton<IAppLanguageServices>(sp => new AppLanguageServices(this));
        serviceCollection.AddTransient<Settings>();
        serviceCollection.AddTransient<MainWindow>();

        // Build service provider
        ServiceProvider = serviceCollection.BuildServiceProvider();

        // Manually create and show MainWindow
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }
}