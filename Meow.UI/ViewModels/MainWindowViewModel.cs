using PropertyChanged;

namespace Meow.UI.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindowViewModel
{
    /// <summary>
    /// 是否自动滚动
    /// </summary>
    public bool IsAutoScroll { get; set; }
}