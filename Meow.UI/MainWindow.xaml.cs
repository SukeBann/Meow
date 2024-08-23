using System.Windows;
using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface.Api;
using Meow.Bootstrapper;
using Meow.Core.Enum;
using Meow.Plugin;
using Meow.Plugin.HelpPlugin;
using Meow.Plugin.NeverStopTalkingPlugin;
using Meow.Plugin.PermissionPlugin;
using Meow.UI.Utils;
using Meow.UI.ViewModels;
using Meow.Utils;
using Serilog;

namespace Meow.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainWindowViewModel _mainWindowViewModel;

    public MainWindow()
    {
        InitializeComponent();
        _mainWindowViewModel = new MainWindowViewModel(){IsAutoScroll = true};
        DataContext = _mainWindowViewModel;
        Loaded += OnLoaded;
        RichTextBox.TextChanged += RichTextBoxExtensions.OnTextChangedAndClear;
    }

    private async void OnLoaded(object sender, RoutedEventArgs args)
    {
        const uint admin = 1052700448;
        LoggerCreator.EditLoggerConfigurationInterface += configuration => RichBoxLogConfig.GetLogRichBoxPanel(
            configuration,
            RichTextBox);

        var littleTang = MeowBootstrapper.Init()
            .ConfigurationBot(@"D:\BotWorkDir", "LittleTang", '/', ' ')
            .SetBotInfo(new BotConfig())
            .BuildMeow();

        littleTang.ImplementEventFromBotContext();

        littleTang.OnBotCaptchaEvent.Subscribe(@event =>
        {
            var (_, botCaptchaEvent) = @event;
            Ioc.GetService<Serilog.ILogger>()?.Information("Bot需要验证码识别: {CaptchaEvent}", botCaptchaEvent.ToString());
            var captcha = Console.ReadLine();
            var randStr = Console.ReadLine();
            if (captcha != null && randStr != null) littleTang.MeowBot.SubmitCaptcha(captcha, randStr);
        });

        littleTang.EditUserPermission(admin, UserPermission.Admin);

        var helpPlugin = new HelpPlugin();
        littleTang.LoadPlugin(helpPlugin);
        littleTang.LoadPlugin(new ConsolePrintMessagePlugin());
        littleTang.LoadPlugin(new PermissionPlugin());
        littleTang.LoadPlugin(new NeverStopTalkingPlugin());

        await littleTang.Login();
    }
}