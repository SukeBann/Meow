using Lagrange.Core.Common;
using Lagrange.Core.Common.Interface.Api;
using Meow.Bootstrapper;
using Meow.Plugin;
using Meow.Plugin.HelpPlugin;
using Meow.Plugin.NeverStopTalkingPlugin;
using Meow.Plugin.PermissionPlugin;
using Meow.Utils;
using Serilog;

namespace Meow.Console;

internal class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            var littleTang = MeowBootstrapper.Init()
                .ConfigurationBot()
                .SetBotInfo(new BotConfig())
                .BuildMeow();

            littleTang.ImplementEventFromBotContext();

            littleTang.OnBotCaptchaEvent.Subscribe(@event =>
            {
                var (_, botCaptchaEvent) = @event;
                Ioc.GetService<ILogger>()?.Information("Bot需要验证码识别: {CaptchaEvent}", botCaptchaEvent.ToString());
                var captcha = System.Console.ReadLine();
                var randStr = System.Console.ReadLine();
                if (captcha != null && randStr != null) littleTang.MeowBot.SubmitCaptcha(captcha, randStr);
            });


            var helpPlugin = new HelpPlugin();
            littleTang.LoadPlugin(helpPlugin);
            littleTang.LoadPlugin(new ConsolePrintMessagePlugin());
            littleTang.LoadPlugin(new PermissionPlugin());
            littleTang.LoadPlugin(new NeverStopTalkingPlugin());

            await littleTang.Login().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            try
            {
                var logger = Ioc.GetService<ILogger>();
                logger?.Error("全局异常捕获", e);
            }
            catch
            {
                System.Console.WriteLine(e.Message);
                System.Console.WriteLine(e.StackTrace);
                return;
            }
    
            System.Console.WriteLine(e.Message);
            System.Console.WriteLine(e.StackTrace);
        }
    }
}