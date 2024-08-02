using System.Windows;
using Meow.Utils;
using Serilog;

namespace Meow.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        var exceptionMessage = $"An error occurred : {e.Exception.Message}";
        e.Handled = true;
        try
        {
            var logger = IOC.GetService<ILogger>();
            if (logger is null)
            {
                MessageBox.Show(exceptionMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                logger?.Error("全局异常捕获:{E}", e.Exception);
            }
        }
        catch
        {
            MessageBox.Show(exceptionMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }
}