using System;
using System.Threading.Tasks;
using Meow.Bootstrapper;
using Meow.Plugin;
using Meow.Plugin.HelpPlugin;
using Meow.Plugin.NeverStopTalkingPlugin;
using Meow.Plugin.OllamaChatPlugin;
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
                .BuildMeow();

            var helpPlugin = new HelpPlugin();
            littleTang.LoadPlugin(helpPlugin);
            littleTang.LoadPlugin(new ConsolePrintMessagePlugin());
            littleTang.LoadPlugin(new PermissionPlugin());
            // littleTang.LoadPlugin(new NeverStopTalkingPlugin());
            littleTang.LoadPlugin(new OllamaChatPlugin());

            await littleTang.Login().ConfigureAwait(false);
            System.Console.ReadLine();
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