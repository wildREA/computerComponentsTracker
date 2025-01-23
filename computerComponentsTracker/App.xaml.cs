using System;
using System.Collections;
using System.Globalization;
using System.Resources;
using System.Windows;

public partial class App : Application
{
    public void ChangeLanguage(string languageCode)
    {
        CultureInfo culture = new CultureInfo(languageCode);
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        }
}
