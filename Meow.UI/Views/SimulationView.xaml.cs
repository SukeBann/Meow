using System.Windows.Controls;
using Meow.Core.Model.Base;
using Meow.UI.ViewModels;

namespace Meow.UI.Views;

public partial class SimulationView : UserControl
{
    public SimulationView(Core.Meow host)
    {
        InitializeComponent();
        SimulationViewModel = new SimulationViewModel(host);
        DataContext = SimulationViewModel;
    }

    #region Properties

    private SimulationViewModel SimulationViewModel { get; set; }

    #endregion

    public void Show()
    {
        var emptyTransparentWindow = new EmptyTransparentWindow
        {
            Content = this
        };
        emptyTransparentWindow.Show();
    }
}