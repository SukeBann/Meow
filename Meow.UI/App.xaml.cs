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
        AppDomain.CurrentDomain.UnhandledException += OnDispatcherUnhandledException;
    }

    private void OnDispatcherUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // 处理非UI线程的未处理异常
        var exception = e.ExceptionObject as Exception;
        var exceptionMessage = $"An unhandled error occurred : {exception?.Message}";

        try
        {
            var logger = Ioc.GetService<ILogger>();
            if (logger is null)
            {
                MessageBox.Show(exceptionMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                logger?.Error("全局异常捕获:{E}", exception);
            }
        }
        catch
        {
            MessageBox.Show(exceptionMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        var exceptionMessage = $"An error occurred : {e.Exception.Message}";
        e.Handled = true;
        try
        {
            var logger = Ioc.GetService<ILogger>();
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