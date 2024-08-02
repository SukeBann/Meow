using Autofac;

namespace Meow.Utils;

public class IOC
{
    /// <summary>
    /// Autofac依赖注入静态服务
    /// </summary>
    public static ILifetimeScope Container { get; set; }

    /// <summary>
    /// 获取服务(Single)  
    /// </summary>
    /// <typeparam name="T">接口类型</typeparam>
    /// <returns></returns>
    public static T? GetService<T>() where T : class
    {
        using var scope = Container.BeginLifetimeScope();
        return scope.Resolve<T>();
    }
}